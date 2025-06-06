// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;

using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace KINEMATION.RetargetPro.Runtime.Features.IKRetargeting
{
    public struct IKRetargetJob : IRetargetJob, IAnimationJob
    {
        public IKRetargetFeature ikFeature;
        public NativeArray<RetargetSceneAtom> sourceChain;
        public NativeArray<IKRetargetStreamAtom> targetChain;
        
        public IKRetargetData ikData;
        
        public void Setup(RetargetFeature feature, Animator animator, KRigComponent source, KRigComponent target)
        {
            ikFeature = feature as IKRetargetFeature;

            Transform sourceRootTransform = source.transform.root;
            Transform targetRootTransform = animator.transform;
            
            ikData.basicData.sourceRoot = animator.BindSceneTransform(sourceRootTransform);
            ikData.basicData.targetRoot = animator.BindStreamTransform(targetRootTransform);

            KTransformChain sourceChainT = RetargetUtility.GetTransformChain(source, ikFeature.sourceRig, 
                ikFeature.sourceChain);
            KTransformChain targetChainT =  RetargetUtility.GetTransformChain(target, ikFeature.targetRig, 
                ikFeature.targetChain);
            
            if(!sourceChainT.IsValid() || !targetChainT.IsValid())
            {
                Debug.LogError("IKRetargetJob: Source or Target chains are NULL!");
                return;
            }

            ikData.offsetSpace = animator.BindStreamTransform(target.GetRigTransform(ikFeature.effectorOffsetSpace));
            
            RetargetJobUtility.SetupSceneAtomChain(animator, ref sourceChain, sourceChainT.transformChain.ToArray(), 
                source.transform.root);
            RetargetJobUtility.SetupStreamIkAtomChain(animator, ref targetChain, targetChainT.transformChain.ToArray());

            float sourceLength = sourceChainT.GetLength(sourceRootTransform);
            float targetLength = targetChainT.GetLength(targetRootTransform);
            
            if (Mathf.Approximately(sourceLength, 0f))
            {
                ikData.basicData.scale = 1f;
                return;
            }
            
            ikData.basicData.scale = targetLength / sourceLength;
        }

        public void SetJobData(AnimationScriptPlayable playable)
        {
            ikData.basicData.featureWeight = ikFeature.featureWeight;
            ikData.basicData.scaleWeight = ikFeature.scaleWeight;
            ikData.basicData.translationWeight = ikFeature.translationWeight;
            ikData.basicData.offset = ikFeature.offset;

            ikData.effectorOffset = ikFeature.effectorOffset;
            ikData.ikWeight = ikFeature.ikWeight;

            ikData.maxIterations = 16;
            ikData.tolerance = 0.001f;
            
            playable.SetJobData(this);
        }

        public void Dispose()
        {
            if (sourceChain.IsCreated) sourceChain.Dispose();
            if (targetChain.IsCreated) targetChain.Dispose();
        }
        
        public void ProcessAnimation(AnimationStream stream)
        {
            RetargetJobUtility.BasicRetarget(stream, sourceChain, targetChain, ikData.basicData);

            if (Mathf.Approximately(ikData.ikWeight, 0f) || targetChain.Length < 3)
            {
                return;
            }

            if (targetChain.Length == 3)
            {
                RetargetJobUtility.SolveTwoBoneIK(stream, sourceChain, targetChain, ikData);
                return;
            }
            
            RetargetJobUtility.SolveChainIK(stream, sourceChain, targetChain, ikData);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
        }
    }
}