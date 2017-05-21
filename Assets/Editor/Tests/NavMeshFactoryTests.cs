using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class NavMeshFactoryTests
{
    [Test]
    public void OnePointIntersection()
    {
        NavMeshTriangle A = new NavMeshTriangle(Vector3(0, 0.1f, 2), Vector3(2, 0.1f, 2), Vector3(2, 0.1f, 0));
        NavMeshTriangle B = new NavMeshTriangle(Vector3(0, 0, 1), Vector3(1.5f, 0, 1), Vector3(1.5f, 0, 0));

        NavMeshFactory factory = new NavMeshFactory();
		NavMesh mesh = factory.BuildMesh(ToList(A, B));

        AssertIntersections(mesh, Vector3(1, 0, 1), Vector3(1.5f, 0, 0.5f));
    }

    private void AssertIntersections(NavMesh mesh, params Vector3[] expectedIntersections)
    {
        List<Vector3> missingIntersections = new List<Vector3>();
        missingIntersections.AddRange(expectedIntersections);

        foreach (NavMeshVertex v in mesh.Verticies.Values)
        {
            Vector3 meshIntersection = v.position;
            for (int i = 0; i < expectedIntersections.Length; i++)
            {
                Vector3 expectedIntersection = expectedIntersections[i];
                if (meshIntersection == expectedIntersection)
                {
                    missingIntersections.Remove(expectedIntersection);
                }
            }
        }

        if(missingIntersections.Count > 0)
        {
            string error = "Interceptions incorrect. Expected: ";
            foreach (Vector3 v in expectedIntersections)
            {
                error += v + " ";
            }
            error += "\nBut Got\n";
            foreach (Vector3 v in missingIntersections)
            {
                error += v + " ";
            }
            Assert.Fail(error);
        }
    }

    private NavMeshTriangle[] ToArray(params NavMeshTriangle[] triangles)
    {
        return triangles;
    }

	private List<NavMeshTriangle> ToList(params NavMeshTriangle[] triangles)
	{
		return new List<NavMeshTriangle>(triangles);
	}

    private Vector3 Vector3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }
}
