using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace Landmark
{
    public static class Utils
    {
        public static SerializableDictionary<string, GameObject> CollectModelBoneData(this GameObject current)
        {
            var modelBoneData = new SerializableDictionary<string, GameObject>();
            var norepeat = current.GetComponentsInChildren<Transform>().GroupBy(x => x.name)
                .Distinct()
                .Select(x => x.FirstOrDefault().gameObject)
                .ToList();

            foreach (var componentsInChild in norepeat)
            {
                if (!componentsInChild.name.Contains("Landmark"))
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
            else
            {
                if(!Directory.Exists(GlobalConfig.LandmarkConfigPath))
                {
                    Directory.CreateDirectory(GlobalConfig.LandmarkConfigPath);
                }
                Debug.LogWarning("Using DefaultLandmarkConfig");
                var defaultPath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("DefaultLandmark")[0]);
                StreamReader sr = new StreamReader(defaultPath);
                var str = sr.ReadToEnd();
                using (FileStream stream = new FileStream(path,FileMode.OpenOrCreate))
                {
                    var bytes = Encoding.UTF8.GetBytes(str);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
                var data = JObject.Parse(str);
                sr.Close();
                return data[type];
            }

            throw new FileNotFoundException();
        }

        private static void ModifySpecifyConfigType(string path,string type,JToken obj)
        {
            StreamReader sr = new StreamReader(path);
            var data = JObject.Parse(sr.ReadToEnd());
            data[type] = obj;
            sr.Close();
            File.WriteAllText(path, data.ToString());
        }

        public static void CreateDirctory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }            
        }

        public static void ModifyConfigFile(string fileName, string type, JToken obj)
        {
            var path = GlobalConfig.LandmarkConfigPath + "/" + fileName + "_landmarks.json";
            if (File.Exists(path))
            {
                ModifySpecifyConfigType(path, type, obj); 
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


