using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Source))]
[CanEditMultipleObjects]
public class SourceEditor : Editor
{
    SerializedProperty lookAtPoint;

    void OnEnable()
    {
        lookAtPoint = serializedObject.FindProperty("tao");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(lookAtPoint);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.LabelField("(Below this object)");
        
    }
}