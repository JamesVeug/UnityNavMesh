﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;


public class NavMesh
{
    public List<NavMeshPolygon> Mesh;
    public List<NavMeshIntersection> Intersections;
    public Dictionary<int, NavMeshVertex> Verticies;
}

public class NavMeshIntersection
{
    public NavMeshPolygon polygon;
    public List<NavMeshPolygon> intersections = new List<NavMeshPolygon>();
}


public class NavMeshFactory
{

    private class SubtractTest
    {
        public NavMeshVertex vertex;
        public NavMeshVertex nextVertex;
        public bool IsIntersection;

        public SubtractTest() { }
        public SubtractTest(NavMeshVertex v, NavMeshVertex n, bool isIntersection = false)
        {
            vertex = v;
            nextVertex = n;
            IsIntersection = isIntersection;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            SubtractTest c = obj as SubtractTest;
            return c.vertex.ID == vertex.ID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public int startX = 0;
    public int startY = 0;

    public float MinSnapDistance = 0.125f;
    public int WorldHeight = 10;
    public int StepHeight = 2;

    private List<NavMeshPolygon> m_navMesh;
    private List<NavMeshIntersection> m_intersections;

    private int m_polygonID = 0;
    private int m_vertexID = 0;
    private Dictionary<int, NavMeshVertex> m_verticies;

    public void Reset()
    {
        m_polygonID = 0;
        m_vertexID = 0;
    }

    public NavMesh BuildMesh(List<NavMeshTriangle> objectList)
    {
        m_intersections = new List<NavMeshIntersection>();
        m_verticies = new Dictionary<int, NavMeshVertex>();
        m_navMesh = new List<NavMeshPolygon>();
		        
        objectList.Sort((a, b) =>
        {
            return Mathf.FloorToInt(a.Min().y - b.Min().y);
        });

        // Build Mesh
        for (int i = 0; i < objectList.Count; i++)
        {
            NavMeshTriangle collider = objectList[i];

            List<NavMeshPolygon> newPolygons = new List<NavMeshPolygon>();
            newPolygons.Add(CreatePolygon(collider));
            AddPolygons(newPolygons);
        }

		// Logging
		NavMeshLog.Instance.Clear();
		NavMeshLog.Instance.Log(m_navMesh, LogStep.Stage, "Colliders to Mesh");

        // Subtract
        List<NavMeshPolygon> totalSubtracts = new List<NavMeshPolygon>();
        for (int i = 0; i < m_navMesh.Count; i++)
        {
            NavMeshPolygon polygon = m_navMesh[i];

            int worldSize = m_navMesh.Count;
            if (SubtractPolygonFromWorld(polygon, i))
            {
				// Subtracted polygon from the world
				NavMeshLog.Instance.Log(m_navMesh, LogStep.Stage, string.Format("Subtracting {0} from world.", polygon.ID));
            }

            int subtracted = Math.Max(0, m_navMesh.Count - worldSize);
            i += subtracted;
        }

        Debug.Log("Finished building Mesh with " + m_navMesh.Count + " triangles");

		NavMeshLog.Instance.Log(m_navMesh, LogStep.Completion, "Finished Building NavMesh");

        NavMesh mesh = new NavMesh();
        mesh.Mesh = m_navMesh;
        mesh.Intersections = m_intersections;
        mesh.Verticies = m_verticies;

        return mesh;
    }

    private bool AddPolygons(List<NavMeshPolygon> polygons)
    {
        int size = m_navMesh.Count;
        for (int i = 0; i < polygons.Count; i++)
        {
            m_navMesh.Add(polygons[i]);
        }
        return size != m_navMesh.Count;
    }

    private bool SubtractPolygonFromWorld(NavMeshPolygon polygon, int index)
    {
        if (m_navMesh.Count == 0)
        {
            return false;
        }


        // Subtract
        List<NavMeshPolygon> newPolygons = new List<NavMeshPolygon>();
        for (int i = m_navMesh.Count - 1; i >= 0; i--)
        {
            NavMeshPolygon worldMeshPolygon = m_navMesh[i];
            if (worldMeshPolygon == polygon)
            {
                continue;
            }

            if (polygon.Intercepts(worldMeshPolygon))
            {
                List<NavMeshPolygon> subtractedPolygons = null;
                if(polygon.Max().y >= worldMeshPolygon.Max().y)
                {
                    subtractedPolygons = Subtract(worldMeshPolygon, polygon);
                }
                else
                {
                    subtractedPolygons = Subtract(polygon, worldMeshPolygon);
                }
                
                newPolygons.AddRange(subtractedPolygons);
                m_navMesh.RemoveAt(i);

                if(i <= index)
                {
                    index--;
                }
            }
        }

        if (newPolygons.Count > 0)
        {
            if (index > m_navMesh.Count - 1)
            {
                m_navMesh.AddRange(newPolygons);
            }
            else
            {
                m_navMesh.InsertRange(index + 1, newPolygons);
            }

            m_navMesh.RemoveAt(index);
            return true;
        }

        return false;
    }

    private List<NavMeshPolygon> Subtract(NavMeshPolygon polygon, NavMeshPolygon subtractThis)
    {
        Debug.LogWarning(string.Format("Unstracting {0} from {1}", subtractThis.ID, polygon.ID));
        List<NavMeshPolygon> result = new List<NavMeshPolygon>();

        // Create Linked list of inner verticies
        List<SubtractTest> finalInnerVerticies = new List<SubtractTest>();
        for (int i = 0; i < polygon.Verticies.Count; i++)
        {
            SubtractTest test = new SubtractTest();
            test.vertex = polygon.Verticies[i];

            int index = (i + 1) % polygon.Verticies.Count;
            test.nextVertex = polygon.Verticies[index];
            finalInnerVerticies.Add(test);
        }

        // Create Linked list of outter verticies
        List<SubtractTest> finalOutterVerticies = new List<SubtractTest>();
        for (int i = 0; i < subtractThis.Verticies.Count; i++)
        {
            SubtractTest test = new SubtractTest();
            test.vertex = subtractThis.Verticies[i];

            int index = (i + 1) % subtractThis.Verticies.Count;
            test.nextVertex = subtractThis.Verticies[index];
            finalOutterVerticies.Add(test);
        }

        // Get interceptions
        int interceptions = 0;
        int extraVerticies = 0;
        int removedVerticies = 0;
        SubtractTest lastIntersection = null;
        for (int k = 0; k < finalOutterVerticies.Count; k++)
        {
            NavMeshVertex edgeV1 = finalOutterVerticies[k].vertex;
            NavMeshVertex edgeV2 = finalOutterVerticies[k].nextVertex;

            Debug.LogWarning(string.Format("Starting Outter Edge {0}-{1}\n\tFinalInner: {2}\n\tFinalOutter: {3}", edgeV1.position, edgeV2.position, ToIDString(finalInnerVerticies), ToIDString(finalOutterVerticies)));
            for (int i = 0; i < finalInnerVerticies.Count; i++)
            {
                SubtractTest outterVertex = finalInnerVerticies[i];
                Debug.Log(string.Format("Innner Edge {0}-{1}", outterVertex.vertex.position, outterVertex.nextVertex.position));
                int outterIndex = i;


                // Get intersection
                NavMeshVertex intersection = null;
                SubtractTest t = null;
                Vector3 intersectionPosition = Vector3.zero;
                if (NavMeshEdge.LineIntersection3D(edgeV1.position, edgeV2.position, outterVertex.vertex.position, outterVertex.nextVertex.position, ref intersectionPosition))
                {
                    intersection = CreateVertex(intersectionPosition);

                    t = new SubtractTest(intersection, outterVertex.nextVertex, true);
                    outterVertex.nextVertex = intersection;

                    InsertVertex(i + 1, t, finalInnerVerticies);
                    InsertVertex(finalOutterVerticies, k, edgeV1, edgeV2, intersectionPosition);
                    k++;

                    Debug.Log(string.Format("Intersection between {0}-{1} and {2}-{3} as {4} ({5}-{6}) and {7}-{8})", edgeV1.ID, edgeV2.ID, outterVertex.vertex.ID, t.nextVertex.ID, intersection.ID, edgeV1.position, edgeV2.position, outterVertex.vertex.position, t.nextVertex.position));
                    interceptions++;
                    i++;
                }

                // Add Extra verticies
                if (polygon.PointInTriangle(outterVertex.vertex.position))
                {
                    if (intersection != null)
                    {
                        SubtractTest newVertex = new SubtractTest(edgeV1, outterVertex.nextVertex);
                        InsertVertex(i + 1, newVertex, finalInnerVerticies);
                        extraVerticies++;
                        i++;
                    }
                    /*else if (lastIntersection != null)
                    {
                        /*SubtractTest newVertex = new SubtractTest(edgeV2, lastIntersection.nextVertex);
                        t.nextVertex = newVertex.vertex;

                        InsertVertex(i + 1, newVertex, finalInnerVerticies);
                        extraVerticies++;
                        i++;
                    }
                    else
                    {
                        // We need to add edgeV1 and MAYBE edgeV2 into the list(inner/outter), after outterVertex.nextVertex.ID
                        Debug.LogError(string.Format("Unknown case: {0}\n\tFinalInnerVerticies: {1}\n\tFinalOutterVerticies: {2}\n\tIntersections: {3} \n\tExtraVerticies: {4} \n\tIntersections: {5}", edgeV1.position, finalInnerVerticies.Count, finalOutterVerticies.Count, interceptions, extraVerticies, removedVerticies));
                    }*/
                    Debug.Log(string.Format("Extra Vertex! {0} inside \n{1}", edgeV1.position, ToUsableString(finalInnerVerticies)));
                }

                lastIntersection = t;
            }
        }

        Debug.LogWarning("Finished Edges");

        // Remove subtracted verticies
        for (int i = 0; i < finalOutterVerticies.Count; i++)
        {
            SubtractTest outterVertex = finalOutterVerticies[i];
            if (outterVertex.IsIntersection)
            {
                continue;
            }

            Debug.Log("Checking if can remove Vertex: " + outterVertex.vertex.position);
            if (polygon.Contains(outterVertex.vertex))
            {
                Debug.Log("\tSubtracting Vertex " + outterVertex.vertex.position);
                SubtractTest previousOutterVertex = finalOutterVerticies[(i - 1 + finalOutterVerticies.Count) % finalOutterVerticies.Count];
                previousOutterVertex.nextVertex = outterVertex.nextVertex;

                removedVerticies++;

                // Remove this vertex
                finalOutterVerticies.RemoveAt(i);
                i--;
                
                Debug.Log("\t\tFinal Vectors: " + ToUsableString(finalOutterVerticies));
            }
        }

        Debug.LogWarning("finalVerticies.Count: " + finalInnerVerticies.Count);
        Debug.LogWarning("extraVerticies: " + extraVerticies);
        Debug.LogWarning("removedVerticies: " + removedVerticies);

		NavMeshLog.Instance.Log(ToPolygon(finalInnerVerticies), LogStep.Stage, string.Format("Subtracted {0} from {1}", polygon.ID, subtractThis.ID));

        // Teselate if the subtraction is inside
        if (extraVerticies > 0 || removedVerticies > 0)
        {
            // TODO - Return result of teselation
            result.AddRange(Tessellate(finalInnerVerticies));
        }
        else if (finalInnerVerticies.Count > 2)
        {
            // Only a triangle left
            result.Add(polygon);
        }
        else
        {
            // Teselate remaining 
            Debug.LogWarning("Unknown case!!!");
        }
        Debug.LogWarning("Done Inner Verticies. Tesselating Outter Vericies of size " + finalOutterVerticies.Count);

        // Tessellate Outter triangle
        result.AddRange(Tessellate(finalOutterVerticies));


        return result;
    }

	private NavMeshPolygon ToPolygon(List<SubtractTest> list)
	{
		NavMeshPolygon polygon = new NavMeshPolygon(-1);
		for (int i = 0; i < list.Count; i++)
		{
			polygon.AddVertex(list[i].vertex);
		}
		return polygon;
	}

    private void InsertVertex(List<SubtractTest> verticies, int startingIndex, NavMeshVertex start, NavMeshVertex end, Vector3 intersectionPosition)
    {
        NavMeshVertex v = CreateVertex(intersectionPosition);

        int insertionindex = startingIndex;
        SubtractTest current = verticies[insertionindex];
        SubtractTest next = verticies[(insertionindex + 1) % verticies.Count];

        while (insertionindex < (verticies.Count - 1) && verticies[insertionindex + 1].vertex.ID != end.ID)
        {
            current = verticies[insertionindex];
            next = verticies[insertionindex + 1];

            Vector3 vertex = current.vertex.position;

            if (start.position.y < end.position.y && intersectionPosition.y < vertex.y)
            {
                break;
            }
            else if (start.position.y > end.position.y && intersectionPosition.y > vertex.y)
            {
                break;
            }
            else if (start.position.x < end.position.x && intersectionPosition.x < vertex.x)
            {
                break;
            }
            else if (start.position.x > end.position.x && intersectionPosition.x > vertex.x)
            {
                break;
            }
            else
            {
                Debug.LogWarning(string.Format("Unknown Insertion condition {0} between positions {1} and {2}", intersectionPosition, start.position, end.position));
            }

            insertionindex++;
        }

        SubtractTest t = new SubtractTest();
        t.vertex = v;
        t.nextVertex = next.vertex;
        t.IsIntersection = true;

        current.nextVertex = v;
        InsertVertex(insertionindex + 1, t, verticies);
    }

    private void InsertVertex(int index, SubtractTest item, List<SubtractTest> list)
    {
        if (list.Contains(item))
        {
            string p = "[";
            string s = "[";
            for (int i = 0; i < list.Count; i++)
            {
                s += list[i].vertex.ID + " ";
                p += list[i].vertex.position + " ";
            }
            p += "]";
            s += "]";

            Debug.LogWarning(string.Format("Item already exists in list! {0} in list: {1} with positions {2}", item.vertex.position, s, p));
        }
        else
        {
            list.Insert(index, item);
        }
    }

    private List<NavMeshPolygon> Tessellate(List<SubtractTest> verticies)
    {
        List<NavMeshPolygon> polygons = new List<NavMeshPolygon>();

        Debug.Log("Tessellating polygon with " + verticies.Count + " verticies. \n" + ToUsableString(verticies));
        if (verticies.Count < 3)
        {
            return polygons;
        }

        // More than 1 triangle

        // Tesselate by makign a triangle from A,A+1,A+2 by
        // Making a line from A to A+2 as E, if E collides with Any line from A to A+n,
        // Then increment A by 1 and try again until false. If no collisions are made, Make a triangle, remove A+1 and repeat

        int AIndex = 0;
        int BIndex = 0;
        int CIndex = 0;
        Vector3 A, B, C, D, E;

        int iterations = 0;
        while (verticies.Count > 2)
        {
            if (++iterations > 100)
            {
                string list = "";
                for (int i = 0; i < verticies.Count; i++)
                {
                    list += string.Format("({0}-{1})", verticies[i].vertex.ID, verticies[i].vertex.position);
                    if (i + 1 < verticies.Count)
                    {
                        list += ", ";
                    }
                }

                throw new Exception("Tessellate: Iterations exceeded 100 for " + ToUsableString(verticies) + ". Can not finish tesselation." );
            }

            BIndex = (AIndex + 1) % verticies.Count;
            CIndex = (AIndex + 2) % verticies.Count;

            A = verticies[AIndex].vertex.position;
            B = verticies[BIndex].vertex.position;
            C = verticies[CIndex].vertex.position;

            // Check this is facing the right way
            if (!NavMeshUtility.IsFacingUp(A, B, C))
            {
                if(verticies.Count == 3)
                {
                    Debug.Log("Triangle is facing down. Breaking. \n" + A + "\n" + B + "\n" + C);
                    break;
                }
                Debug.Log("Triangle is facing down. Ignoreing. \n" + A + "\n" + B + "\n" + C);
                AIndex = (AIndex + 1) % verticies.Count;
                continue;
            }

            bool intersected = false;
            for (int DIndex = BIndex; DIndex < verticies.Count - 1; DIndex++)
            {
                D = verticies[DIndex].vertex.position;
                E = verticies[DIndex + 1].vertex.position;

                Vector3 intersection = Vector3.zero;
                if (NavMeshEdge.LineIntersection3D(A, C, D, E, ref intersection))
                {
                    Debug.Log(string.Format("Intersection during Tessellation between {0}-{1} and {2}-{3}", verticies[AIndex].vertex.ID, verticies[CIndex].vertex.ID, verticies[DIndex].vertex.ID, verticies[DIndex + 1].vertex.ID));
                    intersected = true;
                    break;
                }
            }

            if (!intersected)
            {
                Debug.Log(string.Format("Creating Tessellated Polygon from {0} {1} {2}\n{3} {4} {5}", verticies[AIndex].vertex.ID, verticies[BIndex].vertex.ID, verticies[CIndex].vertex.ID, verticies[AIndex].vertex.position, verticies[BIndex].vertex.position, verticies[CIndex].vertex.position));
				NavMeshPolygon polygon = CreatePolygon(A, B, C);
				polygons.Add(polygon);
                verticies.RemoveAt(BIndex);

				NavMeshLog.Instance.Log(polygon, LogStep.Tesselation, "Tesselated Portion");
            }

            AIndex = (AIndex + 1) % verticies.Count;
        }

        return polygons;
    }

    private List<NavMeshPolygon> CreatePolygons(BoxCollider c)
    {
        Vector3 tr = c.transform.TransformPoint(new Vector3(c.size.x, c.size.y, c.size.z) * 0.5f);   // Top Right
        Vector3 tl = c.transform.TransformPoint(new Vector3(-c.size.x, c.size.y, c.size.z) * 0.5f);  // Top Left
        Vector3 bl = c.transform.TransformPoint(new Vector3(-c.size.x, c.size.y, -c.size.z) * 0.5f); // Bottom Left
        Vector3 br = c.transform.TransformPoint(new Vector3(c.size.x, c.size.y, -c.size.z) * 0.5f); // Bottom Right


        NavMeshPolygon polygonA = CreatePolygon(tl, tr, br);
        NavMeshPolygon polygonB = CreatePolygon(tl, br, bl);


        List<NavMeshPolygon> polygons = new List<NavMeshPolygon>();
        polygons.Add(polygonA);
        polygons.Add(polygonB);

        return polygons;
    }

    private NavMeshPolygon CreatePolygon(NavMeshTriangle c)
    {
        List<Vector3> positions = c.GetPositions();
        return CreatePolygon(positions[0], positions[1], positions[2]);
    }

    private NavMeshPolygon CreatePolygon(Vector3 a, Vector3 b, Vector3 c)
    {
        return CreatePolygon(CreateVertex(a), CreateVertex(b), CreateVertex(c));
    }

    private NavMeshPolygon CreatePolygon(NavMeshVertex a, NavMeshVertex b, NavMeshVertex c)
    {
        NavMeshPolygon polygon = new NavMeshPolygon(m_polygonID++);

        polygon.AddVertex(a);
        polygon.AddVertex(b);
        polygon.AddVertex(c);

        return polygon;
    }

    private NavMeshVertex CreateVertex(Vector3 position)
    {
        // NOTE
        // Snapping 2 together is risky!
        // Create polygon to bridge them instead?
        // Or just new closest vertex?

        // Find close verticies and snap to the nearest one
        NavMeshVertex closestVertex = GetClosestVertex(position);
        if (closestVertex != null)
        {
            return closestVertex;
        }

        // Save new vertexand return vertex
        NavMeshVertex vertex = new NavMeshVertex(position + Vector3.zero, m_vertexID++);
        m_verticies.Add(vertex.ID, vertex);
        return vertex;
    }

    private NavMeshVertex GetClosestVertex(Vector3 position)
    {
        List<NavMeshVertex> closeVerticies = new List<NavMeshVertex>();

        NavMeshVertex closestVertex = null;
        float closestDistance = MinSnapDistance;
        foreach (var vertex in m_verticies.Values)
        {
            float distance = (vertex.position - position).magnitude;
            if (distance <= closestDistance)
            {
                closestVertex = vertex;
                closestDistance = distance;
            }
        }

        return closestVertex;
    }

    private string ToIDString(List<SubtractTest> list)
    {
        string s = "[";
        for (int i = 0; i < list.Count; i++)
        {
            s += list[i].vertex.ID + " ";
        }
        return s + "]";
    }

    private string ToUsableString(List<SubtractTest> list)
    {
        string s = "";
        for (int i = 0; i < list.Count; i++)
        {
            s += list[i].vertex.position + "\n";
        }
        return s + "";
    }
}