using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using System.Linq;

public class Rock : MonoBehaviour
{
    public AudioClip breakSound;

    public Material RockMaterial;
    public Material HighlightedMaterial;
    private PointCloud pointCloud;

    public bool RenderPoints = true;

    // Start is called before the first frame update
    void Start() {}

    public void Init(PointCloud pointCloud) {
        this.pointCloud = pointCloud;
        UpdateMesh();
    }

    void UpdateMesh()
    {
        Mesh m = pointCloud.ConvexHullMesh();
        GetComponent<MeshFilter>().mesh = m;
        GetComponent<MeshCollider>().sharedMesh = m;

        if (RenderPoints) 
        {
            foreach (Vector3 p in pointCloud.Points) 
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = transform;
                sphere.transform.localPosition = p;
                sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                sphere.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    public void PickUp()
    {
        transform.SetParent(Camera.main.transform);
        transform.localPosition = new Vector3(0, 0, 5);

        var body = GetComponent<Rigidbody>(); 
        body.useGravity = false;
        body.angularVelocity = Vector3.zero;
        body.velocity = Vector3.zero;
        body.angularDrag = 1f;

        

        gameObject.layer = 6;
        GetComponent<Renderer>().material = HighlightedMaterial;
    }

    public void Drop()
    {
        transform.SetParent(null);

        var body = GetComponent<Rigidbody>(); 
        body.useGravity = true;

        gameObject.layer = 0;
        GetComponent<Renderer>().material = RockMaterial;
    }

    public void Zoom(float distance)
    {
        transform.localPosition = new Vector3(0, 0, distance);
    }

    public void Put(Vector3 pos)
    {
        transform.position = pos;

        var body = GetComponent<Rigidbody>(); 
        body.angularVelocity = Vector3.zero;
        body.velocity = Vector3.zero;
    }

    public void ApplyForce(Vector3 rot)
    {
        var body = GetComponent<Rigidbody>(); 
        body.velocity += rot;
    }

    public void ApplyRotateForce(Vector3 rot)
    {
        var body = GetComponent<Rigidbody>(); 
        body.angularVelocity += rot;
    }

    public void Split(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        p1 = transform.InverseTransformPoint(p1);
        p2 = transform.InverseTransformPoint(p2);
        p3 = transform.InverseTransformPoint(p3);

        foreach (var pc in pointCloud.SplitAlongPlane(p1, p2, p3))
        {
            if (pc.Points.Count >= 4)
            {
                Vector3 pcCenter = pc.Barycenter();
                pc.Recenter();
                var r = RockHandler.InstatiateRock(pc, transform.position + transform.rotation * pcCenter, transform.rotation);
                r.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;
            }
        }
        Destroy(gameObject);

        AudioSource.PlayClipAtPoint(breakSound, transform.position, 1f);
    }

    public float CircumsphereRadius()
    {
        return pointCloud.CircumsphereRadius();
    }

}
