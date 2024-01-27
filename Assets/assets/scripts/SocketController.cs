using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

using SocketIOClient;
using Newtonsoft.Json;
using SocketIOClient.Newtonsoft.Json;

public class SocketController : MonoBehaviour
{
    public GameController game;
    SocketIOUnity socket;

    private static string URL = "http://localhost";
    private static string PORT = "3000";

    bool isConnected = false;
    // Start is called before the first frame update
    void Start()
    {
      socket = new SocketIOUnity(new Uri(URL+":"+PORT), new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        ///// reserved socketio events
        socket.OnConnected += (sender, e) =>
        {
          this.isConnected = true;
          this.game.startGame();
            Debug.Log("socket.OnConnected");
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };

        Debug.Log("Connecting...");
        socket.Connect();

        socket.OnUnityThread("unitytest", (data) =>
        {
            Debug.Log("incoming mesagesiuh");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendChunk(float[] data) {
      if (isConnected) {
        string jsonString = JsonConvert.SerializeObject(data);
        socket.Emit("purebytes", jsonString);
      }
    }

    void AcceptMessage() {

    }
}
