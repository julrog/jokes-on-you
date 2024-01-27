using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlienController : MonoBehaviour
{
  public TMP_Text alienComment;
  public GameController alien;
  public bool idle;
  private int currentTweenId;
  public bool onStage = false;
  public void moveAlien()
  {
    float randomX = Random.Range(-3f, 3f);
    float randomY = Random.Range(-3f, 3f);
    LeanTween.cancel(currentTweenId);
    this.currentTweenId = LeanTween.move(gameObject, new Vector3(randomX , randomY, 8), .3f).setOnComplete( ()=> { this.onStage = true; } ).id;
    // LeanTween.move(notePlane, new Vector3(-5, 4, -3), 1);
  }

  public void alienIdle()
  {
    int randomNumber = Random.Range(-1, 2);
    // Setzen Sie die Zufallszahl auf -3 oder +3, falls sie negativ oder positiv ist
    float randomX = 10f;
    if (randomNumber < 0)
    {
      randomX = -10f;
    }
    else if (randomNumber > 0)
    {
      randomX = 10f;
    }
    //The mouse is no longer hovering over the GameObject so output this message each frame
    // Debug.Log("Mouse is no longer on GameObject.");
    LeanTween.cancel(currentTweenId);
    this.currentTweenId = LeanTween.move(gameObject, new Vector3(randomX , transform.position.y, 8), .3f).setOnComplete( ()=> { this.onStage = false; } ).id;
  }

  public void displayComment(string text) {
    alienComment.gameObject.SetActive(true);
    alienComment.text = text;
    InvokeRepeating("DelayedAction", 5f, Mathf.Infinity);
  }

  public void DelayedAction()
    {
        alienComment.gameObject.SetActive(false);
        CancelInvoke("DelayedAction");
    }
}
