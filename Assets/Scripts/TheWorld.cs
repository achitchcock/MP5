using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheWorld : MonoBehaviour {

    public SliderWithEcho mSlider;
    public SliderWithEcho nSlider;
    public GameObject controlPointSpheres;
    public GameObject xyzHandle;
    private GameObject mSelectedPoint;
    int m_size;
    int n_size;
    public List<int> triangles;
    public List<Vector3> vertices;
    List<GameObject> controlPoints;
    Mesh mesh;
    GameObject mSelectedDirection;
    Material mOriginalMaterial;

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
        mSelectedPoint = null;
        triangles = new List<int>();
        calculateVertices();
        createControlPoints();
        calculateTriangles();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        createMesh();
        xyzHandle.transform.FindChild("X").GetComponent<mouseDrag>().setDragListner(pointMovedX);
        xyzHandle.transform.FindChild("X").GetComponent<mouseDrag>().up = false;
        xyzHandle.transform.FindChild("Y").GetComponent<mouseDrag>().setDragListner(pointMovedY);
        xyzHandle.transform.FindChild("Y").GetComponent<mouseDrag>().up = false;
        xyzHandle.transform.FindChild("Z").GetComponent<mouseDrag>().setDragListner(pointMovedZ);
        xyzHandle.transform.FindChild("Z").GetComponent<mouseDrag>().up = true;

        xyzHandle.SetActive(false);
        
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Clicked!");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit, 100))
            {
                // clicked point is already selected
                if (mSelectedPoint == hit.transform.gameObject)
                {
                    xyzHandle.SetActive(false);
                    mSelectedPoint.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/default", typeof(Material));
                    mSelectedPoint = null;
                }
                // clicked object is a control point --> select it
                else if(hit.transform.gameObject.GetComponent<controlPoint>() != null)
                {
                    if (mSelectedPoint != null)
                    {
                        mSelectedPoint.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/default", typeof(Material));
                    }
                    int num = hit.transform.gameObject.GetComponent<controlPoint>().myNumber;
                    mSelectedPoint = hit.transform.gameObject;
                    mSelectedPoint.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/yellow", typeof(Material));
                    //Debug.Log(mSelectedPoint.GetComponent<Renderer>().material);
                    //print("Hit something:" + num);
                    xyzHandle.SetActive(true);
                    xyzHandle.transform.localPosition = controlPoints[num].transform.localPosition;
                }
                // clicked item is a directional arrow
                else if (hit.transform.parent = xyzHandle.transform)
                {
                    // no direction selestec --> select this one
                    if (mSelectedDirection == null)
                    {
                        mSelectedDirection = hit.transform.gameObject;
                        mOriginalMaterial = mSelectedDirection.GetComponent<Renderer>().material;
                        mSelectedDirection.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/yellow", typeof(Material));
                    }
                    // a direction is already active --> switch to new direction
                    else if (mSelectedDirection!= null && mSelectedDirection != hit.transform.gameObject)
                    {
                        mSelectedDirection.GetComponent<Renderer>().material = mOriginalMaterial;
                        mSelectedDirection = hit.transform.gameObject;
                        mOriginalMaterial = mSelectedDirection.GetComponent<Renderer>().material;
                        mSelectedDirection.GetComponent<Renderer>().material = (Material)Resources.Load("Materials/yellow", typeof(Material));

                    }
                    // seledcted direction is deselected
                    else if (mSelectedDirection == hit.transform.gameObject)
                    {
                        mSelectedDirection.GetComponent<Renderer>().material = mOriginalMaterial;
                        mSelectedDirection = null;
                    }
                }
            }
        }
	}


    void sliderChanged(float val)
    {
        xyzHandle.SetActive(false);
        foreach (GameObject point in controlPoints)
        {
            Destroy(point.gameObject);
        }
        controlPoints.Clear();
        vertices.Clear();
        calculateVertices();
        calculateTriangles();
        createControlPoints();
        createMesh();
    }

    void calculateVertices()
    {
        vertices.Clear();
        float xs = n_size / (nSlider.GetSliderValue() - 1);
        float zs = m_size / (mSlider.GetSliderValue() - 1);
        for (float z = 0; z < mSlider.GetSliderValue(); z ++)
        {
            for (float x = 0; x <nSlider.GetSliderValue(); x ++ )
            {
                vertices.Add(new Vector3(x*xs, 0, z*zs));
            }
        }
    }

    void pointMovedX(float dist)
    {
        if (dist != 0)
            print("X: " + dist);
        mSelectedPoint.transform.Translate(new Vector3(dist, 0, 0));
        xyzHandle.transform.Translate(new Vector3(dist, 0, 0));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
    }

    void pointMovedY(float dist)
    {
        if (dist != 0)
            print("Y: " +dist);
        mSelectedPoint.transform.Translate(new Vector3(0, dist, 0));
        xyzHandle.transform.Translate(new Vector3(0, dist, 0));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
    }

    void pointMovedZ(float dist)
    {
        if(dist!=0)
            print("Z: " + dist);
        mSelectedPoint.transform.Translate(new Vector3(0, 0, dist));
        xyzHandle.transform.Translate(new Vector3(0, 0, dist));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
    }



    void updateVertices(int pointNum)
    {
        vertices[pointNum] = controlPoints[pointNum].transform.localPosition;
        mesh.vertices = vertices.ToArray();
    }

    void calculateTriangles()
    {
        triangles.Clear();
        int n = (int) nSlider.GetSliderValue();
        int m = (int) mSlider.GetSliderValue();
        for (int y = 0; y < (m-1); y++)
        {
            for (int x = 0; x < (n - 1); x++)
            {
                {
                    int i = y*n + x;
                    // lower triangle
                    triangles.Add(i);
                    triangles.Add(i + n);
                    triangles.Add(i + n + 1);
                    // upper triangle
                    triangles.Add(i);
                    triangles.Add(i + n + 1);
                    triangles.Add(i + 1);
                }
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
        controlPointSpheres.transform.DetachChildren();
        int num = 0;
        foreach (Vector3 point in vertices)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.localPosition = point;
            p.transform.parent = controlPointSpheres.transform;
            p.AddComponent<controlPoint>();
            p.GetComponent<controlPoint>().SetControlListener(updateVertices);
            p.GetComponent<controlPoint>().myNumber = num;
            num++;
            controlPoints.Add(p);
        }
    }


}
