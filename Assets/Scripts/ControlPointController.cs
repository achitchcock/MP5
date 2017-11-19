using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointController : MonoBehaviour {

    private bool pointsActive;

	// Use this for initialization
	void Start () {
        pointsActive = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("left ctrl"))
        {
            if (!pointsActive)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).transform.gameObject.SetActive(true);
                }
                pointsActive = true;
            }
            //Debug.Log("CTRL");

        }
        else
        {
            if (true) //child selected
            {

            }
            if (pointsActive)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).transform.gameObject.SetActive(false);
                }
                pointsActive = false;
            }
        }
	}
}
