using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
                if (!Directory.Exists(GlobalConfig.LandmarkConfigPath))
                {
                    Directory.CreateDirectory(GlobalConfig.LandmarkConfigPath);
                }
                Debug.LogWarning("Using DefaultLandmarkConfig");
                var defaultPath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("DefaultLandmark")[0]);
                StreamReader sr = new StreamReader(defaultPath);
                var str = sr.ReadToEnd();
                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
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

        private static void ModifySpecifyConfigType(string path, string type, JToken obj)
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

        public static void SaveTexture2DLocally(Texture2D texture2D, string textName, string path)
        {
            byte[] bytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(path + "/" + textName + ".png", bytes);
        }

        public static void AutoCameraPositioning(GameObject targetObject, Transform spawnPoint, float magicalDistance = 5f, float magicalRatio = 0.7f)
        {
            Debug.Log(GameManager.instance.SkinnedCollisionHelper);
            Mesh mesh = GameManager.instance.SkinnedCollisionHelper.Mesh;

            var landmarks = targetObject.GetComponent<CharacterModule>().Landmarks;

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

        public static void WriteData(string filePath, LandmarkModule moduel)
        {
            var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            string visibilities = "";
            foreach (var item in moduel.ScreenCoordinate)
            {
                visibilities += "\"" + "(" + item.X + "," + item.Y + "," + item.X + "," + (int)item.visibility + ")" + "\"" + ",";
            }
            string bbox = "\"" + "(" + moduel.CharacterBindingBox.X + "," + moduel.CharacterBindingBox.Y + " " + moduel.CharacterBindingBox.Width + "," + moduel.CharacterBindingBox.Height + ")" + "\"";
            var context = moduel.ImagePath + "," + visibilities + bbox;
            sw.WriteLine(context);
            sw.Close();
        }

        public static void WriteTitle(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write("Image,");
            for (int i = 0; i < 134; ++i)
            {
                sw.Write($"{i},");
            }
            sw.Write("BBox\n");
            sw.Close();
        }


        public static LandmarkModule CaculateLandmarkModuel(ScopeInfo info, GameObject obj)
        {
            var specimagePath = $"{info.SceneId}-{info.CharacterName}-{info.SpawnpointName}-{info.Facing}-{info.Pose}.jpg"; ;
            List<LandmarkInfo> landmarks = GetLandmarkInfos(obj);
            CharacterBoundingBox characterBoundingBox = GetBoundingBox(obj);
            return new LandmarkModule(specimagePath, landmarks, characterBoundingBox);
        }

        private static List<LandmarkInfo> GetLandmarkInfos(GameObject obj, int hitThreshold = 50)
        {
            var characterScript = obj.GetComponent<CharacterModule>();
            bool isInsideOfScreen(Vector3 pixCoord)
            {
                int height = Screen.currentResolution.height;
                int width = Screen.currentResolution.width;

                if (pixCoord.x >= 0 && pixCoord.x < width && pixCoord.y >= 0 && pixCoord.y <= height)
                    return true;
                return false;
            }

            var joint2skins = characterScript.Visibility;
            var landmarks = characterScript.Landmarks;
            List<LandmarkInfo> infos = new List<LandmarkInfo>();
            List<int> hitCount = new List<int>();
            Dictionary<int, int> skin2joint = new Dictionary<int, int>();

            // Init
            for (int i = 0; i < landmarks.Count; ++i)
            {
                hitCount.Add(0);
            }
            foreach (var kvp in joint2skins)
            {
                int jointId = kvp.Key;
                var skinIds = kvp.Value;

                foreach (var skinId in skinIds)
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

            Material redMat = new Material(Shader.Find("HDRP/Lit"));
            Material greenMat = new Material(Shader.Find("HDRP/Lit"));
            redMat.color = Color.red;
            greenMat.color = Color.green;

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
                    if (hitCount[id] > hitThreshold)
                        info.visibility = Visibility.Visible;
                }
                if (info.visibility == Visibility.Visible)
                    landmarks[id].GetComponent<Renderer>().material = greenMat;
                else
                    landmarks[id].GetComponent<Renderer>().material = redMat;
                infos.Add(info);
            }
            return infos;
        }

        private static CharacterBoundingBox GetBoundingBox(GameObject obj)
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

            var bbox = new CharacterBoundingBox();
            bbox.X = min.x;
            bbox.Y = max.y;
            bbox.Width = max.x - min.x;
            bbox.Height = max.y - min.y;
            return bbox;
        }

        static List<GameObject> FindLandmarks(GameObject obj)
        {
            List<GameObject> landmarks = new List<GameObject>();
            foreach (var item in obj.GetComponentsInChildren<Transform>())
            {
                if (item.gameObject.CompareTag("Landmark"))
                {
                    landmarks.Add(item.gameObject);
                }
            }

            landmarks = landmarks.OrderBy((item) => int.Parse(item.name.Substring(8))).ToList();
            return landmarks;
        }

        public static SortedList<int, (string, int, Vector3)> ReadBarycentric(GameObject obj)
        {
            SortedList<int, (string, int, Vector3)> id2bary = new SortedList<int, (string, int, Vector3)>();
            string typeName = "barycentricCoordinates";
            string meshField = "mesh";
            string coordField = "coordinate";
            string triangleIndexField = "triangleIndex";

            JObject bary = (JObject)GetJPropertyByFile(obj.name, typeName);
            if (bary != null)
            {
                foreach (JProperty x in (JToken)bary)
                {
                    int landmarkId = int.Parse(x.Name);
                    if (id2bary.ContainsKey(landmarkId))
                        continue;
                    JObject record = JObject.Parse(x.Value.ToString());
                    string mesh = record[meshField].ToString();
                    int triangleIndex = int.Parse(record[triangleIndexField].ToString());
                    JArray coord = JArray.Parse(record[coordField].ToString());
                    Vector3 vec = new Vector3(float.Parse(coord[0].ToString()), float.Parse(coord[1].ToString()), float.Parse(coord[2].ToString()));
                    id2bary.Add(landmarkId, (mesh, triangleIndex, vec));
                }
            }
            return id2bary;
        }

        static List<BarycentricCoodinatesModule> WriteBarycentric(GameObject obj, int landmarkId, string mesh, int triangleIndex, Vector3 barycentricCoordinate)
        {
            string typeName = "barycentricCoordinates";
            string meshField = "mesh";
            string coordField = "coordinate";
            string triangleIndexField = "triangleIndex";

            List<BarycentricCoodinatesModule> barycentricCoodinates = new List<BarycentricCoodinatesModule>();

            SortedList<int, (string, int, Vector3)> id2bary = ReadBarycentric(obj);

            if (id2bary.ContainsKey(landmarkId))
            {
                id2bary.Remove(landmarkId);
            }

            id2bary.Add(landmarkId, (mesh, triangleIndex, barycentricCoordinate));



            JObject bary = new JObject();
            foreach (KeyValuePair<int, (string, int, Vector3)> kvp in id2bary)
            {
                barycentricCoodinates.Add(new BarycentricCoodinatesModule
                {
                    LandmarkIndex = kvp.Key.ToString(),
                    TriangleIndex = kvp.Value.Item2,
                    Coordinate = kvp.Value.Item3
                });
                JArray coord = new JArray();
                coord.Add(kvp.Value.Item3.x);
                coord.Add(kvp.Value.Item3.y);
                coord.Add(kvp.Value.Item3.z);

                JObject record = new JObject();
                record[meshField] = kvp.Value.Item1;
                record[triangleIndexField] = kvp.Value.Item2;
                record[coordField] = coord;

                bary[kvp.Key.ToString()] = record;
            }

            ModifyConfigFile(obj.name, typeName, bary);
            return barycentricCoodinates;
        }

        public static void ApplyBarycentricCoordinates(GameObject obj)
        {
            var landmarks = obj.GetComponent<CharacterModule>().Landmarks;
            var bary = ReadBarycentric(obj);

            // find all GameObjects with tag "CollisionMesh"
            Dictionary<string, MeshFilter> collisionMeshFilters = new Dictionary<string, MeshFilter>();
            foreach (Transform tf in obj.GetComponentsInChildren<Transform>())
            {
                if (tf.CompareTag("CollisionMesh"))
                {
                    collisionMeshFilters.Add(tf.name, tf.GetComponent<MeshFilter>());
                }
            }

            foreach (var kvp in bary)
            {
                int landmarkId = kvp.Key;
                string meshName = kvp.Value.Item1;
                int triangleIndex = kvp.Value.Item2;
                Vector3 coord = kvp.Value.Item3;

                MeshFilter meshFilter = collisionMeshFilters[meshName];
                Mesh mesh = meshFilter.mesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                Vector3 p0 = vertices[triangles[triangleIndex * 3]];
                Vector3 p1 = vertices[triangles[triangleIndex * 3 + 1]];
                Vector3 p2 = vertices[triangles[triangleIndex * 3 + 2]];
                Vector3 point = p0 * coord.x + p1 * coord.y + p2 * coord.z;
                landmarks[landmarkId].transform.position = meshFilter.transform.TransformPoint(point);
            }
        }

        public static void DisplayLandmark(GameObject character, bool visible = true)
        {
            var module = character.GetComponent<CharacterModule>();
            foreach (var item in module.Landmarks)
            {
                item.GetComponent<MeshRenderer>().enabled = visible;
            }
        }
    }

}


