//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;
//using Newtonsoft.Json.Linq;
//using System.IO;
//using System.Linq;

//public class LandmarkStatus : MonoBehaviour
//{
//    public TextAsset config;
//    public GameObject landmarkPrefab;
//    public bool showLandmarks = true;

//    public int numberOfLandmarks;

//    SkinnedCollisionHelper _skinnedCollisionHelper;
//    public GameObject markerPrefab;
//    public bool useMeshFollower = true;
//    JObject _landmarkInfo;
//    SortedList<int, Transform> _landmarks;
//    SortedList<int, int> _meshFollower = new SortedList<int, int>();
//    Dictionary<int, Transform> _markers = new Dictionary<int, Transform>();
//    GameObject _pointCloud;
//    const string MESH_FOLLOWER = "meshFollower";


//    private void Awake()
//    {
//        _landmarkInfo = JObject.Parse(config.text);

//        if (_landmarkInfo.ContainsKey(MESH_FOLLOWER))
//        {
//            foreach (JProperty it in _landmarkInfo[MESH_FOLLOWER])
//            {
//                var key = int.Parse(it.Name);
//                var val = int.Parse(it.Value.ToString());
//                _meshFollower.Add(key, val);
//            }
//        }
//    }


//    // Start is called before the first frame update
//    void Start()
//    {
//        _skinnedCollisionHelper = GetComponentInChildren<SkinnedCollisionHelper>();
//        _landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);
//        numberOfLandmarks = _landmarks.Count;
//    }


//    // Update is called once per frame
//    void Update()
//    {
//        if (useMeshFollower)
//        {
//            RelocateSkinLandmarks();
//        }
//    }


//    public int GetNumberOfLandmarks()
//    {
//        return FindUtils.FindAllLandmarks(transform, Tags.Landmark).Count;
//    }


//    public bool ImportAnnotation()
//    {
//        _landmarkInfo = JObject.Parse(config.text);
//        bool hasAnnotation = _landmarkInfo.ContainsKey("annotation");
//        if (hasAnnotation)
//        {
//            var landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);
//            JArray localpos = (JArray)_landmarkInfo["annotation"];
//            if (localpos.Count != landmarks.Count)
//                return false;

//            for (int i = 0; i < landmarks.Count; ++i)
//            {
//                var pos = localpos[i];
//                float xx = float.Parse(pos[0].ToString());
//                float yy = float.Parse(pos[1].ToString());
//                float zz = float.Parse(pos[2].ToString());
//                Vector3 p = new Vector3(xx, yy, zz);
//                landmarks[i].transform.localPosition = p;
//            }
//        }
//        else
//        {
//            Debug.LogError("No annotation field");
//        }
//        return hasAnnotation;
//    }


//    public bool ExportAnnotation()
//    {
//        _landmarkInfo = JObject.Parse(config.text);
//        var annotations = new JArray();

//        SortedList<int, Transform> landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);
//        for (int i = 0; i < landmarks.Count; ++i)
//        {
//            if (landmarks.ContainsKey(i))
//            {
//                Vector3 p = landmarks[i].localPosition;
//                var pos = new List<float>();
//                System.Math.Round(p.x, 3);
//                pos.Add((float)System.Math.Round(p.x, 4));
//                pos.Add((float)System.Math.Round(p.y, 4));
//                pos.Add((float)System.Math.Round(p.z, 4));
//                annotations.Add(JToken.FromObject(pos));
//            }
//            else
//            {
//                var pos = new List<float>();
//                pos.Add(0f);
//                pos.Add(0f);
//                pos.Add(0f);
//                annotations.Add(JToken.FromObject(pos));
//            }
//        }

//        _landmarkInfo["annotation"] = annotations;
//        File.WriteAllText(AssetDatabase.GetAssetPath(config), _landmarkInfo.ToString());

//        Debug.Log($"Export to {AssetDatabase.GetAssetPath(config)}");
//        return true;
//    }


