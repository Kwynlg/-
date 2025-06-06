// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace KINEMATION.RetargetPro.Runtime.Features.BasicRetargeting
{
    public struct BasicRetargetJob : IAnimationJob, IRetargetJob
    {
        public BasicRetargetFeature basicFeature;
        public NativeArray<RetargetSceneAtom> sourceChain;
        public NativeArray<RetargetStreamAtom> targetChain;
        
        public BasicRetargetData basicData;
        
        public void Setup(RetargetFeature feature, Animator animator, KRigComponent source, KRigComponent target)
        {
            basicFeature = feature as BasicRetargetFeature;
            
            Transform sourceRootTransform = source.transform.root;
            Transform targetRootTransform = animator.transform;
            
            basicData.sourceRoot = animator.BindSceneTransform(sourceRootTransform);
            basicData.targetRoot = animator.BindStreamTransform(targetRootTransform);
            
            KTransformChain sourceChainT = RetargetUtility.GetTransformChain(source, basicFeature.sourceRig, 
                basicFeature.sourceChain);
            KTransformChain targetChainT =  RetargetUtility.GetTransformChain(target, basicFeature.targetRig, 
                basicFeature.targetChain);
            
            if(!sourceChainT.IsValid() || !targetChainT.IsValid())
            {
                Debug.LogError("IKRetargetJob: Source or Target chains are NULL!");
                return;
            }
            
            RetargetJobUtility.SetupSceneAtomChain(animator, ref sourceChain, sourceChainT.transformChain.ToArray(), 
                source.transform.root);
            RetargetJobUtility.SetupStreamAtomChain(animator, ref targetChain, targetChainT.transformChain.ToArray());

            float sourceLength = sourceChainT.GetLength(sourceRootTransform);
            float targetLength = targetChainT.GetLength(targetRootTransform);
            
            if (Mathf.Approximately(sourceLength, 0f))
            {
                basicData.scale = 1f;
                return;
            }
            
            basicData.scale = targetLength / sourceLength;
        }

        public void SetJobData(AnimationScriptPlayable playable)
        {
            basicData.featureWeight = basicFeature.featureWeight;
            basicData.scaleWeight = basicFeature.scaleWeight;
            basicData.translationWeight = basicFeature.translationWeight;
            basicData.offset = basicFeature.offset;
            
            playable.SetJobData(this);
        }

        public void Dispose()
        {
            if (sourceChain.IsCreated) sourceChain.Dispose();
            if (targetChain.IsCreated) targetChain.Dispose();
        }
        
        public void ProcessAnimation(AnimationStream stream)
        {
            RetargetJobUtility.BasicRetarget(stream, sourceChain, targetChain, basicData);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
        }
    }
}