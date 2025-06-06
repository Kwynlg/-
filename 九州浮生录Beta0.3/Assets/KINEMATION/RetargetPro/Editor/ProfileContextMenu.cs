// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using KINEMATION.RetargetPro.Runtime;
using KINEMATION.RetargetPro.Runtime.Features;
using KINEMATION.RetargetPro.Runtime.Features.IKRetargeting;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

using FuzzySharp;

namespace KINEMATION.RetargetPro.Editor
{
    public class ProfileContextMenu
    {
        private const string MenuItemName = "Assets/Create Retarget Profile";

        private static KRig[] GetRigAssets()
        {
            if (Selection.objects.Length < 2)
            {
                return null;
            }

            List<KRig> result = null;

            foreach (var selection in Selection.objects)
            {
                if(selection is not KRig) continue;

                result ??= new List<KRig>();
                result.Add(selection as KRig);

                if (result.Count == 2) break;
            }
            
            return result?.ToArray();
        }
        
        [MenuItem(MenuItemName, true)]
        private static bool ValidateCreateRetargetProfile()
        {
            return GetRigAssets() != null;
        }

        [MenuItem(MenuItemName)]
        private static void CreateRetargetProfile()
        {
            KRig[] rigAssets = GetRigAssets();
            if (rigAssets == null) return;

            string path = AssetDatabase.GetAssetPath(rigAssets[0]);
            path = path.Substring(0, path.LastIndexOf('/'));

            path = $"{path}/Retarget_{rigAssets[0].name}_{rigAssets[1].name}.asset";

            RetargetProfile newProfile = (RetargetProfile) ScriptableObject.CreateInstance(typeof(RetargetProfile));

            newProfile.sourceRig = rigAssets[0];
            newProfile.targetRig = rigAssets[1];
            newProfile.retargetFeatures = new List<RetargetFeature>();
            
            Undo.RegisterCreatedObjectUndo(newProfile, "Create Retarget Profile");
            AssetDatabase.CreateAsset(newProfile, AssetDatabase.GenerateUniqueAssetPath(path));

            foreach (var sourceChain in rigAssets[0].rigElementChains)
            {
                int bestScore = -1;
                string targetChainName = string.Empty;
                
                foreach (var targetChain in rigAssets[1].rigElementChains)
                {
                    string a = Regex.Replace(sourceChain.chainName, @"\d", "");
                    string b = Regex.Replace(targetChain.chainName, @"\d", "");
                    
                    int score = Fuzz.WeightedRatio(a, b) + Fuzz.PartialRatio(a, b);
                    if (score > bestScore)
                    {
                        targetChainName = targetChain.chainName;
                        bestScore = score;
                    }
                }
                
                if (bestScore < 120)
                {
                    Debug.LogWarning($"Couldn't map {sourceChain.chainName}: no similar chains found.");
                    targetChainName = "None";
                }
                
                IKRetargetFeature feature = (IKRetargetFeature) 
                    ScriptableObject.CreateInstance(typeof(IKRetargetFeature));
                    
                feature.ikWeight = 0f;

                feature.name = feature.GetType().Name;
                feature.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

                feature.sourceRig = rigAssets[0];
                feature.targetRig = rigAssets[1];
                    
                feature.sourceChain = sourceChain.chainName;
                feature.targetChain = targetChainName;
                    
                Undo.RegisterCreatedObjectUndo(feature, "Create Retarget Feature");
                newProfile.retargetFeatures.Add(feature);
                    
                AssetDatabase.AddObjectToAsset(feature, newProfile);
            }
            
            EditorUtility.SetDirty(newProfile);
            AssetDatabase.SaveAssets();
        }
    }
}