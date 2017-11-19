using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlPoint : MonoBehaviour {

    public int myNumber;


    public delegate void controlCallbackDelegate(int p);      // defined a new data type
    private controlCallbackDelegate mCallBack = null;           // private instance of the data type


    // Use this for initialization
    void Start () {
		
	}

    public void SetControlListener(controlCallbackDelegate listener)
    {
        mCallBack = listener;
    }

	
    public int getNumber()
    {
        return myNumber;
    }



}