//    public bool GenerateLandmarks()
//    {
//        DomainRandomization random = gameObject.GetComponent<DomainRandomization>();
//        if (random == null)
//            gameObject.AddComponent<DomainRandomization>();
//        // find the skin gameobject and attach SkinnedCollisionHelper and MeshCollider on it
//        Transform body = transform.Find(CharacterCreaterBodyPart.Body);
//        if (body)
//        {
//            _skinnedCollisionHelper = body.GetComponent<SkinnedCollisionHelper>();
//            if (_skinnedCollisionHelper == null)
//                body.gameObject.AddComponent<SkinnedCollisionHelper>();
//            MeshCollider collider = body.GetComponent<MeshCollider>();
//            if (collider == null)
//            {
//                body.gameObject.AddComponent<MeshCollider>();
//            }
//            collider.sharedMesh = body.GetComponent<SkinnedMeshRenderer>().sharedMesh;
//        }

//        _landmarkInfo = JObject.Parse(config.text);

//        foreach (JProperty x in _landmarkInfo["definition"])
//        {
//            Transform bodyPart = FindUtils.FindTransformByName(transform, x.Name);
//            if (bodyPart == null)
//            {
//                Debug.LogError($"No Body Part {x.Name}");
//                return false;
//            }

//            foreach (int id in x.Value)
//            {
//                string name = $"Landmark{id}";
//                Transform landmarkTransform = FindUtils.FindTransformByName(transform, name);

//                if (landmarkTransform == null)
//                {
//                    GameObject landmark = Instantiate(landmarkPrefab, bodyPart);
//                    landmark.name = name;
//                }
//            }
//        }

//        return true;
//    }


//    public bool GenerateSkinLandmarks()
//    {
//        Mississippi("t-pose");
//        StartCoroutine(_GenerateSkinLandmarks());
//        return true;
//    }


//    public bool DeleteLandmarks()
//    {
//        var objs = FindUtils.FindGameObjectsWithTag(transform.gameObject, Tags.Landmark);
//        foreach (GameObject obj in objs)
//        {
//            DestroyImmediate(obj);
//        }
//        return true;
//    }


//    public void GenerateMarkers()
//    {
//        if (_pointCloud)
//        {
//            Destroy(_pointCloud);
//            _markers.Clear();
//        }
//        _pointCloud = new GameObject("PointCloud");

//        _markers.Clear();
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        var baked = _skinnedCollisionHelper.mesh;
//        for (int i = 0; i < baked.vertices.Length; ++i)
//        {
//            Vector3 v = _skinnedCollisionHelper.transform.TransformPoint(baked.vertices[i]);
//            var obj = Instantiate(markerPrefab);
//            obj.transform.SetParent(_pointCloud.transform);
//            obj.transform.position = v;
//            obj.name = $"vertex{i}";
//            _markers.Add(i, obj.transform);
//        }
//    }


//    public bool DeleteMarkers()
//    {
//        DestroyImmediate(_pointCloud);
//        _markers.Clear();
//        return true;
//    }


//    public void AutoMeshFollower()
//    {
//        string[] fields = { "t-pose", "f-pose", "a-pose" };
//        if (_skinnedCollisionHelper == null)
//            _skinnedCollisionHelper = GetComponentInChildren<SkinnedCollisionHelper>();
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        var landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);

//        HashSet<int> ids = new HashSet<int>();
//        _landmarkInfo = JObject.Parse(config.text);
//        foreach (string field in fields)
//        {
//            foreach (JProperty x in _landmarkInfo[field])
//            {
//                foreach (JProperty y in x.Value)
//                {
//                    int id = int.Parse(y.Value.ToString());
//                    ids.Add(id);
//                }
//            }
//        }

