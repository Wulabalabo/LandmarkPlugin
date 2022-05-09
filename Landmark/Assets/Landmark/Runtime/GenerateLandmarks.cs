using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;


namespace Landmark
{
    public class GenerateLandmarks : MonoBehaviour
    {
        public GameObject Character, Point;
        public SerializableDictionary<string, GameObject> ModelBoneDataDictionary;
        public List<LandmarkPoint> Landmarks=new List<LandmarkPoint>();
        public List<AnimationClip> AnimationClips=new List<AnimationClip>();

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
                    Landmarks.Add(new LandmarkPoint(landmark,landmark.transform.position));
                }
            }
        }

        public void SaveLandmarksPosition()
        {
            var annotations=new JArray();
            foreach (var landmarkPoint in Landmarks)
            {
                annotations.Add(new JArray(landmarkPoint.Pos.x, landmarkPoint.Pos.y, landmarkPoint.Pos.z));
            }
            Utils.ModifyConfigFile(Character.name, "annotation",annotations);
        }

        public void ImportLandmarksPosition()
        {
            var info = Utils.GetJPropertyByFile(Character.name, "annotation");
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
                    Landmarks[i].Object.transform.localPosition = pos;
                    Landmarks[i].Pos = pos;
                }
                return;
            }

            throw new UnityException();
        }

        public void ImportCharacterAnimations()
        {
            if (Character == null)
            {
                throw new NullReferenceException();
            }
            AnimationClips.Clear();
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Character);

            var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            foreach (var assetRepresentation in assetRepresentationsAtPath)
            {
                var animationClip = assetRepresentation as AnimationClip;

                if (animationClip != null)
                {
                    AnimationClips.Add(animationClip);
                }
            }
        }

        public void ClearLandmarks()
        {
            foreach (var landmark in Landmarks)
            {
                GameObject.DestroyImmediate(landmark.Object);
            }

            Landmarks.Clear();
        }

    }
}

