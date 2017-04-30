using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshUtility : MonoBehaviour {

    /// <summary>
    /// Checks if the specified ray hits the triagnlge descibed by p1, p2 and p3.
    /// Möller–Trumbore ray-triangle intersection algorithm implementation.
    /// </summary>
    /// <param name="p1">Vertex 1 of the triangle.</param>
    /// <param name="p2">Vertex 2 of the triangle.</param>
    /// <param name="p3">Vertex 3 of the triangle.</param>
    /// <param name="ray">The ray to test hit for.</param>
    /// <returns><c>true</c> when the ray hits the triangle, otherwise <c>false</c></returns>
    public static bool Intersect(NavMeshPolygon polygon, Ray ray)
    {
        Vector3 p1 = polygon.Verticies[0].position;
        Vector3 p2 = polygon.Verticies[1].position;
        Vector3 p3 = polygon.Verticies[2].position;

        // Vectors from p1 to p2/p3 (edges)
        Vector3 e1, e2;

        Vector3 p, q, t;
        float det, invDet, u, v;


        //Find vectors for two edges sharing vertex/point p1
        e1 = p2 - p1;
        e2 = p3 - p1;

        // calculating determinant 
        p = Vector3.Cross(ray.direction, e2);

        //Calculate determinat
        det = Vector3.Dot(e1, p);

        //if determinant is near zero, ray lies in plane of triangle otherwise not
        if (det > -Mathf.Epsilon && det < Mathf.Epsilon) { return false; }
        invDet = 1.0f / det;

        //calculate distance from p1 to ray origin
        t = ray.origin - p1;

        //Calculate u parameter
        u = Vector3.Dot(t, p) * invDet;

        //Check for ray hit
        if (u < 0 || u > 1) { return false; }

        //Prepare to test v parameter
        q = Vector3.Cross(t, e1);

        //Calculate v parameter
        v = Vector3.Dot(ray.direction, q) * invDet;

        //Check for ray hit
        if (v < 0 || u + v > 1) { return false; }

        if ((Vector3.Dot(e2, q) * invDet) > Mathf.Epsilon)
        {
            //ray does intersect
            return true;
        }

        // No hit at all
        return false;
    }

    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsFacingUp(Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 normal = NavMeshUtility.Normal(A, B, C);
        return Vector3.Dot(normal, Vector3.up) < 0;
    }

    public static Vector3 Normal(Vector3 A, Vector3 B, Vector3 C)
    {
        return Vector3.Cross(B - A, B - C).normalized;
    }
}

public static class VectorHelpers
{
    public static Vector3 ZeroY(this Vector3 v)
    {
        v.y = 0;
        return v;
    }

    public static Vector2 RemoveY(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 AddY(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
}