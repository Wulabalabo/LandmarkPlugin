using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance { get { return _instance; } }

    public List<GameObject> Characters= new List<GameObject>();
    public UiManager uiManager;

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
        var prefabs = Resources.LoadAll("Prefabs");
        Characters = prefabs.Select((item) =>
        {
            return item as GameObject;
        }).ToList();
        CurrentCharacter=Instantiate(Characters[0]);
        uiManager.InitOption();
    }


    internal void ChangeCharacter(string name)
    {
        DestroyImmediate(CurrentCharacter);
        CurrentCharacter = Instantiate(Characters.Where((item) =>  item.name==name).First());
    }

    internal void DoLogic()
    {
        throw new NotImplementedException();
    }
}
