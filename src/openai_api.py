import json
from openai import OpenAI
from dotenv import load_dotenv

load_dotenv()

feelings = ['Interessiert', 'Traurig', 'Wütend', 'Gelangweilt', 'Lachend']
feelings_string = ' '.join(feelings)
ALIEN_CONFIG = {
    'feels': {"role": "system", "content": f'Du bist ein Außerirdischer, der in einer Weltraumbar einen seltenen Menschen zuhört. Gib an, welches dieser Gefühle das Gesagte am ehesten bei dir auslöst? ${feelings_string}. Zuletzt gibt eine Liste von mindestens 4 sehr kurzen Sätzen mit jeweils maximal 8 Wörtern, die du dem Comedian als Reaktion vom Publikum zurückrufen würdest. Antworte mit einem JSON-String mit dem Gefühl und dem Array von Sätzen in Form: (Gefühl, Sätze).'},
}
GOOD_WORDS = ['Essen', 'Sport']
BAD_WORDS = ['Mond', 'Raumschiff']


class OpenAiHandler:
    def __init__(self) -> None:
        self.client = OpenAI(organization='org-aymUQS2F0jigiUYI1EsyGBwH')
        self.config = ALIEN_CONFIG

    def set_config(self, config):
        self.config = config

    def run(self, text, config='regular'):
        completion = self.client.chat.completions.create(
            model="gpt-3.5-turbo-1106",
            response_format={"type": "json_object"},
            messages=[
                self.config[config],
                {"role": "user", "content": text}
            ]
        )
        return completion.choices[0].message.content

    def judge(self, text, good_words=GOOD_WORDS, bad_words=BAD_WORDS):
        word_text = ' '.join(good_words)
        word_text += ' '.join(bad_words)
        judge_options = {
            "role": "system", "content": f'Gib durch einen Boolean an, welche von den Wörtern: [${word_text}] thematisch zu dem Text passt der gleich gesagt wird. Jedes Wort das ähnlich oder gut im Kontext zu dem gesagten passt soll mit "true" markiert werden. Gib ein JSON einem Boolean je Wort.'}
        completion = self.client.chat.completions.create(
            model="gpt-3.5-turbo-1106",
            response_format={"type": "json_object"},
            messages=[
                judge_options,
                {"role": "user", "content": text}
            ]
        )
        judging = completion.choices[0].message.content
        print('JUDGING: ', judging)

        score = 0
        try:
            judging = json.loads(judging)
            judging = dict((k.lower(), v)
                           for k, v in judging.items())
            for gw in good_words:
                if gw.lower() in judging:
                    try:
                        if type(judging[gw.lower()]) == bool and judging[gw.lower()]:
                            score += 1
                    except Exception as e:
                        pass
            for bw in bad_words:
                if bw.lower() in judging:
                    try:
                        if type(judging[bw.lower()]) == bool and judging[bw.lower()]:
                            score -= 2
                    except Exception as e:
                        pass
        except Exception as e:
            pass
        return score

    def close(self):
        self.client.close()


if __name__ == "__main__":
    oai = OpenAiHandler()
