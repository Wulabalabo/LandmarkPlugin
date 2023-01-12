using Landmark;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

public class PoseRandomRange
{
    public string Axis;
    public float Min;
    public float Max;
}
public class PoseRandomization
{
    static Dictionary<Transform, List<PoseRandomRange>> RandomKeyValue = new Dictionary<Transform, List<PoseRandomRange>>();
    static Dictionary<Transform, Vector3> initLocalEulerKeyValue = new Dictionary<Transform, Vector3>();

    public static void Init(GameObject character)
    {
        RandomKeyValue.Clear();
        initLocalEulerKeyValue.Clear();
        var path = GlobalConfig.PoseRadomizationConfigPath + "PoseRandomization/" + character.name + ".json";
        JObject data;

        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            data = JObject.Parse(sr.ReadToEnd());
            sr.Close();
        }
        else
        {
            StreamReader sr = new StreamReader(GlobalConfig.DefaultRandomization);
            data = JObject.Parse(sr.ReadToEnd());
            sr.Close();
        }           
        
        var roots = character.transform.Find("root").gameObject.GetComponentsInChildren<Transform>();

        foreach (var pair in data)
        {
            var tempPoseRangeList = new List<PoseRandomRange>();
            foreach (var item in JObject.Parse(pair.Value.ToString()))
            {
                tempPoseRangeList.Add(new PoseRandomRange()
                {
                    Axis = item.Key,
                    Min = (float)item.Value[0],
                    Max = (float)item.Value[1]
                });
            }

            foreach (var item in roots)
            {
                if (item.name == pair.Key.ToString())
                {
                    initLocalEulerKeyValue.Add(item, item.localEulerAngles);
                    RandomKeyValue.Add(item, tempPoseRangeList);
                }
            }
        }
        
    }


    public static void ChangePose()
    {
        if (RandomKeyValue.Count < 0)
        {
            Debug.LogError("Need Init First!");
            return;
        }
        foreach (var item in RandomKeyValue)
        {
            Vector3 localEuler = item.Key.localEulerAngles;
            foreach (var range in item.Value)
            {
                float min = Mathf.Min(range.Min, range.Max);
                float max = Mathf.Max(range.Min, range.Max);
                float angle = Random.Range(min, max);
                switch (range.Axis)
                {
                    case "x":
                        item.Key.localEulerAngles = new Vector3(localEuler.x += angle, localEuler.y, localEuler.z);
                        break;
                    case "y":
                        item.Key.localEulerAngles = new Vector3(localEuler.x, localEuler.y += angle, localEuler.z);
                        break;
                    case "z":
                        item.Key.localEulerAngles = new Vector3(localEuler.x, localEuler.y, localEuler.z += angle);
                        break;
                }
            }
        }
    }

    public static void PoseReset()
    {
        foreach(var item in initLocalEulerKeyValue)
        {
            item.Key.localEulerAngles = item.Value;
        }
    }
}
