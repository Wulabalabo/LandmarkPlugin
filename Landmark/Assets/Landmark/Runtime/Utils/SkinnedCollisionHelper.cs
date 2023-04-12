/*
 * Author: birns92
 * https://answers.unity.com/questions/1197217/can-a-mesh-collider-work-with-an-animated-skinned.html
 */
using UnityEngine;
using System.Collections;
using Unity.Collections;
using System;
using System.Collections.Generic;

namespace Landmark
{
    public class SkinnedCollisionHelper:MonoBehaviour
    {
        // Public variables
        // Instance variables        
        public CWeightList[] _nodeWeights;
       // array of node weights (one per node)
        public Vector3[] _newVert; // array for the regular update of the collision mesh

        public Mesh Mesh;//private Mesh mesh;
        public MeshCollider _collide;// the dynamically-updated collision mesh
        public MeshFilter _filter;
        public SkinnedMeshRenderer _rend;

        public GameObject UpdateGameObject;
        // quick pointer to the mesh collider that we're updating
        // Function:    Start
        // This basically translates the information about the skinned mesh into
        // data that we can internally use to quickly update the collision mesh.

        public void Dispose()
        {
            UpdateGameObject = null;
            _rend= null;
            _collide = null;
            _filter = null;
            Mesh = null;
            _nodeWeights = null;
            _newVert = null;
        }

        public void GenerateMesh()
        {
            _rend = gameObject.GetOrAddComponent<SkinnedMeshRenderer>();
            gameObject.GetComponent<MeshFilter>().mesh=
            Mesh = new Mesh
            {
                vertices = _rend.sharedMesh.vertices,
                uv = _rend.sharedMesh.uv,
                triangles = _rend.sharedMesh.triangles
            };
        }
        public void Init(GameObject obj)
        {

            UpdateGameObject = obj;
            _rend = obj.GetOrAddComponent<SkinnedMeshRenderer>();
            _collide = obj.GetOrAddComponent<MeshCollider>();
            _filter=obj.GetOrAddComponent<MeshFilter>();
            if (_collide != null && _rend != null)
            {
                Mesh baseMesh = _rend.sharedMesh;
               
                Mesh = new Mesh
                {
                    vertices = baseMesh.vertices,
                    uv = baseMesh.uv,
                    triangles = baseMesh.triangles
                };
                _newVert = new Vector3[baseMesh.vertices.Length];
                ushort i;
                // Make a CWeightList for each bone in the skinned mesh         
                _nodeWeights = new CWeightList[_rend.bones.Length];
                for (i = 0; i < _rend.bones.Length; i++)
                {
                    _nodeWeights[i] = new CWeightList
                    {
                        Transform = _rend.bones[i]
                    };
                }

                // Create a bone weight list for each bone, ready for quick calculation during an update...
                for (i = 0; i < baseMesh.vertices.Length; i++)
                {
                    BoneWeight bw = baseMesh.boneWeights[i];
                    Vector3 localPt;
                    if (bw.weight0 != 0.0f)
                    {
                        localPt = baseMesh.bindposes[bw.boneIndex0].MultiplyPoint3x4(baseMesh.vertices[i]);
                        _nodeWeights[bw.boneIndex0].Weights.Add(new CVertexWeight(i, localPt, bw.weight0));
                    }

                    if (bw.weight1 != 0.0f)
                    {
                        localPt = baseMesh.bindposes[bw.boneIndex1].MultiplyPoint3x4(baseMesh.vertices[i]);
                        _nodeWeights[bw.boneIndex1].Weights.Add(new CVertexWeight(i, localPt, bw.weight1));
                    }

                    if (bw.weight2 != 0.0f)
                    {
                        localPt = baseMesh.bindposes[bw.boneIndex2].MultiplyPoint3x4(baseMesh.vertices[i]);
                        _nodeWeights[bw.boneIndex2].Weights.Add(new CVertexWeight(i, localPt, bw.weight2));
                    }

                    if (bw.weight3 != 0.0f)
                    {
                        localPt = baseMesh.bindposes[bw.boneIndex3].MultiplyPoint3x4(baseMesh.vertices[i]);
                        _nodeWeights[bw.boneIndex3].Weights.Add(new CVertexWeight(i, localPt, bw.weight3));
                    }
                }

                UpdateCollisionMesh();
            }
            else
            {
                Debug.LogError(obj.name + ": SkinnedCollisionHelper: this object either has no SkinnedMeshRenderer or has no MeshCollider!");
            }
        }

        

        // Function:    UpdateCollisionMesh
        //  Manually recalculates the collision mesh of the skinned mesh on this
        // object.
        public  void UpdateCollisionMesh()
        {
            if (Mesh == null) return;
            // Start by initializing all vertices to 'empty'
            for (int i = 0; i < _newVert.Length; i++)
            {
                _newVert[i] = new Vector3(0, 0, 0);
            }

            // Now get the local positions of all weighted indices...
            foreach (CWeightList wList in _nodeWeights)
            {
                foreach (CVertexWeight vw in wList.Weights)
                {
                    _newVert[vw.Index] += wList.Transform.localToWorldMatrix.MultiplyPoint3x4(vw.LocalPosition) * vw.Weight;
                }
            }


            // Now convert each point into local coordinates of this object.
            for (int i = 0; i < _newVert.Length; i++)
            {
                _newVert[i] = UpdateGameObject.transform.InverseTransformPoint(_newVert[i]);
            }

            // Update the mesh (& collider) with the updated vertices
            Mesh.vertices = _newVert;
            Mesh.RecalculateBounds();
            _filter.sharedMesh = Mesh;
            _collide.sharedMesh = Mesh;
        }
    }

    [Serializable]
    public class CVertexWeight
    {
        public int Index;
        public Vector3 LocalPosition;
        public float Weight;
        public CVertexWeight(int i, Vector3 p, float w)
        {
            Index = i;
            LocalPosition = p;
            Weight = w;
        }
    }
    [Serializable]
    public class CWeightList
    {
        public Transform Transform;
        public List<CVertexWeight> Weights;
        public CWeightList()
        {
            Weights = new List<CVertexWeight>();
        }
    }


}