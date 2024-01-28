FROM nvidia/cuda:12.2.2-cudnn8-runtime-ubuntu22.04

ARG DEBIAN_FRONTEND=noninteractive

# Remove any third-party apt sources to avoid issues with expiring keys.
RUN rm -f /etc/apt/sources.list.d/*.list

# Install some basic utilities.
RUN apt-get update

RUN apt-get install -y \
    curl \
    ca-certificates \
    sudo \
    bzip2 \ 
    libx11-6 portaudio19-dev ffmpeg wget
    
    
RUN rm -rf /var/lib/apt/lists/*

RUN apt update

RUN apt install python3-dev -y && \
    apt install python-is-python3

RUN apt install python3-pip -y
    

# Create a working directory.
RUN mkdir /app
WORKDIR /app

RUN apt update --fix-missing
RUN python -m pip install whisper-live

COPY src/download.py download.py
RUN python download.py
COPY src/download-medium.py download-medium.py
RUN python download-medium.py
#COPY src/download-large.py download-large.py
#RUN python download-large.py

COPY requirements.txt requirements.txt 
RUN python -m pip install -r requirements.txt

COPY src src

CMD ["python", "src/server_for_unity.py"]


