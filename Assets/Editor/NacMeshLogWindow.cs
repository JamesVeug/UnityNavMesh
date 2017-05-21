using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NavMeshLogWindow : EditorWindow 
{
	[MenuItem("NavMesh/Open Log")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(NavMeshLogWindow));
	}

	[MenuItem("NavMesh/Create Data")]
	public static void CreateData()
	{
		NavMeshLogData asset = ScriptableObject.CreateInstance<NavMeshLogData>();
		AssetDatabase.CreateAsset(asset, "Assets/Resources/NavMeshLogData.asset");

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private NavMeshLog m_log;
	private NavMeshLogData m_currentData;

	public void OnEnable()
	{
		m_log = NavMeshLog.Instance;
		m_currentData = AssetDatabase.LoadAssetAtPath<NavMeshLogData>("Assets/Resources/NavMeshLogData.asset");
	}

	public void OnFocus()
	{
		m_log = NavMeshLog.Instance;
		m_currentData = AssetDatabase.LoadAssetAtPath<NavMeshLogData>("Assets/Resources/NavMeshLogData.asset");
	}

	void OnGUI()
	{
		bool refreshMono = false;

		for (int i = 0; i < m_currentData.History.Count; i++)
		{
			LogState state = m_currentData.History[i];
			bool selected = m_currentData.IsStateActivated(state.ID);

			bool pressed = CreateButton(state, selected);
			if (pressed)
			{
				if (!selected)
				{
					m_currentData.AddActivatedState(state.ID);
					selected = true;
				}
				else
				{
					if(state.Step == LogStep.
					m_currentData.RemoveActivatedState(state.ID);
					selected = false;
				}
				refreshMono = true;
			}

			// Ignore the next stages
			if (state.Step == LogStep.Stage && !selected)
			{
				for (int j = i + 1; j < m_currentData.History.Count; j++)
				{
					if (StepHasSubStates(m_currentData.History[j].Step))
					{
						break;
					}

					// Remove SubState
					if (m_currentData.IsStateActivated(m_currentData.History[j].ID))
					{
						m_currentData.RemoveActivatedState(m_currentData.History[j].ID);
						refreshMono = true;
					}
					i++;
				}
			}
		}

		if (refreshMono)
		{
			RefreshVisuals();
		}
	}

	public bool StepHasSubStates(LogStep step)
	{
		return step == LogStep.Completion ||
			step == LogStep.Start ||
			step == LogStep.Stage;
	}

	private void DisableSteps(LogStep step)
	{
		for (int i = 0; i < m_log.Data.History.Count; i++)
		{
			LogState state = m_log.Data.History[i];
			LogStep logStep = state.Step;
			if ((logStep & step) == logStep)
			{
				m_log.Data.RemoveActivatedState(state.ID);
			}			 
		}
	}

	private void RefreshVisuals()
	{
		m_log.RebuildMono();
	}

	private bool CreateButton(LogState state, bool isPressed)
	{
		string stateName = state.Step.ToString();
		string name = stateName;
		if (isPressed)
		{
			name = ">  " + name.ToUpper() + " " + state.Message;
		}
		else
		{
			name = "< " + name + " " + state.Message;
		}

		if (state.Step != LogStep.Completion &&
			state.Step != LogStep.Start &&
			state.Step != LogStep.Stage)
		{
			name = "\t" + name;
		}

		bool pressed = false;
		using (new EditorGUILayout.HorizontalScope(GUILayout.Width(280.0f)))
		{
			GUIStyle titleStyle = new GUIStyle(EditorStyles.miniButtonLeft);
			titleStyle.fontStyle = isPressed ? FontStyle.Bold : FontStyle.Normal;
			titleStyle.alignment = TextAnchor.MiddleLeft;

			GUI.color = isPressed ? Color.green : Color.white;
			pressed = GUILayout.Button(name, titleStyle);
		}
		return pressed;
	}
}
