import json
from openai import OpenAI
from dotenv import load_dotenv

load_dotenv()

feelings = ['Interessiert', 'Traurig', 'Wütend', 'Gelangweilt', 'Lachend']
feelings_string = ' '.join(feelings)
ALIEN_FEEL_CONFIG = {
    'feels': {"role": "system", "content": f'Du bist ein Außerirdischer, der in einer Weltraumbar einen seltenen Menschen zuhört. Gib an, welches dieser Gefühle das Gesagte am ehesten bei dir auslöst? ${feelings_string}. Zuletzt gibt eine Liste von mindestens 4 sehr kurzen Sätzen mit jeweils maximal 8 Wörtern, die du dem Comedian als Reaktion vom Publikum zurückrufen würdest. Antworte mit einem JSON-String mit dem Gefühl und dem Array von Sätzen in Form: (Gefühl, Sätze).'},
    'feels-more': {"role": "system", "content": f'Du bist ein Außerirdischer, der in einer Weltraumbar einen seltenen Menschen zuhört. Gib an, welches dieser Gefühle [${feelings_string}] das Gesagte am ehesten bei dir auslöst? Zuletzt gibt eine Liste von MINDESTENS 10 sehr kurzen Ausrufen, die du dem Redner als Reaktion vom Publikum zurückrufen würdest. Die Ausrufe sollten jeweils maximal 6 Wörter lang sein. Antworte mit einem JSON-String mit dem Gefühl und dem Array von Ausrufen in Form von: (Gefühl, Ausrufe).'},
}
ALIEN_JUDGE_CONFIG = {
    'judge': 'Gib durch einen Boolean an, welche von den Wörtern: [{words}] thematisch zu dem Text passt der gleich gesagt wird. Jedes Wort das ähnlich oder gut im Kontext zu dem gesagten passt soll mit "true" markiert werden. Gib ein JSON einem Boolean je Wort.',
    'judge-sense': 'Gib durch einen Boolean an, welche von den Wörtern: [{words}] thematisch zu dem Text passt der gleich gesagt wird. Jedes Wort das ähnlich oder gut im Kontext zu dem gesagten passt soll mit "true" markiert werden. Gib ein JSON einem Boolean je Wort und füge ein weiteres Feld hinzu das prüft, ob das Gesagte wie ein echter Text wirkt und bewerte es mit "true" wenn es zutrifft und "false" wenn nicht. Das Feld soll den Namen "sinnvoll" haben.'
}
GOOD_WORDS = ['Essen', 'Sport']
BAD_WORDS = ['Mond', 'Raumschiff']


class OpenAiHandler:
    def __init__(self) -> None:
        self.client = OpenAI(organization='org-aymUQS2F0jigiUYI1EsyGBwH')
        self.feel_config = ALIEN_FEEL_CONFIG
        self.judge_config = ALIEN_JUDGE_CONFIG

    def set_config(self, config):
        self.feel_config = config

    def run(self, text, config='regular'):
        completion = self.client.chat.completions.create(
            model="gpt-3.5-turbo-1106",
            response_format={"type": "json_object"},
            messages=[
                self.feel_config[config],
                {"role": "user", "content": text}
            ]
        )
        return completion.choices[0].message.content

    def judge(self, text, good_words=GOOD_WORDS, bad_words=BAD_WORDS, config='judge'):
        word_text = ' '.join(good_words)
        word_text += ' '.join(bad_words)
        judge_options = {
            "role": "system", "content": ALIEN_JUDGE_CONFIG[config].format(words=word_text)}
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
            
            if 'sinnvoll' in judging:
                if type(judging['sinnvoll']) == bool and not judging['sinnvoll']:
                    score -= 5

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
