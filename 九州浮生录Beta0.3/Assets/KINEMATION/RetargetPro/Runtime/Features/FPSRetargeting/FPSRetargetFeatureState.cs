// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KINEMATION.RetargetPro.Runtime.Features.FPSRetargeting
{
    public class FPSRetargetFeatureState : RetargetFeatureState
    {
        protected KTransformChain _sourceRightArmChain;
        protected KTransformChain _sourceLeftArmChain;
        
        protected KTransformChain _targetRightArmChain;
        protected KTransformChain _targetLeftArmChain;
        
        protected KTransformChain _sourceWeaponChain;
        protected KTransformChain _targetWeaponChain;

        protected FPSRetargetFeature _asset;
        
        protected void RetargetChains(KTransformChain source, KTransformChain target)
        {
            if (source == null || target == null) return;
            if (source.transformChain.Count != target.transformChain.Count) return;
            
            int count = source.transformChain.Count;
            for (int i = 0; i < count; i++)
            {
                KTransform cachedSourcePose = source.cachedTransforms[i];
                KTransform cachedTargetPose = target.cachedTransforms[i];
                
                var baseRotation = GetTargetRoot().rotation * target.cachedTransforms[i].rotation;

                // Compute the delta dynamically, as bone sizes might differ.
                Quaternion delta = Quaternion.Inverse(cachedSourcePose.rotation) * cachedTargetPose.rotation;
                var outRotation = source.transformChain[i].rotation * delta;
                outRotation = Quaternion.Slerp(baseRotation, outRotation, _asset.featureWeight);
                
                target.transformChain[i].rotation = outRotation;
            }
        }
        
        protected void ApplyIK(KTransformChain source, KTransformChain target, Vector3 effectorOffset)
        {
            RetargetChains(source, target);
            
            if (_sourceWeaponChain == null || _targetWeaponChain == null) return;
            if (_sourceWeaponChain.transformChain.Count != _targetWeaponChain.transformChain.Count) return;

            Transform sourceWeapon = _sourceWeaponChain.transformChain[0];
            Transform targetWeapon = _targetWeaponChain.transformChain[0];

            if (sourceWeapon == null || targetWeapon == null) return;
            
            if (source == null || target == null) return;
            if (source.transformChain.Count != target.transformChain.Count) return;

            int count = source.transformChain.Count;
            
            KTransform ikTarget = new KTransform(source.transformChain[count - 1]);
            Transform tip = target.transformChain[count - 1];
            Transform mid = target.transformChain[count - 2];
            Transform root = target.transformChain[count - 3];
            
            ikTarget = new KTransform(sourceWeapon).GetRelativeTransform(ikTarget, false);
            ikTarget.position += effectorOffset;
            ikTarget = new KTransform(targetWeapon).GetWorldTransform(ikTarget, false);
        
            KTwoBoneIkData twoBoneIkData = new KTwoBoneIkData()
            {
                root = new KTransform(root),
                mid = new KTransform(mid),
                tip = new KTransform(tip),
                hint = new KTransform(mid),
                target = ikTarget,
                hasValidHint = true,
                rotWeight = -1f,
                posWeight = _asset.featureWeight,
                hintWeight = _asset.featureWeight
            };
            
            KTwoBoneIK.Solve(ref twoBoneIkData);

            root.rotation = twoBoneIkData.root.rotation;
            mid.rotation = twoBoneIkData.mid.rotation;
            tip.rotation = twoBoneIkData.tip.rotation;
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
            
            if (_sourceRightArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source Right Arm chain is NULL.");
                return false;
            }
            
            if (_sourceLeftArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source Left Arm chain is NULL.");
                return false;
            }
            
            if (_targetRightArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Target Right Arm chain is NULL.");
                return false;
            }
            
            if (_targetLeftArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Target Left Arm chain is NULL.");
                return false;
            }
            
            if (_sourceWeaponChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source Weapon chain is NULL.");
                return false;
            }
            
            if (_targetWeaponChain == null)
            {
                Debug.LogError($"{GetType().Name}: Target Weapon chain is NULL.");
                return false;
            }

            return true;
        }

        protected override void Initialize(RetargetFeature newAsset)
        {
            _asset = newAsset as FPSRetargetFeature;
            if (_asset == null) return;
            
            // 1. Initialize all the bone chains.
            
            _sourceRightArmChain = RetargetUtility.GetTransformChain(sourceRigComponent, _asset.sourceRig, 
                _asset.sourceRightArm);
            _sourceLeftArmChain = RetargetUtility.GetTransformChain(sourceRigComponent, _asset.sourceRig, 
                _asset.sourceLeftArm);
            
            _targetRightArmChain = RetargetUtility.GetTransformChain(targetRigComponent, _asset.targetRig, 
                _asset.targetRightArm);
            _targetLeftArmChain = RetargetUtility.GetTransformChain(targetRigComponent, _asset.targetRig, 
                _asset.targetLeftArm);
            
            _sourceWeaponChain = RetargetUtility.GetTransformChain(sourceRigComponent, _asset.sourceRig, 
                _asset.sourceWeapon);
            _targetWeaponChain = RetargetUtility.GetTransformChain(targetRigComponent, _asset.targetRig, 
                _asset.targetWeapon);
            
            if (_sourceRightArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source Right Arm chain is NULL.");
                return;
            }
            
            if (_sourceLeftArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source Left Arm chain is NULL.");
                return;
            }
            
            if (_targetRightArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Target Right Arm chain is NULL.");
                return;
            }
            
            if (_targetLeftArmChain == null)
            {
                Debug.LogError($"{GetType().Name}: Target Left Arm chain is NULL.");
                return;
            }
            
            if (_sourceWeaponChain == null)
            {
                Debug.LogError($"{GetType().Name}: Source Weapon chain is NULL.");
                return;
            }
            
            if (_targetWeaponChain == null)
            {
                Debug.LogError($"{GetType().Name}: Target Weapon chain is NULL.");
                return;
            }
            
            // 2. Cache the initial poses.
            
            _sourceWeaponChain.CacheTransforms(ESpaceType.ComponentSpace);
            _sourceRightArmChain.CacheTransforms(ESpaceType.ComponentSpace);
            _sourceLeftArmChain.CacheTransforms(ESpaceType.ComponentSpace);
            
            _targetWeaponChain.CacheTransforms(ESpaceType.ComponentSpace);
            _targetRightArmChain.CacheTransforms(ESpaceType.ComponentSpace);
            _targetLeftArmChain.CacheTransforms(ESpaceType.ComponentSpace);
        }
        
        public override void Retarget(float time = 0f)
        {          
            RetargetChains(_sourceWeaponChain, _targetWeaponChain);

            Transform sourceWeaponBone = _sourceWeaponChain.transformChain[0];
            Transform targetWeaponBone = _targetWeaponChain.transformChain[0];

            Vector3 cachedWeaponPosition = _targetWeaponChain.cachedTransforms[0].position;
            Vector3 weaponPosition = GetSourceRoot().InverseTransformPoint(sourceWeaponBone.position);

            weaponPosition += _asset.weaponOffset;
            weaponPosition = Vector3.Lerp(cachedWeaponPosition, weaponPosition, _asset.featureWeight);
            targetWeaponBone.position = GetTargetRoot().TransformPoint(weaponPosition);

            Vector3 positionOffset = Vector3.zero;
            foreach (var offset in _asset.rightHandOffsets)
            {
                positionOffset += offset.positionOffset / 100f * offset.blendCurve.Evaluate(time);
            }
            
            ApplyIK(_sourceRightArmChain, _targetRightArmChain, positionOffset + _asset.rightHandOffset);
            
            positionOffset = Vector3.zero;
            foreach (var offset in _asset.leftHandOffsets)
            {
                positionOffset += offset.positionOffset / 100f * offset.blendCurve.Evaluate(time);;
            }
            ApplyIK(_sourceLeftArmChain, _targetLeftArmChain, positionOffset + _asset.leftHandOffset);
        }

#if UNITY_EDITOR
        private Vector3 RenderHandle(Transform space, Transform bone)
        {
            Vector3 handlePos = Handles.PositionHandle(bone.position, space.rotation);
            return space.InverseTransformPoint(handlePos) - space.InverseTransformPoint(bone.position);
        }
        
        public override void OnSceneGUI()
        {
            Transform weapon = _targetWeaponChain.transformChain[0];
            Transform rightHand = _targetRightArmChain.transformChain[^1];
            Transform leftHand = _targetLeftArmChain.transformChain[^1];

            float size = 0.03f;
            var color = Handles.color;
            Handles.color = Color.cyan;
            if (Handles.Button(weapon.position, weapon.rotation, size, size, Handles.SphereHandleCap))
            {
                _asset.enableWeaponHandle = true;
                _asset.enableRightHandHandle = _asset.enableLeftHandle = false;
            }
            
            if (Handles.Button(rightHand.position, weapon.rotation, size, size, Handles.SphereHandleCap))
            {
                _asset.enableRightHandHandle = true;
                _asset.enableWeaponHandle = _asset.enableLeftHandle = false;
            }
            
            if (Handles.Button(leftHand.position, weapon.rotation, size, size, Handles.SphereHandleCap))
            {
                _asset.enableLeftHandle = true;
                _asset.enableWeaponHandle = _asset.enableRightHandHandle = false;
            }
            
            if (_asset.enableWeaponHandle)
            {
                _asset.weaponOffset += RenderHandle(GetTargetRoot(), weapon);
            }
            
            if (_asset.enableRightHandHandle)
            {
                _asset.rightHandOffset += RenderHandle(weapon, rightHand);
            }

            if (_asset.enableLeftHandle)
            {
                _asset.leftHandOffset += RenderHandle(weapon, leftHand);
            }
            
            Handles.color = Color.white;
            Handles.Label(weapon.position, "Gun Goal");
            Handles.Label(rightHand.position, "Right Hand Goal");
            Handles.Label(leftHand.position, "Left Hand Goal");
            Handles.color = color;
        }
#endif
    }
}