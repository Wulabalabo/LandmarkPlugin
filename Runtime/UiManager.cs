using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Landmark
{
    public class UiManager : MonoBehaviour
    {
        public Dropdown CharacterOption;
        public Dropdown SceneOption;
        public Dropdown SpawnOption;
        public GameObject DebugPanel;
        public Button StartBtn;
        public Toggle DebugMode;
        public void InitOption()
        {
            CharacterOptionInitial();
            SceneOptionInitial();


            DebugMode.isOn = false;
            DebugPanel.SetActive(false);

            DebugMode.onValueChanged.AddListener((value) =>
            {
                DebugPanel.SetActive(value);
            });
            StartBtn.onClick.AddListener(() =>
            GameManager.instance.DoLogic());
        }

        private void CharacterOptionInitial()
        {
            var listOptions = new List<Dropdown.OptionData>();

            for (int i = 0; i < GameManager.instance.Characters.Count; i++)
            {
                listOptions.Add(new Dropdown.OptionData(GameManager.instance.Characters[i].name));
            }
            CharacterOption.AddOptions(listOptions);
            CharacterOption.onValueChanged.AddListener((index) => GameManager.instance.DebugManager.GenerateCharacter(GameManager.instance.DebugManager.Characters[index].name));
        }

        private void SceneOptionInitial()
        {
            var listOptions = new List<Dropdown.OptionData>();

            foreach (var item in GameManager.instance.logicScriptable.ScenesLogic)
            {
                listOptions.Add(new Dropdown.OptionData(item.Key));
            }
            SceneOption.AddOptions(listOptions);
            SceneOption.onValueChanged.AddListener((index) =>
            {

                GameManager.instance.DebugManager.ChangeScene(SceneOption.options[index].text);
                SpawnOptionInitial(SceneOption.options[index].text);
            }
            ) ;            
        }

        private void SpawnOptionInitial(string key)
        {
            SpawnOption.ClearOptions();
            var listOptions = new List<Dropdown.OptionData>();
            if (GameManager.instance.DebugManager.CurrentLogic.ScenesLogic.ContainsKey(key))
            {
                GameManager.instance.DebugManager.GenerateSpawnPoints(key);
                foreach (var item in GameManager.instance.DebugManager.CurrentSpawnPoints)
                {
                    listOptions.Add(new Dropdown.OptionData(item.name));
                }
                SpawnOption.AddOptions(listOptions);
                SpawnOption.onValueChanged.AddListener((index) => {
                    GameManager.instance.DebugManager.CurrentCharacter.transform.position = GameManager.instance.DebugManager.CurrentSpawnPoints[index].transform.position;
                    Utils.AutoCameraPositioning(GameManager.instance.DebugManager.CurrentCharacter, GameManager.instance.DebugManager.CurrentSpawnPoints[index].transform); });
            }
            
        }

        public void Display(bool display)
        {
            DebugPanel.SetActive(display);
            StartBtn.gameObject.SetActive(display);
        }


    }
}

