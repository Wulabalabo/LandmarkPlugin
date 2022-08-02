using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landmark
{
    public class LandmarkConfig 
    {
      
    }

    public class Definition
    {
        public int[] head;
        public int[] CC_Base_L_Eye;
        public int[] CC_Base_R_Eye;
        public int[] upperarm_l;
        public int[] upperarm_r;
        public int[] lowerarm_l;
        public int[] lowerarm_r;
        public int[] hand_l;
        public int[] hand_r;
        public int[] thigh_l;
        public int[] thigh_r;
        public int[] calf_l;
        public int[] calf_r;
        public int[] foot_l;
        public int[] foot_r;
        public int[] neck_01;
        public int[] upperarm_twist_01_l;
        public int[] upperarm_twist_01_r;
        public int[] lowerarm_twist_01_l;
        public int[] lowerarm_twist_01_r;
        public int[] thigh_twist_01_l;
        public int[] thigh_twist_01_r;
        public int[] calf_twist_01_l;
        public int[] calf_twist_01_r;
        public int[] CC_Base_L_RibsTwist;
        public int[] CC_Base_R_RibsTwist;
        public int[] spine_02;
        public int[] spine_01;
        public int[] pelvis;

        public Definition()
        {
            head = new []{0,3,4};
            CC_Base_L_Eye = new[] {1};
            CC_Base_R_Eye = new[] {2};
            upperarm_l = new[] {5, 18, 19, 20, 21, 22, 23};
            upperarm_r = new[] {6, 24, 25, 26, 27, 28, 29};
            lowerarm_l = new[] {7, 30, 31, 32, 33};
            lowerarm_r = new[] {8, 34, 35, 36, 37};
            hand_l = new[] {9, 38, 39, 40, 41};
            hand_r = new[] {10, 42, 43, 44, 45};
            thigh_l = new[] {11, 46, 47, 48};
            thigh_r = new[] {12, 49, 50, 51};
            calf_l = new[] {13, 52, 53, 54, 55};
            calf_r = new[] {14, 56, 57, 58, 59};
            foot_l = new[] {15, 60, 61, 62, 63};
            foot_r = new[] {16, 64, 65, 66, 67};
            neck_01 = new[] {17, 68, 69, 70, 71};
            upperarm_twist_01_l = new[] {72, 80, 81, 82, 83};
            upperarm_twist_01_r = new[] {73, 84, 85, 86, 87};
            lowerarm_twist_01_l = new[] {74, 88, 89, 90, 91};
            lowerarm_twist_01_r = new[] {75, 92, 93, 94, 95};
            thigh_twist_01_l = new[] {76, 96, 97, 98, 99};
            thigh_twist_01_r = new[] {77, 100, 101, 102, 103};
            calf_twist_01_l = new[] {78, 104, 105, 106, 107};
            calf_twist_01_r = new[] {79, 108, 109, 110, 111};
            CC_Base_L_RibsTwist = new[] {112, 114, 115, 116, 117};
            CC_Base_R_RibsTwist = new[] {113, 115, 117, 120, 121};
            spine_02 = new[] {122, 125, 126, 127};
            spine_01 = new[] {123, 128, 129, 130};
            pelvis = new[] {124, 131, 132, 133};
        }

        public Definition(int[] head, int[] ccBaseLEye, int[] ccBaseREye, int[] upperarmL, int[] upperarmR, int[] lowerarmL, int[] lowerarmR, int[] handL, int[] handR, int[] thighL, int[] thighR, int[] calfL, int[] calfR, int[] footL, int[] footR, int[] neck01, int[] upperarmTwist01L, int[] upperarmTwist01R, int[] lowerarmTwist01L, int[] lowerarmTwist01R, int[] thighTwist01L, int[] thighTwist01R, int[] calfTwist01L, int[] calfTwist01R, int[] ccBaseLRibsTwist, int[] ccBaseRRibsTwist, int[] spine02, int[] spine01, int[] pelvis)
        {
            this.head = head;
            CC_Base_L_Eye = ccBaseLEye;
            CC_Base_R_Eye = ccBaseREye;
            upperarm_l = upperarmL;
            upperarm_r = upperarmR;
            lowerarm_l = lowerarmL;
            lowerarm_r = lowerarmR;
            hand_l = handL;
            hand_r = handR;
            thigh_l = thighL;
            thigh_r = thighR;
            calf_l = calfL;
            calf_r = calfR;
            foot_l = footL;
            foot_r = footR;
            neck_01 = neck01;
            upperarm_twist_01_l = upperarmTwist01L;
            upperarm_twist_01_r = upperarmTwist01R;
            lowerarm_twist_01_l = lowerarmTwist01L;
            lowerarm_twist_01_r = lowerarmTwist01R;
            thigh_twist_01_l = thighTwist01L;
            thigh_twist_01_r = thighTwist01R;
            calf_twist_01_l = calfTwist01L;
            calf_twist_01_r = calfTwist01R;
            CC_Base_L_RibsTwist = ccBaseLRibsTwist;
            CC_Base_R_RibsTwist = ccBaseRRibsTwist;
            spine_02 = spine02;
            spine_01 = spine01;
            this.pelvis = pelvis;
        }
    }
}

