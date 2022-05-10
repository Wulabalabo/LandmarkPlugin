using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace Landmark
{
    public static class Utils
    {
        public static SerializableDictionary<string, GameObject> CollectModelBoneData(this GameObject currentTf)
        {
            var modelBoneData = new SerializableDictionary<string, GameObject>();
            foreach (var componentsInChild in currentTf.GetComponentsInChildren<Transform>())
            {
                modelBoneData.Add(componentsInChild.gameObject.name, componentsInChild.gameObject);
            }

            return modelBoneData;
        }

        public static JToken GetJPropertyByFile(string fileName, string type)
        {
            var path = GlobalConfig.LandmarkConfigPath + "/" + fileName + "_landmarks.json";
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                var data = JObject.Parse(sr.ReadToEnd());
                sr.Close();
                return data[type];
            }

            throw new FileNotFoundException();
        }

        public static void ModifyConfigFile(string fileName, string type, JToken obj)
        {
            var path = GlobalConfig.LandmarkConfigPath + "/" + fileName + "_landmarks.json";
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                var data = JObject.Parse(sr.ReadToEnd());
                data[type] = obj;
                sr.Close();
                File.WriteAllText(path, data.ToString());
                return;
            }

            throw new FileNotFoundException();
        }

        public static void SaveTexture2DLocally(Texture2D texture2D, string textName,string path)
        {
            byte[] bytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(path+"/"+textName+".png",bytes);
        }
    }

}


