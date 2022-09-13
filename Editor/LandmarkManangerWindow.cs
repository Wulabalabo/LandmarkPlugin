using Landmark;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditorInternal;

public class LandmarkManangerWindow : EditorWindow
{
    private GameObject _character;
    public GameObject Point;
    public LandmarkManager Script;

    private SkinnedCollisionHelper _collisionHelper;
    [SerializeField]
    List<BarycentricCoodinates> barycentricCoodinates = new List<BarycentricCoodinates>();
    private SerializedObject _objectSo = null;
    private SerializedProperty _objProperty=null;
    private int _currentClipIndex = 0;
    private string _boneCount="";
    private float _scale = 0.02f;
    private bool _mouseSelect;
    LandmarkManangerWindow()
    {
        this.titleContent = new GUIContent("LandmarkEditor");
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    // Manual Annotation
    void OnSceneGUI(SceneView iSceneView)
    {
        Event ev = Event.current;
        if (_mouseSelect)
        {
            if (ev.type == EventType.MouseDown)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                int layerMask = 0;
                layerMask |= (1 << LayerMask.NameToLayer("Character"));

                if (Physics.Raycast(ray, out hit, 10, layerMask))
                {
                    
                    Transform selected = Selection.activeTransform;
                    if (selected != null && selected.tag.CompareTo("Landmark") == 0)
                    {
                        barycentricCoodinates.Clear();
                        selected.transform.position = hit.point;
                        int landmarkId = int.Parse(selected.name.Remove(0, 8));
                        barycentricCoodinates= Script.Insert2Barycentric(_character, landmarkId, hit.triangleIndex, hit.barycentricCoordinate);
                        _objectSo.Update();
                    }
                }
            }            
        }
    }

    private void OnEnable()
    {
        _collisionHelper = new SkinnedCollisionHelper();
        _objectSo = new SerializedObject(this);
        _objProperty = _objectSo.FindProperty("barycentricCoodinates");
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
        EditorGUI.BeginChangeCheck();
        _character = EditorGUILayout.ObjectField("Character:", _character, typeof(GameObject), true) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            Script.AnimationClips.Clear();
            _boneCount = "0";
            if (_character != null)
            {
                Script.InitCharacter(_character);
                _collisionHelper.Init(_character.transform.Find("CC_Game_Body").gameObject);
            }
        }

        _scale = EditorGUILayout.FloatField("PointScale", _scale);      

        if (_character == null)
        {
            return;
        }
        EditorGUI.BeginDisabledGroup(true);
        _boneCount = EditorGUILayout.TextField("Bone:", _boneCount.ToString());
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Save Change"))
        {
            Script.SavePrefab(_character);
        }

        #region Generate Landmarks
        SubTitle("Generate Landmarks");

        _mouseSelect=GUILayout.Toggle(_mouseSelect, "MouseSelect");

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
            Script.ClearLandmarks(_character);
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

        EditorGUI.BeginChangeCheck();
        _currentClipIndex = EditorGUILayout.Popup(_currentClipIndex, Script.AnimationClips.Select((item) => item.name).ToArray());
        if (EditorGUI.EndChangeCheck()&& Script.AnimationClips.Count > 0 && _character != null)
        {
            Script.AnimationClips[_currentClipIndex].SampleAnimation(_character, 0);
            _collisionHelper.UpdateCollisionMesh();
        }

        if (GUILayout.Button("Set Default Pose"))
        {
            Script.AnimationClips.Clear();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        #endregion

        #region Barycentric Coodinates Controller
        SubTitle("Barycentric Coodinates");

        _objectSo.Update();
        
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(_objProperty, true);

        if (EditorGUI.EndChangeCheck())
        {
            _objectSo.ApplyModifiedProperties();
        }

        if(GUILayout.Button("Confirm Change"))
        {
            Script.ConfirmBarycentricChange(_character, barycentricCoodinates);
        }
        #endregion
    }
}
