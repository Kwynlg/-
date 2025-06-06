// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using KINEMATION.RetargetPro.Runtime;

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.RetargetPro.Editor
{
    public class RetargetAnimBaker : Object
    {
        public bool IsInitialized { get; private set; }
        public RetargetProComponent RetargetComponent { get; private set; }
        
        private const string DefaultSavePath = "Assets/RetargetedAnimations";

        public delegate void OnProfileChanged(RetargetProfile profile);
        public OnProfileChanged onProfileChanged;
        
        private GameObject _sourceCharacter;
        private GameObject _targetCharacter;
        
        private GameObject _sourceCharacterInstance;
        private GameObject _targetCharacterInstance;
        private GameObject _itemInstance;

        private RetargetProfile _retargetProfile;
        private RetargetProfile _cachedRetargetProfile;
        
        private bool _copyClipSettings = true;
        private bool _useRootMotion = true;
        private bool _keyframeAll = true;
        
        private bool _isInitialized;
        
        private KRigComponent _sourceRigComponent;
        private KRigComponent _targetRigComponent;
        
        private float _frameRate = 24f;
        private string _savePath = DefaultSavePath;
        
        public string GetTargetName()
        {
            string result = "RetargetResult";

            if (_targetCharacter != null)
            {
                result = _targetCharacter.name;
            }

            return result;
        }

        public string GetTargetDirectory()
        {
            return _savePath;
        }
        
        public void RetargetAtTime(AnimationClip clip, AnimationClip itemClip, float time)
        {
            if (!IsInitialized) return;
            
            clip.SampleAnimation(_sourceCharacterInstance, time);
            if (itemClip != null && _itemInstance != null)
            {
                itemClip.SampleAnimation(_itemInstance, time);
            }
            
            RetargetComponent.RetargetTransforms(time);
        }
        
        public void InitializeBaker()
        {
            if (_sourceCharacter == null) _sourceCharacter = _retargetProfile.sourceCharacter;
            if (_targetCharacter == null) _targetCharacter = _retargetProfile.targetCharacter;
            
            _sourceCharacterInstance = _sourceCharacter;
            _targetCharacterInstance = _targetCharacter;
            
            if (EditorUtility.IsPersistent(_sourceCharacter))
            {
                _sourceCharacterInstance = Instantiate(_sourceCharacter);
            }
            
            if (EditorUtility.IsPersistent(_targetCharacter))
            {
                _targetCharacterInstance = Instantiate(_targetCharacter);
            }
            
            RetargetComponent = new RetargetProComponent();
            RetargetComponent.Initialize(_sourceCharacterInstance, _targetCharacterInstance, _retargetProfile);

            _sourceRigComponent = _sourceCharacterInstance.GetComponentInChildren<KRigComponent>();
            _targetRigComponent = _targetCharacterInstance.GetComponentInChildren<KRigComponent>();
            
            if (_sourceRigComponent == null || _targetRigComponent == null)
            {
                Debug.LogError($"Rig Component not found!");
                return;
            }

            if (!_sourceRigComponent.CompareRig(_retargetProfile.sourceRig))
            {
                Debug.LogWarning($"Rig mismatch: {_retargetProfile.sourceRig.name} is not up to date.");
            }
            
            if (!_targetRigComponent.CompareRig(_retargetProfile.targetRig))
            {
                Debug.LogWarning($"Rig mismatch: {_retargetProfile.targetRig.name} is not up to date.");
            }
            
            _sourceRigComponent.CacheHierarchyPose();
            _targetRigComponent.CacheHierarchyPose();

            IsInitialized = true;
        }
        
        public void UnInitializeBaker()
        {
            if (!IsInitialized) return;
            
            IsInitialized = false;
            
            if (_sourceRigComponent != null)
            {
                _sourceRigComponent.ApplyHierarchyCachedPose();
            }

            if (_targetRigComponent != null)
            {
                _targetRigComponent.ApplyHierarchyCachedPose();
            }
            
            RetargetComponent.DestroyRetargetFeatures();
            
            if(EditorUtility.IsPersistent(_sourceCharacter)) DestroyImmediate(_sourceCharacterInstance);
            if(EditorUtility.IsPersistent(_targetCharacter)) DestroyImmediate(_targetCharacterInstance);
        }
        
        public AnimationClip BakeAnimation(AnimationClip animationToRetarget)
        {
            if (_sourceRigComponent == null || _targetRigComponent == null)
            {
                Debug.LogError("RetargetAnimBaker: Rig Component is NULL!");
                return null;
            }
            
            GenericAnimationBaker baker = new GenericAnimationBaker();
            
            var toExclude = _retargetProfile.excludeChain.elementChain
                .Select(item => _targetRigComponent.GetRigTransform(item))
                .ToArray();

            var bones = _targetRigComponent.GetHierarchy()
                .Where(item => !toExclude.Contains(item))
                .ToArray();
            
            baker.Initialize(_targetRigComponent.transform.root.gameObject, bones, _keyframeAll);

            AnimationClip clip = new AnimationClip
            {
                name = $"{_targetCharacter.name}_{animationToRetarget.name}",
                frameRate = _frameRate
            };

            float playBack = 0f;
            float delta = 1f / _frameRate;
            
            while (playBack <= animationToRetarget.length)
            {
                RetargetAtTime(animationToRetarget, null, playBack);
                baker.BakeAnimationFrame(playBack);
                playBack += delta;
            }
            
            baker.WriteToClip(clip);
            if (_useRootMotion) baker.WriteRootMotion(animationToRetarget, clip);
            clip.EnsureQuaternionContinuity();
            
            if (_copyClipSettings)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(animationToRetarget);
                var events = AnimationUtility.GetAnimationEvents(animationToRetarget);
                
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                AnimationUtility.SetAnimationEvents(clip, events);
            }
            
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            string path = AssetDatabase.GenerateUniqueAssetPath($"{_savePath}/{clip.name}.anim");
            
            AssetDatabase.CreateAsset(clip, path);
            
            _sourceRigComponent.ApplyHierarchyCachedPose();
            _targetRigComponent.ApplyHierarchyCachedPose();

            return clip;
        }

        public bool Render()
        {
            GUIContent content = new GUIContent("Source Character", "The mode to retarget FROM.");
            _sourceCharacter = (GameObject) EditorGUILayout.ObjectField(content, _sourceCharacter, 
                typeof(GameObject), true);
            
            content = new GUIContent("Target Character", "The model to retarget TO.");
            _targetCharacter = (GameObject) EditorGUILayout.ObjectField(content, _targetCharacter, 
                typeof(GameObject), true);
            
            content = new GUIContent("Item", "Weapon or Item which can be animated.");
            _itemInstance = (GameObject) EditorGUILayout.ObjectField(content, _itemInstance, 
                typeof(GameObject), true);
            
            content = new GUIContent("Profile", "Retargeting settings asset.");
            _retargetProfile = (RetargetProfile) EditorGUILayout.ObjectField(content, _retargetProfile, 
                typeof(RetargetProfile), false);

            if (_cachedRetargetProfile != _retargetProfile)
            {
                onProfileChanged?.Invoke(_retargetProfile);
            }

            _cachedRetargetProfile = _retargetProfile;

            content = new GUIContent("Frame Rate", "Frames per second of this animation.");
            _frameRate = EditorGUILayout.Slider(content, _frameRate, 24f, 240f);
            
            content = new GUIContent("Copy Clip Settings", 
                "If settings (Loop Time, Bake Into Pose, etc.) need to be copied.");
            _copyClipSettings = EditorGUILayout.Toggle(content, _copyClipSettings);
            
            content = new GUIContent("Use Root Motion", "If root motion curves need to be copied.");
            _useRootMotion = EditorGUILayout.Toggle(content, _useRootMotion);
            
            content = new GUIContent("Keyframe All", 
                "If TRUE, every bone will be keyframed. Set it to FALSE to decrease the clip size.");
            _keyframeAll = EditorGUILayout.Toggle(content, _keyframeAll);
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Save Folder");
            
            if (GUILayout.Button(_savePath))
            {
                string path = EditorUtility.OpenFolderPanel("Select Directory", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _savePath = path;
                    string assetsPath = Application.dataPath;

                    _savePath = $"Assets{path.Substring(assetsPath.Length)}";
                }
            }
            
            EditorGUILayout.EndHorizontal();

            if (_retargetProfile == null)
            {
                EditorGUILayout.HelpBox("Select Retarget Profile", MessageType.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(_savePath))
            {
                EditorGUILayout.HelpBox("Select the save folder", MessageType.Warning);
                return false;
            }

            if (_sourceCharacter == null && _retargetProfile.sourceCharacter == null)
            {
                EditorGUILayout.HelpBox("Source Character is NULL!", MessageType.Warning);
                return false;
            }
            
            if (_targetCharacter == null && _retargetProfile.targetCharacter == null)
            {
                EditorGUILayout.HelpBox("Target Character is NULL!", MessageType.Warning);
                return false;
            }

            return true;
        }
    }
}