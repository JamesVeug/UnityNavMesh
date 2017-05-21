using UnityEngine;
using System.Collections.Generic;

public class NavMeshVertex
{
    public int ID;
    public Vector3 position;

    public NavMeshVertex(Vector3 position, int ID)
    {
        this.position = position;
        this.ID = ID;
    }

    public static NavMeshVertex MaxX(NavMeshVertex v1, NavMeshVertex v2)
    {
        return v1.position.x >= v2.position.x ? v1 : v1;
    }

    public static NavMeshVertex MaxY(NavMeshVertex v1, NavMeshVertex v2)
    {
        return v1.position.y >= v2.position.y ? v1 : v1;
    }

    public static NavMeshVertex MinX(NavMeshVertex v1, NavMeshVertex v2)
    {
        return v1.position.x <= v2.position.x ? v1 : v1;
    }
}