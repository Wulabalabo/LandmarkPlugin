using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Landmark
{
    [Serializable]
    public class LandmarkPoint
    {

        public GameObject Object;
        public Vector3 Pos ;
        public LandmarkPoint(GameObject obj,Vector3 pos)
        {
            Object = obj;
            Pos = pos;
        }
    }
}

