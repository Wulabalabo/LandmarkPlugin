using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Landmark
{
    public class UiManager : MonoBehaviour
    {
        public Dropdown Option;
        public Button StartBtn;
        public void InitOption()
        {
            DontDestroyOnLoad(this);
            var listOptions = new List<Dropdown.OptionData>();

            for (int i = 0; i < GameManager.instance.Characters.Count; i++)
            {
                listOptions.Add(new Dropdown.OptionData(GameManager.instance.Characters[i].name));
            }
            Option.AddOptions(listOptions);
            Option.onValueChanged.AddListener((index) => GameManager.instance.ChangeCharacter(GameManager.instance.Characters[index].name));

            StartBtn.onClick.AddListener(() =>
            GameManager.instance.DoLogic());
        }


    }
}

