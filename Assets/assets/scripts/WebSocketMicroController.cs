using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class WebSocketMicroController : MonoBehaviour
{
    public WebSocketController sc;
    const int FREQUENCY = 16000;
    int lastPos, pos;

    private float updateInterval = 1f;  // Throttle auf 1 Sekunde
    private float timer = 0f;

    private const int chunkSize = 1024;

    private Coroutine myCoroutine;
 
    // Use this for initialization
    void Start()
    {
    }

    public void openMic() {
        Debug.Log("Mic will open");
        sc.StartSendTranscription();
        // Beginnen Sie mit der Aufnahme vom Mikrofon
        AudioClip microphoneClip = Microphone.Start(null, true, 60, FREQUENCY);
        
        // Warten Sie, bis das Mikrofon bereit ist
        while (!(Microphone.GetPosition(null) > 0)) { }

        // Setzen Sie die Position des Mikrofons auf den Anfang
        Microphone.GetPosition(null);

        // Hier können Sie die Chunk-Logik implementieren und die Daten senden oder anderweitig verarbeiten
        myCoroutine = StartCoroutine(SendMicrophoneChunks(microphoneClip));
    }

    public void closeMic() {
      Invoke("invoke", 5f);
      Microphone.End(null);
    }

    void invoke() {
      sc.game.canTalk = false;
      sc.SendTranscriptionEnd();
      StopCoroutine(myCoroutine);
    }
 
    void OnDestroy(){
        Microphone.End(null);
    }

    byte[] ConvertFloatArrayToByteArray(float[] floatArray)
    {
        byte[] byteArray = new byte[floatArray.Length]; // 4 Bytes pro float
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    IEnumerator SendMicrophoneChunks(AudioClip microphoneClip)
    {
        int position = 0;
        int lpos = 0;

        while (true)
        {
            // Warten Sie, bis genügend Daten für einen Chunk verfügbar sind
            while (position + chunkSize > microphoneClip.samples)
            {
                yield return null;
            }

            // Extrahieren Sie die Daten für den aktuellen Chunk
            float[] data = new float[chunkSize * 4];
            microphoneClip.GetData(data, position);

            // Konvertieren Sie die Float-Daten in Byte-Daten
            byte[] byteData = ConvertFloatArrayToByteArray(data);

            // Hier können Sie den Chunk senden oder anderweitig verwenden
            // Debug.Log("Sending chunk of size: " + byteData.Length + " bytes");
            // Debug.Log("send data");
            sc.SendChunk(byteData);

            // Aktualisieren Sie die Position für den nächsten Chunk
            position += chunkSize;

            // Get the data from microphone.
            // microphoneClip.GetData(data, lpos);

            // // Put the data in the audio source.
            // AudioSource audio = GetComponent<AudioSource>();
            // audio.clip.SetData(data, lpos);
            
            // if(!audio.isPlaying) {
            //   audio.Play();
            // }

            // lastPos = pos;  

            // Optional: Fügen Sie eine Pause ein, um die Rate zu steuern
            yield return new WaitForSeconds(0.1f);
        }
    }
}
