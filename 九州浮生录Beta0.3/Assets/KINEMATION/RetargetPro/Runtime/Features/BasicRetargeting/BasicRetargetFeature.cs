// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Attributes;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace KINEMATION.RetargetPro.Runtime.Features.BasicRetargeting
{
    public class BasicRetargetFeature : RetargetFeature, IDynamicRetarget
    {
        [ElementChainSelector("sourceRig")] public string sourceChain;
        [ElementChainSelector("targetRig")] public string targetChain;
        
        [Range(0f, 1f)] public float scaleWeight = 1f;
        [Range(0f, 1f)] public float translationWeight;
        
        public Vector3 offset = Vector3.zero;
        
        public override RetargetFeatureState CreateFeatureState()
        {
            return new BasicRetargetFeatureState();
        }
        
        public virtual IRetargetJob SetupRetargetJob(PlayableGraph graph, out AnimationScriptPlayable playable)
        {
            BasicRetargetJob job = new BasicRetargetJob();
            playable = AnimationScriptPlayable.Create(graph, job);
            return job;
        }
    }
}