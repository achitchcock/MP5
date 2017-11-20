using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseDrag : MonoBehaviour {

    private float init_x;
    private float init_y;
    public GameObject xyzHandle;
    public bool up;

    public delegate void dragCallbackDelegate(float dist);      // defined a new data type
    private dragCallbackDelegate mCallBack = null;           // private instance of the data type


    public void setDragListner(dragCallbackDelegate callback)
    {
        mCallBack = callback;
    }

    void OnMouseDown()
    {
        init_x = Input.mousePosition.x;
        init_y = Input.mousePosition.y;
    }

    void OnMouseDrag()
    {
        float new_x = Input.mousePosition.x;
        float new_y = Input.mousePosition.y;
        float distance = Mathf.Sqrt(Mathf.Pow((float)(new_x - init_x) , 2) + Mathf.Pow((float)(new_y - init_y),  2));
        Vector2 dist = new Vector2(new_x, new_y) - new Vector2(init_x, init_y);

        if(up)
        {
            mCallBack(0.1f*(new_y - init_y));
        }
        else
        {
            mCallBack(0.1f*(new_x - init_x));
        }

        init_x = new_x;
        init_y = new_y;
    }

}
