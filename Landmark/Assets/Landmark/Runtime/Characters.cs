using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Characters : MonoBehaviour
{
    public List<AnimationClip> AnimationClips = new List<AnimationClip>();
    public List<GameObject> Landmarks = new List<GameObject>();
    public SerializableDictionary<int, int[]> Visibility = new SerializableDictionary<int, int[]>();
}
