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

    public TMP_Text roundText;
    public TMP_Text scoreText;
    public TMP_Text feelingText;

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

    public bool canDecreaseMicTimer = true;

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
                if (canDecreaseMicTimer) {
                    timerText.text = ""+(ownTimer - count);
                }
                // Setzen Sie den Timer zurück
                timer = 0f;
            }
        }
        // Debug.Log(":::" + count + ":" + ownTimer + ";" + canTalk);
        if (count == ownTimer && canTalk) {
            Debug.Log("finish trough timeout");
            // send and wait for response.
            count = 0;
        }

        if (oldTalkState && !canTalk) {
            this.endRound();
        }
    }

    public void endRound() {
        // send 
        canDecreaseMicTimer = false;
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

    public void OpenAIResponse(OpenAiResponse response) {
        Debug.Log("incoming api response");
        // OpenAiResponse response = new OpenAiResponse();
        Debug.Log("this.round" + this.round + this.maxRound);
        
        this.round = this.round + 1;
        this.DisplayComments(response.sentences);

        Debug.Log("this.currentScore:" + this.currentScore + "response.score:" + response.score + "this.round:" + this.round);
        if (this.round == 0) {
            this.GoodAIResponse(response.sentences);
        } else {
            if (response.score >= (this.currentScore / this.round)) {
                this.GoodAIResponse(response.sentences);
            } else {
                this.BadAIResponse(response.sentences);
            }
        }

        this.currentScore += response.score;
        
        gameStruct.selectNewGameCriteria();
        
        this.scoreText.text = "" + (this.currentScore / this.round);
        this.roundText.text = "" + (this.round + 1);
        this.feelingText.text = "Aliens sind: " + response.feeling;

        if (this.round > this.maxRound) {
            Debug.Log("Close and finish");
            var aliensNotOnStage = this.aliens.Where(alienController => alienController.onStage);
            if (aliensNotOnStage.Any()) {
                int randomIndex = Random.Range(1, aliensNotOnStage.Count());
                for (int i = 0; i < randomIndex; i++) {
                    AlienController alien = aliensNotOnStage.ElementAt(i);
                    alien.partyHard();
                }
            }
            Invoke("closeCurtains", 5f);
        }
    }

    void closeCurtains() {
        curtainAnimator.closeCurtains();
    }

    public void callOpenAIResponse() {
        OpenAiResponse res = new OpenAiResponse();
        res.score = -1;
        res.feeling = "Glücklich";
        res.sentences = new string[] {"WAS?", "Diggi"};
        this.OpenAIResponse(res);
    }

    async void DisplayComments(string[] msges) {
        string txtToDisplay = msges[Random.Range(0, msges.Length - 1)];
        var aliensNotOnStage = this.aliens.Where(alienController => alienController.onStage);
        if (aliensNotOnStage.Any()) {
            int randomIndex = aliensNotOnStage.Count();
            for (int i = 0; i < randomIndex; i++) {
                int canITellYouSomething = Random.Range(0, 4);
                if (canITellYouSomething >= 2) {
                    AlienController alien = aliensNotOnStage.ElementAt(i);
                    int randomSentence = Random.Range(1, msges.Length);
                    string msg = msges[randomSentence];
                    alien.displayComment(msg);
                }
            }
        }
    }

    public async void GoodAIResponse(string[] msges) {
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

    public async void BadAIResponse(string[] msges) {
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
