using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConfig
{
    public static string DataPath => Application.dataPath;

    public static string LandmarkConfigPath =  DataPath + "/Configs/LandmarkConfigs";

    public static string DefaultRandomization = DataPath + "/Configs/DefaultRandomization.json";

    public static string PoseRadomizationConfigPath = DataPath + "/Configs/";

    public static string CharactersModelsPath =  DataPath + "/Resources/Prefabs";

    public static bool DisplayLandmark = false;
}
