using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class MicroController : MonoBehaviour
{
    public SocketController sc;
    const int FREQUENCY = 44100;
    AudioClip mic;
    int lastPos, pos;
 
    // Use this for initialization
    void Start () {
    }

    public void openMic() {
        mic = Microphone.Start(null, true, 15, FREQUENCY);
 
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = AudioClip.Create("test", 10 * FREQUENCY, mic.channels, FREQUENCY, false);
        audio.loop = true;
    }

    public void closeMic() {
        Microphone.End(null);
    }
   
    // Update is called once per frame
    void Update () {
        if((pos = Microphone.GetPosition(null)) > 0){
            if(lastPos > pos)    lastPos = 0;
 
            if(pos - lastPos > 0){
                // Allocate the space for the sample.
                float[] sample = new float[(pos - lastPos) * mic.channels];
                // Annahme: 'audioBytes' ist das Ziel-Byte-Array
                byte[] audioBytes = new byte[sample.Length * sizeof(float)];
                // Kopieren Sie die Daten von 'sample' in 'audioBytes'
                Buffer.BlockCopy(sample, 0, audioBytes, 0, audioBytes.Length);

                sc.SendChunk(sample);


                // // Get the data from microphone.
                // mic.GetData(sample, lastPos);
 
                // // Put the data in the audio source.
                // AudioSource audio = GetComponent<AudioSource>();
                // audio.clip.SetData(sample, lastPos);
               
                // if(!audio.isPlaying)    audio.Play();
 
                // lastPos = pos;  
            }
        }
    }
 
    void OnDestroy(){
        Microphone.End(null);
    }
}
