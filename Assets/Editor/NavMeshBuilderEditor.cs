using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMeshBuilder))]
public class NavMeshBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Build Nav Mesh"))
        {
            ((NavMeshBuilder)target).BuildMesh();
        }
        
        GUILayout.Space(25);

        
    }
}


