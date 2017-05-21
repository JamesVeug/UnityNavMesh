using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class NavMeshBuilder : MonoBehaviour
{
	public NavMeshTriangleMono TriangleTemplate;

	public List<NavMeshTriangleMono> ExtraTriangles;
    public List<Color> colors;
    
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


		List<NavMeshTriangle> triangles = ConvertCollisionsToTriangles();
		for (int i = 0; i < ExtraTriangles.Count; i++)
		{
			triangles.Add(ExtraTriangles[i].Data);
		}

        if(m_factory == null)
        {
            m_factory = new NavMeshFactory();
        }
		
        m_factory.Reset();
        m_navMesh = m_factory.BuildMesh(triangles);
    }

	private List<NavMeshTriangle> ConvertCollisionsToTriangles()
    {
        BoxCollider[] colliders = FindObjectsOfType<BoxCollider>();
		List<NavMeshTriangle> triangles = new List<NavMeshTriangle>();

        foreach (BoxCollider c in colliders)
        {
            triangles.AddRange(ColliderToTriangles(c));
        }

		return triangles;
    }

    public List<NavMeshTriangle> ColliderToTriangles(BoxCollider c)
    {

        Bounds b = c.bounds;
        Vector3 tr = c.transform.TransformPoint(new Vector3(c.size.x, c.size.y, c.size.z) * 0.5f);   // Top Right
        Vector3 tl = c.transform.TransformPoint(new Vector3(-c.size.x, c.size.y, c.size.z) * 0.5f);  // Top Left
        Vector3 bl = c.transform.TransformPoint(new Vector3(-c.size.x, c.size.y, -c.size.z) * 0.5f); // Bottom Left
        Vector3 br = c.transform.TransformPoint(new Vector3(c.size.x, c.size.y, -c.size.z) * 0.5f); // Bottom Right

        GameObject o = new GameObject(c.gameObject.name + " Mest");
        o.transform.parent = transform;

		List<NavMeshTriangle> triangles = new List<NavMeshTriangle>();

		NavMeshTriangle triangle1 = new NavMeshTriangle();
		triangle1.SetPositions(tl, tr, br);

		NavMeshTriangle triangle2 = new NavMeshTriangle();
        triangle2.SetPositions(br, bl, tl);

		triangles.Add(triangle1);
		triangles.Add(triangle2);

		return triangles;
    }

    /*private void OnDrawGizmos()
    {
        if(m_navMesh == null)
        {
            return;
        }

        for (int i = 0; i < m_navMesh.Mesh.Count; i++)
        {
            if(colors.Count <= i)
            {
                colors.Add(Color.black);
            }

            Gizmos.color = colors[i];
            Handles.color = colors[i];

            NavMeshPolygon polygon = m_navMesh.Mesh[i];
            DrawPolygonGizmo(polygon.Verticies, 0.1f);
            Handles.Label(polygon.Center(), polygon.ID.ToString());
        }
    }

    private void DrawPolygonGizmo(List<NavMeshVertex> verticies, float heightOffset)
    {
        Vector3 height = new Vector3(0, heightOffset, 0);

        Vector3 center = Vector3.zero;
        for (int i = 0; i < verticies.Count; i++)
        {
            center += verticies[i].position;
        }
        center = center / verticies.Count;

        for (int i = 0; i < verticies.Count; i++)
        {
            Vector3 startPosition = verticies[i].position - (verticies[i].position - center).normalized * 0.1f;
            Vector3 endPosition = i + 1 < verticies.Count ? verticies[i + 1].position : verticies[0].position;
            endPosition -= (endPosition - center).normalized * 0.1f;

            // Vertex
            Gizmos.DrawSphere(startPosition + height, 0.25f);
            Handles.Label(startPosition + Vector3.up, verticies[i].ID.ToString());

            // Edge
            Gizmos.DrawLine(startPosition + height, endPosition + height);
        }
    }*/
}


