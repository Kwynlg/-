// Designed by KINEMATION, 2024.

using KINEMATION.RetargetPro.Runtime.Features;
using KINEMATION.KAnimationCore.Runtime.Rig;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

using System.Collections.Generic;

namespace KINEMATION.RetargetPro.Runtime
{
    public struct DynamicRetargetPair
    {
        public IRetargetJob job;
        public AnimationScriptPlayable playable;
    }
    
    public class DynamicRetargeter : MonoBehaviour
    {
        [SerializeField] private GameObject animationSource;
        [SerializeField] private RetargetProfile retargetProfileAsset;

        private Animator _animator;

        private PlayableGraph _playableGraph;
        private AnimationLayerMixerPlayable _mixerPlayable;
        private List<DynamicRetargetPair> _retargetJobs;
        
        private void Start()
        {
            if (animationSource == null)
            {
                Debug.LogError("DynamicRetargeter: Animation Source not assigned!");
                return;
            }

            if (retargetProfileAsset == null)
            {
                Debug.LogError("DynamicRetargeter: Retarget Profile is NULL!");
                return;
            }
            
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("DynamicRetargeter: Animator is NULL!");
                return;
            }

            KRigComponent sourceRigComponent = animationSource.GetComponentInChildren<KRigComponent>();
            if (sourceRigComponent == null)
            {
                Debug.LogError("DynamicRetargeter: Source Rig component not found!");
                return;
            }
            
            KRigComponent targetRigComponent = GetComponentInChildren<KRigComponent>();
            if (targetRigComponent == null)
            {
                Debug.LogError("DynamicRetargeter: Target Rig component not found!");
                return;
            }
            
            sourceRigComponent.Initialize();
            targetRigComponent.Initialize();

            if (retargetProfileAsset.sourcePose != null)
            {
                retargetProfileAsset.sourcePose.SampleAnimation(animationSource, 0f);
            }

            if (retargetProfileAsset.targetPose != null)
            {
                retargetProfileAsset.targetPose.SampleAnimation(gameObject, 0f);
            }

            _retargetJobs = new List<DynamicRetargetPair>();

            _playableGraph = PlayableGraph.Create($"{gameObject.name}_Graph");
            _mixerPlayable = AnimationLayerMixerPlayable.Create(_playableGraph);

            Playable targetPlayable = _mixerPlayable;
            int num = retargetProfileAsset.retargetFeatures.Count;

            for (int i = num - 1; i >= 0; i--)
            {
                var feature = retargetProfileAsset.retargetFeatures[i];
                
                var dynamicRetarget = feature as IDynamicRetarget;
                if (dynamicRetarget == null) continue;

                var retargetJob = dynamicRetarget.SetupRetargetJob(_playableGraph, out var scriptPlayable);
                retargetJob.Setup(feature, _animator, sourceRigComponent, targetRigComponent);
                retargetJob.SetJobData(scriptPlayable);

                targetPlayable.AddInput(scriptPlayable, 0, 1f);
                targetPlayable = scriptPlayable;

                _retargetJobs.Add(new DynamicRetargetPair()
                {
                    job = retargetJob,
                    playable = scriptPlayable
                });
            }
            
            var output = AnimationPlayableOutput.Create(_playableGraph, $"{gameObject.name}_Output", _animator);
            output.SetSourcePlayable(_mixerPlayable);
            
            _playableGraph.Play();
        }

#if UNITY_EDITOR
        private void Update()
        {
            int num = _retargetJobs.Count;
            for (int i = 0; i < num; i++) _retargetJobs[i].job.SetJobData(_retargetJobs[i].playable);
        }
#endif

        private void OnDestroy()
        {
            _playableGraph.Stop();
            _playableGraph.Destroy();
            
            foreach (var retargetJob in _retargetJobs) retargetJob.job.Dispose();
        }
    }
}