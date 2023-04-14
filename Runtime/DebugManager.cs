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

        internal void Init(List<GameObject> characters)
        {
            Characters = characters;       
            CurrentLogic = GameManager.instance.logicScriptable;
            CurrentScene = CurrentLogic.ScenesLogic.ElementAt(0).Key;
            GenerateSpawnPoints(CurrentScene);
            GenerateCharacter(DefaultCharacterName);
            if (CurrentCharacter != null)
            {
                MoveCharacterToSpawn(0);
            }            
        }

        public void PlayAnimationClip(AnimationClip clip)
        {
            clip.SampleAnimation(CurrentCharacter, 0);
            foreach (var collisionHelper in CurrentCharacter.GetComponent<CharacterModule>().Helpers)
            {
                collisionHelper.UpdateCollisionMesh();
            }
        }

        public void PlayRandomPos()
        {
            PoseRandomization.PoseReset();
            PoseRandomization.ChangePose();
            foreach (var collisionHelper in CurrentCharacter.GetComponent<CharacterModule>().Helpers)
            {
                collisionHelper.UpdateCollisionMesh();
            }
        }

        public void FacingChange()
        {
            CurrentCharacter.transform.Rotate(0, 90, 0);
        }

        public void MoveCharacterToSpawn(int index)
        {
            CurrentCharacter.transform.position = CurrentSpawnPoints[index].transform.position;
            CurrentCharacter.transform.rotation = CurrentSpawnPoints[index].transform.rotation;
            Utils.AutoCameraPositioning(CurrentCharacter, CurrentSpawnPoints[index].transform);
        }

        public void GenerateCharacter(string name)
        {
            if (CurrentCharacter != null)
            {
                DestroyImmediate(CurrentCharacter);
            }

            var pickedName = Characters?.Where((item) => item.name == name)?.FirstOrDefault();
            
            if(pickedName!=null)
            {
                CurrentCharacter = Instantiate(pickedName, parent: GameManager.instance.DebugModePos);
            }

            if (CurrentCharacter != null)
            {
                PoseRandomization.Init(CurrentCharacter);

                foreach (Transform transform in CurrentCharacter.GetComponentsInChildren<Transform>())
                {
                    if (transform.CompareTag("CollisionMesh"))
                    {
                        transform.GetComponent<SkinnedCollisionHelper>().GenerateMesh();
                    }
                }
                Utils.GetLandmarkInfos(CurrentCharacter);
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
            DestroyImmediate(CurrentCharacter);
        }
    }
}