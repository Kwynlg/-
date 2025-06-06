// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;

using System.Collections.Generic;
using KINEMATION.KAnimationCore.Runtime.Attributes;
using KINEMATION.RetargetPro.Runtime.Features;
using UnityEngine;

namespace KINEMATION.RetargetPro.Runtime
{
    [CreateAssetMenu(fileName = "NewRetargetProfile", menuName = "KINEMATION/Retarget Profile")]
    public class RetargetProfile : ScriptableObject, IRigObserver
    {
        [Header("Model")]
        
        [Tooltip("The model to retarget FROM. Note: make sure it has a Rig Component attached!")]
        public GameObject sourceCharacter;
        [Tooltip("The model to retarget TO. Note: make sure it has a Rig Component attached!")]
        public GameObject targetCharacter;
        
        [Header("Pose")]
        
        [Tooltip("This pose is sampled before retargeting. It can be an A or a T pose.")]
        public AnimationClip sourcePose;
        [Tooltip("This pose is sampled before retargeting. It can be an A or a T pose.")]
        public AnimationClip targetPose;
        
        [Header("Rig")]
        
        [Tooltip("Rig asset with bone chains for the SOURCE model.")]
        public KRig sourceRig;
        [Tooltip("Rig asset with bone chains for the TARGET model.")]
        public KRig targetRig;
        
        [RigAssetSelector("targetRig")] public KRigElementChain excludeChain = new KRigElementChain()
        {
            chainName = "Elements To Exclude",
            isStandalone = true
        };
        
        [HideInInspector] public List<RetargetFeature> retargetFeatures = new List<RetargetFeature>();
        
        public void OnRigUpdated()
        {
#if UNITY_EDITOR
            foreach (var feature in retargetFeatures)
            {
                feature.sourceRig = sourceRig;
                feature.targetRig = targetRig;
                feature.OnRigUpdated();
            }
#endif
        }
        
#if UNITY_EDITOR
        private KRig _cachedSourceRig;
        private KRig _cachedTargetRig;

        private void OnValidateRig(KRig rig, ref KRig cachedRig)
        {
            if (rig == cachedRig)
            {
                return;
            }

            if (cachedRig != null)
            {
                cachedRig.UnRegisterObserver(this);
            }
            
            if (rig != null)
            {
                rig.RegisterRigObserver(this);
            }
            
            OnRigUpdated();
            cachedRig = rig;
        }
        
        private void OnValidate()
        {
            OnValidateRig(sourceRig, ref _cachedSourceRig);
            OnValidateRig(targetRig, ref _cachedTargetRig);
        }
#endif
    }
}