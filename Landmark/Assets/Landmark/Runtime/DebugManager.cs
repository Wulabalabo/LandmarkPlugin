using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Landmark {
    public class DebugManager : MonoBehaviour
    {
        public string DefaultCharacterName;
        public GameObject CurrentCharacter;
        public LogicScriptable CurrentLogic;
        public List<GameObject> Characters = new List<GameObject>();
        public string CurrentScene;
        public List<Transform> CurrentSpawnPoints = new List<Transform>();
        private List<SkinnedCollisionHelper> _collisionHelpers = new List<SkinnedCollisionHelper>();

        internal void Init(List<GameObject> characters)
        {
            Characters = characters;       
            CurrentLogic = GameManager.instance.logicScriptable;
            CurrentScene = CurrentLogic.ScenesLogic.ElementAt(0).Key;
            GenerateSpawnPoints(CurrentScene);
            GenerateCharacter(DefaultCharacterName);            
        }

        public void PlayAnimationClip(AnimationClip clip)
        {
            clip.SampleAnimation(CurrentCharacter, 0);
            foreach (var collisionHelper in _collisionHelpers)
            {
                collisionHelper.UpdateCollisionMesh();
            }
        }

        public void PlayRandomPos()
        {
            PoseRandomization.PoseReset();
            PoseRandomization.ChangePose();
            foreach (var collisionHelper in _collisionHelpers)
            {
                collisionHelper.UpdateCollisionMesh();
            }
        }

        public void FacingChange()
        {
            CurrentCharacter.transform.Rotate(0, 90, 0);
        }

        public void GenerateCharacter(string name)
        {
            DestroyImmediate(CurrentCharacter);
            
            _collisionHelpers.Clear();

            CurrentCharacter = Instantiate(Characters.Where((item) => item.name == name).First(), parent: GameManager.instance.DebugModePos);

            PoseRandomization.Init(CurrentCharacter);

            foreach (Transform transform in CurrentCharacter.GetComponentsInChildren<Transform>())
            {
                if (transform.CompareTag("CollisionMesh"))
                {
                    SkinnedCollisionHelper helper = new SkinnedCollisionHelper();
                    if (transform.name.Equals("CC_Game_Body"))
                    {
                        GameManager.instance.SkinnedCollisionHelper = helper;
                    }
                    helper.Init(transform.gameObject);
                    _collisionHelpers.Add(helper);
                }
            }
        }

        public void GenerateSpawnPoints(string key)
        {            
            var currentSpawnPointObj = CurrentLogic.ScenesLogic[key];
            CurrentSpawnPoints = currentSpawnPointObj.GetComponentsInChildren<Transform>().Where((item) => item.name != currentSpawnPointObj.name).ToList();
        }

        public void ChangeScene(string SceneName)
        {
            GameManager.instance.SceneLogic(SceneName);
            CurrentScene = SceneName;
        }

        public void QuitDebugMode()
        {
            _collisionHelpers.Clear();
            DestroyImmediate(CurrentCharacter);
        }
    }
}