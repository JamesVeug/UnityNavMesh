using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NavMeshTriangleMono : MonoBehaviour
{
    public Color color = Color.black;

    public NavMeshTriangle Data
    {
        get
        {
            if (!Application.isPlaying)
            {
                m_data.GetPositions().Clear();
                foreach (Transform child in transform)
                {
                    m_data.GetPositions().Add(child.position);
                }
            }
            return m_data;
        }
    }

    private NavMeshTriangle m_data = new NavMeshTriangle();

    void OnDrawGizmos()
    {
        float heightOffset = 0.1f;

        Gizmos.color = color;
        Handles.color = Color.black;

        var positions = Data.GetPositions();
        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawSphere(positions[i] + new Vector3(0, heightOffset), 0.25f);
            Vector3 startVertex = positions[i];
            Vector3 endVertex = i + 1 < positions.Count ? positions[i + 1] : positions[0];

            Handles.Label(startVertex + Vector3.up, i.ToString());
            Gizmos.DrawLine(startVertex + new Vector3(0, heightOffset, 0), endVertex + new Vector3(0, heightOffset, 0));
        }
    }
}