//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        Mesh baked = _skinnedCollisionHelper.mesh;
//        _meshFollower.Clear();
//        foreach (int id in ids)
//        {
//            float minDistance = float.MaxValue;
//            Vector3 pos = landmarks[id].position;
//            for (int i = 0; i < baked.vertices.Length; ++i)
//            {
//                Vector3 v = _skinnedCollisionHelper.transform.TransformPoint(baked.vertices[i]);
//                float dist = (landmarks[id].position - v).magnitude;
//                if (dist < minDistance)
//                {
//                    minDistance = dist;
//                    pos = v;
//                    _meshFollower[id] = i;
//                }
//            }
//            landmarks[id].position = new Vector3(pos.x, pos.y, pos.z);
//        }

//        JObject obj = new JObject();
//        foreach (int landmarkId in _meshFollower.Keys)
//        {
//            obj.Add(landmarkId.ToString(), _meshFollower[landmarkId]);
//        }

//        _landmarkInfo[MESH_FOLLOWER] = obj;

//        File.WriteAllText(AssetDatabase.GetAssetPath(config), _landmarkInfo.ToString());
//        Debug.Log($"Export to {AssetDatabase.GetAssetPath(config)}");

//    }


//    IEnumerator _GenerateSkinLandmarks()
//    {
//        Transform leftThigh = FindUtils.FindTransformByName(transform, CharacterCreaterBodyPart.LeftThigh);
//        leftThigh.Rotate(Vector3.up, 90);
//        leftThigh.Rotate(Vector3.right, 45);
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        yield return null;
//        Mississippi("f-pose");
//        leftThigh.Rotate(Vector3.right, -45);
//        leftThigh.Rotate(Vector3.up, -90);
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        yield return null;

//        Transform leftUpperArm = FindUtils.FindTransformByName(transform, CharacterCreaterBodyPart.LeftUpperArm);
//        Transform rightUpperArm = FindUtils.FindTransformByName(transform, CharacterCreaterBodyPart.RightUpperArm);
//        leftUpperArm.Rotate(Vector3.up, -45);
//        rightUpperArm.Rotate(Vector3.up, -45);
//        Transform leftClavicle = FindUtils.FindTransformByName(transform, "clavicle_l");
//        Transform rightClavicle = FindUtils.FindTransformByName(transform, "clavicle_r");
//        leftClavicle.Rotate(Vector3.up, -45);
//        rightClavicle.Rotate(Vector3.up, -45);
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        yield return null;
//        Mississippi("a-pose");
//        leftClavicle.Rotate(Vector3.up, 45);
//        rightClavicle.Rotate(Vector3.up, 45);
//        leftUpperArm.Rotate(Vector3.up, 45);
//        rightUpperArm.Rotate(Vector3.up, 45);
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        yield return null;
//    }


//    void Mississippi(string field)
//    {
//        _landmarkInfo = JObject.Parse(config.text);
//        foreach (JProperty x in _landmarkInfo[field])
//        {
//            Transform jointLandmark = FindUtils.FindTransformByName(transform, x.Name);
//            Transform bodyPart = jointLandmark.parent;
//            Vector3 dir = bodyPart.up;
//            foreach (JProperty y in x.Value)
//            {
//                int id = int.Parse(y.Value.ToString());
//                var landmark = _landmarks[id];
//                float distance = 0.5f;
//                switch (y.Name)
//                {
//                    case "+x":
//                        dir = bodyPart.right * distance;
//                        break;
//                    case "-x":
//                        dir = -bodyPart.right * distance;
//                        break;
//                    case "+y":
//                        dir = bodyPart.up * distance;
//                        break;
//                    case "-y":
//                        dir = -bodyPart.up * distance;
//                        break;
//                    case "+z":
//                        dir = bodyPart.forward * distance;
//                        break;
//                    case "-z":
//                        dir = -bodyPart.forward * distance;
//                        break;
//                }
//                Vector3 start = jointLandmark.position + dir;

//                RaycastHit hit;
//                RaycastableLayers[] layers = { RaycastableLayers.Raycastable, RaycastableLayers.Character, RaycastableLayers.Landmark };
//                int layerMask = 0;
//                foreach (var layer in layers)
//                {
//                    layerMask |= (1 << (int)layer);
//                }
//                if (Physics.Raycast(start, -dir, out hit, 10, layerMask))
//                {
//                    if (hit.collider.name == CharacterCreaterBodyPart.Body)
//                        landmark.position = hit.point;
//                }
//            }
//        }
//    }


