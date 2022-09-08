using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConfig
{
    public static string DataPath => Application.dataPath;

    public static string LandmarkConfigPath =  DataPath + "/Configs/LandmarkConfigs";

    public static string CharactersModelsPath =  DataPath + "/Characters/Prefabs";
}
