using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Finger))]
public class FingerInspector : Editor
{
    private bool foldoutGroupTouchedObjects = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Finger script = serializedObject.targetObject as Finger;
        foldoutGroupTouchedObjects = EditorGUILayout.Foldout(foldoutGroupTouchedObjects, "Touched Objects");
        if (foldoutGroupTouchedObjects)
        {
            foreach (GameObject obj in script.touchedObjects)
            {
                EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject), true);
            }
        }
    }
}
