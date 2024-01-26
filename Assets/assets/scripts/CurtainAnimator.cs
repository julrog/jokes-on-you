using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainAnimator : MonoBehaviour
{
    public GameObject leftPlane;
    public GameObject rightPlane;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("starts.");
        LeanTween.move(leftPlane, new Vector3(-6, 0, 10), 5);
        // var leftInitialTween = new PositionTween {
        //     to = new Vector3(-6, 0, 10),
        //     duration = 5,
        // };
        // var rightInitialTween = new PositionTween {
        //     to = new Vector3(6, 0, 10),
        //     duration = 5,
        // };
        // var leftOpenTween = new PositionTween {
        //     to = new Vector3(-20, 0, 10),
        //     duration = 5,
        // };
        // var rightOpenTween = new PositionTween {
        //     to = new Vector3(20, 0, 10),
        //     duration = 5,
        // };
        // leftPlane.AddTween(leftOpenTween);
        // rightPlane.AddTween(rightOpenTween);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openCurtains() {
        LeanTween.move(leftPlane, new Vector3(-20, 0, 10), 5);
        LeanTween.move(rightPlane, new Vector3(20, 0, 10), 5);
    }
    public void closeCurtains() {
        LeanTween.move(leftPlane, new Vector3(-6, 0, 10), 5);
        LeanTween.move(rightPlane, new Vector3(6, 0, 10), 5);
    }
}
