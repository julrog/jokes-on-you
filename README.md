# Story Teller

Tell the aliens stories and trigger different emotions, avoiding words and topics that the aliens don't like and instead using topics that they do like.
In-game language is German only for now.

Deutsch:
Erzähle den Aliens Geschichten und löse verschiedene Emotionen aus, vermeide dabei Worte und Themen, die die Aliens nicht mögen und nutze stattdessen Themen, die sie mögen.

# Getting Started

## Server

### Requirements

* You need to have [Docker](https://www.docker.com/) installed.
* You need to have an [OpenAI-API](https://platform.openai.com/docs/quickstart/step-2-setup-your-api-key) Key

If you dont have a company key try it here:
[OpenAI-API](https://platform.openai.com/account/organization)

### Start server

* Clone this repository
* Add your OpenAI access details in a `.env` file inside the repo folder. The file should look like this:

```txt
OPENAI_API_KEY=yourkey
OPENAI_API_ORG=yourorg
```

or copy .env.example and rename it to .env and edit the properties in there

* Build and start the container:
* Run the following 2 commands in your Terminal from the root folder of the project
```bash
docker build . -t jokes-on-you -f docker/Dockerfile
docker run -it --gpus all -p 9000:9000 --env-file .env jokes-on-you:latest 
```

* The audio transcription works best with a NVIDIA-GPU on Windows

## Start the Game via Unity

* Have the server running locally.
* Load the build game from https://globalgamejam.org/games/2024/storyteller-8 or start the Unity project.
* The curtains in game open when the game can successfully connect to the server
* Enjoy!
