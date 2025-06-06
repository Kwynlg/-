// Designed by KINEMATION, 2024.

using KINEMATION.RetargetPro.Runtime;
using KINEMATION.RetargetPro.Runtime.Features;
using KINEMATION.RetargetPro.Runtime.Features.BasicRetargeting;
using KINEMATION.ScriptableWidget;

using UnityEditor;
using UnityEngine;

namespace KINEMATION.RetargetPro.Editor
{
    [CustomEditor(typeof(RetargetProfile))]
    public class RetargetProfileEditor : UnityEditor.Editor
    {
        private ScriptableComponentListWidget _scriptableWidgetList;
        private RetargetProfile _retargetProfile;

        private int _prevSelected = -1;
        
        private void OnEnable()
        {
            _retargetProfile = target as RetargetProfile;
            if (_retargetProfile == null) return;
            
            _scriptableWidgetList = new ScriptableComponentListWidget("Retarget Feature");
            _scriptableWidgetList.Init(serializedObject, typeof(RetargetFeature), 
                "retargetFeatures", false);

            _scriptableWidgetList.onComponentSelected = (index) =>
            {
                if(_prevSelected > -1) _retargetProfile.retargetFeatures[_prevSelected].drawGizmos = false;
                _retargetProfile.retargetFeatures[index].drawGizmos = true;
                _prevSelected = index;
            };

            _scriptableWidgetList.onComponentAdded = () =>
            {
                _retargetProfile.OnRigUpdated();
            };

            _scriptableWidgetList.onDrawComponentHeader = (property, rect) =>
            {
                RetargetFeature coreFeature = property.objectReferenceValue as RetargetFeature;
                if (coreFeature == null) return;
                
                BasicRetargetFeature feature = coreFeature as BasicRetargetFeature;

                float paddingRight = 0f;
                float iconPadding = 6f;
                float iconWidth = 24f;
                rect.width -= paddingRight + iconWidth + iconPadding;
                
                if (feature == null)
                {
                    EditorGUI.LabelField(rect, property.objectReferenceValue.name);
                }
                else
                {
                    string label = feature.sourceChain;
                    label = string.IsNullOrEmpty(label) ? "None" : label;
                    EditorGUI.LabelField(rect, label);
                    
                    GUIStyle labelStyle = GUI.skin.label;
                    float labelWidth = labelStyle.CalcSize(new GUIContent(feature.targetChain)).x;

                    float rectX = rect.x;
                    rect.x += Mathf.Min(rect.width / 2f, rect.width - labelWidth);
                    
                    label = feature.targetChain;
                    label = string.IsNullOrEmpty(label) ? "None" : label;
                    EditorGUI.LabelField(rect, label);
                    rect.x = rectX;
                }
                
                rect.x += rect.width;
                rect.width = iconWidth;
                
                bool enabled = EditorGUI.Toggle(rect, coreFeature.featureWeight > 0f);

                if (coreFeature.featureWeight == 0f && enabled)
                {
                    coreFeature.featureWeight = 1f;
                    EditorUtility.SetDirty(_retargetProfile);
                }

                if (coreFeature.featureWeight > 0f && !enabled)
                {
                    coreFeature.featureWeight = 0f;
                    EditorUtility.SetDirty(_retargetProfile);
                }
            };

            _scriptableWidgetList.headerSpaceRatio = 0.7f;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            _scriptableWidgetList.OnGUI();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}