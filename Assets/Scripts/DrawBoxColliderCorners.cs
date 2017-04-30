using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBoxColliderCorners : MonoBehaviour
{
    
    void OnDrawGizmosSelected()
    {
        BoxCollider b = GetComponent<BoxCollider>();
        float size = 0.25f;

        Gizmos.color = Color.green;

        // Bottom
        /*Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f), size);
        Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f), size);
        Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f), size);
        Gizmos.DrawSphere(transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f), size);*/

        // Top
        Vector3 tr = transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, b.size.z) * 0.5f);
        Vector3 tl = transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, b.size.z) * 0.5f);
        Vector3 bl = transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, -b.size.z) * 0.5f);
        Vector3 br = transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, -b.size.z) * 0.5f);

        Gizmos.DrawSphere(tr, size); // Top Right
        Gizmos.DrawSphere(tl, size); // Top Left
        Gizmos.DrawSphere(bl, size); // Bottom Left
        Gizmos.DrawSphere(br, size); // Bottom Right

        List<Vector3> vectors = new List<Vector3>();
        vectors.Add(tr);
        vectors.Add(br);
        vectors.Add(bl);
        vectors.Add(tl);

        for (int i = 0; i < vectors.Count; i++)
        {
            ShowCrossProduct(vectors, i);
        }
    }

    void ShowCrossProduct(List<Vector3> innerVerticies, int index)
    {
        // Iterate each inner
        int nextIndex = (index + 1) % innerVerticies.Count;
        int prevIndex = ((index - 1) + innerVerticies.Count) % innerVerticies.Count;

        // Crossproduct with next and prev
        Vector3 crossProduct = cross(innerVerticies[index], innerVerticies[nextIndex], innerVerticies[prevIndex]);
        Gizmos.DrawLine(innerVerticies[index], innerVerticies[index] + crossProduct);


        Vector3 crossProduct2 = (innerVerticies[index] - innerVerticies[prevIndex]).normalized;
        Vector3 a = innerVerticies[index] + (innerVerticies[nextIndex] - innerVerticies[index]) / 2;
        Gizmos.DrawLine(a, a + crossProduct2);
    }

    private Vector3 cross(Vector3 v, Vector3 a, Vector3 b)
    {
        return (v - a + v - b).normalized;
    }
}
