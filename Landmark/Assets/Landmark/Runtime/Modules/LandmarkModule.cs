using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public enum Visibility
{
    Unlabelled = 0,
    Invisible = 1,
    Visible = 2,    
}

public class LandmarkInfo
{
    public float X;
    public float Y;
    public float Z;
    public Visibility visibility;
    public LandmarkInfo(float x, float y, float z, Visibility visibility)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.visibility = visibility;
    }
    public override string ToString()
    {
        return $"\"({X},{Y},{Z},{(int)visibility})\"";
    }
}

public class CharacterBoundingBox
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    public CharacterBoundingBox(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    public override string ToString()
    {
        return $"\"({X},{Y},{Width},{Height})\"";
    }
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
    public override string ToString()
    {
        return $"{this.ImagePath},{string.Join(",", ScreenCoordinate)},{CharacterBindingBox}\"";
    }
}
