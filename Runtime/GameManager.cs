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
        private string _infoSaveDirctory;
        private string _infoSavePath;
        private string _infoSaveTime;
        private ScopeInfo _currentScope;
        private List<SkinnedCollisionHelper> _collisionHelpers=new List<SkinnedCollisionHelper>();
        public static GameManager instance { get { return _instance; } }

        public List<GameObject> Characters = new List<GameObject>();
        public SkinnedCollisionHelper SkinnedCollisionHelper { get; private set; }
        public UiManager uiManager;
        public LogicScriptable logicScriptable;

        public GameObject CurrentCharacter;

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
            Utils.CreateDirctory(GlobalConfig.LandmarkInfoSavePath);
            var prefabs = Resources.LoadAll("Prefabs");
            Characters = prefabs.Select((item) =>
            {
                return item as GameObject;
            }).ToList();
            uiManager.InitOption();
        }


        internal void ChangeCharacter(string name)
        {
            DestroyImmediate(CurrentCharacter);
            CurrentCharacter = Instantiate(Characters.Where((item) => item.name == name).First());
        }

        internal void DoLogic()
        {
            Debug.Log("Start");
            uiManager.Display(false);
            StartCoroutine(SceneChangeLogic());
        }

        bool ChangeSceneLogic()
        {
            return false;
        }

        IEnumerator SceneChangeLogic()
        {
            _currentScope = new ScopeInfo();
            foreach (var item in logicScriptable.ScenesLogic)
            {
                _currentScope.SceneId = 0;
                //yield return new WaitUntil(() => ChangeSceneLogic());
                yield return StartCoroutine(PositionChangeLogic(item.Value));
            }
            Debug.Log("Done");
            uiManager.Display(true);
            yield break;
        }

        IEnumerator PositionChangeLogic(GameObject gameObject)
        {
            var objs =gameObject.GetComponentsInChildren<Transform>();
            foreach (var obj in objs)
            {
                _currentScope.SpawnpointName = obj.name;
                yield return StartCoroutine(CharactorLogic(obj));
            }
            yield break;
        }

        IEnumerator CharactorLogic(Transform pos)
        {
            //for (int i = 0; i < Characters.Count; i++)
            //{
                CurrentCharacter = Instantiate(Characters[0], pos.position, Quaternion.identity);
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

                #region generate save info
                _infoSaveTime = DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
                _infoSaveDirctory = GlobalConfig.LandmarkInfoSavePath + "/" + _infoSaveTime + CurrentCharacter.name ;
                _infoSavePath = GlobalConfig.LandmarkInfoSavePath + "/" + _infoSaveTime + CurrentCharacter.name + ".csv";
                Utils.CreateDirctory(_infoSaveDirctory);
                Utils.WriteTitle(_infoSavePath);
                #endregion


                for (int j = 0; j < logicScriptable.Facings.Length; j++)
                {
                    _currentScope.Facing = logicScriptable.Facings[j].ToString();
                    CurrentCharacter.transform.localRotation = Quaternion.Euler(Vector3.up * logicScriptable.Facings[j]);
                    Utils.AutoCameraPositioning(CurrentCharacter,pos);
                    yield return StartCoroutine(PosesLogic(CurrentCharacter));
                }
                //random animation
                PoseRandomization.Init(CurrentCharacter);
                _currentScope.Facing = "";
                for (int k = 0; k < logicScriptable.EachRandomPosesTimes; k++)
                {                    
                    PoseRandomization.ChangePose();
                    foreach (var collisionHelper in _collisionHelpers)
                    {
                        collisionHelper.UpdateCollisionMesh();
                    }
                    Utils.ApplyBarycentricCoordinates(CurrentCharacter);
                    _currentScope.Pose = "RandmoPose" + k.ToString();
                   
                    var data = Utils.CaculateLandmarkModuel(_infoSaveDirctory,_currentScope,CurrentCharacter);
                    Utils.WriteData(_infoSavePath,data);
                    yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);
                }
                PoseRandomization.PoseReset();
                _collisionHelpers.Clear();
                Destroy(CurrentCharacter);
                CurrentCharacter = null;
            //}
        }

        private IEnumerator PosesLogic(GameObject character)
        {
            
            var animations = character.GetComponent<Characters>().AnimationClips;
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
                var data = Utils.CaculateLandmarkModuel(_infoSaveDirctory, _currentScope,character);
                Utils.WriteData(_infoSavePath,data);
                yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);
            }   
            
        }
    }
}