//    void RelocateSkinLandmarks()
//    {
//        var landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);
//        _skinnedCollisionHelper.UpdateCollisionMesh();
//        Mesh baked = _skinnedCollisionHelper.mesh;
//        foreach (int landmarkId in _meshFollower.Keys)
//        {
//            Vector3 v = _skinnedCollisionHelper.transform.TransformPoint(baked.vertices[_meshFollower[landmarkId]]);
//            landmarks[landmarkId].position = v;
//        }
//    }


//    public List<Visibility> GetVisibilities()
//    {
//        var landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);

//        _landmarkInfo = JObject.Parse(config.text);
//        Dictionary<int, HashSet<int>> jointHitCount = new Dictionary<int, HashSet<int>>();
//        Dictionary<int, int> hitTable = new Dictionary<int, int>();
//        List<Visibility> visibility = new List<Visibility>(landmarks.Count);

//        foreach (JProperty x in _landmarkInfo["visibility"])
//        {
//            foreach (int y in x.Value)
//            {
//                hitTable[y] = int.Parse(x.Name);
//            }
//        }

//        // Raycast at each landmark
//        for (int id = 0; id < landmarks.Count; ++id)
//        {
//            visibility.Add(Visibility.Unlabelled);
//            Vector3 pixCoord = Camera.main.WorldToScreenPoint(landmarks[id].position);
//            pixCoord.z = 0;

//            // set visibility = 0 if the pixel is out of the screen.
//            if (pixCoord.x < 0 || pixCoord.x >= 1080 || pixCoord.y < 0 && pixCoord.y >= 1920)
//            {
//                continue;
//            }

//            Vector3 start = Camera.main.ScreenToWorldPoint(pixCoord);
//            Vector3 direction = (landmarks[id].position - start).normalized;
//            start += direction * Camera.main.nearClipPlane;
//            RaycastHit hit;

//            RaycastableLayers[] layers = { RaycastableLayers.Raycastable, RaycastableLayers.Character, RaycastableLayers.Landmark };
//            int layerMask = 0;
//            foreach (var layer in layers)
//            {
//                layerMask |= (1 << (int)layer);
//            }

//            if (Physics.Raycast(start, direction, out hit, 10, layerMask))
//            {
//                if (hit.collider.name == landmarks[id].name)
//                {
//                    visibility[id] = Visibility.Visible;
//                    if (hitTable.ContainsKey(id))
//                    {
//                        int hitJointLandmarkId = hitTable[id];

//                        if (!jointHitCount.ContainsKey(hitJointLandmarkId))
//                        {
//                            jointHitCount[hitJointLandmarkId] = new HashSet<int>();
//                        }
//                        jointHitCount[hitJointLandmarkId].Add(id);
//                    }
//                }
//            }
//        }

//        foreach (int id in jointHitCount.Keys)
//        {
//            if (jointHitCount[id].Count > 0)
//            {
//                visibility[id] = Visibility.Visible;
//            }
//        }
//        return visibility;
//    }


//    public List<Vector3> GetWorldPositions()
//    {
//        var landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);
//        var worldPositions = new List<Vector3>();
//        for (int i = 0; i < landmarks.Count; ++i)
//        {
//            Vector3 p = landmarks.Values[i].position;
//            worldPositions.Add(new Vector3(p.x, p.y, p.z));
//        }

//        return worldPositions;
//    }


//    public void ShowLandmarks(bool visible)
//    {
//        var landmarks = FindUtils.FindAllLandmarks(transform, Tags.Landmark);
//        foreach (Transform landmark in landmarks.Values)
//        {
//            landmark.GetComponent<MeshRenderer>().enabled = visible;
//        }
//    }
//}
