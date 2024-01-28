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
    public Micro microAnimator;
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

    public GameObject s1;
    public GameObject s2;
    public GameObject sL;
    // Start is called before the first frame update
    void Start()
    {
        // this.startGame();
        gameStruct.selectNewGameCriteria();
        // this.makeAliensThink();
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
        if (canTalk && canDecreaseMicTimer) {
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
            count = 0;
            microAnimator.quitRound();
        }
    }

    public void endRound() {
        // make aliens think
        this.makeAliensThink();
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
        
        this.round = this.round + 1;
        this.DisplayComments(response.sentences);

        Debug.Log("this.currentScore:" + this.currentScore + "response.score:" + response.score + "this.round:" + this.round);
        if (this.round == 0) {
            this.GoodAIResponse(response.sentences);
        } else {
            if (response.score >= (this.currentScore / this.round)) {
              if (response.score > 0) {
                this.GoodAIResponse(response.sentences);
              }
            } else {
                this.BadAIResponse(response.sentences);
            }
        }

        this.currentScore += response.score;
        
        gameStruct.selectNewGameCriteria();
        
        this.scoreText.text = "" + (this.currentScore / this.round);
        this.roundText.text = "" + this.round;
        this.feelingText.text = "Deine Geschichte aus Runde:" + this.round + " löste aus: " + response.feeling + " anstelle: " + gameStruct.lastFeeling;

        Debug.Log("this.round" + this.round + this.maxRound);
        if (this.round >= this.maxRound && (this.currentScore / this.round >= 7)) {
            Debug.Log("Close and finish");
            this.s1.SetActive(true);
            var aliensNotOnStage = this.aliens.Where(alienController => alienController.onStage);
            if (aliensNotOnStage.Any()) {
                for (int i = 0; i < aliensNotOnStage.Count(); i++) {
                    AlienController alien = aliensNotOnStage.ElementAt(i);
                    alien.partyHard();
                }
            }
        }
        if (this.round >= this.maxRound) {
          if (!this.s1.activeSelf) {
            this.s2.SetActive(true);
          }
          this.sL.SetActive(false);
          Invoke("closeCurtains", 5f);
        }
    }

    public void closeCurtains() {
        curtainAnimator.closeCurtains();
    }

    public void callOpenAIResponse() {
        OpenAiResponse res = new OpenAiResponse();
        res.score = 0;
        res.feeling = "Glücklich";
        res.sentences = new string[] {"WAS?", "Diggi", "GEIL", "du siehst gut aus!", "Heirate mich!", "PARTY HARD :O"};
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

    void makeAliensThink() {
      var aliensNotOnStage = this.aliens.Where(alienController => alienController.onStage);
      if (aliensNotOnStage.Any()) {
        for (int i = 0; i < aliensNotOnStage.Count(); i++) {
          AlienController alien = aliensNotOnStage.ElementAt(i);
          int randomNumber = Random.Range(0, 2);
          bool randomBool = (randomNumber % 2 == 0);
          alien.flip(randomBool);
        }
      }
    }
 }
