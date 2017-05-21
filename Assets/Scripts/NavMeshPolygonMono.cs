using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NavMeshPolygonMono : MonoBehaviour
{
    public Color color = Color.black;

	public NavMeshPolygon Data
    {
        get
        {
			return m_data;
        }
		set
		{
			m_data = value;
		}
    }

	private NavMeshPolygon m_data;

    void OnDrawGizmos()
    {
        float heightOffset = 0.15f;

        Gizmos.color = color;
        Handles.color = new Color(0, 0, 0, color.a);

        var positions = Data.Verticies;
        for (int i = 0; i < positions.Count; i++)
        {
			Vector3 startVertex = positions[i].position + transform.position + new Vector3(0, heightOffset, 0);
			Vector3 endVertex = i + 1 < positions.Count ? positions[i + 1].position : positions[0].position;
			endVertex += transform.position + new Vector3(0, heightOffset, 0);


			Gizmos.DrawSphere(startVertex, 0.25f);

            Handles.Label(startVertex + Vector3.up, i.ToString());
            Gizmos.DrawLine(startVertex, endVertex);
        }
    }
}
