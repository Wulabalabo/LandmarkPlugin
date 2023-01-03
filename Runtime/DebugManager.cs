using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Landmark {
    public class DebugManager : MonoBehaviour
    {
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
            GenerateCharacter("Apple_Bianca");
            
        }

        public void GenerateCharacter(string name)
        {
            DestroyImmediate(CurrentCharacter);
            CurrentCharacter = Instantiate(Characters.Where((item) => item.name == name).First(), parent: GameManager.instance.DebugModePos);
            foreach (Transform transform in CurrentCharacter.GetComponentsInChildren<Transform>())
            {
                if (transform.CompareTag("CollisionMesh"))
                {
                    SkinnedCollisionHelper helper = new SkinnedCollisionHelper();
                    if (transform.name.Equals("CC_Game_Body"))
                    {
                        GameManager.instance.SkinnedCollisionHelper = helper;
                        Debug.Log("Set");
                    }
                    helper.Init(transform.gameObject);
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

        public IEnumerator QuitDebugMode()
        {
            DestroyImmediate(CurrentCharacter);
            yield return null;
        }
    }
}