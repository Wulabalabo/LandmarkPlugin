using Landmark;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class LandmarkManangerWindow : EditorWindow
{
    private GameObject _character;
    public LandmarkManager Script;
    public GameObject Point;
    private int _currentClipIndex = 0;
    private string _boneCount="";
    private float _scale = 0;
    LandmarkManangerWindow()
    {
        this.titleContent = new GUIContent("LandmarkEditor");
    }

    private void OnEnable()
    {
       
       
    }

    private void SubTitle(string title)
    {
        GUILayout.Space(10);

        GUILayout.Label(title);
    }

    [MenuItem("Viewpointet/LandmarkEditor")]
    static void CreateWindow()
    {
        EditorWindow.GetWindow<LandmarkManangerWindow>();
    }

   
    private void OnGUI()
    {
        
        _character = EditorGUILayout.ObjectField("Character:", _character, typeof(GameObject), true) as GameObject;
        _scale = EditorGUILayout.FloatField("PointScale", _scale);
        

        if (_character == null)
        {
            Script.AnimationClips.Clear();
            _boneCount = "0";
            return;
        }
        EditorGUI.BeginDisabledGroup(true);
        _boneCount = EditorGUILayout.TextField("Bone:", _boneCount.ToString());
        EditorGUI.EndDisabledGroup();

        #region Generate Landmarks
        SubTitle("Generate Landmarks");

        if (GUILayout.Button("Generate Bone Dictionary"))
        {
            Script.InitModelBoneData(_character);
            _boneCount = Script.ModelBoneDataDictionary.Count.ToString();
        }

        if (GUILayout.Button("Generate Landmarks"))
        {
            Script.GenerateLandmarksForCharacter(_character, Point, _scale);
        }


        if (GUILayout.Button("Clear Landmarks"))
        {
            Script.ClearLandmarks();
        }

        #endregion


        #region Config Setting
        SubTitle("Config Setting");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export Annotation"))
        {
            Script.SaveLandmarksPosition(_character);
        }

        if (GUILayout.Button("Import Annotation"))
        {
            Script.ImportLandmarksPosition(_character);
        }

        EditorGUILayout.EndHorizontal();
        #endregion


        #region Animation

        SubTitle("Animation");

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Import Animation") && _character != null)
        {
            Script.ImportCharacterAnimations(_character);
        }

        EditorGUILayout.BeginHorizontal();

        _currentClipIndex = EditorGUILayout.Popup(_currentClipIndex, Script.AnimationClips.Select((item) => item.name).ToArray());
        if (Script.AnimationClips.Count > 0 && _character != null)
        {
            Script.AnimationClips[_currentClipIndex].SampleAnimation(_character, 0);
        }

        if (GUILayout.Button("Set Default Pose"))
        {
            Script.AnimationClips.Clear();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        #endregion
    }
}
