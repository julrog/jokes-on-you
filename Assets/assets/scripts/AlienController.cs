using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlienController : MonoBehaviour
{
  public Material materialToColor;
  public TMP_Text alienComment;
  public GameController alien;
  public bool idle;
  private int currentTweenId;
  public bool onStage = false;
  public void moveAlien()
  {
    Invoke("makeAnimation", 0.1f);
    Invoke("makeAnimation", 0.2f);
    Invoke("makeAnimation", 0.3f);
    Invoke("makeAnimation", 0.4f);
    Invoke("makeAnimation", 0.5f);
    Invoke("makeAnimation", 0.6f);
    Invoke("makeAnimation", 0.7f);
    Invoke("makeAnimation", 0.8f);
    Invoke("makeAnimation", 0.9f);
  }

  public void makeAnimation() {
    float randomX = Random.Range(-8f, 8f);
    float randomY = Random.Range(-3f, 1f);
    LeanTween.cancel(currentTweenId);
    this.currentTweenId = LeanTween.move(gameObject, new Vector3(randomX , randomY, 8), .3f).setOnComplete( ()=> { this.onStage = true; } ).id;

    // materialToColor.color = Color.red;
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
    InvokeRepeating("DelayedAction", 8f, Mathf.Infinity);
  }

  public void DelayedAction()
    {
        alienComment.gameObject.SetActive(false);
        CancelInvoke("DelayedAction");
    }

  public void partyHard() {
    LeanTween.move(gameObject, new Vector3(transform.position.x, transform.position.y + 1, 8), .3f).setOnComplete( ()=> {
      LeanTween.move(gameObject, new Vector3(transform.position.x, transform.position.y - 1, 8), .3f).setOnComplete( ()=> { this.partyHard(); } );
    } );
  }
}
