// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;

using UnityEngine.Animations;
using UnityEngine.Playables;

using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace KINEMATION.RetargetPro.Runtime.Features
{
    public interface IDynamicRetarget
    {
        public IRetargetJob SetupRetargetJob(PlayableGraph graph, out AnimationScriptPlayable playable);
    }
    
    public interface IRetargetJob
    {
        public void Setup(RetargetFeature feature, Animator animator, KRigComponent source, KRigComponent target);

        public void SetJobData(AnimationScriptPlayable playable);
        
        public void Dispose();
    }

    public struct RetargetStreamAtom
    {
        public TransformStreamHandle handle;
        public KTransform cachedMeshPose;
        public KTransform cachedLocalPose;
    }

    public struct RetargetSceneAtom
    {
        public TransformSceneHandle handle;
        public KTransform cachedMeshPose;
    }

    public struct BasicRetargetData
    {
        public float scale;
        public float scaleWeight;
        public float featureWeight;
        public float translationWeight;
        public Vector3 offset;

        public TransformSceneHandle sourceRoot;
        public TransformStreamHandle targetRoot;
    }

    public struct IKRetargetStreamAtom
    {
        public RetargetStreamAtom basicAtom;
        public Vector3 position;
        public float length;
    }

    public struct IKRetargetData
    {
        public BasicRetargetData basicData;
        public float ikWeight;
        public Vector3 effectorOffset;
        public TransformStreamHandle offsetSpace;
        public int maxIterations;
        public float tolerance;
    }
    
    public class RetargetJobUtility
    {
        public static KTransform GetPoseFromHandle(AnimationStream stream, TransformStreamHandle handle)
        {
            return new KTransform()
            {
                position = handle.GetPosition(stream),
                rotation = handle.GetRotation(stream),
                scale = handle.GetLocalScale(stream)
            };
        }
        
        public static KTransform GetPoseFromHandle(AnimationStream stream, TransformSceneHandle handle)
        {
            return new KTransform()
            {
                position = handle.GetPosition(stream),
                rotation = handle.GetRotation(stream),
                scale = handle.GetLocalScale(stream)
            };
        }
        
        public static void SetupStreamAtomChain(Animator animator, ref NativeArray<RetargetStreamAtom> streamChain, 
            Transform[] transformChain)
        {
            if (transformChain == null)
            {
                return;
            }

            int num = transformChain.Length;
            streamChain = new NativeArray<RetargetStreamAtom>(num, Allocator.Persistent);

            Transform root = animator.transform;
            for (int i = 0; i < num; i++)
            {
                KTransform cachedMeshPose = new KTransform
                {
                    position = root.InverseTransformPoint(transformChain[i].position),
                    rotation = Quaternion.Inverse(root.rotation) * transformChain[i].rotation
                };
                
                streamChain[i] = new RetargetStreamAtom()
                {
                    handle = animator.BindStreamTransform(transformChain[i]),
                    cachedMeshPose = cachedMeshPose,
                    cachedLocalPose = new KTransform(transformChain[i], false),
                };
            }
        }
        
        public static void SetupSceneAtomChain(Animator animator, ref NativeArray<RetargetSceneAtom> streamChain, 
            Transform[] transformChain, Transform root)
        {
            if (transformChain == null)
            {
                return;
            }

            int num = transformChain.Length;
            streamChain = new NativeArray<RetargetSceneAtom>(num, Allocator.Persistent);

            KTransform rootTransform = new KTransform(root);
            for (int i = 0; i < num; i++)
            {
                streamChain[i] = new RetargetSceneAtom()
                {
                    handle = animator.BindSceneTransform(transformChain[i]),
                    cachedMeshPose = rootTransform.GetRelativeTransform(new KTransform(transformChain[i]), true),
                };
            }
        }

        public static void SetupStreamIkAtomChain(Animator animator, ref NativeArray<IKRetargetStreamAtom> streamChain,
            Transform[] transformChain)
        {
            if (transformChain == null)
            {
                return;
            }

            int num = transformChain.Length;
            streamChain = new NativeArray<IKRetargetStreamAtom>(num, Allocator.Persistent);

            Transform root = animator.transform;
            for (int i = 0; i < num; i++)
            {
                KTransform cachedMeshPose = new KTransform
                {
                    position = root.InverseTransformPoint(transformChain[i].position),
                    rotation = Quaternion.Inverse(root.rotation) * transformChain[i].rotation
                };

                streamChain[i] = new IKRetargetStreamAtom()
                {
                    basicAtom = new RetargetStreamAtom()
                    {
                        handle = animator.BindStreamTransform(transformChain[i]),
                        cachedMeshPose = cachedMeshPose,
                        cachedLocalPose = new KTransform(transformChain[i], false)
                    }
                };
            }
        }

        public static void RetargetAtoms(AnimationStream stream, RetargetSceneAtom source, RetargetStreamAtom target, 
            BasicRetargetData retargetData)
        {
            Quaternion sourceRotation = source.handle.GetRotation(stream);
            
            float scale = Mathf.Lerp(1f, retargetData.scale, retargetData.scaleWeight);
            
            Quaternion delta = Quaternion.Inverse(source.cachedMeshPose.rotation) * target.cachedMeshPose.rotation;
            Quaternion targetRotation = sourceRotation * delta;
            
            target.handle.SetLocalRotation(stream, target.cachedLocalPose.rotation);
            targetRotation = Quaternion.Slerp(target.handle.GetRotation(stream), targetRotation, 
                retargetData.featureWeight);
            
            target.handle.SetRotation(stream, targetRotation);

            KTransform sourceRoot = new KTransform()
            {
                position = retargetData.sourceRoot.GetPosition(stream),
                rotation = retargetData.sourceRoot.GetRotation(stream),
                scale = retargetData.sourceRoot.GetLocalScale(stream)
            };
            
            KTransform targetRoot = new KTransform()
            {
                position = retargetData.targetRoot.GetPosition(stream),
                rotation = retargetData.targetRoot.GetRotation(stream),
                scale = retargetData.targetRoot.GetLocalScale(stream)
            };
            
            Vector3 sourceLocal = sourceRoot.InverseTransformPoint(source.handle.GetPosition(stream), true);
            sourceLocal -= source.cachedMeshPose.position;
            sourceLocal *= scale;
            
            Vector3 targetPosition = target.cachedMeshPose.position + sourceLocal + retargetData.offset;
            targetPosition = targetRoot.TransformPoint(targetPosition, true);
            target.handle.SetPosition(stream, targetPosition);

            targetPosition = target.handle.GetLocalPosition(stream);
            
            targetPosition = Vector3.Lerp(target.cachedLocalPose.position, targetPosition, 
                retargetData.translationWeight * retargetData.featureWeight);
            target.handle.SetLocalPosition(stream, targetPosition);
        }
        
        public static void BasicRetarget(AnimationStream stream, NativeArray<RetargetSceneAtom> source, 
            NativeArray<RetargetStreamAtom> target, BasicRetargetData retargetData)
        {
            if (source.Length == target.Length)
            {
                for (int i = 0; i < target.Length; i++)
                {
                    RetargetAtoms(stream, source[i], target[i], retargetData);
                }
                return;
            }
            
            int sourceCount = source.Length;
            int targetCount = target.Length;

            for (int i = 0; i < sourceCount; i++)
            {
                int targetIndex = Mathf.FloorToInt((targetCount - 1) * ((float) i / (sourceCount - 1)));
                targetIndex = Mathf.Clamp(targetIndex, 0, targetCount - 1);
                RetargetAtoms(stream, source[i], target[targetIndex], retargetData);
            }
        }
        
        public static void BasicRetarget(AnimationStream stream, NativeArray<RetargetSceneAtom> source, 
            NativeArray<IKRetargetStreamAtom> target, BasicRetargetData retargetData)
        {
            if (source.Length == target.Length)
            {
                for (int i = 0; i < target.Length; i++)
                {
                    RetargetAtoms(stream, source[i], target[i].basicAtom, retargetData);
                }
                return;
            }
            
            int sourceCount = source.Length;
            int targetCount = target.Length;

            for (int i = 0; i < sourceCount; i++)
            {
                int targetIndex = Mathf.FloorToInt((targetCount - 1) * ((float) i / (sourceCount - 1)));
                targetIndex = Mathf.Clamp(targetIndex, 0, targetCount - 1);
                RetargetAtoms(stream, source[i], target[targetIndex].basicAtom, retargetData);
            }
        }

        public static bool SolveFABRIK(NativeArray<IKRetargetStreamAtom> atoms, Vector3 target, float maxReach, 
            int maxIterations, float tolerance)
        {
            var rootToTargetDir = target - atoms[0].position;
            if (rootToTargetDir.sqrMagnitude > KMath.Square(maxReach))
            {
                var dir = rootToTargetDir.normalized;
                for (int i = 1; i < atoms.Length; ++i)
                {
                    var atom = atoms[i];
                    atom.position = atoms[i - 1].position + dir * atoms[i - 1].length;
                    atoms[i] = atom;
                }

                return true;
            }

            int tipIndex = atoms.Length - 1;
            float sqrTolerance = KMath.Square(tolerance);
            
            if (KMath.SqrDistance(atoms[tipIndex].position, target) > sqrTolerance)
            {
                var rootPos = atoms[0].position;
                int iteration = 0;

                do
                {
                    var atom = atoms[tipIndex];
                    atom.position = target;
                    atoms[tipIndex] = atom;
                    
                    for (int i = tipIndex - 1; i > -1; --i)
                    {
                        atom = atoms[i];
                        atom.position = atoms[i + 1].position + 
                               (atoms[i].position - atoms[i + 1].position).normalized * atoms[i].length;
                        atoms[i] = atom;
                    }

                    atom = atoms[0];
                    atom.position = rootPos;
                    atoms[0] = atom;
                    
                    for (int i = 1; i < atoms.Length; ++i)
                    {
                        atom = atoms[i];
                        atom.position = atoms[i - 1].position + 
                                        (atoms[i].position - atoms[i - 1].position).normalized * atoms[i - 1].length;
                        atoms[i] = atom;
                    }
                        
                } while (KMath.SqrDistance(atoms[tipIndex].position, target) > sqrTolerance && 
                         ++iteration < maxIterations);

                return true;
            }

            return false;
        }

        public static Vector3 GetEffector(Vector3 target, KTransform sourceRoot, KTransform targetRoot, 
            Vector3 sourcePose, Vector3 targetPose, float scale)
        {
            Vector3 effector = target - sourceRoot.TransformPoint(sourcePose, true);
            effector = effector * scale + targetRoot.TransformPoint(targetPose, true);
            return effector;
        }

        public static void SolveTwoBoneIK(AnimationStream stream, NativeArray<RetargetSceneAtom> source, 
            NativeArray<IKRetargetStreamAtom> target, IKRetargetData ikData)
        {
            KTransform sourceTransform = GetPoseFromHandle(stream, ikData.basicData.sourceRoot);
            KTransform targetTransform = GetPoseFromHandle(stream, ikData.basicData.targetRoot);

            var tipHandle = target[^1].basicAtom.handle;
            var midHandle = target[^2].basicAtom.handle;
            var rootHandle = target[^3].basicAtom.handle;
            
            Vector3 sourceTip = source[^1].handle.GetPosition(stream);
            KTransform tip = GetPoseFromHandle(stream, tipHandle);
            KTransform mid = GetPoseFromHandle(stream, midHandle);
            KTransform root = GetPoseFromHandle(stream, rootHandle);
            
            Vector3 sourcePose = source[^1].cachedMeshPose.position;
            Vector3 targetPose = target[^1].basicAtom.cachedMeshPose.position;
            
            float scale = Mathf.Lerp(1f, ikData.basicData.scale, ikData.basicData.scaleWeight);
            
            Vector3 effector = GetEffector(sourceTip, sourceTransform, targetTransform, 
                sourcePose, targetPose, scale);
            
            KTransform offsetSpace = GetPoseFromHandle(stream, ikData.offsetSpace);
            KTransform ikTarget = new KTransform()
            {
                position = effector,
                rotation = Quaternion.identity,
                scale = Vector3.one
            };

            ikTarget.position = KAnimationMath.MoveInSpace(offsetSpace, ikTarget, ikData.effectorOffset, 1f);

            float weight = ikData.ikWeight * ikData.basicData.featureWeight;
            KTwoBoneIkData twoBoneIkData = new KTwoBoneIkData()
            {
                root = root,
                mid = mid,
                tip = tip,
                hint = mid,
                target = ikTarget,
                hasValidHint = true,
                rotWeight = -1f,
                posWeight = weight,
                hintWeight = weight
            };
            
            KTwoBoneIK.Solve(ref twoBoneIkData);
            
            rootHandle.SetRotation(stream, twoBoneIkData.root.rotation);
            midHandle.SetRotation(stream, twoBoneIkData.mid.rotation);
            tipHandle.SetRotation(stream, twoBoneIkData.tip.rotation);
        }

        public static void SolveChainIK(AnimationStream stream, NativeArray<RetargetSceneAtom> source, 
            NativeArray<IKRetargetStreamAtom> target, IKRetargetData ikData)
        {
            float maxReach = 0f;
            
            for (int i = 0; i < target.Length; i++)
            {
                var atom = target[i];
                Vector3 position = target[i].basicAtom.handle.GetPosition(stream);

                float distance = 0f;
                if (i != target.Length - 1)
                {
                    distance = Vector3.Distance(position, target[i + 1].basicAtom.handle.GetPosition(stream));
                }

                maxReach += distance;
                
                atom.position = position;
                atom.length = distance;
                target[i] = atom;
            }
            
            KTransform sourceRoot = GetPoseFromHandle(stream, ikData.basicData.sourceRoot);
            KTransform targetRoot = GetPoseFromHandle(stream, ikData.basicData.targetRoot);
            
            Vector3 sourceTip = source[^1].handle.GetPosition(stream);
            Vector3 sourcePose = source[^1].cachedMeshPose.position;
            Vector3 targetPose = target[^1].basicAtom.cachedMeshPose.position;
            
            float scale = Mathf.Lerp(1f, ikData.basicData.scale, ikData.basicData.scaleWeight);
            Vector3 effector = GetEffector(sourceTip, sourceRoot, targetRoot, 
                sourcePose, targetPose, scale);
            
            KTransform offsetSpace = GetPoseFromHandle(stream, ikData.offsetSpace);
            KTransform ikTarget = new KTransform()
            {
                position = effector,
                rotation = Quaternion.identity,
                scale = Vector3.one
            };

            ikTarget.position = KAnimationMath.MoveInSpace(offsetSpace, ikTarget, ikData.effectorOffset, 1f);
            
            if (!SolveFABRIK(target, ikTarget.position, maxReach, ikData.maxIterations, ikData.tolerance))
            {
                return;
            }
            
            int tipIndex = target.Length - 1;
            Quaternion tipRotation = target[^1].basicAtom.handle.GetRotation(stream);
            
            // 3. Apply rotations.
            for (int i = 0; i < tipIndex; ++i)
            {
                KTransform thisTransform = target[i].basicAtom.cachedMeshPose;
                KTransform nextTransform = target[i + 1].basicAtom.cachedMeshPose;
                
                var prevDir = nextTransform.position - thisTransform.position;
                var newDir = target[i + 1].position - target[i].position;

                Quaternion baseRot = target[i].basicAtom.handle.GetRotation(stream);
                Quaternion targetRot = KMath.FromToRotation(prevDir, newDir) * thisTransform.rotation;
                
                targetRot = Quaternion.Slerp(baseRot, targetRot, ikData.ikWeight);
                
                baseRot = targetRoot.rotation * thisTransform.rotation;
                targetRot = Quaternion.Slerp(baseRot, targetRot, ikData.basicData.featureWeight);
                
                target[i].basicAtom.handle.SetRotation(stream, targetRot);
            }

            target[^1].basicAtom.handle.SetRotation(stream, tipRotation);
        }
    }
}