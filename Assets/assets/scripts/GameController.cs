using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class GameController : MonoBehaviour
{
    public CurtainAnimator curtainAnimator;
    public WebSocketMicroController micController;
    public GameObject notePlane;

    public GameStruct gameStruct;

    public int round = 0;
    public int maxRound;
    public int currentScore;

    public bool canTalk = false;

    public int ownTimer = 15;

    public TMP_Text timerText;

    public TMP_Text speechText;
    public string finishedText;

    public AlienController[] aliens;
    // Start is called before the first frame update
    void Start()
    {
        // this.startGame();
        gameStruct.selectNewGameCriteria();
    }

    private int count = 0;
    private float timer = 0f;
    private float updateInterval = 1f;

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("canTalk" + this.canTalk);
        bool oldTalkState = canTalk;
        if (canTalk) {
            // Aktualisieren Sie den Timer
            timer += Time.deltaTime;

            // Überprüfen Sie, ob eine Sekunde vergangen ist
            if (timer >= updateInterval)
            {
                // Inkrementieren Sie die Zählung
                count++;

                // Hier können Sie die Zählung verwenden oder anderweitig verarbeiten
                Debug.Log("Count: " + count);
                timerText.text = ""+(ownTimer - count);
                // Setzen Sie den Timer zurück
                timer = 0f;
            }
        }
        // Debug.Log(":::" + count + ":" + ownTimer + ";" + canTalk);
        if (count == ownTimer && canTalk) {
            Debug.Log("finish trough timeout");
            // send and wait for response.
            canTalk = false;
            count = 0;
        }

        if (oldTalkState && !canTalk) {
            this.endRound();
        }
    }

    public void endRound() {
        // send 
        Debug.Log("Send break to websocket server and wait for response");
        this.micController.closeMic();
        count = 0;
        timerText.text = ""+(ownTimer);
    }

    public void startGame() {
        curtainAnimator.openCurtains();
    }

    public void setSpeechText(string text) {
        this.speechText.text = this.finishedText + text;
    }

    public void saveToStaticText(string text) {
        Debug.Log("will save finished Text" + text);
        this.finishedText = this.finishedText + text;
        this.speechText.text = this.finishedText;
    }

    public void OpenAIResponse(/*OpenAiResponse response*/) {
        Debug.Log("incoming api response");
        OpenAiResponse response = new OpenAiResponse();
        response.score = 7;
        this.round = this.round + 1;
        Debug.Log("this.round" + this.round + this.maxRound);
        this.currentScore = this.currentScore / this.round;

        if (this.currentScore < response.score) {
            this.GoodAIResponse();
        } else {
            this.BadAIResponse();
        }

        gameStruct.selectNewGameCriteria();

        if (this.round >= this.maxRound) {
            Debug.Log("Close and finish");
            Invoke("closeCurtains", 5f);
        }
    }

    void closeCurtains() {
        curtainAnimator.closeCurtains();
    }

    public async void GoodAIResponse() {
        Debug.Log("will start good ai respo.");
        var aliensNotOnStage = this.aliens.Where(alienController => !alienController.onStage);
        if (aliensNotOnStage.Any()) {
            int randomIndex = Random.Range(1, aliensNotOnStage.Count());
            for (int i = 0; i < randomIndex; i++) {
                AlienController alien = aliensNotOnStage.ElementAt(i);
                alien.moveAlien();
            }
        }
    }

    public async void BadAIResponse() {
        var aliensNotOnStage = this.aliens.Where(alienController => alienController.onStage);
        if (aliensNotOnStage.Any()) {
            int randomIndex = Random.Range(1, aliensNotOnStage.Count());
            for (int i = 0; i < randomIndex; i++) {
                AlienController alien = aliensNotOnStage.ElementAt(i);
                alien.alienIdle();
            }
        }
    }
}
