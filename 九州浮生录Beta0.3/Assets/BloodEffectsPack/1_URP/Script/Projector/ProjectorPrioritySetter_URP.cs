using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace BloodEffectsPack
{
    public class ProjectorPrioritySetter_URP : MonoBehaviour
    {

        private int priority = 0;
        // Start is called before the first frame update

        void OnEnable()
        {
            SetPriority();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetPriority()
        {
            DecalProjector decalProjector = GetComponent<DecalProjector>();
            if (decalProjector == null)
            {
                Debug.LogError("No DecalProjector component found on this GameObject.");
                return;
            }

            float timeSinceStart = Time.time;
            priority = Mathf.RoundToInt((timeSinceStart / 0.25f) % 101) - 50;
            decalProjector.material.SetFloat("_DrawOrder", priority);

        }
    }
}
