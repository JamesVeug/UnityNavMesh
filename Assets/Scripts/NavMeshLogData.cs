using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum LogStep
{
	Start = 1 << 0,
	Stage = 1 << 1,
	Subtract = 1 << 2,
	Tesselation = 1 << 3,
	Completion = 1 << 4,
}

public class LogState
{
	public static int CURRENT_ID = 0;

	public LogStep Step;
	public string Message;
	public List<List<NavMeshVertex>> Log;
	public int ID = CURRENT_ID++;

	public LogState(LogStep state, string message)
	{
		Step = state;
		Message = message;
		Log = new List<List<NavMeshVertex>>();
	}
}

public class NavMeshLogData : ScriptableObject
{
	public event Action OnActivatedStatesChanged = delegate { };

	public List<LogState> History {
		get
		{
			if (m_history == null)
			{
				m_history = new List<LogState>();
			}
			return m_history;
		}
	}

	[SerializeField]
	private List<LogState> m_history;

	[SerializeField]
	private HashSet<int> m_activatedStates = new HashSet<int>(); // For visual Log Debugging

	public void AddActivatedState(int id)
	{
		m_activatedStates.Add(id);
		OnActivatedStatesChanged();
	}

	public void RemoveActivatedState(int id)
	{
		m_activatedStates.Remove(id);
		OnActivatedStatesChanged();
	}

	public bool IsStateActivated(int id)
	{
		return m_activatedStates.Contains(id);
	}
}
