using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;


namespace BloodEffectsPack
{
    [CustomEditor(typeof(BloodEffectsPack.ProjectorSpawner_URP))]
    public class ProjectorSpawner_URP_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            var spawner = (BloodEffectsPack.ProjectorSpawner_URP)target;
            EditorGUILayout.LabelField("Rendering Layers", EditorStyles.boldLabel);

            var pipelineAsset = GraphicsSettings.currentRenderPipeline;
            string[] layerNames = pipelineAsset != null ? pipelineAsset.renderingLayerMaskNames : new string[32];
            if (layerNames == null || layerNames.Length == 0)
                layerNames = new string[] { "Layer 0", "Layer 1", "Layer 2", "Layer 3" };

            spawner.renderingLayerMask = EditorGUILayout.MaskField("Rendering Layer Mask", spawner.renderingLayerMask, layerNames);

            // Draw default inspector
            DrawDefaultInspector();

            // Apply changes
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}
