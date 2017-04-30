using System;
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

    public NavMesh BuildMesh(NavMeshTriangle[] objectList)
    {
        m_intersections = new List<NavMeshIntersection>();
        m_verticies = new Dictionary<int, NavMeshVertex>();
        m_navMesh = new List<NavMeshPolygon>();
        
        Array.Sort<NavMeshTriangle>(objectList, (a, b) =>
        {
            return Mathf.FloorToInt(a.Min().y - b.Min().y);
        });

        // Build Mesh
        for (int i = 0; i < objectList.Length; i++)
        {
            NavMeshTriangle collider = objectList[i];

            List<NavMeshPolygon> newPolygons = new List<NavMeshPolygon>();
            newPolygons.Add(CreatePolygon(collider));
            AddPolygons(newPolygons);
        }

        // Subtract
        for (int i = 0; i < m_navMesh.Count; i++)
        {

            if (i >= m_navMesh.Count || i < 0)
            {
                Debug.Log("WHAT");
            }
            NavMeshPolygon polygon = m_navMesh[i];

            int worldSize = m_navMesh.Count;
            if (SubtractPolygonFromWorld(polygon, i))
            {
                //m_navMesh.re
            }

            int subtracted = Math.Max(0, m_navMesh.Count - worldSize);
            i += subtracted;
        }

        Debug.Log("Finished building Mesh with " + m_navMesh.Count + " triangles");

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

        int size = m_navMesh.Count;
        List<NavMeshPolygon> newPolygons = new List<NavMeshPolygon>();

        // Subtract
        for (int i = m_navMesh.Count - 1; i >= 0; i--)
        {
            NavMeshPolygon worldMeshPolygon = m_navMesh[i];
            if (worldMeshPolygon == polygon)
            {
                continue;
            }

            if (polygon.Intercepts(worldMeshPolygon))
            {
                List<NavMeshPolygon> subtractedPolygons = Subtract(worldMeshPolygon, polygon);
                newPolygons.AddRange(subtractedPolygons);
                m_navMesh.RemoveAt(i);
            }
        }

        if (index > m_navMesh.Count - 1)
        {
            m_navMesh.AddRange(newPolygons);
        }
        else
        {
            m_navMesh.InsertRange(index+1, newPolygons);
        }

        return size != m_navMesh.Count;
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
        NavMeshVertex lastIntersection = null;
        for (int k = 0; k < finalOutterVerticies.Count; k++)
        {
            NavMeshVertex edgeV1 = finalOutterVerticies[k].vertex;
            NavMeshVertex edgeV2 = finalOutterVerticies[k].nextVertex;

            Debug.LogWarning(string.Format("Starting Edge {0}-{1}\n\tFinalInner: {2}\n\tFinalOutter: {3}", edgeV1.ID, edgeV2.ID, ToString(finalInnerVerticies), ToString(finalOutterVerticies)));
            for (int i = 0; i < finalInnerVerticies.Count; i++)
            {
                SubtractTest outterVertex = finalInnerVerticies[i];
                Debug.Log(string.Format("Vertex {0}-{1}", outterVertex.vertex.ID, outterVertex.nextVertex.ID));
                int outterIndex = i;


                // Get intersection
                NavMeshVertex intersection = null;
                Vector3 intersectionPosition = Vector3.zero;
                if (NavMeshEdge.LineIntersection3D(edgeV1.position, edgeV2.position, outterVertex.vertex.position, outterVertex.nextVertex.position, ref intersectionPosition))
                {
                    intersection = CreateVertex(intersectionPosition);

                    SubtractTest t = new SubtractTest(intersection, outterVertex.nextVertex, true);
                    outterVertex.nextVertex = intersection;

                    InsertVertex(i + 1, t, finalInnerVerticies);
                    InsertVertex(finalOutterVerticies, k, edgeV1, edgeV2, intersectionPosition);
                    k++;

                    Debug.Log(string.Format("Intersection between {0}-{1} and {2}-{3} as {4} ({5}-{6}) and {7}-{8})", edgeV1.ID, edgeV2.ID, outterVertex.vertex.ID, t.nextVertex.ID, intersection.ID, edgeV1.position, edgeV2.position, outterVertex.vertex.position, t.nextVertex.position));
                    interceptions++;
                    i++;
                }

                // Add Extra verticies
                else if (polygon.Contains(edgeV1))
                {
                    Debug.Log("Extra Vertex!");
                    if (intersection != null)
                    {
                        SubtractTest newVertex = new SubtractTest(intersection, edgeV1);
                        InsertVertex(i + 1, newVertex, finalInnerVerticies);
                    }
                    else if (lastIntersection != null)
                    {
                        // Create 2. Forward and backwards

                        SubtractTest newVertex = new SubtractTest(edgeV2, edgeV1);
                        InsertVertex(i + 1, newVertex, finalInnerVerticies);


                        SubtractTest previousVertex = new SubtractTest(edgeV1, lastIntersection);
                        InsertVertex(i + 1, previousVertex, finalInnerVerticies);
                    }
                    else
                    {
                        // We need to add edgeV1 and MAYBE edgeV2 into the list(inner/outter), after outterVertex.nextVertex.ID
                        Debug.LogError(string.Format("Unknown case:\n\tFinalInnerVerticies: {0}\n\tFinalOutterVerticies: {1}\n\tIntersections: {2} \n\tExtraVerticies: {3} \n\tIntersections: {4}", finalInnerVerticies.Count, finalOutterVerticies.Count, interceptions, extraVerticies, removedVerticies));
                    }

                    extraVerticies++;
                    i++;
                }

                lastIntersection = intersection;
            }
        }

        Debug.LogWarning("Finished Edges");

        // Remove subtracted verticies
        for (int i = 0; i < finalInnerVerticies.Count; i++)
        {
            SubtractTest outterVertex = finalInnerVerticies[i];
            if (outterVertex.IsIntersection)
            {
                continue;
            }

            Debug.Log("Checking if can remove Vertex: " + outterVertex.vertex.ID);
            if (subtractThis.Contains(outterVertex.vertex))
            {
                Debug.Log("\tSubtracting Vertex " + outterVertex.vertex.ID);
                SubtractTest previousOutterVertex = finalInnerVerticies[(i - 1 + finalInnerVerticies.Count) % finalInnerVerticies.Count];
                previousOutterVertex.nextVertex = outterVertex.nextVertex;

                removedVerticies++;

                // Remove this vertex
                finalInnerVerticies.RemoveAt(i);
                i--;

                StringBuilder b = new StringBuilder("\t\tFinal Vectors: ");
                foreach (SubtractTest t in finalInnerVerticies)
                {
                    b.Append(t.vertex.ID + " ");
                }
                Debug.Log(b.ToString());
            }
        }

        Debug.LogWarning("finalVerticies.Count: " + finalInnerVerticies.Count);
        Debug.LogWarning("extraVerticies: " + extraVerticies);
        Debug.LogWarning("removedVerticies: " + removedVerticies);

        // Teselate if the subtraction is inside
        if (extraVerticies == 3 && removedVerticies == 0)
        {
            // TODO - Return result of teselation
            result.AddRange(Tessellate(finalInnerVerticies));
        }
        else if (finalInnerVerticies.Count == 3 && extraVerticies == 0 && removedVerticies == 0)
        {
            // Only a triangle left
            result.Add(polygon);
        }
        else
        {
            // Teselate remaining 
            result.AddRange(Tessellate(finalInnerVerticies));
        }

        // Tessellate Outter triangle
        /*if(finalOutterVerticies.Count > 3)
        {
            result.AddRange(Tessellate(finalOutterVerticies));
        }*/


        return result;
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

            throw new Exception(string.Format("Item already exists in list! {0} in list: {1} with positions {2}", item.vertex.ID, s, p));
        }
        else
        {
            list.Insert(index, item);
        }
    }

    private List<NavMeshPolygon> Tessellate(List<SubtractTest> verticies)
    {
        List<NavMeshPolygon> polygons = new List<NavMeshPolygon>();

        Debug.Log("Tessellating polygon with " + verticies.Count + " verticies.");
        if (verticies.Count == 3)
        {
            // Return the triangle
            polygons.Add(CreatePolygon(verticies[0].vertex, verticies[1].vertex, verticies[2].vertex));
            return polygons;
        }
        else if (verticies.Count == 0)
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
            if (++iterations > 1000)
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

                throw new Exception("Tessellate: Iterations exceeded 1000 for " + list + ". Can not finish tesselation." );
            }

            BIndex = (AIndex + 1) % verticies.Count;
            CIndex = (AIndex + 2) % verticies.Count;

            A = verticies[AIndex].vertex.position;
            B = verticies[BIndex].vertex.position;
            C = verticies[CIndex].vertex.position;

            // Check this is facing the right way
            Vector3 normal = A.Normal(B, C);
            if (!NavMeshUtility.IsFacingUp(A, B, C))
            {
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
                Debug.Log(string.Format("Creating Tessellated Polygon from {0} {1} {2}", verticies[AIndex].vertex.ID, verticies[BIndex].vertex.ID, verticies[CIndex].vertex.ID));
                polygons.Add(CreatePolygon(A, B, C));
                verticies.RemoveAt(BIndex);
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

    private string ToString(List<SubtractTest> list)
    {
        string s = "[";
        for (int i = 0; i < list.Count; i++)
        {
            s += list[i].vertex.ID;
        }
        return s + "]";
    }
}