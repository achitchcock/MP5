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
    private GameObject mSelectedDirection;
    private bool isPlane;
    private Vector2 cylinderCenter;
    private int m_size;
    private int n_size;
    private List<int> triangles;
    private List<Vector3> vertices;
    private List<Vector3> normals;
    private List<GameObject> controlPoints;
    public List<Vector2> uv;
    private Dictionary<int, List<Vector3>> adjacencies;
    private Mesh mesh;
    private Material mOriginalMaterial;

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
        normals = new List<Vector3>();
        mSelectedPoint = null;
        triangles = new List<int>();
        cylinderCenter = new Vector2(10, 8);
        cylinderRot.InitSliderRange(10,360,270);
        cylinderRot.SetSliderListener(angleChanged);
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().mesh;
        reset.onClick.AddListener(resetMesh);
        xyzHandle.transform.FindChild("X").GetComponent<mouseDrag>().setDragListner(pointMovedX);
        xyzHandle.transform.FindChild("X").GetComponent<mouseDrag>().up = false;
        xyzHandle.transform.FindChild("Y").GetComponent<mouseDrag>().setDragListner(pointMovedY);
        xyzHandle.transform.FindChild("Y").GetComponent<mouseDrag>().up = false;
        xyzHandle.transform.FindChild("Z").GetComponent<mouseDrag>().setDragListner(pointMovedZ);
        xyzHandle.transform.FindChild("Z").GetComponent<mouseDrag>().up = true;
        initMeshType(meshType.value);
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

    // Mesh resolution sliders
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
    }

    //  Initialize a mesh that is a plane or a cylinder
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
            nSlider.TheSlider.minValue = 4 ;
        }
        mesh.Clear();
        vertices.Clear();
        foreach (var pt in controlPoints)
        {
            GameObject.Destroy(pt);
        }
        
        adjacencies.Clear();
        mSelectedPoint = null;
        xyzHandle.SetActive(false);
        triangles.Clear();
        calculateVertices();
        calculateTriangles();
        calculateNormals();
        calculateUV();
        controlPoints.Clear();
        createControlPoints();
        createNormals();
        createMesh();

    }

    void createMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();
        //GetComponent<Renderer>().material.
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

    void angleChanged(float angle)
    {
        if (!isPlane)
        {
            int m = (int)mSlider.GetSliderValue();
            int n = (int)nSlider.GetSliderValue();
            for (int i = 0; i < m; i++)
            {
                followPoint(i * n);
            }
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
            float radius = 4;
            float init_x = cylinderCenter.x + radius;
            int m = (int)mSlider.GetSliderValue();
            int n = (int)nSlider.GetSliderValue();
            float angle = cylinderRot.GetSliderValue() / (n - 1);
            float y_loc = -m_size / 2;
            float offset = (float)m_size / (float)(m-1);
            for (int j = 0; j < m; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    float xpos = cylinderCenter.x + radius * Mathf.Cos(angle * i * (Mathf.PI / 180));
                    float zpos = cylinderCenter.y + radius * Mathf.Sin(angle * i*(Mathf.PI / 180));
                    vertices.Add(new Vector3(xpos, y_loc+j*offset, zpos));
                }
            }
        }
    }

    void pointMovedX(float dist)
    {
        mSelectedPoint.transform.Translate(new Vector3(dist, 0, 0));
        xyzHandle.transform.Translate(new Vector3(dist, 0, 0));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
        if (!isPlane)
        {
            followPoint(mSelectedPoint.GetComponent<controlPoint>().myNumber);
        }
    }

    void pointMovedY(float dist)
    {
        mSelectedPoint.transform.Translate(new Vector3(0, dist, 0));
        xyzHandle.transform.Translate(new Vector3(0, dist, 0));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
        if (!isPlane)
        {
            followPoint(mSelectedPoint.GetComponent<controlPoint>().myNumber);
        }
    }

    void pointMovedZ(float dist)
    {
        mSelectedPoint.transform.Translate(new Vector3(0, 0, dist));
        xyzHandle.transform.Translate(new Vector3(0, 0, dist));
        updateVertices(controlPoints.IndexOf(mSelectedPoint));
        if (!isPlane)
        {
            followPoint(mSelectedPoint.GetComponent<controlPoint>().myNumber);
        }
    }

    void followPoint(int pt)
    {
        float radius = controlPoints[pt].transform.localPosition.x - cylinderCenter.x;
        float zOffset = controlPoints[pt].transform.localPosition.z - cylinderCenter.y;
        float AdditionalAngle = Mathf.Asin(zOffset / radius);
        float angle = cylinderRot.GetSliderValue() / (nSlider.GetSliderValue() - 1);
        for (int i = 1; i < nSlider.GetSliderValue(); i++)
        {
            float x = cylinderCenter.x + radius * Mathf.Cos((angle+AdditionalAngle) * i * (Mathf.PI / 180));
            float z = cylinderCenter.y + radius * Mathf.Sin((angle + AdditionalAngle) * i * (Mathf.PI / 180));
            float y = controlPoints[pt].transform.localPosition.y;
            controlPoints[pt+i].transform.localPosition = new Vector3(x, y, z);
            vertices[pt+i] = controlPoints[pt + i].transform.localPosition;
        }
        mesh.vertices = vertices.ToArray();
        calculateNormals();
        createNormals();
        mesh.normals = normals.ToArray();
    }



    void updateVertices(int pointNum)
    {
        vertices[pointNum] = controlPoints[pointNum].transform.localPosition;
        mesh.vertices = vertices.ToArray();
        calculateNormals();
        createNormals();
        mesh.normals = normals.ToArray();
    }


    void calculateUV()//Matrix3x3 trs)
    {


        uv = new List<Vector2>();
        int m = (int)mSlider.GetSliderValue();
        int n = (int)nSlider.GetSliderValue();
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float x_trans = 0.5f;
                float y_trans = 0.2f;
                float x_scale = 1.5f;
                float z_scale = 1.5f;
                float z_rot = 45;

                float x = (((float)j % n) / (n - 1));
                float y = (((float)i % m) / (m - 1));
                Vector2 init = new Vector2(x*x_scale, y*z_scale);

                Vector2 rot = Quaternion.AngleAxis(z_rot, Vector3.up) * init;

                Vector2 res = new Vector3(rot.x + x_trans, rot.y + y_trans);
                //trs
                uv.Add(res);

            }
        }
    }

    void calculateTriangles()
    {
        
        triangles.Clear();
        normals.Clear();
        adjacencies.Clear();
        int n = (int) nSlider.GetSliderValue();
        int m = (int) mSlider.GetSliderValue();
        //print("Vertices:"+vertices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            adjacencies.Add(i, new List<Vector3>());
            //print("Added: " + i);
        }
        
        for (int y = 0; y < (m-1); y++)
        {
            for (int x = 0; x < (n - 1); x++)
            {
                {
                    // triangle number
                    int i = y*n + x;
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

                    // update triagnles list
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

                Vector3 norm = Vector3.Cross((c2 - c1), (c3 - c1)).normalized;
                if (isPlane)
                {
                    t_norm.Add(norm);
                }
                else
                {
                    t_norm.Add(-norm);
                }
            }
            Vector3 result = t_norm[0];
            for (int j = 1; j < t_norm.Count; j++)
            {
                result += t_norm[j];
            }
            if (isPlane)
            {
                normals.Add(result);
            }
            else
            {
                normals.Add(-result);
            }
        }
        // cylinder rotation 360 - merge start/end normals
        if (!isPlane && cylinderRot.GetSliderValue()==360)
        {
            print("Full circle!");
            int m = (int)mSlider.GetSliderValue();
            int n = (int)nSlider.GetSliderValue();
            for (int i = 0; i < normals.Count; i += n)
            {
                
                Vector3 res = (normals[i + n - 1] + normals[i]).normalized;
                normals[i] = res;
                normals[i + n - 1] = res;
            }
        }
        
    }




    void createControlPoints()
    {
        foreach (GameObject cpt in controlPoints)
        {
            Destroy(cpt);
        }
        controlPointSpheres.transform.DetachChildren();
        controlPoints.Clear();
        int num = 0;
        foreach (Vector3 point in vertices)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.localPosition = point;
            p.transform.parent = controlPointSpheres.transform;
            p.AddComponent<controlPoint>();
            p.GetComponent<controlPoint>().SetControlListener(updateVertices);
            p.GetComponent<controlPoint>().myNumber = num;
            if (!isPlane && num % nSlider.GetSliderValue() != 0)
            {
                p.layer = 2;  // ignore raycast
                p.GetComponent<Renderer>().material.color = Color.black;
            }
            num++;
            controlPoints.Add(p);

        }
    }

    void createNormals()
    {
        //print("creating normals");
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
            pivot.transform.localRotation = Quaternion.FromToRotation(normLine.transform.up, normals[i]);
        }
    }
}
