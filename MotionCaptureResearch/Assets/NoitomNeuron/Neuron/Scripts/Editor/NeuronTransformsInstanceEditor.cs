﻿using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Neuron.NeuronTransformsInstance))]
public class NeuronTransformsInstanceEditor : Editor 
{

    // https://catlikecoding.com/unity/tutorials/editor/custom-list/
    SerializedProperty transformsField;
    //void OnEnable()
    //{
    //    // Setup the SerializedProperties.
    //    transformsField = serializedObject.FindProperty("transforms");
    //}

    public override void OnInspectorGUI()
    {
        Neuron.NeuronTransformsInstance script = (Neuron.NeuronTransformsInstance)target;

        DrawDefaultInspector();
        if (GUILayout.Button("bind"))
        {
            Debug.Log("[NeuronTransformsInstanceVR] - LOAD all Transform references into the bones list!");

            script.Bind(script.transform, script.prefix);

            EditorUtility.SetDirty(script);

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        }

        // 这种做法适合不定长数组,对定长数组不起作用
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("transforms"), new GUIContent("transforms"), true);
        serializedObject.Update();
        transformsField = serializedObject.FindProperty("transforms");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("transforms"), new GUIContent("transforms"), true);

        EditorGUI.indentLevel += 1;
       // if (transformsField.isExpanded)
        {
            //EditorGUILayout.PropertyField(transformsField.FindPropertyRelative("Array.size"));
            for (int i = 0; i < transformsField.arraySize; i++)
            {
                EditorGUILayout.PropertyField(transformsField.GetArrayElementAtIndex(i), new GUIContent(((Neuron.NeuronBones)i).ToString()));
            }
        }
        EditorGUI.indentLevel -= 1;
        serializedObject.ApplyModifiedProperties();

    }
}