using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrawBoxColliderCorners))]
public class DrawBoxColliderCornersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Rotate Y Clockwise"))
        {
            ((DrawBoxColliderCorners)target).transform.Rotate(Vector3.up, 15);
        }

        if (GUILayout.Button("Rotate Y Anti-Clockwise"))
        {
            ((DrawBoxColliderCorners)target).transform.Rotate(Vector3.up, -15);
        }
    }
}


