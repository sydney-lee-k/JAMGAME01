using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ConstitutingDestructible : MonoBehaviour
{
    [Header("Basics")]
    [SerializeField] private GameObject destructiblePrefab;
    [SerializeField] private float lifetime = 1f;
    private float curLifetime;

    [Header("Easing")]
    [SerializeField] private float easingDuration = 1f;
    [SerializeField] private EasingFunction.Ease easingFunction;

    [Header("FragmentSettings")]
    [SerializeField] private bool startScaleAtZero;
    [SerializeField] private bool startKinematic;
    [SerializeField] private Vector3 globalOffset;
    [SerializeField] private Vector3 randomOffsetMin, randomOffsetMax;

    [Header("Extras")]
    [SerializeField] private Transform transformAndFinaleSpot;
    [SerializeField] private GameObject transformParticle;
    [SerializeField] private bool parentFragmentParticle;
    [SerializeField] private GameObject fragmentParticle;
    [SerializeField] private GameObject finaleParticle;

    private List<GameObject> fragments = new();
    private List<Vector3> fragmentPositions = new();
    private List<Vector3> fragmentRotations = new();
    private List<Vector3> fragmentScale = new();

    private CustomFracture fractureController;

    private bool reconstituting;
    private bool initialized;

    private void Start()
    {
        fractureController = destructiblePrefab.GetComponent<CustomFracture>();

        if (!transformAndFinaleSpot)
            transformAndFinaleSpot = transform;

        InitiateDestruction();
    }

    private void InitiateDestruction()
    {
        fractureController.CauseFracture();

        fragments.Clear();
        fragmentPositions.Clear();
        fragmentRotations.Clear();
        fragmentScale.Clear();

        foreach (Transform child in fractureController.fragmentRoot.transform)
        {
            GameObject frag = child.gameObject;
            fragments.Add(frag);
            
            Vector3 originalPos = frag.transform.position;
            
            Vector3 displacedPos = originalPos + globalOffset;
            
            Vector3 randomOffset = new Vector3(
                Random.Range(randomOffsetMin.x, randomOffsetMax.x),
                Random.Range(randomOffsetMin.y, randomOffsetMax.y),
                Random.Range(randomOffsetMin.z, randomOffsetMax.z)
            );

            Vector3 finalStartPos = displacedPos + randomOffset;

            frag.transform.position = finalStartPos;
            
            fragmentPositions.Add(originalPos);
            
            fragmentRotations.Add(frag.transform.rotation.eulerAngles);
            fragmentScale.Add(frag.transform.localScale);

            if (startKinematic && frag.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
            if (startScaleAtZero) frag.transform.localScale = Vector3.zero;
        }

        initialized = true;
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Constitute();
        }
    }

    public void Constitute()
    {
        if (!initialized || reconstituting) return;

        StartCoroutine(LerpFragmentsToPosition());
    }

    private IEnumerator LerpFragmentsToPosition()
    {
        reconstituting = true;

        float elapsedTime = 0f;
        float t;

        var func = EasingFunction.GetEasingFunction(easingFunction);

        List<Vector3> startPositions = new();
        List<Quaternion> startRotations = new();
        List<Vector3> startScales = new();

        // Capture starting state
        for (int i = 0; i < fragments.Count; i++)
        {
            if (fragments[i] == null) continue;

            startPositions.Add(fragments[i].transform.position);
            startRotations.Add(fragments[i].transform.rotation);
            startScales.Add(fragments[i].transform.localScale);

            if (fragments[i].TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;

            if (fragmentParticle)
            {
                var fx = Instantiate(fragmentParticle, fragments[i].transform.position, Quaternion.identity);

                if (parentFragmentParticle)
                    fx.transform.SetParent(fragments[i].transform);
            }
        }

        // Animate
        while (elapsedTime < easingDuration)
        {
            elapsedTime += Time.deltaTime;
            t = Mathf.Clamp01(elapsedTime / easingDuration);

            for (int i = 0; i < fragments.Count; i++)
            {
                if (fragments[i] == null) continue;

                // Position
                fragments[i].transform.position = new Vector3(
                    func(startPositions[i].x, fragmentPositions[i].x, t),
                    func(startPositions[i].y, fragmentPositions[i].y, t),
                    func(startPositions[i].z, fragmentPositions[i].z, t)
                );

                // Rotation (FIXED: quaternion lerp instead of Euler math)
                fragments[i].transform.rotation = Quaternion.Slerp(
                    startRotations[i],
                    Quaternion.Euler(fragmentRotations[i]),
                    t
                );

                // Scale (FIXED: now actually applied)
                fragments[i].transform.localScale = new Vector3(
                    func(startScales[i].x, fragmentScale[i].x, t),
                    func(startScales[i].y, fragmentScale[i].y, t),
                    func(startScales[i].z, fragmentScale[i].z, t)
                );
            }

            yield return null;
        }

        // Cleanup
        foreach (var frag in fragments)
        {
            if (frag) Destroy(frag);
        }

        fragments.Clear();
        fragmentPositions.Clear();
        fragmentRotations.Clear();
        fragmentScale.Clear();

        if (fractureController.fragmentRoot)
            Destroy(fractureController.fragmentRoot);

        if (finaleParticle)
        {
            Instantiate(finaleParticle, transformAndFinaleSpot.position, transformAndFinaleSpot.rotation);
        }
        
        destructiblePrefab.SetActive(true);
        reconstituting = false;
    }
}