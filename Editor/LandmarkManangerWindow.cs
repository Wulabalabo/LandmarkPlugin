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

    private List<SkinnedCollisionHelper> _collisionHelpers = new List<SkinnedCollisionHelper>();
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
                        int landmarkId = int.Parse(selected.name.Substring(8));
                        barycentricCoodinates= Script.WriteBarycentric(_character, landmarkId, hit.collider.name, hit.triangleIndex, hit.barycentricCoordinate);
                        _objectSo.Update();
                    }
                }
            }            
        }
    }

    private void OnEnable()
    {
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
                foreach (Transform tf in _character.GetComponentsInChildren<Transform>())
                {
                    if (tf.CompareTag("CollisionMesh"))
                    {
                        SkinnedCollisionHelper helper = new SkinnedCollisionHelper();
                        helper.Init(tf.gameObject);
                        _collisionHelpers.Add(helper);
                    }
                }
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
            foreach(var helper in _collisionHelpers)
                helper.UpdateCollisionMesh();
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

        #region Pose Random
        SubTitle("Pose Random");

        if (GUILayout.Button("RandomPoseTest"))
        {
            PoseRandomization.Init(_character);
        }

        if (GUILayout.Button("ChangePoseTest"))
        {
            PoseRandomization.ChangePose();
            foreach (var helper in _collisionHelpers)
                helper.UpdateCollisionMesh();
        }

        if (GUILayout.Button("ResetPose"))
        {
            PoseRandomization.PoseReset();
            foreach (var helper in _collisionHelpers)
                helper.UpdateCollisionMesh();
        }
        #endregion
        

        
    }

    void ProceduralLandmark()
    {
        #region Elbow Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Elbow Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "elbow");
        }
        if (GUILayout.Button("Reset Elbow Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "elbow");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Upperarm Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Upperarm Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "upperarm");
        }
        if (GUILayout.Button("Reset Upperarm Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "upperarm");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Shoulder Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Shoulder Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "shoulder");
        }
        if (GUILayout.Button("Reset Shoulder Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "shoulder");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Neck Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Neck Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "neck");
        }
        if (GUILayout.Button("Reset Neck Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "neck");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Wrist Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Wrist Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "wrist");
        }
        if (GUILayout.Button("Reset Wrist Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "wrist");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Hip Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Hip Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "hip");
        }
        if (GUILayout.Button("Reset Hip Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "hip");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Knee Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Knee Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "knee");
        }
        if (GUILayout.Button("Reset Knee Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "knee");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Ankle Landmarks
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Ankle Landmarks"))
        {
            Script.GenerateSkinLandmarks(_character, "ankle");
        }
        if (GUILayout.Button("Reset Ankle Landmarks"))
        {
            Script.ResetSkinLandmarks(_character, "ankle");
        }
        EditorGUILayout.EndHorizontal();
        #endregion
    }
}
