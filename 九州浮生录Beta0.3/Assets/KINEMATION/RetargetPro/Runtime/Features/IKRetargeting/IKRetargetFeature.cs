// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using KINEMATION.RetargetPro.Runtime.Features.BasicRetargeting;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace KINEMATION.RetargetPro.Runtime.Features.IKRetargeting
{
    public class IKRetargetFeature : BasicRetargetFeature
    {
        [Range(0f, 1f)] public float ikWeight = 1f;
        public Vector3 effectorOffset;
        public KRigElement effectorOffsetSpace;
        
        public override RetargetFeatureState CreateFeatureState()
        {
            return new IKRetargetingState();
        }

        public override IRetargetJob SetupRetargetJob(PlayableGraph graph, out AnimationScriptPlayable playable)
        {
            IKRetargetJob job = new IKRetargetJob();
            playable = AnimationScriptPlayable.Create(graph, job);
            return job;
        }

#if UNITY_EDITOR
        public override void OnRigUpdated()
        {
            if (targetRig == null) return;
            effectorOffsetSpace = targetRig.GetElementByName(effectorOffsetSpace.name);
        }
#endif
    }
}