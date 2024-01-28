import json
import threading
import time
import uuid
import numpy as np
from whisper_live.server import TranscriptionServer
from websockets.sync.server import serve
import argparse

from process_transcript import SpeechProcessor

import torch

from openai_api import GOOD_WORDS, BAD_WORDS, OpenAiHandler

device = "cuda" if torch.cuda.is_available() else "cpu"
print('DEVICE: ', device)


class CustomServer(TranscriptionServer):
    def __init__(self):
        super().__init__()
        self.last_status = {}
        self.openai_handler = OpenAiHandler()
        '''self.test_send_t = threading.Thread(target=self.send_messages)
        self.test_send_t.setDaemon(True)
        self.test_send_t.start()

    def send_messages(self):
        while True:
            current_time = time.time() - self.start_test
            if self.status == "Start" and current_time > 30:
                self.client_socket.send('{"status": "ANALYZE"}')
            time.sleep(0.01)'''

    def recv_audio(self, websocket):
        print("New client connected")
        cid = str(uuid.uuid4())

        options = websocket.recv()
        try:
            options = json.loads(options)
            cid = options["uid"]
        except Exception as e:
            pass

        client = SpeechProcessor(
            websocket,
            multilingual=True,
            language='de',
            task='transcribe',
            client_uid=cid,
            model_size="medium",
            initial_prompt=None,
            vad_parameters=None
        )

        self.last_status[websocket] = "TRANSCRIBE"
        self.clients[websocket] = client
        self.clients_start_time[websocket] = time.time()
        while True:
            try:
                frame_data = websocket.recv()
                is_message = False
                try:
                    message_data = json.loads(frame_data)
                    status = message_data['status']
                    if self.last_status[websocket] != status:
                        if status == "ANALYZE":
                            print('Parameters: ', message_data)
                            detected_speech = self.clients[websocket].get_full_text(
                            )
                            print('Detected Speech: ', detected_speech)                            
                                                                                        
                            good_words = GOOD_WORDS
                            if 'goodwords' in message_data:
                                good_words = message_data['goodwords'].split(
                                    ';')
                            bad_words = BAD_WORDS
                            if 'badwords' in message_data:
                                bad_words = message_data['badwords'].split(
                                    ';')
                            score = self.openai_handler.judge(
                                detected_speech, good_words, bad_words, 'judge-sense')
                            
                            feels = self.openai_handler.run(detected_speech, 'feels-more')

                            final_response = {}
                            try:
                                feels_obj = json.loads(feels)
                                feels_obj = dict((k.lower(), v)
                                                 for k, v in feels_obj.items())
                                if feels_obj['gefühl'].lower() == message_data['feeling'].lower():
                                    score += 5
                                final_response['feeling'] = feels_obj.get(
                                    'gefühl')

                                if 'sätze' in feels_obj:
                                    final_response['sentences'] = feels_obj.get(
                                        'sätze')
                                elif 'ausrufe' in feels_obj:
                                    final_response['sentences'] = feels_obj.get(
                                        'ausrufe')

                            except Exception as e:
                                pass
                            final_response['score'] = score if score >= 0 else 0

                            json_msg = json.dumps({
                                "openAiResponse": final_response,
                                "transcription": {
                                    "finished": '',
                                    "current": ''
                                }
                            })
                            print(message_data['status'], json_msg)
                            self.clients[websocket].websocket.send(json_msg)

                        else:
                            print(message_data['status'])
                            self.clients[websocket].reset_text()
                        self.last_status[websocket] = status

                    is_message = True
                except Exception as e:
                    pass

                if not is_message:
                    print('receive audio')
                    frame_np = np.frombuffer(frame_data, dtype=np.float32)

                    self.clients[websocket].add_frames(frame_np)

                elapsed_time = time.time() - \
                    self.clients_start_time[websocket]
                if elapsed_time >= self.max_connection_time:
                    print('Client removed after in')
                    self.clients[websocket].disconnect()
                    self.clients[websocket].cleanup()
                    self.clients.pop(websocket)
                    self.clients_start_time.pop(websocket)
                    websocket.close()
                    del websocket
                    break

            except Exception as e:
                if self.clients[websocket].model_size is not None:
                    self.clients[websocket].cleanup()
                self.clients.pop(websocket)
                self.clients_start_time.pop(websocket)
                del websocket
                break

    def run(self, host, port=9000):
        """
        Run the transcription server.

        Args:
            host (str): The host address to bind the server.
            port (int): The port number to bind the server.
        """
        with serve(self.recv_audio, host, port) as server:
            server.serve_forever()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', '-p',
                        type=int,
                        default=9000,
                        help="Websocket port to run the server on.")
    parser.add_argument('--backend', '-b',
                        type=str,
                        default='faster_whisper',
                        help='Backends from ["tensorrt", "faster_whisper"]')
    parser.add_argument('--faster_whisper_custom_model_path', '-fw',
                        type=str, default=None,
                        help="Custom Faster Whisper Model")
    parser.add_argument('--trt_model_path', '-trt',
                        type=str,
                        default=None,
                        help='Whisper TensorRT model path')
    parser.add_argument('--trt_multilingual', '-m',
                        action="store_true",
                        help='Boolean only for TensorRT model. True if multilingual.')
    args = parser.parse_args()

    if args.backend == "tensorrt":
        if args.trt_model_path is None:
            raise ValueError("Please Provide a valid tensorrt model path")

    server = CustomServer()
    server.run(
        "0.0.0.0",
        port=args.port,
    )
