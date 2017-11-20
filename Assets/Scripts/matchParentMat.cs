using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class matchParentMat : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        GetComponent<Renderer>().material = transform.parent.GetComponent<Renderer>().material;
	}
}
