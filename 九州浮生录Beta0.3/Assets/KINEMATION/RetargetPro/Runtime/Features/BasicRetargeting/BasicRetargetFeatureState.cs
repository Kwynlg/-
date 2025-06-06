// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using KINEMATION.KAnimationCore.Runtime.Core;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.RetargetPro.Runtime.Features.BasicRetargeting
{
    public class BasicRetargetFeatureState : RetargetFeatureState
    {
        private BasicRetargetFeature _asset;
        
        protected KTransformChain sourceChain;
        protected KTransformChain targetChain;
        
        protected float chainScale = 1f;
        protected bool _chainSizeMatch;

        private List<Vector3> _localTargetPositions;
        private List<Quaternion> _localTargetRotations;
        
        protected void RetargetBone(int sourceIndex, int targetIndex)
        {
            var cachedSourcePose = sourceChain.cachedTransforms[sourceIndex];
            var cachedTargetPose = targetChain.cachedTransforms[targetIndex];
            
            Transform sourceBone = sourceChain.transformChain[sourceIndex];
            Transform targetBone = targetChain.transformChain[targetIndex];
            
            float scale = Mathf.Lerp(1f, chainScale, _asset.scaleWeight);
            
            // Compute the delta dynamically, as bone sizes might differ.
            Quaternion orientationDelta = Quaternion.Inverse(cachedSourcePose.rotation) * cachedTargetPose.rotation;
            var targetRotation = sourceBone.rotation * orientationDelta;

            targetBone.rotation = targetRotation;
            
            targetBone.localRotation = Quaternion.Slerp(_localTargetRotations[targetIndex], 
                targetBone.localRotation, _asset.featureWeight);
            
            // Apply translation.
            Vector3 sourceLocal = GetSourceRoot().InverseTransformPoint(sourceBone.position);
            sourceLocal -= sourceChain.cachedTransforms[sourceIndex].position;
            sourceLocal *= scale;

            // Apply component space additive animation.
            Vector3 targetLocal = targetChain.cachedTransforms[targetIndex].position;
            targetLocal = GetTargetRoot().TransformPoint(targetLocal + sourceLocal);
            targetBone.position = targetLocal;

            KAnimationMath.MoveInSpace(GetTargetRoot(), targetBone, _asset.offset, 1f);
            
            // Blend with the default pose.
            targetBone.localPosition = Vector3.Lerp(_localTargetPositions[targetIndex], targetBone.localPosition,
                _asset.translationWeight * _asset.featureWeight);
        }
        
        protected override void Initialize(RetargetFeature newAsset)
        {
            _asset = newAsset as BasicRetargetFeature;
            
            if (_asset == null) return;

            // Initialize chain references.
            sourceChain = RetargetUtility.GetTransformChain(sourceRigComponent, _asset.sourceRig, _asset.sourceChain);
            targetChain = RetargetUtility.GetTransformChain(targetRigComponent, _asset.targetRig, _asset.targetChain);
            
            if (sourceChain == null || targetChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source or Target chain is null.");
                return;
            }

            _chainSizeMatch = sourceChain.transformChain.Count == targetChain.transformChain.Count;

            // Cache the current pose for both characters.
            sourceChain.CacheTransforms(ESpaceType.ComponentSpace, GetSourceRoot());
            targetChain.CacheTransforms(ESpaceType.ComponentSpace, GetTargetRoot());
            
            _localTargetPositions = new List<Vector3>();
            _localTargetRotations = new List<Quaternion>();
            
            float sourceChainLength = 0f;
            float targetChainLength = 0f;
            
            int count = targetChain.transformChain.Count;
            for (int i = 0; i < count; i++)
            {
                Transform targetBone = targetChain.transformChain[i];
                _localTargetPositions.Add(targetBone.localPosition);
                _localTargetRotations.Add(targetBone.localRotation);
                
                // If we have only one bone in the chain, use mesh space delta.
                if (count == 1)
                {
                    Vector3 targetMS = GetTargetRoot().InverseTransformPoint(targetBone.position);
                    targetChainLength = targetMS.magnitude;
                }
                else if (i > 0)
                {
                    targetChainLength += (targetBone.position - targetChain.transformChain[i - 1].position).magnitude;
                }
            }

            count = sourceChain.transformChain.Count;
            for (int i = 0; i < count; i++)
            {
                Transform sourceBone = sourceChain.transformChain[i];
                
                if (count == 1)
                {
                    Vector3 sourceMS = GetSourceRoot().InverseTransformPoint(sourceBone.position);
                    sourceChainLength = sourceMS.magnitude;
                }
                else if (i > 0)
                {
                    sourceChainLength += (sourceBone.position - sourceChain.transformChain[i - 1].position).magnitude;
                }
            }

            if (Mathf.Approximately(sourceChainLength, 0f))
            {
                chainScale = 1f;
                return;
            }
            
            chainScale = targetChainLength / sourceChainLength;
        }

        public override bool IsValid()
        {
            if (sourceRigComponent == null)
            {
                Debug.LogError("FPSRetargetFeature: Source Rig Component is NULL!");
                return false; 
            }
            
            if (targetRigComponent == null)
            {
                Debug.LogError("FPSRetargetFeature: Target Rig Component is NULL!");
                return false; 
            }
            
            if (sourceChain == null || targetChain == null) return false;
            if (!sourceChain.IsValid() || !targetChain.IsValid()) return false;
            return true;
        }

        // Used when source and target chain size is matched.
        protected void RetargetMatchedBones()
        {
            int count = targetChain.transformChain.Count;
            // Perform a simple rotation retargeting.
            for (int i = 0; i < count; i++) RetargetBone(i, i);
        }

        // Used when source and target chain sizes do not match.
        protected void RetargetDistributedBones()
        {
            int sourceCount = sourceChain.transformChain.Count;
            int targetCount = targetChain.transformChain.Count;

            for (int i = 0; i < sourceCount; i++)
            {
                int targetIndex = Mathf.FloorToInt((targetCount - 1) * ((float) i / (sourceCount - 1)));
                targetIndex = Mathf.Clamp(targetIndex, 0, targetCount - 1);
                RetargetBone(i, targetIndex);
            }
        }

        public override void Retarget(float time = 0f)
        {
            if (_chainSizeMatch)
            {
                RetargetMatchedBones();
                return;
            }

            RetargetDistributedBones();
        }
        
#if UNITY_EDITOR
        public override void OnSceneGUI()
        {
            RenderBoneChain(targetChain, Color.cyan);
        }
#endif
    }
}