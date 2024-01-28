using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Micro : MonoBehaviour
{
    public GameController game;
    public WebSocketMicroController micController;
    private bool isOver;
    private int currentTweenId;
    void OnMouseOver()
    {
      //If your mouse hovers over the GameObject with the script attached, output this message
      // Debug.Log("Mouse is over GameObject.");
      if (!isOver) {
        isOver = true;
        LeanTween.cancel(currentTweenId);
        this.currentTweenId = LeanTween.move(this.gameObject, new Vector3(6 , -1, 6), .3f).setOnComplete( ()=> { Debug.Log("finished"); this.game.canTalk = true; this.micController.openMic(); this.game.canDecreaseMicTimer = true; } ).id;
        // LeanTween.move(notePlane, new Vector3(-5, 4, -3), 1);
      }
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        // Debug.Log("Mouse is no longer on GameObject.");
        LeanTween.cancel(currentTweenId);
        this.currentTweenId = LeanTween.move(this.gameObject, new Vector3(6 , -5, 6), .3f).setOnComplete( ()=> { Debug.Log("finish down"); this.game.endRound(); } ).id;
        isOver = false;
    }
}
