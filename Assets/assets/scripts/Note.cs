using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private bool isOver;
    private int currentTweenId;
    void OnMouseOver()
    {
        //If your mouse hovers over the GameObject with the script attached, output this message
        // Debug.Log("Mouse is over GameObject.");
        if (!isOver) {
            isOver = true;
            LeanTween.cancel(currentTweenId);
            this.currentTweenId = LeanTween.move(this.gameObject, new Vector3(-3 , 1, -3), .3f).id;
            // LeanTween.move(notePlane, new Vector3(-5, 4, -3), 1);
        }
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        // Debug.Log("Mouse is no longer on GameObject.");
        LeanTween.cancel(currentTweenId);
        this.currentTweenId = LeanTween.move(this.gameObject, new Vector3(-3, -7, -3), .3f).id;
        isOver = false;
    }
}
