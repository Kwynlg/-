// Designed by KINEMATION, 2024.

using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;

namespace KINEMATION.RetargetPro.Runtime.Features
{
    public class RetargetUtility
    {
        public static KTransformChain GetTransformChain(KRigComponent rigComponent, KRig rigAsset, string chainName)
        {
            if (rigComponent == null || rigAsset == null)
            {
                return null;
            }

            var rigChain = rigAsset.GetElementChainByName(chainName);
            if (rigChain == null)
            {
                return null;
            }

            KTransformChain output = new KTransformChain();

            foreach (var element in rigChain.elementChain)
            {
                Transform bone = rigComponent.GetRigTransform(element.name);

                if (bone == null)
                {
                    Debug.LogWarning($"Failed to find {element.name} of {rigAsset.name} {rigChain.chainName}");
                    continue;
                }
                
                output.transformChain.Add(bone);
            }
            
            return output;
        }
    }
}