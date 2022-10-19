using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Characters : MonoBehaviour
{
    public List<AnimationClip> AnimationClips = new List<AnimationClip>();
    public List<GameObject> Landmarks = new List<GameObject>();
    public SerializableDictionary<string, int[]> Visibility = new SerializableDictionary<string, int[]>();
}
