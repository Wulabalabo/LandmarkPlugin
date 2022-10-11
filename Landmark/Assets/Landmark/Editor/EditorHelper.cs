using Landmark;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorHelper 
{
    public static void ImportCharacterAnimations(GameObject obj, List<AnimationClip> AnimationClips)
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
        if (folder.Length > 0)
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
}
