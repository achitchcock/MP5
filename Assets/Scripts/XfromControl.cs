﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XfromControl : MonoBehaviour {
    public Toggle T, R, S;
    public SliderWithEcho X, Y, Z;
    public Text ObjectName;
    public GameObject theWorld;
    public Button resetTexture;
    public Toggle Tm, Rm, Sm;

    float autoScale;
    bool up;

    float prevXt, prevYt;
    float prevXs, prevYs;
    float prevZr;


    bool ignoreListner;

    private Transform mSelected;
    private Vector3 mPreviousSliderValues = Vector3.zero;

	// Use this for initialization
	void Start () {
        T.onValueChanged.AddListener(SetToTranslation);
        R.onValueChanged.AddListener(SetToRotation);
        S.onValueChanged.AddListener(SetToScaling);
        X.SetSliderListener(XValueChanged);
        Y.SetSliderListener(YValueChanged);
        Z.SetSliderListener(ZValueChanged);
        ignoreListner = false;
        resetTexture.onClick.AddListener(resetTex);

        Tm.isOn = false;
        Sm.isOn = false;
        Rm.isOn = false;

        T.isOn = true;
        R.isOn = false;
        S.isOn = false;
        SetToTranslation(true);
        // previous scale
        prevYs = 1;
        prevXs = 1;
        // previous translation
        prevXt = 0;
        prevYt = 0;
        // previous rotation
        prevZr = 0;

        autoScale = 1;
        up = true;
	}
    
    void Update()
    {
        if (Sm.isOn)
        {
            if (up)
            {
                theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateScale(new Vector2(1.1f, 1.1f));
                autoScale += 0.1f;
                if (autoScale > 3)
                {
                    up = false;
                }
            }
            else
            {
                theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateScale(new Vector2(0.9f, 0.9f));
                autoScale -= 0.1f;
                if (autoScale < -1)
                {
                    up = true;
                }
            }
            
        }
        if (Rm.isOn)
        {
            theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateRotation(1);
        }
        if (Tm.isOn)
        {
            theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateTranslation(new Vector2(0.1f, 0));
        }
    }

    void resetTex()
    {
        Tm.isOn = false;
        Sm.isOn = false;
        Rm.isOn = false;
        theWorld.GetComponent<TheWorld>().myTRS = Matrix3x3.identity;
        prevYs = 1;
        prevXs = 1;
        prevXt = 0;
        prevYt = 0;
        prevZr = 0;
        if (T.isOn)
        {
            SetToTranslation(true);
        }
        else if (R.isOn)
        {
            SetToRotation(true);
        }
        else if (S.isOn)
        {
            SetToRotation(true);
        }
    }

    void SetToTranslation(bool v)
    {
        ignoreListner = true;

        X.InitSliderRange(-4, 4, prevXt);
        Y.InitSliderRange(-4, 4, prevYt);
        Z.InitSliderRange(0, 0, 0);

        ignoreListner = false;
    }

    void SetToScaling(bool v)
    {
        ignoreListner = true;

        X.InitSliderRange(0.1f, 10, prevXs);
        Y.InitSliderRange(0.1f, 10, prevYs);
        Z.InitSliderRange(0, 0, 0);

        ignoreListner = false;
    }

    void SetToRotation(bool v)
    {
        ignoreListner = true;

        X.InitSliderRange(0, 0, 0);
        Y.InitSliderRange(0, 0, 0);
        Z.InitSliderRange(-180, 180, prevZr);

        ignoreListner = false;
    }

    void XValueChanged(float v)
    {
        if (ignoreListner)
        {
            return;
        }

        if (T.isOn)
        {
            float newX = X.GetSliderValue();
            theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateTranslation(new Vector2(newX - prevXt, 0));
            prevXt = newX;
        }

        if (S.isOn)
        {
            float newX = X.GetSliderValue();
            theWorld.GetComponent<TheWorld>().myTRS = Matrix3x3Helpers.CreateTRS(new Vector2(prevXt, prevYt), prevZr, new Vector2(newX, prevYs));
            prevXs = newX;
        }




    }
    
    void YValueChanged(float v)
    {
        if (ignoreListner)
        {
            return;
        }
        if (T.isOn)
        {
            float newY = Y.GetSliderValue();
            theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateTranslation(new Vector2(0, newY - prevYt ));
            prevYt = newY;
        }

        if (S.isOn)
        {
            float newY = Y.GetSliderValue();
            theWorld.GetComponent<TheWorld>().myTRS = Matrix3x3Helpers.CreateTRS(new Vector2(prevXt, prevYt), prevZr, new Vector2(prevXs, newY));
            prevYs = newY;
        }

    }

    void ZValueChanged(float v)
    {

        if (ignoreListner)
        {
            return;
        }
        if (R.isOn)
        {
            float newZ = Z.GetSliderValue();
            theWorld.GetComponent<TheWorld>().myTRS *= Matrix3x3Helpers.CreateRotation(newZ-prevZr);
            prevZr = newZ;
        }
    }
}