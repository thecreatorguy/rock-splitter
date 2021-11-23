using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Geometry{
    public enum Orientation
    {
        In, Out, Coplanar
    }

    public class Edge
    {
        public Vector3 P1;
        public Vector3 P2;

        public Edge(Vector3 P1, Vector3 P2)
        {
            this.P1 = P1;
            this.P2 = P2;
        }

        public override bool Equals(object other){
            if (other is Edge) {
                Edge face = (Edge)other;
                return P1 == face.P1 && P2 == face.P2;
            }
            else {
                return false;
            }
        }

        public override int GetHashCode(){
            return P1.GetHashCode() ^ P2.GetHashCode();
        }

        public Edge Reversed()
        {
            return new Edge(P2, P1);
        }
    }

    public class Triangle 
    {
        public Vector3[] Points;
        public Vector3 P1 {get => Points[0]; set => Points[0] = value;}
        public Vector3 P2 {get => Points[1]; set => Points[1] = value;}

        public Vector3 P3 {get => Points[2]; set => Points[2] = value;}

        public Vector3 Normal {get => Vector3.Cross(P2 - P1, P3 - P1).normalized;}
        
        public List<Edge> Edges {get => new List<Edge>(new Edge[] {new Edge(P1, P2), new Edge(P2, P3), new Edge(P3, P1)});}

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Points = new Vector3[3];
            Points[0] = p1;
            Points[1] = p2;
            Points[2] = p3;
        }

        public override bool Equals(object other){
            if (other is Triangle) {
                Triangle face = (Triangle)other;
                return P1 == face.P1 && P2 == face.P2 && P3 == face.P3;
            }
            else {
                return false;
            }
        }

        public override int GetHashCode(){
            return P1.GetHashCode() ^ P2.GetHashCode() ^ P3.GetHashCode();
        }

        public void Flip()
        {
            Vector3 temp = P3;
            P3 = P2;
            P2 = temp;
        }

        public void PointAway(Vector3 p)
        {
            if (Facing(p)) Flip();
        }

        public static Orientation Orient(Vector3 a, Vector3 b, Vector3 c, Vector3 x)
        {
            // https://math.stackexchange.com/questions/214187/point-on-the-left-or-right-side-of-a-plane-in-3d-space

            Vector3 b1 = b - a;
            Vector3 c1 = c - a;
            Vector3 x1 = x - a;

            float result = (b1.x * c1.y * x1.z + b1.y * c1.z * x1.x + b1.z * c1.x * x1.y) -
                            (b1.x * c1.z * x1.y + b1.y * c1.x * x1.z + b1.z * c1.y * x1.x);

            if (result < 0) return Orientation.In;
            if (result > 0) return Orientation.Out;
            return Orientation.Coplanar;
        }

        public bool Facing(Vector3 p) 
        {
            return Orient(P1, P2, P3, p) == Orientation.Out;
        }

        public Vector3 Barycenter() 
        {
            return (P1 + P2 + P3) / 3;
        }

        public float Area()
        {
            // https://www.quora.com/How-can-I-find-the-area-of-a-triangle-in-3D-coordinate-geometry
            Vector3 ab = P2 - P1;
            Vector3 ac = P3 - P1;
            return Vector3.Cross(ab, ac).magnitude / 2f;
        }
    }
}