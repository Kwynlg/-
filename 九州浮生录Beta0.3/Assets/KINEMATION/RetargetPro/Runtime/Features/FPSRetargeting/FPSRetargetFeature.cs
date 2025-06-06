// Designed by KINEMATION, 2024.

using System;
using System.Collections.Generic;
using KINEMATION.KAnimationCore.Runtime.Attributes;
using UnityEngine;

namespace KINEMATION.RetargetPro.Runtime.Features.FPSRetargeting
{
    [Serializable]
    public struct FPSRetargetOffset
    {
        public AnimationCurve blendCurve;
        public Vector3 positionOffset;
    }
    
    public class FPSRetargetFeature : RetargetFeature
    {
        [Header("Source Chains")]
        [ElementChainSelector("sourceRig")] public string sourceRightArm;
        [ElementChainSelector("sourceRig")] public string sourceLeftArm;
        [ElementChainSelector("sourceRig")] public string sourceWeapon;
        
        [Header("Target Chains")]
        [ElementChainSelector("targetRig")] public string targetRightArm;
        [ElementChainSelector("targetRig")] public string targetLeftArm;
        [ElementChainSelector("targetRig")] public string targetWeapon;

        [Header("Gizmos")]
        public bool enableWeaponHandle;
        public bool enableRightHandHandle;
        public bool enableLeftHandle;

        public Vector3 rightHandOffset = Vector3.zero;
        public Vector3 leftHandOffset = Vector3.zero;
        public Vector3 weaponOffset = Vector3.zero;

        public List<FPSRetargetOffset> rightHandOffsets = new List<FPSRetargetOffset>();
        public List<FPSRetargetOffset> leftHandOffsets = new List<FPSRetargetOffset>();
        
        public override RetargetFeatureState CreateFeatureState()
        {
            return new FPSRetargetFeatureState();
        }
    }
}