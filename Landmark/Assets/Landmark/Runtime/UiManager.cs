using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Landmark
{
    public class UiManager : MonoBehaviour
    {
        public Dropdown CharacterOption;
        public Dropdown SceneOption;
        public Dropdown SpawnOption;
        public Dropdown AnimationOption;
        public GameObject DebugPanel;
        public InputField VisibilityInputField;
        public Text VisibilityResult;
        public Button StartBtn;
        public Button RandomPosBtn;
        public Button FacingBtn;
        public Toggle DebugMode;

        private List<int> caculateList = new List<int>();
        public void InitOption()
        {
            CharacterOptionInitial();
            SceneOptionInitial();
            AnimationOptionInitial();
            SpawnOptionInitial(GameManager.instance.DebugManager.CurrentLogic.ScenesLogic.ElementAt(0).Key);

            DebugMode.isOn = false;
            DebugPanel.SetActive(false);

            VisibilityInputField.onValueChanged.AddListener((value) =>
            {
                CaculateVisibilities(value);
            });

            DebugMode.onValueChanged.AddListener((value) =>
            {
                DebugPanel.SetActive(value);
            });

            RandomPosBtn.onClick.AddListener(() =>
            {
                GameManager.instance.DebugManager.PlayRandomPos();
                CaculateVisibilities(VisibilityInputField.text);
            });

            FacingBtn.onClick.AddListener(() =>
            {
                GameManager.instance.DebugManager.FacingChange();
                CaculateVisibilities(VisibilityInputField.text);
            });

            StartBtn.onClick.AddListener(() =>
            GameManager.instance.DoLogic());
        }

        private void CaculateVisibilities(string value)
        {
            StartCoroutine(ICaculateVisibilities(value));
        }

        private IEnumerator ICaculateVisibilities(string value)
        {
            caculateList.Clear();
            var input = value.Split(',');
            string result = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].Contains("-"))
                {
                    var item = input[i].Split('-');
                    var caculateditem = Array.ConvertAll(item, s => int.TryParse(s, out int i) ? i : 0);
                    if (caculateditem.Length != 2 || caculateditem[0] > caculateditem[1])
                    {
                        result = "Something wrong about Input!";
                        goto Result;
                    }
                    var lerp = caculateditem[1] - caculateditem[0];
                    for (int k = 0; k < lerp + 1; k++)
                    {
                        caculateList.Add(caculateditem[0] + k);
                    }
                }
                if (int.TryParse(input[i], out int a))
                {
                    caculateList.Add(int.Parse(input[i]));
                }
            }

            yield return new WaitForFixedUpdate();
            var allInfos = Utils.GetLandmarkInfos(GameManager.instance.DebugManager.CurrentCharacter);
            for (int i = 0; i < allInfos.Count; i++)
            {
                if (caculateList.Contains(i))
                {
                    result += i + ":" + allInfos[i].visibility.ToString() + "\n";
                }
            }
        Result:
            VisibilityResult.text = result;

        }

        private void CharacterOptionInitial()
        {
            var listOptions = new List<Dropdown.OptionData>();

            for (int i = 0; i < GameManager.instance.Characters.Count; i++)
            {
                listOptions.Add(new Dropdown.OptionData(GameManager.instance.Characters[i].name));
            }
            CharacterOption.AddOptions(listOptions);
            CharacterOption.onValueChanged.AddListener((index) =>
            {
                GameManager.instance.DebugManager.GenerateCharacter(GameManager.instance.DebugManager.Characters[index].name);
                AnimationOptionInitial();
            });

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
            );
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
                SpawnOption.onValueChanged.AddListener((index) =>
                {
                    GameManager.instance.DebugManager.CurrentCharacter.transform.position = GameManager.instance.DebugManager.CurrentSpawnPoints[index].transform.position;
                    GameManager.instance.DebugManager.CurrentCharacter.transform.rotation = GameManager.instance.DebugManager.CurrentSpawnPoints[index].transform.rotation;
                    Utils.AutoCameraPositioning(GameManager.instance.DebugManager.CurrentCharacter, GameManager.instance.DebugManager.CurrentSpawnPoints[index].transform);
                });
            }

        }

        private void AnimationOptionInitial()
        {
            AnimationOption.ClearOptions();
            var listOptions = new List<Dropdown.OptionData>();
            var animations = GameManager.instance.DebugManager.CurrentCharacter.GetComponent<CharacterModule>().AnimationClips;
            foreach (var item in animations)
            {
                listOptions.Add(new Dropdown.OptionData(item.name));
            }
            AnimationOption.AddOptions(listOptions);
            AnimationOption.onValueChanged.AddListener((index) =>
                {
                    GameManager.instance.DebugManager.PlayAnimationClip(animations[index]);
                    CaculateVisibilities(VisibilityInputField.text);
                }
            );
        }

        public void Display(bool display)
        {
            DebugMode.gameObject.SetActive(display);
            DebugPanel.SetActive(display);
            StartBtn.gameObject.SetActive(display);
        }


    }
}
