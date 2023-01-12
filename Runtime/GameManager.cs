using Landmark;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Landmark
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private ScopeInfo _currentScope;
        public bool IsSceneChangeDone = false;
        private List<SkinnedCollisionHelper> _collisionHelpers = new List<SkinnedCollisionHelper>();
        public static GameManager instance { get { return _instance; } }

        public List<GameObject> Characters = new List<GameObject>();
        public SkinnedCollisionHelper SkinnedCollisionHelper { get; set; }
        public UiManager uiManager;
        public DebugManager DebugManager;
        public Transform DebugModePos;
        public LocalSceneManager localSceneManager;
        public LogicScriptable logicScriptable;
        public List<Transform> CurrentSpawnPoints;


        public GameObject CurrentCharacter;
        public Action<int> OnSceneChangeCompleted;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            Utils.CreateDirctory(logicScriptable.OutputDirctory);

            var prefabs = Resources.LoadAll("Prefabs");
            Characters = prefabs.Select((item) =>
            {
                return item as GameObject;
            }).ToList();

            localSceneManager.Init();
            DebugManager.Init(Characters);
            uiManager.InitOption();
        }

        internal void DoLogic()
        {
            Debug.Log("Start");

            _currentScope = new ScopeInfo()
            {
                InfoSaveTime = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")

            };

            _currentScope.InfoSaveDirctory = logicScriptable.OutputDirctory + "/" + _currentScope.InfoSaveTime;
            _currentScope.InfoSavePath = logicScriptable.OutputDirctory + "/" + _currentScope.InfoSaveTime + ".csv";


            Utils.CreateDirctory(_currentScope.InfoSaveDirctory);
            Utils.WriteTitle(_currentScope.InfoSavePath);
            DebugManager.QuitDebugMode();

            uiManager.Display(false);
            StartCoroutine(SceneChangeLogic());
        }


        public void SceneLogic(string SceneName)
        {            
            var process = SceneManager.LoadSceneAsync(SceneName);
            process.completed += (a) =>
            {
                if (_currentScope == null) return;
                _currentScope.SceneId = SceneManager.GetActiveScene().buildIndex;
                OnSceneChangeCompleted?.Invoke(_currentScope.SceneId);                
            };
            IsSceneChangeDone = false;
        }

        IEnumerator SceneChangeLogic()
        {
            foreach (var item in logicScriptable.ScenesLogic)
            {
                SceneLogic(item.Key);
                yield return new WaitUntil(() => IsSceneChangeDone);
                yield return StartCoroutine(PositionChangeLogic(item.Value));
            }
            Debug.Log("Done");
            uiManager.Display(true);
            yield return new WaitForEndOfFrame();
        }

        IEnumerator PositionChangeLogic(GameObject gameObject)
        {
            CurrentSpawnPoints = null;
            CurrentSpawnPoints = gameObject.GetComponentsInChildren<Transform>().Where((item) =>
            {
                return item.name != gameObject.name;
            }).ToList();
            
            foreach (var obj in CurrentSpawnPoints)
            {
                _currentScope.SpawnpointName = obj.name;
                yield return StartCoroutine(CharactorLogic(obj));
            }
            yield break;
        }

        IEnumerator CharactorLogic(Transform pos)
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                CurrentCharacter = Instantiate(Characters[i], pos.position, Quaternion.identity);
                CurrentCharacter.name = CurrentCharacter.name.Replace("(Clone)", "");
                Utils.DisplayLandmark(CurrentCharacter);
                _currentScope.CharacterName = CurrentCharacter.name;
                foreach (Transform transform in CurrentCharacter.GetComponentsInChildren<Transform>())
                {
                    if (transform.CompareTag("CollisionMesh"))
                    {
                        SkinnedCollisionHelper helper = new SkinnedCollisionHelper();
                        if (transform.name.Equals("CC_Game_Body"))
                        {
                            SkinnedCollisionHelper = helper;
                        }
                        helper.Init(transform.gameObject);
                        _collisionHelpers.Add(helper);
                    }
                }

                for (int j = 0; j < logicScriptable.Facings.Length; j++)
                {                   
                    _currentScope.Facing = logicScriptable.Facings[j].ToString();
                    CurrentCharacter.transform.localRotation = Quaternion.Euler(Vector3.up * logicScriptable.Facings[j]);
                    Utils.AutoCameraPositioning(CurrentCharacter, pos);
                    yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);
                    yield return StartCoroutine(PosesLogic(CurrentCharacter));
                }
                //random animation
                PoseRandomization.Init(CurrentCharacter);
                _currentScope.Facing = "";
                for (int k = 0; k < logicScriptable.EachRandomPosesTimes; k++)
                {
                    PoseRandomization.PoseReset();
                    PoseRandomization.ChangePose();
                    foreach (var collisionHelper in _collisionHelpers)
                    {
                        collisionHelper.UpdateCollisionMesh();
                    }
                    Utils.ApplyBarycentricCoordinates(CurrentCharacter);
                    _currentScope.Pose = "RandmoPose" + k.ToString();

                    var data = Utils.CaculateLandmarkModuel(_currentScope, CurrentCharacter);
                    Utils.WriteData(_currentScope.InfoSavePath, data);
                    yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);

                    yield return new WaitForEndOfFrame();
                    ScreenCapture.CaptureScreenshot(_currentScope.InfoSaveDirctory + "/" + data.ImagePath);
                }
                _collisionHelpers.Clear();
                Destroy(CurrentCharacter);
            }
        }

        private IEnumerator PosesLogic(GameObject character)
        {

            var animations = character.GetComponent<CharacterModule>().AnimationClips;
            animations = animations.GetRange(0, logicScriptable.EachFixedPosesTimes);
            //fixed animation
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].SampleAnimation(character, 0);
                foreach (var collisionHelper in _collisionHelpers)
                {
                    collisionHelper.UpdateCollisionMesh();
                }
                Utils.ApplyBarycentricCoordinates(character);
                _currentScope.Pose = animations[i].name;
                var data = Utils.CaculateLandmarkModuel(_currentScope, character);
                Utils.WriteData(_currentScope.InfoSavePath, data);
                yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);

                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(_currentScope.InfoSaveDirctory + "/" + data.ImagePath);
            }

        }
    }
}
