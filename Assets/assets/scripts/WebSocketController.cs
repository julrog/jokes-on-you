using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

[System.Serializable]
public class Transcription
{
  public string current;
  public string finished;
}

[System.Serializable]
public class OpenAiResponse
{
  public int score;
  public string feeling;
  public string[] sentences;
}

[System.Serializable]
public class Response
{
  public Transcription transcription;
  public OpenAiResponse openAiResponse;
}

public class WebSocketController : MonoBehaviour
{
  WebSocket websocket;
  public GameController game;

  bool isConnected = false;
  bool serverIsReasy = false;
  // Start is called before the first frame update
  async void Start()
  {
    websocket = new WebSocket("ws://localhost:9000");

    websocket.OnOpen += () =>
    {
      this.isConnected = true;
      this.game.startGame();
      Debug.Log("Connection open!");
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
      game.closeCurtains();
    };

    websocket.OnMessage += (bytes) =>
    {
      Debug.Log("OnMessage!");
      // Debug.Log(bytes);
      // getting the message as a string
      var message = System.Text.Encoding.UTF8.GetString(bytes);
      Debug.Log("OnMessage! " + message);
      Response playerData = JsonUtility.FromJson<Response>(message);
      Debug.Log("incoming mesages" + message);
      if (playerData != null)
      {
        Transcription trans = playerData.transcription;
        OpenAiResponse aiRes = playerData.openAiResponse;
        if (trans.finished != "")
        {
          game.saveToStaticText(trans.finished);
        }
        if (trans.current != "" && this.game.canTalk)
        {
          game.setSpeechText(trans.current);
        }
        if (aiRes.feeling != "")
        {
          Debug.Log("OpenAIRes beeing triggered" + aiRes.feeling);
          game.OpenAIResponse(aiRes);
        }
      }
      else
      {
        Debug.Log("asdasdasd" + message);
      }
    };

    // Keep sending messages at every 0.3s
    // InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

    // waiting for messages
    Debug.Log("try to connect");
    await websocket.Connect();
  }

  void Update()
  {
#if !UNITY_WEBGL || UNITY_EDITOR
    websocket.DispatchMessageQueue();
#endif
  }

  // async void SendWebSocketMessage()
  // {
  //   if (websocket.State == WebSocketState.Open)
  //   {
  //     // Sending bytes
  //     await websocket.Send(new byte[] { 10, 20, 30 });

  //     // Sending plain text
  //     await websocket.SendText("plain text message");
  //   }
  // }

  public async void SendTranscriptionEnd()
  {
    CriteriaStruct gs = game.gameStruct.currentCrits;
    Debug.Log("GS:" + gs.feeling);
    string combinedString = string.Join(";", gs.noGos);
    string combinedString1 = string.Join(";", gs.toUse);
    await websocket.SendText("{\"status\": \"ANALYZE\", \"feeling\": \"" + gs.feeling + "\", \"badwords\": \"" + combinedString + "\", \"goodwords\": \"" + combinedString1 + "\"}");
  }

  public async void StartSendTranscription()
  {
    await websocket.SendText("{\"status\": \"TRANSCRIBE\"}");
  }

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }

  public async void SendChunk(byte[] data)
  {
    // Sending bytes
    if (this.isConnected/* && this.game.canTalk*/)
    {
      await websocket.Send(data);
    }
  }
}
