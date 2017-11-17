using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheWorld : MonoBehaviour {

    public SliderWithEcho mSlider;
    public SliderWithEcho nSlider;
    int m_size;
    int n_size;

    List<Vector3> vertices;


	// Use this for initialization
	void Start () {
        m_size = 10;
        n_size = 10;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void calculateVertices()
    {

    }

}
