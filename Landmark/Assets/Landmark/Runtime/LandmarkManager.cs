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
        List<BarycentricCoodinates> barycentricCoodinates = new List<BarycentricCoodinates>();

        public void InitCharacter(GameObject obj)
        {
            var info = Utils.GetJPropertyByFile(obj.name, "definition");
            if (info == null)
            {
                Debug.LogError($"Can Not Find {obj.name} Config file");
                return;
            }
            var LandmarkRoot = info.Select(x =>
            {
                var temp = (JProperty)x;
                return temp.Name;
            }).ToList();

            foreach (var item in obj.GetComponentsInChildren<Transform>())
            {                
                if (item.gameObject.name.Contains("Landmark"))
                {
                    item.gameObject.tag = "Landmark";
                    Landmarks.Add(item.gameObject);
                }
                else
                {
                    item.gameObject.layer = LayerMask.NameToLayer("Character");
                }
            }
            if (Directory.Exists(GlobalConfig.CharactersModelsPath))
            {
                SavePrefab(obj);
            }
            else
            {
                Debug.LogError($"{GlobalConfig.CharactersModelsPath} Not Exists!");
                return;
            }
            
            
        }

        public void SavePrefab(GameObject obj)
        {
            UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(obj, $"{GlobalConfig.CharactersModelsPath}/{obj.name}.prefab", UnityEditor.InteractionMode.UserAction);
        }

        public void InitModelBoneData(GameObject obj)
        {           
            ModelBoneDataDictionary.Clear();
            ModelBoneDataDictionary = obj.transform.Find("root").gameObject.CollectModelBoneData();
        }
        public void GenerateLandmarksForCharacter(GameObject obj, GameObject point, float scale)
        {
            ClearLandmarks(obj);
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

        void UpdateLandmarks(GameObject obj)
        {
            Landmarks.Clear();
            foreach (Transform landmarkTransform in obj.GetComponentInChildren<Transform>())
            {
                if (landmarkTransform.CompareTag("Landmark"))
                {
                    Landmarks.Add(landmarkTransform.gameObject);
                }
            }
            Landmarks = Landmarks.OrderBy((item) => int.Parse(item.gameObject.name.Substring(8))).ToList();
        }

        public void ClearLandmarks(GameObject obj)
        {
            UnityEditor.PrefabUtility.UnpackPrefabInstance(obj,PrefabUnpackMode.OutermostRoot,InteractionMode.UserAction);
            foreach (var landmark in Landmarks)
            {
                GameObject.DestroyImmediate(landmark);
            }

            Landmarks.Clear();
            SavePrefab(obj);
        }


        public void GenerateSkinLandmarks(GameObject obj, string field)
        {
            UpdateLandmarks(obj);

            var info = Utils.GetJPropertyByFile(obj.name, field);
            foreach (JProperty lm in info)
            {
                int jointId = int.Parse(lm.Name);
                Transform jointLandmark = Landmarks[jointId].transform;
                Transform bodyPart = jointLandmark.parent;

                float distance = 0.5f;
                Vector3 dir = Vector3.zero;
                foreach (JProperty x in lm.Value)
                {
                    int skinId = int.Parse(x.Value.ToString());
                    Transform landmark = Landmarks[skinId].transform;
                    switch (x.Name)
                    {
                        case "+x":
                            dir = bodyPart.right * distance;
                            break;
                        case "-x":
                            dir = -bodyPart.right * distance;
                            break;
                        case "+y":
                            dir = bodyPart.up * distance;
                            break;
                        case "-y":
                            dir = -bodyPart.up * distance;
                            break;
                        case "+z":
                            dir = bodyPart.forward * distance;
                            break;
                        case "-z":
                            dir = -bodyPart.forward * distance;
                            break;
                    }
                    RaycastHit hit;
                    int layerMask = 0;
                    layerMask |= (1 << LayerMask.NameToLayer("Character"));

                    Vector3 start = landmark.position + dir;
                    landmark.position = start;

                    if (Physics.Raycast(start, -dir, out hit, 10, layerMask))
                    {
                        if (hit.collider.name == "CC_Game_Body")
                        {
                            landmark.parent = bodyPart;
                            landmark.position = hit.point;
                            Insert2Barycentric(obj, skinId, hit.triangleIndex, hit.barycentricCoordinate);
                        }
                    }
                }
            }
        }

        public void ConfirmBarycentricChange(GameObject obj,List<BarycentricCoodinates> barycentricCoodinates)
        {
            string fieldname = "barycentricCoordinates";
            string coordname = "coordinate";
            string triangleIdname = "triangleId";

            JObject bary = new JObject();
            foreach (var kvp in barycentricCoodinates)
            {
                JArray coord = new JArray();
                coord.Add(kvp.Coordinate.x);
                coord.Add(kvp.Coordinate.y);
                coord.Add(kvp.Coordinate.z);

                JObject record = new JObject();
                record[triangleIdname] = kvp.TriangleId;
                record[coordname] = coord;

                bary[kvp.Index] = record;
            }

            Utils.ModifyConfigFile(obj.name, fieldname, bary);
        }


        public List<BarycentricCoodinates> Insert2Barycentric(GameObject obj, int landmarkId, int triangleIndex, Vector3 barycentricCoordinate)
        {
            string fieldname = "barycentricCoordinates";
            string coordname = "coordinate";
            string triangleIdname = "triangleId";

            SortedList<int, (int, Vector3)> id2bary = ReadBarycentric(obj);

            if (id2bary.ContainsKey(landmarkId))
            {
                id2bary[landmarkId] = (triangleIndex, barycentricCoordinate);
            }
            else
            {
                id2bary.Add(landmarkId, (triangleIndex, barycentricCoordinate));
            }



            JObject bary = new JObject();
            foreach (KeyValuePair<int, (int, Vector3)> kvp in id2bary)
            {
                barycentricCoodinates.Add(new BarycentricCoodinates
                {
                    Index=kvp.Key.ToString(),
                    TriangleId=kvp.Value.Item1,
                    Coordinate=kvp.Value.Item2
                });
                JArray coord = new JArray();
                coord.Add(kvp.Value.Item2.x);
                coord.Add(kvp.Value.Item2.y);
                coord.Add(kvp.Value.Item2.z);

                JObject record = new JObject();
                record[triangleIdname] = kvp.Value.Item1;
                record[coordname] = coord;

                bary[kvp.Key.ToString()] = record;
            }
            
            Utils.ModifyConfigFile(obj.name, fieldname, bary);
            return barycentricCoodinates;
        }


        public SortedList<int, (int, Vector3)> ReadBarycentric(GameObject obj)
        {
            SortedList<int, (int, Vector3)> id2bary = new SortedList<int, (int, Vector3)>();
            string fieldname = "barycentricCoordinates";
            string coordname = "coordinate";
            string triangleIdname = "triangleId";

            JObject bary = (JObject)Utils.GetJPropertyByFile(obj.name, fieldname);
            if (bary!=null)
            {
                foreach (JProperty x in (JToken)bary)
                {
                    int landmarkId = int.Parse(x.Name);
                    if (id2bary.ContainsKey(landmarkId))
                        continue;
                    JObject record = JObject.Parse(x.Value.ToString());
                    JArray coord = JArray.Parse(record[coordname].ToString());
                    Vector3 v = new Vector3(float.Parse(coord[0].ToString()), float.Parse(coord[1].ToString()), float.Parse(coord[2].ToString()));
                    id2bary.Add(landmarkId, (int.Parse(record[triangleIdname].ToString()), v));
                }
            }
            return id2bary;
        }


        public void ResetSkinLandmarks(GameObject obj, string field)
        {
            UpdateLandmarks(obj);

            var info = Utils.GetJPropertyByFile(obj.name, field);
            foreach (JProperty lm in info)
            {
                int jointId = int.Parse(lm.Name);
                Transform jointLandmark = Landmarks[jointId].transform;
                Transform bodyPart = jointLandmark.parent;

                foreach (JProperty x in lm.Value)
                {
                    int skinId = int.Parse(x.Value.ToString());
                    Landmarks[skinId].transform.localPosition = Vector3.zero;
                }
            }
        }
    }
}

