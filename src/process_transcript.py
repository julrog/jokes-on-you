import json
import logging
import textwrap
import threading
import time
from whisper_live.transcriber import WhisperModel
import torch
from whisper_live.server import ServeClient



class SpeechProcessor(ServeClient):
    RATE = 16000
    SERVER_READY = "SERVER_READY"
    DISCONNECT = "DISCONNECT"

    def __init__(
        self,
        websocket,
        task="transcribe",
        device=None,
        multilingual=False,
        language=None,
        client_uid=None,
        model_size="small",
        initial_prompt=None,
        vad_parameters={"threshold": 0.8}
    ):
        self.client_uid = client_uid
        self.data = b""
        self.frames = b""
        self.model_sizes = [
            "tiny", "base", "small", "medium", "large-v2", "large-v3"
        ]
        self.multilingual = multilingual
        self.model_size = self.get_model_size(model_size)
        self.language = language if self.multilingual else "en"
        self.task = task
        self.websocket = websocket
        self.initial_prompt = initial_prompt
        self.vad_parameters = vad_parameters or {"threshold": 0.5}
        
        device = "cuda" if torch.cuda.is_available() else "cpu"
        
        if self.model_size == None:
            return
        
        self.transcriber = WhisperModel(
            self.model_size, 
            device=device,
            compute_type="int8" if device=="cpu" else "float16", 
            local_files_only=False,
        )
        
        self.timestamp_offset = 0.0
        self.frames_np = None
        self.frames_offset = 0.0
        self.text = []
        self.current_out = ''
        self.prev_out = ''
        self.t_start=None
        self.exit = False
        self.same_output_threshold = 0
        self.show_prev_out_thresh = 5   # if pause(no output from whisper) show previous output for 5 seconds
        self.add_pause_thresh = 3       # add a blank to segment list as a pause(no speech) for 3 seconds
        self.transcript = []
        self.send_last_n_segments = 10

        # text formatting
        self.wrapper = textwrap.TextWrapper(width=50)
        self.pick_previous_segments = 2

        # threading
        self.trans_thread = threading.Thread(target=self.speech_to_text)
        self.trans_thread.start()
        self.current_finished_text = ''
        self.current_segment = 0
        self.last_segment_text = ''

    def speech_to_text(self):
        """
        Process an audio stream in an infinite loop, continuously transcribing the speech.

        This method continuously receives audio frames, performs real-time transcription, and sends
        transcribed segments to the client via a WebSocket connection.

        If the client's language is not detected, it waits for 30 seconds of audio input to make a language prediction.
        It utilizes the Whisper ASR model to transcribe the audio, continuously processing and streaming results. Segments
        are sent to the client in real-time, and a history of segments is maintained to provide context.Pauses in speech 
        (no output from Whisper) are handled by showing the previous output for a set duration. A blank segment is added if 
        there is no speech for a specified duration to indicate a pause.

        Raises:
            Exception: If there is an issue with audio processing or WebSocket communication.

        """
        while True:
            if self.exit:
                logging.info("Exiting speech to text thread")
                break

            if self.frames_np is None:
                continue

            # clip audio if the current chunk exceeds 30 seconds, this basically implies that
            # no valid segment for the last 30 seconds from whisper
            if self.frames_np[int((self.timestamp_offset - self.frames_offset)*self.RATE):].shape[0] > 25 * self.RATE:
                duration = self.frames_np.shape[0] / self.RATE
                self.timestamp_offset = self.frames_offset + duration - 5

            samples_take = max(0, (self.timestamp_offset -
                               self.frames_offset)*self.RATE)
            input_bytes = self.frames_np[int(samples_take):].copy()
            duration = input_bytes.shape[0] / self.RATE
            if duration < 1.0:
                continue
            try:
                input_sample = input_bytes.copy()

                print('receive audio')
                # whisper transcribe with prompt
                result, info = self.transcriber.transcribe(
                    input_sample,
                    initial_prompt=self.initial_prompt,
                    language=self.language,
                    task=self.task,
                    vad_filter=True,
                    vad_parameters=self.vad_parameters
                )

                if self.language is None:
                    if info.language_probability > 0.5:
                        self.language = info.language
                        logging.info(
                            f"Detected language {self.language} with probability {info.language_probability}")
                        self.websocket.send(json.dumps(
                            {"uid": self.client_uid, "language": self.language, "language_prob": info.language_probability}))
                    else:
                        # detect language again
                        continue

                last_segment = None
                new_text = ''
                if len(result):
                    self.t_start = None
                    last_segment = self.update_segments(result, duration)
                    if len(self.transcript) < self.send_last_n_segments:
                        segments = self.transcript
                    else:
                        segments = self.transcript[-self.send_last_n_segments:]
                    if last_segment is not None:
                        segments = segments  # + [last_segment]
                    current_segment_count = self.current_segment
                    transcript_count = len(self.transcript)
                    if transcript_count > current_segment_count:
                        parse_new = len(self.transcript) - \
                            current_segment_count
                        if parse_new > 0:
                            for i in range(parse_new):
                                new_text += self.transcript[current_segment_count + i]['text']
                            self.current_segment = transcript_count
                            self.current_finished_text += new_text
                else:
                    # show previous output if there is pause i.e. no output from whisper
                    segments = []
                    if self.t_start is None:
                        self.t_start = time.time()
                    current_segment_count = self.current_segment
                    transcript_count = len(self.transcript)
                    if transcript_count > current_segment_count:
                        parse_new = len(self.transcript) - \
                            current_segment_count
                        if parse_new > 0:
                            for i in range(parse_new):
                                new_text += self.transcript[current_segment_count + i]['text']
                            self.current_segment = transcript_count
                            self.current_finished_text += new_text
                    if time.time() - self.t_start < self.show_prev_out_thresh:
                        if len(self.transcript) < self.send_last_n_segments:
                            segments = self.transcript
                        else:
                            segments = self.transcript[-self.send_last_n_segments:]

                    # add a blank if there is no speech for 3 seconds
                    if len(self.text) and self.text[-1] != '':
                        if time.time() - self.t_start > self.add_pause_thresh:
                            self.text.append('')

                try:
                    current = ''
                    if last_segment is not None:
                        current = last_segment['text']
                        self.last_segment_text = current
                    else:
                        self.last_segment_text = ''
                    json_msg = json.dumps({
                        "openAiResponse": {
                            "score": 0,
                            "feeling": "",
                            "sentences": []
                        },
                        "transcription": {
                            "finished": new_text,
                            "current": current
                        }
                    })
                    # print('dattaaa: ', json_msg)
                    self.websocket.send(json_msg)
                except Exception as e:
                    logging.error(
                        f"[ERROR]: Failed to send message to client: {e}")

            except Exception as e:
                logging.error(
                    f"[ERROR]: Failed to transcribe audio chunk: {e}")
                time.sleep(0.01)
        print('Speech ended?')

    def get_full_text(self):
        finished_result = self.current_finished_text + self.last_segment_text
        self.current_finished_text = ''
        return finished_result

    def reset_text(self):
        self.current_finished_text = ''

    def cleanup(self):
        super().cleanup()
