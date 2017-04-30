using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class NavMeshBuilder : MonoBehaviour
{
    public NavMeshTriangleMono TriangleTemplate;
    
    private NavMeshFactory m_factory;
    private NavMesh m_navMesh;
    
    void Start()
    {
        m_factory = new NavMeshFactory();
        BuildMesh();
    }

    public void BuildMesh()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }


        ConvertCollisionsToTriangles();

        NavMeshTriangleMono[] triangleMonos = FindObjectsOfType<NavMeshTriangleMono>();
        NavMeshTriangle[] triangles = new NavMeshTriangle[triangleMonos.Length];
        for (int i = 0; i < triangleMonos.Length; i++)
        {
            triangles[i] = triangleMonos[i].Data;
        }
        
        m_navMesh = new NavMeshFactory().BuildMesh(triangles);
    }

    private void ConvertCollisionsToTriangles()
    {
        BoxCollider[] colliders = FindObjectsOfType<BoxCollider>();

        foreach (BoxCollider c in colliders)
        {
            ColliderToTriangles(c);
        }
    }

    private void ColliderToTriangles(BoxCollider c)
    {

        Bounds b = c.bounds;
        Vector3 tr = c.transform.TransformPoint(new Vector3(c.size.x, c.size.y, c.size.z) * 0.5f);   // Top Right
        Vector3 tl = c.transform.TransformPoint(new Vector3(-c.size.x, c.size.y, c.size.z) * 0.5f);  // Top Left
        Vector3 bl = c.transform.TransformPoint(new Vector3(-c.size.x, c.size.y, -c.size.z) * 0.5f); // Bottom Left
        Vector3 br = c.transform.TransformPoint(new Vector3(c.size.x, c.size.y, -c.size.z) * 0.5f); // Bottom Right

        GameObject o = new GameObject(c.gameObject.name + " Mest");
        o.transform.parent = transform;

        NavMeshTriangleMono triangle1 = Instantiate(TriangleTemplate, o.transform, true);
        triangle1.Data.SetPositions(tl, tr, br);

        NavMeshTriangleMono triangle2 = Instantiate(TriangleTemplate, o.transform, true);
        triangle2.Data.SetPositions(br, bl, tl);
    }

    private void OnDrawGizmos()
    {
        if(m_navMesh == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Handles.color = Color.green;
        for (int i = 0; i < m_navMesh.Mesh.Count; i++)
        {
            NavMeshPolygon polygon = m_navMesh.Mesh[i];
            DrawPolygonGizmo(polygon.Verticies, 0.1f);
            Handles.Label(polygon.Center(), polygon.ID.ToString());
        }
    }

    private void DrawPolygonGizmo(List<NavMeshVertex> verticies, float heightOffset)
    {
        for (int i = 0; i < verticies.Count; i++)
        {
            Gizmos.DrawSphere(verticies[i].position + new Vector3(0, heightOffset), 0.25f);
            NavMeshVertex startVertex = verticies[i];
            NavMeshVertex endVertex = i + 1 < verticies.Count ? verticies[i + 1] : verticies[0];
            Handles.Label(startVertex.position + Vector3.up, verticies[i].ID.ToString());
            Gizmos.DrawLine(startVertex.position + new Vector3(0, heightOffset, 0), endVertex.position + new Vector3(0, heightOffset, 0));
        }
    }
}


