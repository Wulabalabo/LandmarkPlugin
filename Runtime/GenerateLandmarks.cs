using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;


namespace Landmark
{
    public class GenerateLandmarks : MonoBehaviour
    {
        public GameObject Character, Point;
        public SerializableDictionary<string, GameObject> ModelBoneDataDictionary;
        public List<GameObject> Landmarks = new List<GameObject>();

        public void InitModelBoneData()
        {
            ModelBoneDataDictionary.Clear();
            ModelBoneDataDictionary = Character.CollectModelBoneData();
        }
        public void GenerateLandmarksForCharacter()
        {
            ClearLandmarks();
            var info = Utils.GetJPropertyByFile(Character.name, "definition");
            foreach (var jToken in info)
            {
                var jProperty = (JProperty)jToken;
                foreach (int token in jProperty.Value)
                {
                    var landmark = Instantiate(Point, ModelBoneDataDictionary[jProperty.Name].transform);
                    landmark.name = $"Landmark{token}";
                    Landmarks.Add(landmark);
                }
            }
        }

        public void ClearLandmarks()
        {
            foreach (var landmark in Landmarks)
            {
                GameObject.DestroyImmediate(landmark);
            }

            Landmarks.Clear();
        }

    }
}

