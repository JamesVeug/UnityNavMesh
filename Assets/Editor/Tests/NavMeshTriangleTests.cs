using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class NavMeshTriangleTests
{
    private int CURRENT_ID = 0;

    [Test]
    public void Edge2DXY()
    {
        NavMeshEdge edge1 = Edge(new Vector3(0, 0), new Vector3(8, 8, 0));
        NavMeshEdge edge2 = Edge(new Vector3(8, 0), new Vector3(0, 8, 0));
        AssertIntersectionFalse(edge1, edge2);
    }

    [Test]
    public void Edge2DXZ()
    {
        NavMeshEdge edge1 = Edge(new Vector3(0, 0, 0), new Vector3(8, 0, 8));
        NavMeshEdge edge2 = Edge(new Vector3(8, 0, 0), new Vector3(0, 0, 8));
        AssertIntersectionTrue(edge1, edge2, new Vector3(4, 0, 4));
    }

    [Test]
    public void Edge2DXZOffsetXY()
    {
        // Offset x and z by 2
        NavMeshEdge edge1 = Edge(new Vector3(2, 0, 2), new Vector3(10, 0, 10));
        NavMeshEdge edge2 = Edge(new Vector3(10, 0, 2), new Vector3(2, 0, 10));
        AssertIntersectionTrue(edge1, edge2, new Vector3(6, 0, 6));
    }

    [Test]
    public void Edge2DXZOffsetX()
    {
        // Offset x 2
        NavMeshEdge edge1 = Edge(new Vector3(2, 0, 0), new Vector3(10, 0, 8));
        NavMeshEdge edge2 = Edge(new Vector3(10, 0, 0), new Vector3(2, 0, 8));
        AssertIntersectionTrue(edge1, edge2, new Vector3(6, 0, 4));
    }

    [Test]
    public void Edge2DXZOffsetZ()
    {
        // Offset Z 2
        NavMeshEdge edge1 = Edge(new Vector3(0, 0, 2), new Vector3(8, 0, 10));
        NavMeshEdge edge2 = Edge(new Vector3(8, 0, 2), new Vector3(0, 0, 10));
        AssertIntersectionTrue(edge1, edge2, new Vector3(4, 0, 6));
    }

    [Test]
    public void CloseEdges()
    {
        // Offset Z 2
        NavMeshEdge edge1 = Edge(new Vector3(-35.1f, 0, -13.1f), new Vector3(-31.1f, 0, -21.4f));
        NavMeshEdge edge2 = Edge(new Vector3(-35.3f, 0, -9.9f),  new Vector3(-28, 0, -24.4f));
        AssertIntersectionFalse(edge1, edge2);
    }

    private void AssertIntersectionTrue(NavMeshEdge edge1, NavMeshEdge edge2, Vector3 expectedIntersection)
    {
        Vector3 intersection = Vector3.zero;
        if (AssertIntersection(edge1, edge2, expectedIntersection, out intersection))
        {
            AssertVector(intersection, expectedIntersection, "Intersection was not {0} but instead was {1}");
        }
        else
        {
            Assert.Fail("Could not get Intersection and returned " + intersection);
        }
    }

    private bool AssertIntersection(NavMeshEdge edge1, NavMeshEdge edge2, Vector3 expectedIntersection, out Vector3 suppliedIntersection)
    {
        bool inter = false;
        Vector2 intersection = Vector2.zero;
        if (NavMeshEdge.LineIntersection2D(edge1.VertexB.position.RemoveY(),
                                         edge1.VertexA.position.RemoveY(),
                                         edge2.VertexB.position.RemoveY(), 
                                         edge2.VertexA.position.RemoveY(), 
                                         ref intersection))
        {
            AssertVector(intersection.AddY(), expectedIntersection, "Intersection was not {0} but instead was {1}");
            inter = true;
        }

        suppliedIntersection = intersection.AddY();
        return inter;
    }

    private void AssertIntersectionFalse(NavMeshEdge edge1, NavMeshEdge edge2)
    {
        Vector3 intersection = Vector3.zero;
        if (AssertIntersection(edge1, edge2, Vector3.zero, out intersection))
        {
            Assert.Fail("Intersection should not exist but was " + intersection);
        }
    }

    private void AssertVector(Vector3 givenVector, Vector3 expectedVector, string format)
    {
        if(givenVector != expectedVector)
        {
            Assert.Fail(string.Format(format, expectedVector, givenVector));
        }
    }

    private NavMeshEdge Edge(Vector3 A, Vector3 B)
    {
        NavMeshEdge edge = new NavMeshEdge();
        edge.VertexA = new NavMeshVertex(A, CURRENT_ID++);
        edge.VertexB = new NavMeshVertex(B, CURRENT_ID++);
        return edge;
    }
}
