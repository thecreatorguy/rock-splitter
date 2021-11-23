using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Geometry
{

    public class PointCloud
    {

        public List<Vector3> Points;

        public PointCloud(List<Vector3> points)
        {
            Points = points;
        }

        public static List<Triangle> IncrementalConvexHull(List<Vector3> Points)
        {

            int n = Points.Count;
            if (n < 4) return null;

            List<Triangle> hull = new List<Triangle>();
            
            Vector3 center = new Vector3(
                (Points[0].x + Points[1].x + Points[2].x + Points[3].x) / 4, 
                (Points[0].y + Points[1].y + Points[2].y + Points[3].y) / 4,
                (Points[0].z + Points[1].z + Points[2].z + Points[3].z) / 4
            );

            int[,] ixs = {{0, 1, 2}, {0, 2, 3}, {0, 1, 3}, {1, 2, 3}};
            for (int i = 0; i < 4; i++)
            {
                Triangle t = new Triangle(Points[ixs[i, 0]], Points[ixs[i, 1]], Points[ixs[i, 2]]);
                t.PointAway(center);
                hull.Add(t);
            }

            for (int i = 4; i < n; i++)
            {                
                Vector3 p = Points[i];
                List<Triangle> nextHull = new List<Triangle>();
                HashSet<Edge> exposedEdges = new HashSet<Edge>();
                foreach (Triangle t in hull) 
                {
                    if (t.Facing(p))
                    {
                        foreach (Edge e in t.Edges)
                        {
                            if (exposedEdges.Contains(e.Reversed()))
                            {
                                exposedEdges.Remove(e.Reversed());
                            }
                            else
                            {
                                exposedEdges.Add(e);
                            }
                        }
                    } 
                    else 
                    {
                        nextHull.Add(t);
                    }
                }
                foreach (Edge e in exposedEdges)
                {
                    nextHull.Add(new Triangle(p, e.P1, e.P2));
                }
                hull = nextHull;
            }

            return hull;
        }

        public List<Triangle> ConvexHull()
        {
            return DivideAndConquerConvexHull(Points);
        }

        public static List<Triangle> DivideAndConquerConvexHull(List<Vector3> ps)
        {
            var psCopy = new List<Vector3>(ps);
            psCopy.Sort( (a,b) => {
                if (b.x-a.x > 0) return 1;
                if (b.x-a.x < 0) return -1;
                return 0;
            });

            return _DACConvexHull(psCopy, 0, psCopy.Count).Hull;
        }

        private struct DACHullData
        {
            public List<Triangle> Hull {get;}
            public Dictionary<Vector3, List<Vector3>> AdjacencyList {get;}
            public DACHullData(List<Triangle> hull)
            {
                Hull = hull;

                AdjacencyList = new Dictionary<Vector3, List<Vector3>>();
                foreach (Triangle t in hull)
                {
                    if (!AdjacencyList.ContainsKey(t.P1)) AdjacencyList.Add(t.P1, new List<Vector3>());
                    AdjacencyList[t.P1].Add(t.P2);
                    AdjacencyList[t.P1].Add(t.P3);

                    if (!AdjacencyList.ContainsKey(t.P2)) AdjacencyList.Add(t.P2, new List<Vector3>());
                    AdjacencyList[t.P2].Add(t.P1);
                    AdjacencyList[t.P2].Add(t.P3);

                    if (!AdjacencyList.ContainsKey(t.P3)) AdjacencyList.Add(t.P3, new List<Vector3>());
                    AdjacencyList[t.P3].Add(t.P1);
                    AdjacencyList[t.P3].Add(t.P2);
                }
            }
        }

        private static DACHullData _DACConvexHull(List<Vector3> ps, int lowerBound, int upperBound)
        {
            if ((upperBound - lowerBound) < 8) 
            {
                return new DACHullData(
                    IncrementalConvexHull(new List<Vector3>(ps.Skip(lowerBound).Take(upperBound - lowerBound)))
                );
            }
                

            int median = (upperBound - lowerBound) / 2 + lowerBound;
            var left = _DACConvexHull(ps, lowerBound, median);
            var right = _DACConvexHull(ps, median, upperBound);
            return _DACConvexHullMerge(left, right);
        }

        private static DACHullData _DACConvexHullMerge(DACHullData left, DACHullData right)
        {
            var hull = new List<Triangle>(left.Hull);
            hull.AddRange(right.Hull);
            var leftBridgePoints = new List<Vector3>();
            var rightBridgePoints = new List<Vector3>();

            var first = _DACSupportingLine(left, right);
            var cur = first;
            do
            {
                var adjacentPoints = new List<Vector3>();
                if (left.AdjacencyList.ContainsKey(cur.P1))
                {
                    adjacentPoints.AddRange(left.AdjacencyList[cur.P1]);
                    adjacentPoints.AddRange(right.AdjacencyList[cur.P2]);
                }
                else
                {
                    adjacentPoints.AddRange(right.AdjacencyList[cur.P1]);
                    adjacentPoints.AddRange(left.AdjacencyList[cur.P2]);
                }
                
                var next = _DACGiftWrap(cur, adjacentPoints);
                hull.Add(new Triangle(cur.P1, cur.P2, next));
                if (left.AdjacencyList.ContainsKey(next))
                {
                    cur.P1 = next;
                    leftBridgePoints.Add(next);
                }
                else
                {
                    cur.P2 = next;
                    rightBridgePoints.Add(next);
                }
            }
            while (cur != first);

            // Vector3 leftHidden;
            // foreach (Vector3 p in left.AdjacencyList[leftBridgePoints[0]])
            // {
            //     if (p.x > leftBridgePoints[0].x)
            //     {
            //         leftHidden = p;
            //         break;
            //     }
                
            // }

            return new DACHullData(hull);
        }

        private static Edge _DACSupportingLine(DACHullData left, DACHullData right)
        {
            
            return null;
        }

        private static Vector3 _DACGiftWrap(Edge e, List<Vector3> ps)
        {
            Vector3 outermost = ps[0];
            for (int i = 1; i < ps.Count; i++)
            {
                if (Triangle.Orient(e.P1, e.P2, outermost, ps[i]) == Orientation.Out)
                {
                    outermost = ps[i];
                }
            }

            return outermost;
        }

        public Mesh ConvexHullMesh()
        {
            List<Triangle> hull = ConvexHull();

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();
            int i = 0;
            foreach (Triangle t in hull)
            {
                verts.AddRange(t.Points);
                indices.AddRange(new int[]{i++, i++, i++});
                normals.AddRange(new Vector3[]{t.Normal, t.Normal, t.Normal});
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.normals = normals.ToArray();

            return mesh;
        }

        public Vector3 Barycenter() {
            // This was found here: 5th answer, polygon barycenter
            // https://gis.stackexchange.com/questions/22739/finding-center-of-geometry-of-object

            List<Triangle> ch = ConvexHull();
            Vector3 wAvg = Vector3.zero;
            float sa = 0f;
            foreach (var t in ch)
            {
                sa += t.Area();
                wAvg += t.Barycenter() * t.Area();
            }

            return wAvg / sa;
        }

        public void Recenter()
        {
            Vector3 com = Barycenter();
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] -= com;
            }
        }

        public PointCloud[] SplitAlongPlane(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            List<Vector3> ps1 = new List<Vector3>();
            List<Vector3> ps2 = new List<Vector3>();
            for (int i = Points.Count-1; i >= 0; i--)
            {
                if (Triangle.Orient(p1, p2, p3, Points[i]) != Orientation.Out)
                {
                    ps1.Add(Points[i]);
                } 
                else
                {
                    ps2.Add(Points[i]);
                }
            }

            return new PointCloud[2]{new PointCloud(ps1), new PointCloud(ps2)};
        }

        public float CircumsphereRadius()
        {
            float max = 0;
            foreach (Vector3 p in Points)
            {
                if (p.magnitude > max)
                {
                    max = p.magnitude;
                }
            }

            return max;
        }
    }

    
}
