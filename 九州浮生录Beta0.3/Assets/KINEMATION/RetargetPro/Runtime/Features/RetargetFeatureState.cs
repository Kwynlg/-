// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEditor;
using UnityEngine;

namespace KINEMATION.RetargetPro.Runtime.Features
{
    public abstract class RetargetFeatureState
    {
        protected KRigComponent sourceRigComponent;
        protected KRigComponent targetRigComponent;

#if UNITY_EDITOR
        private static void DrawPyramid(Vector3 point1, Vector3 point2, Vector3 size, Color color)
        {
            Vector3 direction = (point2 - point1).normalized;
            
            Vector3 baseCenter = point2;
            Vector3 up = Vector3.Cross(direction, Vector3.right).normalized * size.y / 2;
            Vector3 right = Vector3.Cross(direction, up).normalized * size.x / 2;

            Vector3 baseVertex1 = baseCenter + up + right;
            Vector3 baseVertex2 = baseCenter + up - right;
            Vector3 baseVertex3 = baseCenter - up - right;
            Vector3 baseVertex4 = baseCenter - up + right;
            
            Color originalColor = Handles.color;
            Matrix4x4 originalMatrix = Handles.matrix;
            
            Handles.color = color;
            
            Handles.DrawLine(point1, baseVertex1);
            Handles.DrawLine(point1, baseVertex2);
            Handles.DrawLine(point1, baseVertex3);
            Handles.DrawLine(point1, baseVertex4);

            Handles.DrawLine(baseVertex1, baseVertex2);
            Handles.DrawLine(baseVertex2, baseVertex3);
            Handles.DrawLine(baseVertex3, baseVertex4);
            Handles.DrawLine(baseVertex4, baseVertex1);
            
            Handles.color = originalColor;
            Handles.matrix = originalMatrix;
        }
        
        private static void DrawWireSphere(Vector3 position, float radius, Color color)
        {
            Color originalColor = Handles.color;
            
            Handles.color = color;
            
            Handles.DrawWireArc(position, Vector3.up, Vector3.forward, 360, radius);
            Handles.DrawWireArc(position, Vector3.right, Vector3.up, 360, radius);
            Handles.DrawWireArc(position, Vector3.forward, Vector3.right, 360, radius);
            
            Handles.color = originalColor;
        }
        
        public void RenderBoneChain(KTransformChain chain, Color color)
        {
            if (chain == null)
            {
                return;
            }
            
            int num = chain.transformChain.Count;
            for (int i = 0; i < num; i++)
            {
                Transform bone = chain.transformChain[i];
                Transform nextBone = null;

                if (i < num - 1)
                {
                    nextBone = chain.transformChain[i + 1];
                }
                
                if (nextBone == null || nextBone != null && !nextBone.IsChildOf(bone))
                {
                    nextBone = bone.childCount == 1 ? bone.GetChild(0) : null;
                }
                
                Vector3 origin = bone.position;
                
                if (nextBone != null)
                {
                    Vector3 target = nextBone.position;
                    float size = (target - origin).magnitude * 0.15f;
                    DrawPyramid(target, origin, new Vector3(size, size, size), color);
                    continue;
                }

                float radius = 0.02f;
                if (bone.parent is var parent && parent != null)
                {
                    radius = (parent.position - bone.position).magnitude * 0.07f;
                }
                
                DrawWireSphere(origin, radius, color);
            }
        }
        
        public virtual void OnSceneGUI()
        {
        }
#endif

        public void InitializeComponents(KRigComponent sourceComponent, KRigComponent targetComponent, 
            RetargetFeature featureAsset)
        {
            sourceRigComponent = sourceComponent;
            targetRigComponent = targetComponent;
            Initialize(featureAsset);
        }
        
        public virtual bool IsValid()
        {
            return false;
        }

        public virtual void Retarget(float time = 0f)
        {
        }

        public virtual void OnDestroy()
        {
        }
        
        protected Transform GetSourceRoot()
        {
            return sourceRigComponent.transform.root;
        }

        protected Transform GetTargetRoot()
        {
            return targetRigComponent.transform.root;
        }
        
        protected virtual void Initialize(RetargetFeature newAsset)
        {
        }
    }
}