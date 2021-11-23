using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Geometry
{
    public static class Point
    {

        public static List<Vector3> GeneratePointsInRectPrism(int numPoints, float xScale, float yScale, float zScale)
        {
            List<Vector3> ps = new List<Vector3>(numPoints);
            for (int i = 0; i < numPoints; i++) {
                ps.Add(new Vector3(
                    Random.Range(-xScale/2, xScale/2), 
                    Random.Range(-yScale/2, yScale/2), 
                    Random.Range(-zScale/2, zScale/2)
                ));
            }

            return ps;
        }

        public static List<Vector3> GeneratePointsInCube(int numPoints, float scale)
        {
            return GeneratePointsInRectPrism(numPoints, scale, scale, scale);
        }

        public static List<Vector3> GeneratePointsInRoundedEdges(int numPoints, float xScale, float yScale, float zScale)
        {
            List<Vector3> ps = new List<Vector3>(numPoints);
            for (int i = 0; i < numPoints; i++) {
                float dist = Random.Range(0, 1f);
                if (dist < 0.5f && dist < Random.Range(0, 0.5f))
                {
                    dist = 1 - dist;
                }
                Vector3 p = Vector3.forward * dist / 2;
                p = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), 0) * p;
                
                p.x *= xScale;
                p.y *= yScale;
                p.z *= zScale;

                ps.Add(p);
            }

            return ps;
        }

        public static List<Vector3> GeneratePointsInSphere(int numPoints, float scale) {
            return GeneratePointsInRoundedEdges(numPoints, scale, scale, scale);
        }
    }
}
