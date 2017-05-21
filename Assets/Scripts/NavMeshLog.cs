/// Copyright (c) PikPok.  All rights reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
/// <author>James Peter Veugelaers</author>
public class NavMeshLog : MonoBehaviour
{
	public static NavMeshLog Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = FindObjectOfType<NavMeshLog>();
				if (m_instance == null)
				{
					Debug.LogError("No NavMeshLog Detected!");
				}
			}
			return m_instance;
		}
	}

	public NavMeshPolygonMono PolygonTemplate;

	public NavMeshLogData Data { get { return m_data; } }

	[SerializeField]
	private NavMeshLogData m_data;

	private static NavMeshLog m_instance;

	public void Log(List<NavMeshVertex> verticies, LogStep state, string message)
	{
		LogState log = new LogState(state, message);
		log.Log.Add(NavMeshUtility.DeepClone(verticies));
		m_data.History.Add(log);
	}

	public void Log(NavMeshPolygon polygon, LogStep state, string message)
	{
		LogState log = new LogState(state, message);
		log.Log.Add(NavMeshUtility.DeepClone(polygon.Verticies));
		m_data.History.Add(log);
	}

	public void Log(List<NavMeshPolygon> polygons, LogStep state, string message)
	{
		LogState log = new LogState(state, message);
		for (int i = 0; i < polygons.Count; i++)
		{
			log.Log.Add(NavMeshUtility.DeepClone(polygons[i].Verticies));
		}
		m_data.History.Add(log);
	}

	public void Log(NavMeshPolygon[] polygons, LogStep state, string message)
	{
		LogState log = new LogState(state, message);
		for (int i = 0; i < polygons.Length; i++)
		{
			log.Log.Add(NavMeshUtility.DeepClone(polygons[i].Verticies));
		}
		m_data.History.Add(log);
	}

	public void Log(NavMeshTriangle[] triangles, LogStep state, string message)
	{
		LogState log = new LogState(state, message);
		for (int i = 0; i < triangles.Length; i++)
		{
			List<NavMeshVertex> list = new List<NavMeshVertex>();
			int count = triangles[i].GetPositions().Count;
			for (int j = 0; j < count; j++)
			{
				list.Add(new NavMeshVertex(triangles[i].GetPositions()[j], j - count));				
			}
			log.Log.Add(list);
		}
		m_data.History.Add(log);
	}

	public void Clear()
	{
		m_data.History.Clear();
	}

	public void RebuildMono()
	{
		List<LogState> states = new List<LogState>();
		for (int i = 0; i < m_data.History.Count; i++)
		{
			if (m_data.IsStateActivated(m_data.History[i].ID))
			{
				states.Add(m_data.History[i]);
			}
		}

		states.Sort((a, b) =>
		{
			return a.ID - b.ID;
		});

		foreach (Transform child in transform)
		{
			GameObject.DestroyImmediate(child.gameObject);
		}

		for (int i = 0; i < states.Count; i++)
		{
			for (int j = 0; j < states[i].Log.Count; j++)
			{
				CreatePolygon(states[i].Log[j]);
			}
		}
	}

	private NavMeshPolygonMono CreatePolygon(List<NavMeshVertex> verticies)
	{
		NavMeshPolygonMono polygon = Instantiate(PolygonTemplate, transform, true);

		List<NavMeshVertex> clonedVerticies = NavMeshUtility.DeepClone(verticies);
		NavMeshPolygon data = new NavMeshPolygon(clonedVerticies, 0);
		polygon.Data = data;

		return polygon;
	}
}