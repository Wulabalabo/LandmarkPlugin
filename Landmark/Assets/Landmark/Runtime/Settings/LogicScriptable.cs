using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName ="LogicScriptable")]
public class LogicScriptable : ScriptableObject
{
    public SerializableDictionary<string, GameObject> ScenesLogic = new SerializableDictionary<string, GameObject>();

    public int[] Facings = { 0, 90, 270 };

    public int EachFixedPosesTimes = 6;

    public int EachRandomPosesTimes = 6;

    public float EachAnimationDuration = 0.5f;

    public string OutputDirctory;


}
