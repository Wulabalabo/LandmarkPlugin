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

public struct CharacterBingdingBox
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
}

public class LandmarkModuel 
{
    public string ImagePath;
    public List<LandmarkInfo> ScreenCoordinate;
    public CharacterBingdingBox CharacterBindingBox;

    public LandmarkModuel(string imagePath, List<LandmarkInfo> screenCoordinate, CharacterBingdingBox characterBindingBox)
    {
        ImagePath = imagePath;
        ScreenCoordinate = screenCoordinate;
        CharacterBindingBox = characterBindingBox;
    }
}
