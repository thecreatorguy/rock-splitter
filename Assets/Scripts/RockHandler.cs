using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Geometry;

public class RockHandler : MonoBehaviour
{
    public GameObject RockPrefab;
    public static GameObject staticRockPrefab;

    public Dropdown DensityDropdown;
    public Dropdown WidthDropdown;
    public Dropdown HeightDropdown;
    public Dropdown DepthDropdown;
    public Rock FocusedRock;
    Vector3 lastpos = Vector3.zero;

    float rockPointDensity = 25;
    float rockWidth = 1;
    float rockHeight = 1;
    float rockDepth = 1;

    public enum HandlerState { None, Focused };
    public HandlerState State = HandlerState.None;

    // Start is called before the first frame update
    void Start()
    {
        staticRockPrefab = RockPrefab;

        DensityDropdown.onValueChanged.AddListener(delegate {
            string value = DensityDropdown.options[DensityDropdown.value].text;
            if (value == "Low") 
            {
                rockPointDensity = 25;
            }
            else if (value == "Medium") 
            {
                rockPointDensity = 50;
            }
            else if (value == "High") 
            {
                rockPointDensity = 100;
            }
        });

        WidthDropdown.onValueChanged.AddListener(delegate {
            string value = WidthDropdown.options[WidthDropdown.value].text;
            if (value == "1") 
            {
                rockWidth = 1;
            }
            else if (value == "2") 
            {
                rockWidth = 2;
            }
            else if (value == "3") 
            {
                rockWidth = 3;
            }
        });

        HeightDropdown.onValueChanged.AddListener(delegate {
            string value = HeightDropdown.options[HeightDropdown.value].text;
            if (value == "1") 
            {
                rockHeight = 1;
            }
            else if (value == "2") 
            {
                rockHeight = 2;
            }
            else if (value == "3") 
            {
                rockHeight = 3;
            }
        });

        DepthDropdown.onValueChanged.AddListener(delegate {
            string value = DepthDropdown.options[DepthDropdown.value].text;
            if (value == "1") 
            {
                rockDepth = 1;
            }
            else if (value == "2") 
            {
                rockDepth = 2;
            }
            else if (value == "3") 
            {
                rockDepth = 3;
            }
        });        

        ResetRect();
    }

    public static Rock InstatiateRock(PointCloud pc, Vector3 position, Quaternion rotation)
    {
        if (pc.Points.Count >= 4)
        {
            GameObject r = Instantiate(staticRockPrefab, position, rotation);
            r.GetComponent<Rock>().Init(pc);

            return r.GetComponent<Rock>();
        }
        return null;
    }

    public static Rock InstatiateRock(PointCloud pc)
    {
        return InstatiateRock(pc, new Vector3(0, 1, 0), Quaternion.identity);
    }

    void ResetRect() 
    {
        State = HandlerState.None;
        FocusedRock = null;

        foreach(var r in GameObject.FindGameObjectsWithTag("Rock"))
        {
            Destroy(r);
        }

        InstatiateRock(new PointCloud(Point.GeneratePointsInRectPrism(rockPointDensity, rockWidth, rockHeight, rockDepth)));
    }

    void ResetSphere() 
    {
        State = HandlerState.None;
        FocusedRock = null;

        foreach(var r in GameObject.FindGameObjectsWithTag("Rock"))
        {
            Destroy(r);
        }

        InstatiateRock(new PointCloud(Point.GeneratePointsInRoundedEdges(rockPointDensity, rockWidth, rockHeight, rockDepth)));
    }

    void TableRocks()
    {
        float totalWidth = 0;
        foreach(var go in GameObject.FindGameObjectsWithTag("Rock"))
        {
            Rock r = go.GetComponent<Rock>();
            totalWidth += r.CircumsphereRadius() * 2;
        }

        float leftEdge = -totalWidth / 2;
        foreach(var go in GameObject.FindGameObjectsWithTag("Rock"))
        {
            Rock r = go.GetComponent<Rock>();
            float width = r.CircumsphereRadius();
            r.Put(new Vector3(leftEdge + width, 1, 0));
            leftEdge += width * 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetRect();
        }

        else if (Input.GetKeyDown(KeyCode.S))
        {
            ResetSphere();
        }

        switch (State) {
        case HandlerState.None:
            if (Input.GetMouseButtonDown(0))
            {
                Rock r = GetRockAtMousePos();
                if (r)
                {
                    r.PickUp();
                    FocusedRock = r;
                    State = HandlerState.Focused;
                }
            }

            else if (Input.GetMouseButtonDown(1))
            {
                lastpos = Input.mousePosition;
                Rock r = GetRockAtMousePos();
                if (r)
                {
                    FocusedRock = r;
                }
            }
            else if (Input.GetMouseButton(1))
            {
                if (FocusedRock)
                {
                    Vector3 next = new Vector3(Input.mousePosition.x - lastpos.x, 0, Input.mousePosition.y - lastpos.y);
                    FocusedRock.ApplyForce(next * 0.04f);
                    lastpos = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                FocusedRock = null;
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                TableRocks();
            }
            break;

        case HandlerState.Focused:
            if (Input.GetKeyDown(KeyCode.Z)) 
            {
                if (FocusedRock)
                {
                    FocusedRock.Zoom(3);
                }
            }
            if (Input.GetKeyUp(KeyCode.Z)) 
            {
                if (FocusedRock)
                {
                    FocusedRock.Zoom(5);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) 
                {
                    Rock r;
                    r = hit.transform.gameObject.GetComponent<Rock>();
                    if (r == FocusedRock)
                    {
                        r.Split(hit.point, hit.point + new Vector3(0, 0, 1), hit.point + new Vector3(0, 1, 0));
                    }
                    else
                    {
                        // If it is either not a rock or it is a different rock
                        FocusedRock.Drop();
                    }
                    FocusedRock = null;
                    State = HandlerState.None;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                lastpos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1))
            {
                Vector3 next;
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    next = new Vector3(0, Input.mousePosition.x - lastpos.x + Input.mousePosition.y - lastpos.y, 0);
                }
                else
                {
                    next = new Vector3(Input.mousePosition.y - lastpos.y, 0, -(Input.mousePosition.x - lastpos.x));
                }
                
                FocusedRock.ApplyRotateForce(next * 0.02f);
                lastpos = Input.mousePosition;
            }
            break;
        }
    }

    Rock GetRockAtMousePos() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) 
        {
            return hit.transform.gameObject.GetComponent<Rock>();
        }

        return null;
    }

    
}
