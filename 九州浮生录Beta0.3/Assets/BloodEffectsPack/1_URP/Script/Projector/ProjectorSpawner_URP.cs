using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace BloodEffectsPack
{
    public class ProjectorSpawner_URP : MonoBehaviour
    {
        [HideInInspector] public int renderingLayerMask = 1; // Default to the first rendering layer.

        public bool destroyAfter = true;
        public GameObject sourcePrefab;
        private GameObject currentPrefab;
        private Coroutine currentCoroutine;

        [Header("Loop")]
        public bool isLoop = false;
        public float loopingLifetime = Mathf.Infinity;
        private float loopingLifetimeCounter = 0.0f;

        [Header("Delay")]
        public float delay_min = 0.0f;
        public float delay_max = 0.0f;

        [Header("Lifetime")]
  
        public float lifetime_min = 1.0f;
        public float lifetime_max = 1.0f;
        private float lifetimeCounter = 0.0f;

        [Header("StartPos")]
        public Vector3 startPosOffset = Vector3.zero;

        [Header("Size")]
        public float startSize_min = 1.0f;
        public float startSize_max = 1.0f;

        [Header("Rotation")]
        public float startRotation_min = 0.0f;
        public float startRotation_max = 0.0f;

        [Header("CurveControl")]
        public AnimationCurve frameCurve;
        public AnimationCurve scaleCurve;
        public AnimationCurve opacityCurve;



        void OnEnable()
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            lifetimeCounter = 0.0f;
            loopingLifetimeCounter = 0.0f;

            if (currentPrefab != null)
                Destroy(currentPrefab.gameObject);

            currentCoroutine = StartCoroutine(Spawn());
        }
        public void ResetAndInitialize(int value)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            lifetimeCounter = 0.0f;
            loopingLifetimeCounter = 0.0f;
            if (currentPrefab != null)
                Destroy(currentPrefab.gameObject);

            renderingLayerMask = value;
            currentCoroutine = StartCoroutine(Spawn());
        }


        void Update()
        {
        
        }
        private Material originalMat;
        private IEnumerator Spawn()
        {
            float delay = Random.Range(delay_min, delay_max);
            float lifetime = Random.Range(lifetime_min, lifetime_max);
            float startProjectorSize = Random.Range(startSize_min, startSize_max);
            float startRotation = Random.Range(startRotation_min, startRotation_max);
            yield return new WaitForSeconds(delay);
            currentPrefab = Instantiate(sourcePrefab);

            DecalProjector currentProjector = currentPrefab.GetComponent<DecalProjector>();
            originalMat = currentProjector.material;
            currentProjector.material = null;
            currentProjector.material = Instantiate(originalMat);

            currentProjector.renderingLayerMask = (uint)renderingLayerMask;

            currentPrefab.transform.SetParent(transform);
            currentPrefab.GetComponent<ProjectorPrioritySetter_URP>().SetPriority();

            currentPrefab.transform.localPosition = startPosOffset;


            currentPrefab.transform.localScale = Vector3.one;
            currentPrefab.transform.localEulerAngles = new Vector3(90f, 0f, 0.0f);
            currentPrefab.transform.RotateAround(transform.position, Vector3.up, startRotation);


            while (lifetimeCounter <= lifetime && loopingLifetimeCounter <= loopingLifetime)
            {
                lifetimeCounter += Time.deltaTime;
                loopingLifetimeCounter += Time.deltaTime;

                float samplePos = Mathf.Clamp01(lifetimeCounter / lifetime);

                float scaleValue = scaleCurve.Evaluate(samplePos);
                int frameValue = Mathf.FloorToInt(frameCurve.Evaluate(samplePos));
                float opacityValue = opacityCurve.Evaluate(samplePos);


       

                currentProjector.GetComponent<ProjectorSpriteController_URP>().SetFrameIndex(frameValue);

                currentProjector.fadeFactor = opacityValue;
                currentProjector.size = new Vector3(scaleValue * startProjectorSize, scaleValue * startProjectorSize, 10.0f);
                currentProjector.pivot = new Vector3(0f, 0f, 5f);


                if (isLoop && (lifetimeCounter > lifetime))
                {
                    lifetimeCounter = lifetimeCounter - lifetime;


                    currentPrefab.transform.localPosition = startPosOffset;
                    startRotation = Random.Range(startRotation_min, startRotation_max);
                    currentPrefab.transform.localEulerAngles = new Vector3(90f, 0f, 0.0f);
                    currentPrefab.transform.RotateAround(transform.position, Vector3.up, startRotation);


                    currentPrefab.GetComponent<ProjectorPrioritySetter_URP>().SetPriority();
           

                }



                yield return null;
            }
            if(destroyAfter)
                Destroy(gameObject);
        }
        private void OnDestroy()
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
        }
    }
}
