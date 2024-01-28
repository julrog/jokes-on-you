from openai_api import OpenAiHandler


oai = OpenAiHandler()

feelings = ['Interessiert', 'Traurig', 'Wütend', 'Gelangweilt', 'Glücklich']
bad_words = ['Mond', 'Raumschiff', 'Essen', 'Sport']
good_words = ['Essen', 'Sport', 'Mond', 'Raumschiff',]
good_words_string = ' '.join(good_words)
bad_words_string = ' '.join(bad_words)
feelings_string = ' '.join(feelings)


oai.set_config(
    {'full': {"role": "system", "content": f'Du bist ein Außerirdischer, der in einer Weltraumbar einen seltenen Menschen zuhört. Du bewertest die Aussagen den Menschen. Themen die ähnlich zu folgenden Wörtern passen, kommen gut an: ${good_words_string}. Themen die ähnlich zu folgenden Wörtern sind, kommen sehr sehr schlecht an: ${bad_words_string}. Gib eine Sterne-Bewertung von 1 bis 5 ab, wobei 1 ist miserable 5 ist außerordenlich interessant. Zudem gib an, welches dieser Gefühle das Gesagte am ehesten bei dir auslöst? ${feelings_string}. Zuletz gibt eine Liste von mindestens 4 sehr kurzen mit jeweils maximal 8 Wörtern Sätzen, die du dem Comedian als Reaktion vom Publikum zurückrufen würdest. Antworte mit einem JSON-String mit Bewertung, Gefühl und dem Array von Sätzen.'}, 'regular': {"role": "system", "content": f'Du bist ein Außerirdischer, der in einer Weltraumbar einen seltenen Menschen zuhört. Du bewertest die Aussagen des Menschen. Gib an, welches dieser Gefühle das Gesagte am ehesten bei dir auslöst? ${feelings_string}. Zuletzt gibt eine Liste von mindestens 4 sehr kurzen Sätzen mit jeweils maximal 8 Wörtern, die du dem Comedian als Reaktion vom Publikum zurückrufen würdest. Antworte mit einem JSON-String mit dem Gefühl und dem Array von Sätzen in Form: (Gefühl, Sätze).'},
     'regular-mod': {"role": "system", "content": f'Du bist ein Außerirdischer, der in einer Weltraumbar einen seltenen Menschen zuhört. Gib an, welches dieser Gefühle das Gesagte am ehesten bei dir auslöst? ${feelings_string}. Zuletzt gibt eine Liste von mindestens 4 sehr kurzen Sätzen mit jeweils maximal 8 Wörtern, die du dem Comedian als Reaktion vom Publikum zurückrufen würdest. Antworte mit einem JSON-String mit dem Gefühl und dem Array von Sätzen in Form: (Gefühl, Sätze).'},
     'judge': {"role": "system", "content": f'Gib durch einen Boolean an, welche von den Wörtern: [${good_words_string}] thematisch zu dem Text passt der gleich gesagt wird. Jedes Wort das ähnlich oder gut im Kontext zu dem gesagten passt soll mit "true" markiert werden. Gib ein JSON einem Boolean je Wort.'}
     })

# print(oai.run('Eigentlich wollte ich einen Origami-Kurs belegen, aber kannste knicken!'))
# print(oai.run('Wenn ihr Bouldert ist das wichtigste der Respekt vor der Wand, denn ich bin dabei gestorben.'))

# print(oai.run('Gestern ist mein Hund beim Autounfall gestorben, aber dann wieder auferstanden und kann nun die besten Witze erzählen.', 'regular'))
print(oai.run('Gestern ist mein Hund beim Autounfall gestorben, aber dann wieder auferstanden und kann nun die besten Witze erzählen.', 'judge'))
print(oai.run('Ich bin neulich beim Sportunterricht durchgefallen.', 'judge'))
print(oai.run('Ich habe heute beim Flug mit dem Spaceshuttle um einen Himmelskörper etwas vom Kuchen gegessen.', 'judge'))
