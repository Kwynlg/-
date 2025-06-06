// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using KINEMATION.RetargetPro.Runtime.Features;

using System.Collections.Generic;
using UnityEngine;

namespace KINEMATION.RetargetPro.Runtime
{
    public class RetargetProComponent
    {
        private GameObject _sourceCharacter;
        private GameObject _targetCharacter;

        private RetargetProfile _profile;
        private List<RetargetFeatureState> _retargetFeatureStates = new List<RetargetFeatureState>();

        private bool _initialized;

        public void Initialize(GameObject source, GameObject target, RetargetProfile profile)
        {
            _sourceCharacter = source;
            _targetCharacter = target;
            _profile = profile;

            if (source == null || target == null || profile == null)
            {
                _initialized = false;
                return;
            }
            
            _retargetFeatureStates.Clear();
            
            if (_profile.sourcePose != null)
            {
                _profile.sourcePose.SampleAnimation(_sourceCharacter, 0f);
            }
            
            if (_profile.targetPose != null)
            {
                _profile.targetPose.SampleAnimation(_targetCharacter, 0f);
            }
            
            KRigComponent sourceComponent = _sourceCharacter.GetComponentInChildren<KRigComponent>();
            KRigComponent targetComponent = _targetCharacter.GetComponentInChildren<KRigComponent>();
            
            if (sourceComponent == null || targetComponent == null)
            {
                _initialized = false;
                return;
            }
            
            sourceComponent.Initialize();
            targetComponent.Initialize();
            
            foreach (var feature in _profile.retargetFeatures)
            {
                RetargetFeatureState retargetFeatureState = feature.CreateFeatureState();
                if(retargetFeatureState == null) continue;
                retargetFeatureState.InitializeComponents(sourceComponent, targetComponent, feature);
                _retargetFeatureStates.Add(retargetFeatureState);
            }

            _initialized = true;
        }

        public void RetargetTransforms(float time = 0f)
        {
            if (!_initialized) return;
            foreach (var retargetFeatureState in _retargetFeatureStates)
            {
                if(retargetFeatureState.IsValid()) retargetFeatureState.Retarget(time);
            }
        }

        public void DestroyRetargetFeatures()
        {
            if (!_initialized) return;
            foreach (var retargetFeatureState in _retargetFeatureStates) retargetFeatureState.OnDestroy();
        }
        
#if UNITY_EDITOR
        public void OnSceneGUI()
        {
            return;
            if (!_initialized) return;

            for (int i = 0; i < _retargetFeatureStates.Count; i++)
            {
                if (_profile.retargetFeatures[i].drawGizmos && _retargetFeatureStates[i].IsValid())
                {
                    _retargetFeatureStates[i].OnSceneGUI();
                }
            }
        }
#endif
    }
}