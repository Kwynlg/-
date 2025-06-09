using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BloodEffectsPack
{
    public class ContinuousProjectorSpawner_URP : MonoBehaviour
    {
        [HideInInspector] public int renderingLayerMask = 1; // Default to the first rendering layer.

        [Header("Loop")]
        public bool isLoop = false;
        public float loopingLifetime = Mathf.Infinity;
        private float loopingLifetimeCounter = 0.0f;


        public GameObject sourcePrefab;
        [HideInInspector] public Material originalMat;
    
        [Header("Lifetime")]
        public float lifetime_min = 1.0f;
        public float lifetime_max = 1.0f;

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


        [System.Serializable]
        public class SpawnOption
        {
            [HideInInspector] public GameObject currentPrefab;

            [Header("Delay")]
            public float delay_min = 0.0f;
            public float delay_max = 0.0f;

            [Header("Lifetime")]
            [HideInInspector] public float lifetimeCounter = 0.0f;
        }

        private List<Coroutine> spawnCoroutines = new List<Coroutine>();
        [Header("Spawn Options")]
        public List<SpawnOption> spawnOptions = new List<SpawnOption>();



        private void OnEnable()
        {
            StopAllCoroutines();

            spawnCoroutines.Clear();
            foreach (var option in spawnOptions)
            {
                var coroutine = StartCoroutine(Spawn(option));

                spawnCoroutines.Add(coroutine);
                option.lifetimeCounter = 0.0f;
                if (option.currentPrefab != null)
                    Destroy(option.currentPrefab);
            }
        }

        private void Update()
        {
            loopingLifetimeCounter += Time.deltaTime;
            if(loopingLifetimeCounter >= loopingLifetime)
            {
                Debug.Log("Destroyed");
                Destroy(gameObject);
            }
        }

        private IEnumerator Spawn(SpawnOption option)
        {
            float delay = Random.Range(option.delay_min, option.delay_max);
            float lifetime = Random.Range(lifetime_min, lifetime_max);
            float startProjectorSize = Random.Range(startSize_min,startSize_max);
            float startRotation = Random.Range(startRotation_min,startRotation_max);

            yield return new WaitForSeconds(delay);

            option.currentPrefab = Instantiate(sourcePrefab);

            DecalProjector currentProjector = option.currentPrefab.GetComponent<DecalProjector>();
            originalMat = currentProjector.material;
            currentProjector.material = null;
            currentProjector.material = Instantiate(originalMat);

            currentProjector.renderingLayerMask = (uint)renderingLayerMask;


            option.currentPrefab.transform.SetParent(transform);
            option.currentPrefab.GetComponent<ProjectorPrioritySetter_URP>().SetPriority();

            option.currentPrefab.transform.localPosition = startPosOffset;


            option.currentPrefab.transform.localScale = Vector3.one;
            option.currentPrefab.transform.localEulerAngles = new Vector3(90f, 0f, 0.0f);
            option.currentPrefab.transform.RotateAround(transform.position, Vector3.up, startRotation);


            while (option.lifetimeCounter <= lifetime && loopingLifetimeCounter <= loopingLifetime)
            {
                option.lifetimeCounter += Time.deltaTime;
            

                float samplePos = Mathf.Clamp01(option.lifetimeCounter / lifetime);

                float scaleValue = scaleCurve.Evaluate(samplePos);
                int frameValue = Mathf.FloorToInt(frameCurve.Evaluate(samplePos));
                float opacityValue = opacityCurve.Evaluate(samplePos);




                currentProjector.GetComponent<ProjectorSpriteController_URP>().SetFrameIndex(frameValue);

                currentProjector.fadeFactor = opacityValue;
                currentProjector.size = new Vector3(scaleValue * startProjectorSize, scaleValue * startProjectorSize, 10.0f);
                currentProjector.pivot = new Vector3(0f, 0f, 5f);


                if (isLoop && (option.lifetimeCounter > lifetime))
                {
                    option.lifetimeCounter = option.lifetimeCounter - lifetime;


                    option.currentPrefab.transform.localPosition = startPosOffset;
                    startRotation = Random.Range(startRotation_min, startRotation_max);
                    option.currentPrefab.transform.localEulerAngles = new Vector3(90f, 0f, 0.0f);
                    option.currentPrefab.transform.RotateAround(transform.position, Vector3.up, startRotation);



                    option.currentPrefab.GetComponent<ProjectorPrioritySetter_URP>().SetPriority();


                }
                yield return null;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
