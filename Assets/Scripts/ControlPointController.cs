using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointController : MonoBehaviour {

    private bool pointsActive;
    public GameObject xyzHandle;

	// Use this for initialization
	void Start () {
        pointsActive = true;
        Debug.Assert(xyzHandle != null);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("left ctrl"))
        {
            if (!pointsActive)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    //transform.GetChild(i).transform.gameObject.SetActive(true);
                }
                pointsActive = true;
            }
        }
        else
        {
            if (!xyzHandle.activeSelf)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    //transform.GetChild(i).transform.gameObject.SetActive(false);
                }
                pointsActive = false;
            }
        }
	}
}
