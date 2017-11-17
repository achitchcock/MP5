using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheWorld : MonoBehaviour {

    public SliderWithEcho mSlider;
    public SliderWithEcho nSlider;
    int m_size;
    int n_size;
    public List<int> triangles;
    public List<Vector3> vertices;
    List<GameObject> controlPoints;
    Mesh mesh;

    // Use this for initialization
    void Start () {
        mSlider.InitSliderRange(2, 20, 10);
        mSlider.SetSliderListener(sliderChanged);
        mSlider.TheSlider.wholeNumbers = true;
        nSlider.InitSliderRange(2, 20, 10);
        nSlider.SetSliderListener(sliderChanged);
        nSlider.TheSlider.wholeNumbers = true;
        n_size = 20;
        m_size = 20;
        vertices = new List<Vector3>();
        controlPoints = new List<GameObject>();
        triangles = new List<int>();
        calculateVertices();
        createControlPoints();
        calculateTriangles();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();



        createMesh();
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void sliderChanged(float val)
    {
        Debug.Log(val);
        foreach (GameObject point in controlPoints)
        {
            Destroy(point.gameObject);
        }
        controlPoints.Clear();
        vertices.Clear();
        calculateVertices();
        createControlPoints();
    }

    void calculateVertices()
    {
        int point = 1;
        float xs = n_size / (nSlider.GetSliderValue() - 1);
        float zs = m_size / (mSlider.GetSliderValue() - 1);
        for (float z = 0; z < mSlider.GetSliderValue(); z ++)
        {
            for (float x = 0; x <nSlider.GetSliderValue(); x ++ )
            {
                vertices.Add(new Vector3(x*xs, 0, z*zs));
                point += 1;
            }
        }
    }

    void calculateTriangles()
    {
        int n = (int) nSlider.GetSliderValue();
        int m = (int) mSlider.GetSliderValue();

        for (int i = 0; i < m*(n-1)-1; i++)
        {
            
            {
                // lower triangle
                triangles.Add(i);
                triangles.Add(i+n);
                triangles.Add(i+n+1);
                // upper triangle
                triangles.Add(i);
                triangles.Add(i+n+1);
                triangles.Add(i+1);
            }
        }
    }


    void createMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    void createControlPoints()
    {
        foreach (Vector3 point in vertices)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.localPosition = point;
            controlPoints.Add(p);

        }
    }


}
