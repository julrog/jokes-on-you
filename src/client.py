import json
import threading
import time
from whisper_live.client import TranscriptionClient, Client


class TestUnityClient(Client):
    def __init__(self, host=None, port=None, is_multilingual=False, lang=None, translate=False, model_size="small"):
        super().__init__(host, port, is_multilingual, lang, translate, model_size)
        self.start_test = time.time()
        self.status = "Start"
        self.test_send_t = threading.Thread(
            target=self.send_messages)
        self.test_send_t.setDaemon(True)
        self.test_send_t.start()

    def send_messages(self):
        while True:
            current_time = time.time() - self.start_test
            if self.status == "Start" and current_time > 15:
                self.client_socket.send(
                    '{"status": "ANALYZE", "feeling": "Traurig", "badwords": "Mond;Space", "goodwords": "Sport;Nahrung"}')
                self.status = "ANALYZE"

            if self.status == "ANALYZE" and current_time > 30:
                self.client_socket.send('{"status": "TRANSCRIBE"}')
                self.status = "TRANSCRIBE"
            time.sleep(0.01)

    def on_message(self, ws, message):
        self.last_response_recieved = time.time()
        message = json.loads(message)
        # print(message)

        if "status" in message.keys():
            if message["status"] == "WAIT":
                self.waiting = True
                print(
                    f"[INFO]:Server is full. Estimated wait time {round(message['message'])} minutes."
                )
            elif message["status"] == "ERROR":
                print(f"Message from Server: {message['message']}")
                self.server_error = True
            return

        if "message" in message.keys() and message["message"] == "DISCONNECT":
            print("[INFO]: Server overtime disconnected.")
            self.recording = False

        if "message" in message.keys() and message["message"] == "SERVER_READY":
            self.recording = True
            return

        if "language" in message.keys():
            self.language = message.get("language")
            lang_prob = message.get("language_prob")
            print(
                f"[INFO]: Server detected language {self.language} with probability {lang_prob}"
            )
            return

        if "segments" not in message.keys():
            return

        # message = message["segments"]
        print(message)


class TestUnityTranscriptionClient(TranscriptionClient):
    def __init__(self, host, port, is_multilingual=False, lang=None, translate=False, model_size="small"):
        self.client = TestUnityClient(host, port, is_multilingual,
                                      lang, translate, model_size)


client = TestUnityTranscriptionClient(
    "localhost",
    9000,
    is_multilingual=True,
    lang="de",
    translate=False,
    model_size="medium"
)

client()
