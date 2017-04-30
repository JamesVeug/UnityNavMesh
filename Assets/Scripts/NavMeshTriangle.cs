using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshTriangle
{
    private List<Vector3> positions = new List<Vector3>();

    public NavMeshTriangle()
    {

    }

    public NavMeshTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        SetPositions(a, b, c);
    }

    public List<Vector3> GetPositions()
    {
        return positions;
    }

    public void SetPositions(Vector3 a, Vector3 b, Vector3 c)
    {
        if(positions.Count > 0)
            positions[0] = a;
        else
            positions.Add(a);

        if (positions.Count > 1)
            positions[1] = b;
        else
            positions.Add(b);

        if (positions.Count > 2)
            positions[2] = c;
        else
            positions.Add(c);
    }

    public Vector3 Min()
    {
        var p = GetPositions();
        if(p.Count < 3)
        {
            Debug.LogWarning("Triangle has less than 3 triangles!");
        }

        Vector3 min = p[0];
        for (int i = 1; i < p.Count; i++)
        {
            min.x = Mathf.Min(min.x, p[i].x);
            min.y = Mathf.Min(min.y, p[i].y);
            min.z = Mathf.Min(min.z, p[i].z);
        }
        return min;
    }

    public Vector3 Max()
    {
        var p = GetPositions();
        Vector3 max = p[0];
        for (int i = 1; i < p.Count; i++)
        {
            max.x = Mathf.Max(max.x, p[i].x);
            max.y = Mathf.Max(max.y, p[i].y);
            max.z = Mathf.Max(max.z, p[i].z);
        }
        return max;
    }

    public Rect Bounds()
    {
        Vector3 min = Min().ZeroY();
        Vector3 max = Max().ZeroY();

        return new Rect(min + (max - min) / 2, max - min);
    }
}
