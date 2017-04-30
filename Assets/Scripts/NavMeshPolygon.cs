using System;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshPolygon : IEquatable<NavMeshPolygon>
{
    public int ID { get { return m_id; } }
    public List<NavMeshVertex> Verticies { get { return m_verticies; } }
    public List<NavMeshEdge> Edges { get { return m_edges; } }

    private int m_id;
    private List<NavMeshVertex> m_verticies = new List<NavMeshVertex>(3);
    private List<NavMeshEdge> m_edges = new List<NavMeshEdge>(3);

    public NavMeshPolygon(int id) { m_id = id; }

    public NavMeshPolygon(List<NavMeshVertex> verticies, int id)
    {
        m_id = id;
        AddVerticies(verticies);
    }

    public NavMeshPolygon(NavMeshVertex v1, NavMeshVertex v2, NavMeshVertex v3, int id)
    {
        m_id = id;
        AddVertex(v1);
        AddVertex(v2);
        AddVertex(v3);
    }

    public void AddVerticies(List<NavMeshVertex> verticies)
    {
        for (int i = 0; i < verticies.Count; i++)
        {
            if (!AddVertex(verticies[i]))
            {
                break;
            }
        }
    }

    public bool AddVertex(NavMeshVertex vertex)
    {
        if (m_verticies.Count >= 3)
        {
            Debug.LogError("More than 3 points on polygon!");
            return false;
        }
        m_verticies.Add(vertex);

        if(m_verticies.Count > 1)
        {
            NavMeshEdge edge = new NavMeshEdge();
            edge.VertexA = m_verticies[m_verticies.Count - 2];
            edge.VertexB = m_verticies[m_verticies.Count - 1];
            m_edges.Add(edge);
        }

        if (m_verticies.Count == 3)
        {
            NavMeshEdge edge = new NavMeshEdge();
            edge.VertexA = m_verticies[m_verticies.Count - 1];
            edge.VertexB = m_verticies[0];
            m_edges.Add(edge);
        }

        return true;
    }

    public Vector3 Center()
    {
        Vector3 center = Vector3.zero;
        foreach (NavMeshVertex v in Verticies)
        {
            center += v.position;
        }

        return center / 3;
    }

    public Vector3 Min()
    {
        Vector3 min = m_verticies[0].position;
        for (int i = 1; i < m_verticies.Count; i++)
        {
            min.x = Mathf.Min(min.x, m_verticies[i].position.x);
            min.y = Mathf.Min(min.y, m_verticies[i].position.y);
            min.z = Mathf.Min(min.z, m_verticies[i].position.z);
        }
        return min;
    }

    public Vector3 Max()
    {
        Vector3 max = m_verticies[0].position;
        for (int i = 1; i < m_verticies.Count; i++)
        {
            max.x = Mathf.Max(max.x, m_verticies[i].position.x);
            max.y = Mathf.Max(max.y, m_verticies[i].position.y);
            max.z = Mathf.Max(max.z, m_verticies[i].position.z);
        }
        return max;
    }

    public Rect Bounds()
    {
        Vector3 min = Min().ZeroY();
        Vector3 max = Max().ZeroY();

        return new Rect(min + (max - min) / 2, max - min);
    }

    public bool Intercepts(NavMeshPolygon other)
    {
        // Check within bounds
        Rect thisBounds = this.Bounds();
        thisBounds.height += 0.25f;

        Rect otherBounds = other.Bounds();
        otherBounds.height += 0.25f;

        if (!otherBounds.Overlaps(thisBounds, true))
        {
            return false;
        }

        // Check if our verticies are within theirs
        for(int i = 0; i < m_verticies.Count; i++)
        {
            if (other.Contains(other.Verticies[i]))
            {
                return true;
            }
        }

        // Check for intercepting lines
        for(int i = 0; i < m_edges.Count; i++)
        {
            for (int j = 0; j < other.Edges.Count; j++)
            {
                Vector3 intersection = Vector3.zero;
                if (NavMeshEdge.LineIntersection(m_edges[i], other.Edges[j], ref intersection))
                {
                    return true;
                }
            }
        }

        return true;
    }

    public bool Contains(NavMeshVertex vertex)
    {
        bool b1, b2, b3;

        b1 = sign(vertex.position.ZeroY(), m_verticies[0].position.ZeroY(), m_verticies[1].position.ZeroY()) < 0.0f;
        b2 = sign(vertex.position.ZeroY(), m_verticies[1].position.ZeroY(), m_verticies[2].position.ZeroY()) < 0.0f;
        b3 = sign(vertex.position.ZeroY(), m_verticies[2].position.ZeroY(), m_verticies[0].position.ZeroY()) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    private float sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

    public bool Equals(NavMeshPolygon other)
    {
        return other != null && other.ID == ID;
    }
}