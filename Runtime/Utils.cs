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

        public static void AutoCameraPositioning(GameObject targetObject,Transform spawnPoint,float magicalDistance=5f,float magicalRatio=0.7f)
        {
            
            Mesh mesh = GameManager.instance.SkinnedCollisionHelper.Mesh;

            var landmarks = targetObject.GetComponent<Characters>().Landmarks;

            if (landmarks.Count <= 0)
            {
                throw new InvalidOperationException("There is no landmark within this ");
            }
            if (mesh == null)
                return;

            // place the main camera in front of the character, magical distance away
            Vector2 min2 = Vector2.positiveInfinity; // 2D x,y min
            Vector2 max2 = Vector2.negativeInfinity; // 2D x,y max
            Vector3 min3 = Vector3.positiveInfinity; // 3D x,y,z min
            Vector3 max3 = Vector3.negativeInfinity; // 3D x,y,z max
                                                     //foreach (Transform landmark in landmarks)
            foreach (Vector3 vertex in mesh.vertices)
            {
                // find bounding box
                //Vector3 q = landmark.position;
                Vector3 q = GameManager.instance.SkinnedCollisionHelper.UpdateGameObject.transform.TransformPoint(vertex);
                min3.x = Mathf.Min(q.x, min3.x);
                min3.y = Mathf.Min(q.y, min3.y);
                min3.z = Mathf.Min(q.z, min3.z);
                max3.x = Mathf.Max(q.x, max3.x);
                max3.y = Mathf.Max(q.y, max3.y);
                max3.z = Mathf.Max(q.z, max3.z);
            }
            var mid = min3 * 0.5f + max3 * 0.5f;
            Camera.main.transform.position = mid + spawnPoint.forward * magicalDistance;
            Camera.main.transform.LookAt(mid, Vector3.up);

            foreach (var landmark in landmarks)
            {
                // find bounding box in screen coordinate
                var p = Camera.main.WorldToScreenPoint(landmark.transform.position);
                min2.x = Mathf.Min(p.x, min2.x);
                min2.y = Mathf.Min(p.y, min2.y);
                max2.x = Mathf.Max(p.x, max2.x);
                max2.y = Mathf.Max(p.y, max2.y);
            }

            // make main camera closer to the character
            Vector2 diff = max2 - min2;
            float ratio = Mathf.Max(diff.x / Screen.width, diff.y / Screen.height);

            Camera.main.transform.position = mid + spawnPoint.forward * ((ratio * magicalDistance / magicalRatio));
        }

        public static List<Transform> FindTransformsWithTag(Transform parent, string tag)
        {
            void FindTransformsWithTag(Transform parent, string tag, ref List<Transform> transforms)
            {
                if (parent.CompareTag(tag))
                {
                    transforms.Add(parent);
                }

                foreach (Transform child in parent)
                {
                    FindTransformsWithTag(child, tag, ref transforms);
                }
            }

            List<Transform> transforms = new List<Transform>();
            FindTransformsWithTag(parent, tag, ref transforms);
            return transforms;
        }

        public static void WriteData<T>(string filePath, T t)
        {
            var json = JsonConvert.SerializeObject(t);
            var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(json);
            sw.Close();
        }

        public static LandmarkModuel CaculateLandmarkModuel(string imagePath,GameObject obj)
        {
            List<LandmarkInfo> landmarks = GetLandmarkInfos(obj);
            CharacterBingdingBox characterBingdingBox = GetBoundingBox(obj);
            return new LandmarkModuel(imagePath, landmarks, characterBingdingBox);
        }

        private static List<LandmarkInfo> GetLandmarkInfos(GameObject obj, int hitThreshold=100)
        {
            bool isInsideOfScreen(Vector3 pixCoord)
            {
                int height = Screen.currentResolution.height;
                int width = Screen.currentResolution.width;

                if (pixCoord.x >= 0 && pixCoord.x < width && pixCoord.y >= 0 && pixCoord.y <= height)
                    return true;
                return false;
            }

            var joint2skins = obj.GetComponent<Characters>().Visibility;
            var landmarks = FindLandmarks(obj);
            List<LandmarkInfo> infos = new List<LandmarkInfo>();
            List<int> hitCount = new List<int>();
            Dictionary<int, int> skin2joint = new Dictionary<int, int>();

            // Init
            for (int i = 0; i < landmarks.Count; ++i)
            {
                hitCount.Add(0);
            }
            foreach(var kvp in joint2skins)
            {
                int jointId = kvp.Key;
                var skinIds = kvp.Value;

                foreach(var skinId in skinIds)
                {
                    skin2joint.Add(skinId, jointId);
                }
            }
            

            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Raycastable") | LayerMask.GetMask("Character") | LayerMask.GetMask("Landmark");

            // Raycast on every landmark
            for (int id = 0; id < landmarks.Count; ++id)
            {
                Vector3 pixCoord = Camera.main.WorldToScreenPoint(landmarks[id].transform.position);

                // if pixCoord is out of the screen, skip
                if (!isInsideOfScreen(pixCoord))
                    continue;

                var meshFilter = landmarks[id].GetComponent<MeshFilter>();
                Vector3 start = Camera.main.transform.position;

                // raycast on every vertex on the mesh
                foreach (Vector3 vertex in meshFilter.sharedMesh.vertices)
                {
                    Vector3 end = landmarks[id].transform.TransformPoint(vertex);
                    if (Physics.Linecast(start, end, out hit, layerMask))
                    {
                        if (hit.collider.gameObject == landmarks[id])
                        {
                            ++hitCount[id];

                            // if this is a skin landmark
                            if (skin2joint.ContainsKey(id))
                            {
                                int hitJointLandmarkId = skin2joint[id];
                                ++hitCount[hitJointLandmarkId];
                            }
                        }
                    }
                }
            }

            // set LandmarkInfo for each landmark
            for (int id = 0; id < landmarks.Count; ++id)
            {
                Vector3 pixCoord = Camera.main.WorldToScreenPoint(landmarks[id].transform.position);

                var info = new LandmarkInfo();
                info.X = pixCoord.x;
                info.Y = pixCoord.y;
                info.Z = pixCoord.z;
                info.visibility = Visibility.Unlabelled;

                // if pixCoord is inside of the screen
                if (isInsideOfScreen(pixCoord))
                {
                    if (joint2skins.ContainsKey(id))
                    {
                        if (hitCount[id] > hitThreshold * 2)
                            info.visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (hitCount[id] > hitThreshold)
                            info.visibility = Visibility.Visible;
                    }
                }

                infos.Add(info);
            }
            return infos;
        } 

        private static CharacterBingdingBox GetBoundingBox(GameObject obj)
        {
            var min = Vector2.positiveInfinity;
            var max = Vector2.negativeInfinity;
            GameObject body = obj.transform.Find("CC_Game_Body").gameObject;
            MeshFilter filter = body.GetComponent<MeshFilter>();
            Mesh mesh = filter.mesh;
            foreach (Vector3 vertex in mesh.vertices)
            {
                Vector3 p = body.transform.TransformPoint(vertex);
                Vector3 q = Camera.main.WorldToScreenPoint(p);
                min.x = Mathf.Min(min.x, q.x);
                min.y = Mathf.Min(min.y, q.y);
                max.x = Mathf.Max(max.x, q.x);
                max.y = Mathf.Max(max.y, q.y);
            }

            var bbox = new CharacterBingdingBox();
            bbox.X = min.x;
            bbox.Y = max.y;
            bbox.Width = max.x - min.x;
            bbox.Height = max.y - min.y;
            return bbox;
        }

        static List<GameObject> FindLandmarks(GameObject obj)
        {
            List<GameObject> landmarks = new List<GameObject>();
            Dictionary<int, GameObject> id2landmark = new Dictionary<int, GameObject>();
            foreach (Transform landmarkTransform in obj.GetComponentInChildren<Transform>())
            {
                if (landmarkTransform.CompareTag("Landmark"))
                {
                    int id = int.Parse(landmarkTransform.name.Substring(8));
                    id2landmark.Add(id, landmarkTransform.gameObject);
                }
            }

            for (int i=0; i<id2landmark.Count; ++i)
                landmarks.Add(id2landmark[i]);

            id2landmark.Clear();
            return landmarks;
        }
    }

}


