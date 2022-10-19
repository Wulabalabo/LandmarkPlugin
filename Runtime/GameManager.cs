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
        private string _infoSavePath;
        public static GameManager instance { get { return _instance; } }

        public List<GameObject> Characters = new List<GameObject>();
        public SkinnedCollisionHelper SkinnedCollisionHelper { get; private set; }
        public UiManager uiManager;
        public LogicScriptable logicScriptable;

        public GameObject CurrentCharacter { get; private set; }

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
            SkinnedCollisionHelper = new SkinnedCollisionHelper();
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
            StartCoroutine(SceneChangeLogic());
        }

        bool ChangeSceneLogic()
        {
            return false;
        }

        IEnumerator SceneChangeLogic()
        {
            foreach (var item in logicScriptable.ScenesLogic)
            {
                //yield return new WaitUntil(() => ChangeSceneLogic());
                yield return StartCoroutine(PositionChangeLogic(item.Value));
            }
            Debug.Log("Done");
            yield break;
        }

        IEnumerator PositionChangeLogic(GameObject gameObject)
        {
            var objs = GetComponentsInChildren<Transform>(gameObject);
            foreach (var obj in objs)
            {
                yield return StartCoroutine(CharactorLogic(obj));
            }
            yield break;
        }

        IEnumerator CharactorLogic(Transform pos)
        {
            
            for (int i = 0; i < 1; i++)
            {
                CurrentCharacter = Instantiate(Characters[i], pos.position, Quaternion.identity);
                SkinnedCollisionHelper.Init(CurrentCharacter.transform.Find("CC_Game_Body").gameObject);
                _infoSavePath = GlobalConfig.LandmarkInfoSavePath + "/" + CurrentCharacter.name + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
                Utils.CreateDirctory(_infoSavePath);
                for (int j = 0; j < logicScriptable.Facings.Length; j++)
                {
                    CurrentCharacter.transform.localRotation = Quaternion.Euler(Vector3.up * logicScriptable.Facings[j]);
                    Utils.AutoCameraPositioning(CurrentCharacter,pos);
                    yield return StartCoroutine(PosesLogic(CurrentCharacter));
                }
                Destroy(CurrentCharacter);

            }


            yield break;
        }

        private IEnumerator PosesLogic(GameObject character)
        {
            var dataPath = _infoSavePath + "/data.json";
            var animations = character.GetComponent<Characters>().AnimationClips;
            animations = animations.GetRange(0, logicScriptable.EachFixedPosesTimes);
            //fixed animation
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].SampleAnimation(character, 0);
                SkinnedCollisionHelper.UpdateCollisionMesh();

                var data = Utils.CaculateLandmarkModuel("imagePath",character);
                Utils.WriteData(dataPath, data);
                yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);
            }
            //random animation
            PoseRandomization.Init(CurrentCharacter);
            for (int i = 0; i < logicScriptable.EachRandomPosesTimes; i++)
            {
                PoseRandomization.ChangePose();
                SkinnedCollisionHelper.UpdateCollisionMesh();
                var data = Utils.CaculateLandmarkModuel("imagePath",character);
                Utils.WriteData(dataPath, data);
                yield return new WaitForSeconds(logicScriptable.EachAnimationDuration);
            }
            PoseRandomization.PoseReset();
            
        }
    }
}

