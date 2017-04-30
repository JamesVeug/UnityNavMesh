using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMeshBuilder))]
public class NavMeshBuilderEditor : Editor
{
    string drawTriangles = string.Empty;
    Color col = Color.white;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Build Nav Mesh"))
        {
            ((NavMeshBuilder)target).BuildMesh();
        }
        
        GUILayout.Space(25);

        drawTriangles = EditorGUILayout.TextArea(drawTriangles, GUILayout.MinHeight(100));
        if(GUILayout.Button("Draw Triangles"))
        {
            DrawTriangleString(drawTriangles, (NavMeshBuilder)target);
        }

        if (GUILayout.Button("BuildMeshAs Monos"))
        {
            DrawTriangleString(drawTriangles, (NavMeshBuilder)target);
        }
    }

    private void DrawTriangleString(string triangles, NavMeshBuilder builder)
    {
        // Format
        // -35.3, 5.4, -9.9\n-35.3, 5.4, -9.9\n-35.3, 5.4, -9.9

        List<Vector3> vectors = new List<Vector3>();

        string divided = RemoveAll(triangles, '(', ')');
        string[] split = divided.Split('\n');
        for (int i = 0; i < split.Length; i++)
        {
            Vector3 v = new Vector3();

            string[] split2 = split[i].Split(',');
            for (int j = 0; j < split2.Length; j++)
            {
                if (j == 0) v.x = float.Parse(split2[j]);
                if (j == 1) v.y = float.Parse(split2[j]);
                if (j == 2) v.z = float.Parse(split2[j]);
            }

            vectors.Add(v);
        }

        NavMeshTriangleMono mono = Instantiate(builder.TriangleTemplate, builder.transform, true);
        mono.transform.position = Vector3.zero;
        mono.color = col;

        for (int i = 0; i < vectors.Count; i ++)
        {
            GameObject o = new GameObject(i.ToString());
            o.transform.parent = mono.transform;
            o.transform.localPosition = vectors[i];
        }

        
    }

    string RemoveAll(string input, params char[] target)
    {
        string s = string.Empty;

        for (int i = 0; i < input.Length; i++)
        {
            bool skip = false;
            for (int j = 0; j < target.Length; j++)
            {
                if(input[i] == target[j])
                {
                    skip = true;
                    break;
                }
            }

            if(!skip)
            {
                s += input[i];
            }
        }
        return s;
    }
}


