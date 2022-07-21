using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;


namespace Landmark
{
    public class LandmarkManager : MonoBehaviour
    {
        [SerializeField]
        public SerializableDictionary<string, GameObject> ModelBoneDataDictionary;
        public List<AnimationClip> AnimationClips = new List<AnimationClip>();
        public List<GameObject> Landmarks = new List<GameObject>();

        public void InitModelBoneData(GameObject obj)
        {           
            ModelBoneDataDictionary.Clear();
            ModelBoneDataDictionary = obj.transform.Find("root").gameObject.CollectModelBoneData();
        }
        public void GenerateLandmarksForCharacter(GameObject obj, GameObject point, float scale)
        {
            ClearLandmarks();
            point.transform.localScale = new Vector3(scale, scale, scale);
            var info = Utils.GetJPropertyByFile(obj.name, "definition");
            foreach (var jToken in info)
            {
                var jProperty = (JProperty)jToken;
                foreach (int token in jProperty.Value)
                {
                    var landmark = GameObject.Instantiate(point, ModelBoneDataDictionary[jProperty.Name].transform);
                    landmark.name = $"Landmark{token}";
                    Landmarks.Add(landmark);
                }
            }
            Landmarks = Landmarks.OrderBy((item) => int.Parse(item.gameObject.name.Remove(0, 8))).ToList();
        }

        public void SaveLandmarksPosition(GameObject obj)
        {
            var annotations = new JArray();
            foreach (var landmarkPoint in Landmarks)
            {
                annotations.Add(new JArray(landmarkPoint.transform.localPosition.x, landmarkPoint.transform.localPosition.y, landmarkPoint.transform.localPosition.z));
            }
            Utils.ModifyConfigFile(obj.name, "annotation", annotations);
        }

        public void ImportLandmarksPosition(GameObject obj)
        {
            var info = Utils.GetJPropertyByFile(obj.name, "annotation");
            var count = info.Count();
            if (Landmarks.Count == count)
            {
                for (int i = 0; i < count; i++)
                {
                    var pos = new Vector3
                    {
                        x = (float)info[i][0],
                        y = (float)info[i][1],
                        z = (float)info[i][2],

                    };
                    Landmarks[i].transform.localPosition = pos;
                }
                return;
            }

            throw new UnityException();
        }

        public void ImportCharacterAnimations(GameObject obj)
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            AnimationClips.Clear();
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
            if (path.Contains(".prefab"))
            {
                var tempObject = obj.GetComponentInChildren<SkinnedMeshRenderer>();
                path = AssetDatabase.GetAssetPath(tempObject.sharedMesh);
            }
            var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Replace(obj.name + ".Fbx", "Animations");
                path = path.Replace(obj.name + ".fbx", "Animations");
            }
            Utils.CreateDirctory(path);
            var folder = new DirectoryInfo(path).GetFiles("*.anim");
            if(folder.Length > 0)
            {
                foreach (var item in folder)
                {
                    AnimationClips.Add(AssetDatabase.LoadAssetAtPath<AnimationClip>(path + "/" + item.Name));
                }
            }
            else
            {
                foreach (var assetRepresentation in assetRepresentationsAtPath)
                {
                    var animationClip = assetRepresentation as AnimationClip;
                    var new_animation_clip = new AnimationClip();
                    if (animationClip != null)
                    {
                        EditorUtility.CopySerialized(animationClip, new_animation_clip);
                        AssetDatabase.CreateAsset(new_animation_clip, path + "/" + new_animation_clip.name + ".anim");
                        AnimationClips.Add(animationClip);
                    }
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

