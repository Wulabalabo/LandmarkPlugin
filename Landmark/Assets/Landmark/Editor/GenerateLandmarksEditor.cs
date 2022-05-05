using System.Collections;
using System.Collections.Generic;
using Landmark;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateLandmarks))]
public class GenerateLandmarksEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GenerateLandmarks script = (GenerateLandmarks) target;
        if (GUILayout.Button("Generate Bone Dictionary"))
        {
            script.InitModelBoneData();
        }

        if (GUILayout.Button("Generate Landmarks"))
        {
            script.GenerateLandmarksForCharacter();
        }

        if (GUILayout.Button("Export Annotation"))
        {
            script.SaveLandmarksPosition();
        }

        if (GUILayout.Button("Import Annotation"))
        {
            script.ImportLandmarksPosition();
        }

        if (GUILayout.Button("Clear Landmarks"))
        {
            script.ClearLandmarks();
        }
    }
}
