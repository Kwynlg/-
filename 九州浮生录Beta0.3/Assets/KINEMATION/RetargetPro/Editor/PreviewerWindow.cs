// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.RetargetPro.Editor
{
    public class PreviewerWindow : EditorWindow
    {
        private AnimationClip _clipToPreview;
        private float _sliderValue;

        private GameObject _character;
        private KRigComponent _rigComponent;

        private bool _isPlaying;
        
        [MenuItem("Window/KINEMATION/Animation Previewer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PreviewerWindow), false, "Animation Previewer");
        }

        private void OnGUI()
        {
            _character = (GameObject) EditorGUILayout.ObjectField("Character", _character, 
                typeof(GameObject), true);
            
            _clipToPreview = (AnimationClip) EditorGUILayout.ObjectField("Animation", _clipToPreview, 
                typeof(AnimationClip), false);
            
            if (_character == null)
            {
                EditorGUILayout.HelpBox("Select Character", MessageType.Warning);
                return;
            }
            
            if (_clipToPreview == null)
            {
                EditorGUILayout.HelpBox("Select Animation", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Play"))
            {
                _rigComponent = _character.GetComponentInChildren<KRigComponent>();
                if (_rigComponent == null) return;
                
                _rigComponent.Initialize();
                _rigComponent.CacheHierarchyPose();
                
                _isPlaying = true;
            }
            
            if (GUILayout.Button("Stop") && _isPlaying)
            {
                _isPlaying = false;
                if (_rigComponent != null) _rigComponent.ApplyHierarchyCachedPose();
            }

            if (_isPlaying)
            {
                _sliderValue = EditorGUILayout.Slider(_sliderValue, 0f, _clipToPreview.length);
                _clipToPreview.SampleAnimation(_character, _sliderValue);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void OnDisable()
        {
            if (_isPlaying && _rigComponent != null) _rigComponent.ApplyHierarchyCachedPose();
        }
    }
}