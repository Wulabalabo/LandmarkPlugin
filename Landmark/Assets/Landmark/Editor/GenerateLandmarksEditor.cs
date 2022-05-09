using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Landmark
{
    [CustomEditor(typeof(GenerateLandmarks))]
    public class GenerateLandmarksEditor : Editor
    {
        private GenerateLandmarks _script;
        private int _currentClipIndex = 0;
        private void SubTitle(string title)
        {
            GUILayout.Space(10);

            GUILayout.Label(title);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _script = (GenerateLandmarks)target;

            #region Generate Landmarks
            SubTitle("Generate Landmarks");

            if (GUILayout.Button("Generate Bone Dictionary"))
            {
                _script.InitModelBoneData();
            }

            if (GUILayout.Button("Generate Landmarks"))
            {
                _script.GenerateLandmarksForCharacter();
            }


            if (GUILayout.Button("Clear Landmarks"))
            {
                _script.ClearLandmarks();
            }

            #endregion


            #region Config Setting
            SubTitle("Config Setting");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Export Annotation"))
            {
                _script.SaveLandmarksPosition();
            }

            if (GUILayout.Button("Import Annotation"))
            {
                _script.ImportLandmarksPosition();
            }

            EditorGUILayout.EndHorizontal();
            #endregion


            #region Animation

            SubTitle("Animation");

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Import Animation") && _script.Character != null)
            {
                _script.ImportCharacterAnimations();
            }

            EditorGUILayout.BeginHorizontal();

            _currentClipIndex = EditorGUILayout.Popup(_currentClipIndex, _script.AnimationClips.Select((item) => item.name).ToArray());
            if (_script.AnimationClips.Count> 0 && _script.Character != null)
            {
                _script.AnimationClips[_currentClipIndex].SampleAnimation(_script.Character, 0);
            }

            if (GUILayout.Button("Set Default Pose"))
            {
                _script.AnimationClips.Clear();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            #endregion
        }
    }
}


