# Story Teller

Tell the aliens stories and trigger different emotions, avoiding words and topics that the aliens don't like and instead using topics that they do like.
In-game language is German only for now

Deutsch:
Erzähle den Aliens Geschichten und löse verschiedene Emotionen aus, vermeide dabei Worte und Themen, die die Aliens nicht mögen und nutze stattdessen Themen, die sie mögen.

# Getting Started

## Server

### Requirements

* You need to have [Docker](https://www.docker.com/) installed.
* You need to have an [OpenAI-API](https://platform.openai.com/docs/quickstart/step-2-setup-your-api-key) Key

### Start server

* Clone this repository
* Add your OpenAI access details in a `.env` file inside the repo folder. The file should look like this:

```txt
OPENAI_API_KEY=yourkey
OPENAI_API_ORG=yourorg
```
* Build and start the container:

```bash
docker build . -t jokes-on-you -f docker/Dockerfile
docker run -it --gpus all -p 9000:9000 --env-file .env jokes-on-you:latest 
```

### Start the Game via Unity

