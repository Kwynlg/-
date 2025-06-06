// Designed by KINEMATION, 2024.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KINEMATION.RetargetPro.Editor
{
    public struct AnimationRetargetTask
    {
        public AnimationClip[] clips;
        public RuntimeAnimatorController controller;
    }
    
    public class RetargetContextWindow : EditorWindow
    {
        private const string MenuItemName = "Assets/Animation Retargeting";

        private RetargetAnimBaker _animBaker;
        private AnimationRetargetTask[] _tasks;

        private void RenderPostProcessor()
        {
            if (!_animBaker.Render())
            {
                return;
            }

            if (!GUILayout.Button("Retarget Animations")) return;
            
            _animBaker.InitializeBaker();

            foreach (var task in _tasks)
            {
                bool isController = task.controller != null;
                if (isController && task.clips == null) continue;

                AnimatorOverrideController overrideController = new AnimatorOverrideController(task.controller);
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                List<AnimationClip> bakedClips = isController ? new List<AnimationClip>() : null;
                foreach (var clip in task.clips)
                {
                    AnimationClip bakedClip = _animBaker.BakeAnimation(clip);

                    if (isController && bakedClip != null)
                    {
                        overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, bakedClip));
                        bakedClips.Add(bakedClip);
                    }
                }

                if (!isController) continue;

                for (int i = 0; i < overrides.Count; i++)
                {
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, bakedClips[i]);
                }

                overrideController.ApplyOverrides(overrides);
                RuntimeAnimatorController newAsset = overrideController;

                string path = _animBaker.GetTargetDirectory();
                string controllerName = $"{_animBaker.GetTargetName()}_{task.controller.name}.controller";

                path = $"{path}/{controllerName}";

                AssetDatabase.CreateAsset(newAsset, AssetDatabase.GenerateUniqueAssetPath(path));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            _animBaker.UnInitializeBaker();
        }

        private void OnEnable()
        {
            _animBaker = new RetargetAnimBaker();
        }

        private void OnGUI()
        {
            _animBaker.Render();
        }

        private static AnimationRetargetTask[] TryGetAnimationTasks()
        {
            if (Selection.objects == null) return null;
            
            List<AnimationClip> clips = null;
            List<AnimationRetargetTask> tasks = null;

            foreach (var selection in Selection.objects)
            {
                if (selection is AnimationClip clip)
                {
                    clips ??= new List<AnimationClip>();
                    clips.Add(clip);
                    continue;
                }

                if (selection is RuntimeAnimatorController controller && controller.animationClips.Length > 0)
                {
                    tasks ??= new List<AnimationRetargetTask>();
                    tasks.Add(new AnimationRetargetTask()
                    {
                        clips = controller.animationClips,
                        controller = controller
                    });
                }
                
                string assetPath = AssetDatabase.GetAssetPath(selection);
                if (!assetPath.ToLower().EndsWith(".fbx")) continue;
                
                foreach (Object subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath))
                {
                    clip = subAsset as AnimationClip;
                    if (clip == null) continue;

                    clips ??= new List<AnimationClip>();
                    clips.Add(clip);
                }
            }

            if (clips != null)
            {
                tasks ??= new List<AnimationRetargetTask>();
                tasks.Add(new AnimationRetargetTask()
                {
                    clips = clips.ToArray(),
                    controller = null
                });
            }

            return tasks?.ToArray();
        }
        
        [MenuItem(MenuItemName, true)]
        private static bool ValidateRetargetAnimations()
        {
            var animations = TryGetAnimationTasks();
            return animations != null;
        }

        [MenuItem(MenuItemName)]
        private static void RetargetAnimations()
        {
            var tasks = TryGetAnimationTasks();
            if (tasks == null) return;

            RetargetContextWindow window = GetWindow<RetargetContextWindow>(false, 
                "Retarget Animations", true);

            window._tasks = tasks;
            window.Show();
        }
    }
}
