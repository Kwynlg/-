// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Editor.Misc;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.RetargetPro.Editor
{
    public class RetargetProWindow : EditorWindow
    {
        private AnimationClip _animation;
        private AnimationClip _itemAnimation;
        private RetargetAnimBaker _retargetAnimBaker;
        
        private bool _looping;
        
        // Will bake the motion into an animation clip.
        private float _sliderValueCache = -1f;
        private float _sliderValue = -1f;

        private UnityEditor.Editor _profileEditor;
        private KToolbarWidget _toolbarWidget;
        private Vector2 _scrollPosition;

        private bool _autoPlay = false;
        private float _repaintTimer = 0f;

        private float _lastFrameTime = 0f;
        
        [MenuItem("Window/KINEMATION/Retargeting Pro")]
        public static void ShowWindow()
        {
            GetWindow(typeof(RetargetProWindow), false, "Retargeting Pro");
        }
        
        private void UpdatePlaybackSlider()
        {
            float sliderCache = _sliderValue;
            _sliderValue = EditorGUILayout.Slider(_sliderValue, 0f, _animation.length);

            if (!Mathf.Approximately(sliderCache, _sliderValue))
            {
                _autoPlay = false;
            }
        }

        private void StopRetargetPreview()
        {
            EditorApplication.update -= SampleAnimation;
            _retargetAnimBaker.UnInitializeBaker();
            _sliderValue = _sliderValueCache = 0f;
            _autoPlay = false;
        }
        
        private void RenderBaker()
        {
            if (!_retargetAnimBaker.Render())
            {
                if (_retargetAnimBaker.IsInitialized)
                {
                    StopRetargetPreview();
                }
                return;
            }
            
            var content = new GUIContent("Animation", "The clip to retarget.");
            _animation = (AnimationClip) EditorGUILayout.ObjectField(content, _animation, 
                typeof(AnimationClip), false);
            
            content = new GUIContent("Item Animation", "Animation clip for the weapon or item.");
            _itemAnimation = (AnimationClip) EditorGUILayout.ObjectField(content, _itemAnimation, 
                typeof(AnimationClip), false);
            
            if (_animation == null)
            {
                if (_retargetAnimBaker.IsInitialized)
                {
                    StopRetargetPreview();
                }
                
                EditorGUILayout.HelpBox("Select Animation", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Play") && !_retargetAnimBaker.IsInitialized)
            {
                EditorApplication.update += SampleAnimation;
                
                _retargetAnimBaker.InitializeBaker();
                if (!_retargetAnimBaker.IsInitialized)
                {
                    return;
                }
                
                _retargetAnimBaker.RetargetAtTime(_animation, _itemAnimation, 0f);
            }
            
            if (GUILayout.Button("Stop") && _retargetAnimBaker.IsInitialized)
            {
                StopRetargetPreview();
            }

            if (GUILayout.Button("Loop") && _retargetAnimBaker.IsInitialized)
            {
                _autoPlay = true;
                _lastFrameTime = (float) EditorApplication.timeSinceStartup;
                _repaintTimer = 0f;
            }
            
            UpdatePlaybackSlider();
            
            EditorGUILayout.EndHorizontal();
            
            _retargetAnimBaker.RetargetAtTime(_animation, _itemAnimation, _sliderValue);

            if (GUILayout.Button("Bake Animation"))
            {
                if(!_retargetAnimBaker.IsInitialized) _retargetAnimBaker.InitializeBaker();
               _retargetAnimBaker.BakeAnimation(_animation);
               
               AssetDatabase.SaveAssets();
               AssetDatabase.Refresh();
            }
        }

        private void RenderProfile()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
            }

            if (_profileEditor != null)
            {
                _profileEditor.OnInspectorGUI();
                _retargetAnimBaker.RetargetAtTime(_animation, _itemAnimation, _sliderValue);
            }
            else
            {
                EditorGUILayout.HelpBox("Select a Retarget Profile", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void OnEnable()
        {
            _retargetAnimBaker = new RetargetAnimBaker();
            
            _retargetAnimBaker.onProfileChanged = profile =>
            {
                _profileEditor = profile == null ? null : UnityEditor.Editor.CreateEditor(profile);
            };

            _toolbarWidget = new KToolbarWidget(new KToolbarTab[]
            {
                new KToolbarTab()
                {
                    name = "Baker",
                    onTabRendered = RenderBaker
                },
                new KToolbarTab()
                {
                    name = "Retarget Profile",
                    onTabRendered = RenderProfile
                }
            });
        }

        private void SampleAnimation()
        {
            if (Mathf.Approximately(_sliderValue, _sliderValueCache) && !_autoPlay)
            {
                return;
            }
            
            if (_autoPlay)
            {
                float deltaTime = (float) EditorApplication.timeSinceStartup - _lastFrameTime;
                _sliderValue = _sliderValue + deltaTime > _animation.length ? 0f : _sliderValue + deltaTime;

                if (_repaintTimer > 1f / 60f)
                {
                    _repaintTimer = 0f;
                    Repaint();
                }

                _repaintTimer += deltaTime;
            }
            
            _retargetAnimBaker.RetargetComponent.OnSceneGUI();
            _retargetAnimBaker.RetargetAtTime(_animation, _itemAnimation, _sliderValue);

            _sliderValueCache = _sliderValue;
            _lastFrameTime = (float) EditorApplication.timeSinceStartup;
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5f);
            EditorGUILayout.BeginVertical();
            
            _toolbarWidget.Render();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= SampleAnimation;
            _retargetAnimBaker.UnInitializeBaker();
        }
    }
}
