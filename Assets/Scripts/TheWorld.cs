using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TheWorld : MonoBehaviour {

    public SliderWithEcho mSlider;
    public SliderWithEcho nSlider;
    public SliderWithEcho cylinderRot;
    public Dropdown meshType;
    public GameObject controlPointSpheres;
    public GameObject xyzHandle;
    public Button reset;
    private GameObject mSelectedPoint;
    private bool isPlane;
    private Vector2 cylinderCenter;
    int m_size;
    int n_size;
    public List<int> triangles;
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public Dictionary<int, List<Vector3>> adjacencies;
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
        meshType.value = 0;
        meshType.onValueChanged.AddListener(initMeshType);
        n_size = 20;
        m_size = 20;
        vertices = new List<Vector3>();
        controlPoints = new List<GameObject>();
        adjacencies = new Dictionary<int, List<Vector3>>();
        mSelectedPoint = null;
        triangles = new List<int>();
        cylinderCenter = new Vector2(5, 5);
        cylinderRot.InitSliderRange(10,360,270);
        //calculateVertices();  // replace with initmesh(0)
        //createControlPoints();  // replace with initmesh(0)
        //calculateTriangles();  // replace with initmesh(0)
        //calculateNormals();  // replace with initmesh(0)
        //createNormals();  // replace with initmesh(0)
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;
        //mesh.Clear();  // replace with initmesh(0)
        //createMesh();  // replace with initmesh(0)
        reset.onClick.AddListener(resetMesh);
        xyzHandle.transform.FindChild("X").GetComponent<mouseDrag>().setDragListner(pointMovedX);
        xyzHandle.transform.FindChild("X").GetComponent<mouseDrag>().up = false;
        xyzHandle.transform.FindChild("Y").GetComponent<mouseDrag>().setDragListner(pointMovedY);
        xyzHandle.transform.FindChild("Y").GetComponent<mouseDrag>().up = false;
        xyzHandle.transform.FindChild("Z").GetComponent<mouseDrag>().setDragListner(pointMovedZ);
        xyzHandle.transform.FindChild("Z").GetComponent<mouseDrag>().up = true;
        //xyzHandle.SetActive(false);  // replace with initmesh(0)
        initMeshType(0);
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
        if (isPlane)
        {
            initMeshType(0);

        }
        else
        {
            initMeshType(1);
        }
        /*xyzHandle.SetActive(false);
        foreach (GameObject point in controlPoints)
        {
            Destroy(point.gameObject);
        }
        mesh.Clear();
        controlPoints.Clear();
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
        calculateVertices();
        calculateTriangles();
        calculateNormals();
        createControlPoints();
        createNormals();
        createMesh();*/
    }

    void initMeshType(int val)
    {
        // 0 == Plane
        if (val == 0)
        {
            isPlane = true;
            mSlider.TheSlider.maxValue = 20;
            mSlider.TheSlider.minValue = 2;
            nSlider.TheSlider.maxValue = 20;
            nSlider.TheSlider.minValue = 2;
        }
        else if (val == 1)
        {
            isPlane = false;
            mSlider.TheSlider.maxValue = 20;
            mSlider.TheSlider.minValue = 4;
            nSlider.TheSlider.maxValue = 20;
            nSlider.TheSlider.minValue = 2 ;
        }
        mesh.Clear();
        vertices.Clear();
        foreach (var pt in controlPoints)
        {
            GameObject.Destroy(pt);
        }
        controlPoints.Clear();
        adjacencies.Clear();
        mSelectedPoint = null;
        xyzHandle.SetActive(false);
        triangles.Clear();
        calculateVertices();
        calculateTriangles();
        calculateNormals();
        createControlPoints();
        createNormals();
        createMesh();

    }

    void createMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
    }

    void resetMesh()
    {
        if (isPlane)
        {
            initMeshType(0);
        }
        else
        {
            initMeshType(1);
        }
    }

    void calculateVertices()
    {
        vertices.Clear();
        if (isPlane)
        {
            float xs = n_size / (nSlider.GetSliderValue() - 1);
            float zs = m_size / (mSlider.GetSliderValue() - 1);
            for (float z = 0; z < mSlider.GetSliderValue(); z++)
            {
                for (float x = 0; x < nSlider.GetSliderValue(); x++)
                {
                    vertices.Add(new Vector3(x * xs, 0, z * zs));
                }
            }
        }
        else
        {
            int radius = 4;
            float init_x = cylinderCenter.x + radius;
            int m = (int)mSlider.GetSliderValue();
            int n = (int)nSlider.GetSliderValue();
            List<Vector3> circlePoints = new List<Vector3>();
            float angle = cylinderRot.GetSliderValue() / (m - 1);
            for (int y= n_size; y >= 0; y-=n_size/n)
            {
                for (int i = 0; i < m; i++)
                {
                    //float
                    // FINISH BEFORE RUN
                    //circlePoints.Add(new Vector3(cylinderCenter.x + radius, y,  ));
                }
            }
            

        }
        
    }

    void pointMovedX(float dist)
    {
        if (dist != 0)
            //print("X: " + dist);
        mSelectedPoint.transform.Translate(new Vector3(dist, 0, 0));
        xyzHandle.transform.Translate(new Vector3(dist, 0, 0));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
    }

    void pointMovedY(float dist)
    {
        if (dist != 0)
            //print("Y: " +dist);
        mSelectedPoint.transform.Translate(new Vector3(0, dist, 0));
        xyzHandle.transform.Translate(new Vector3(0, dist, 0));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
    }

    void pointMovedZ(float dist)
    {
        //if(dist!=0)
            //print("Z: " + dist);
        mSelectedPoint.transform.Translate(new Vector3(0, 0, dist));
        xyzHandle.transform.Translate(new Vector3(0, 0, dist));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
    }



    void updateVertices(int pointNum)
    {
        vertices[pointNum] = controlPoints[pointNum].transform.localPosition;
        mesh.vertices = vertices.ToArray();
        calculateNormals();
        createNormals();

        mesh.normals = normals.ToArray();

    }

    void calculateTriangles()
    {
        
        triangles.Clear();
        normals.Clear();
        adjacencies.Clear();
        int n = (int) nSlider.GetSliderValue();
        int m = (int) mSlider.GetSliderValue();
        
        for (int i = 0; i < vertices.Count; i++)
        {
            adjacencies.Add(i, new List<Vector3>());
            print("Added: " + i);
        }
        
        for (int y = 0; y < (m-1); y++)
        {
            for (int x = 0; x < (n - 1); x++)
            {
                {
                    // triangle number
                    int i = y*n + x;
                    print("Here!"+ i);
                    // lower triangle
                    int c1 = i;
                    int c2 = i + n;
                    int c3 = i + n + 1;
                    // update adjacencies list
                    Vector3 t1 = new Vector3(c1,c2,c3);
                    adjacencies[c1].Add(t1);
                    adjacencies[c2].Add(t1);
                    adjacencies[c3].Add(t1);
                    //  update triangles list
                    triangles.Add(i);
                    triangles.Add(i + n);
                    triangles.Add(i + n + 1);

                    // upper triangle
                    c1 = i;
                    c2 = i + n + 1;
                    c3 = i + 1;
                    // update adjacencies list
                    Vector3 t2 = new Vector3(c1, c2, c3);
                    adjacencies[c1].Add(t2);
                    adjacencies[c2].Add(t2);
                    adjacencies[c3].Add(t2);
                    // update triagles list
                    triangles.Add(i);
                    triangles.Add(i + n + 1);
                    triangles.Add(i + 1);
                }
            }
        }
    }

    void calculateNormals()
    {
        normals.Clear();
        for (int i = 0; i < vertices.Count; i++)
        {
            List<Vector3> t_norm = new List<Vector3>();
            foreach (var tri in adjacencies[i])
            {
                Vector3 c1 = vertices[(int)tri.x];
                Vector3 c2 = vertices[(int)tri.y];
                Vector3 c3 = vertices[(int)tri.z];

                Vector3 norm = Vector3.Cross((c2 - c1), (c3 - c1));
                t_norm.Add(norm);
            }
            Vector3 result = t_norm[0];
            for (int j = 1; j < t_norm.Count; j++)
            {
                result += t_norm[j];
            }
            normals.Add(result);
            print("Added Normal: "+i);
        }
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
            if (!isPlane && num % mSlider.GetSliderValue() != 0)
            {
                p.layer = 2;  // ignore raycast
            }
            num++;
            controlPoints.Add(p);

        }
    }

    void createNormals()
    {
        print("creating normals");
        for (int i = 0; i < controlPoints.Count; i++)
        {
            if (controlPoints[i].transform.childCount > 0)
            {
                GameObject.Destroy(controlPoints[i].transform.GetChild(0).gameObject);
                controlPoints[i].transform.DetachChildren();
            }
            GameObject normLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GameObject pivot = new GameObject();
            pivot.transform.parent = controlPoints[i].transform;
            pivot.transform.localPosition = new Vector3(0, 0, 0);
            pivot.transform.localScale = new Vector3(1, 1, 1);
            normLine.transform.parent = pivot.transform;
            normLine.transform.localScale = new Vector3(0.05f, 2, 0.05f);
            normLine.transform.localPosition = new Vector3(0, 0, 0);
            normLine.transform.Translate(normLine.transform.up * 2);
            
            //Quaternion rot = new Quaternion();
            //rot.eulerAngles = normals[i];
            pivot.transform.localRotation = Quaternion.FromToRotation(normLine.transform.up, normals[i]);


        }
    }


}
