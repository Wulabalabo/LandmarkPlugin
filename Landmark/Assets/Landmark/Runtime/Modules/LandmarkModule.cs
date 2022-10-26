using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Visibility
{
    Unlabelled = 0,
    Invisible = 1,
    Visible = 2,    
}

public struct LandmarkInfo
{
    public float X;
    public float Y;
    public float Z;
    public Visibility visibility;
}

public struct CharacterBoundingBox
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
}

public class LandmarkModule 
{
    public string ImagePath;
    public List<LandmarkInfo> ScreenCoordinate;
    public CharacterBoundingBox CharacterBindingBox;

    public LandmarkModule(string imagePath, List<LandmarkInfo> screenCoordinate, CharacterBoundingBox characterBindingBox)
    {
        ImagePath = imagePath;
        ScreenCoordinate = screenCoordinate;
        CharacterBindingBox = characterBindingBox;
    }
}