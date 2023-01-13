using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landmark
{
    public class CharacterModule : MonoBehaviour
    {
        public List<AnimationClip> AnimationClips = new List<AnimationClip>();
        public List<GameObject> Landmarks = new List<GameObject>();
        public SerializableDictionary<int, int[]> Visibility = new SerializableDictionary<int, int[]>();
        public List<SkinnedCollisionHelper> Helpers;
        public SkinnedCollisionHelper BodyHelper;
    }
}

