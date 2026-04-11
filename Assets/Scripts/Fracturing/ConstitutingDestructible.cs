using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ConstitutingDestructible : MonoBehaviour
{
    [Header("Basics")]
    [SerializeField] private GameObject destructiblePrefab;
    public bool startConstructed = false;

    [Header("Easing")]
    public float easingDuration = 1f;
    [SerializeField] private EasingFunction.Ease easingFunction;

    [Header("Fragment Settings")]
    [SerializeField] private bool startKinematic = true;
    [SerializeField] private Vector3 globalOffset;
    [SerializeField] private Vector3 randomOffsetMin, randomOffsetMax;
    [SerializeField] private bool disableColliderWhenShattered;

    [Tooltip("If scale is negative, original scale is kept")]
    [SerializeField] private float destroyedFragmentScale = -1f;

    #if UNITY_EDITOR
    [Header("Dev Controls")]
    [Tooltip("Calls Constitute()")] [SerializeField] private bool triggerReconstitute;
    [Tooltip("Calls Reshatter()")] [SerializeField] private bool triggerFracture;
    [Tooltip("Calls ResetAssembledState()")] [SerializeField] private bool triggerResetToAssembled;
    [Tooltip("Calls ResetDestroyedState()")] [SerializeField] private bool triggerResetToDestroyed;
    #endif
    
    private List<GameObject> fragments = new();

    // Positions
    private List<Vector3> assembledPositions = new();
    private List<Quaternion> assembledRotations = new();

    private List<Vector3> destroyedPositions = new();
    private List<Quaternion> destroyedRotations = new();

    // NEW: Scales
    private List<Vector3> assembledScales = new();
    private List<Vector3> destroyedScales = new();

    private CustomFracture fractureController;
    
    private bool reconstituting;
    private bool isAssembled = false;

    private void Awake()
    {
        fractureController = destructiblePrefab.GetComponent<CustomFracture>();
        Initialization();
        if (startConstructed) ResetToAssembledShape(); else ResetDestroyedState();
    }
    
    #if UNITY_EDITOR
    //For debugging, manually triggering or undoing fracture to see how it looks. Will not be in the build
    private void Update()
    {
        if (triggerFracture)
        {
            triggerFracture = false;
            Reshatter();
        }
        if (triggerReconstitute)
        {
            triggerReconstitute = false;
            Constitute();
        }
        if (triggerResetToAssembled)
        {
            triggerResetToAssembled = false;
            ResetToAssembledShape();
        }
        if (triggerResetToDestroyed)
        {
            triggerResetToDestroyed = false;
            ResetDestroyedState();
        }
    }
    #endif

    private void Initialization()
    {
        fractureController.CauseFracture();
        fragments.Clear();
        assembledPositions.Clear();
        assembledRotations.Clear();
        destroyedPositions.Clear();
        destroyedRotations.Clear();
        assembledScales.Clear();
        destroyedScales.Clear();

        foreach (Transform child in fractureController.fragmentRoot.transform)
        {
            GameObject frag = child.gameObject;
            fragments.Add(frag);

            Vector3 assembledPos = frag.transform.position;
            Quaternion assembledRot = frag.transform.rotation;
            Vector3 assembledScale = frag.transform.localScale;

            Vector3 displacedPos = assembledPos + globalOffset;

            Vector3 randomOffset = new Vector3(
                Random.Range(randomOffsetMin.x, randomOffsetMax.x),
                Random.Range(randomOffsetMin.y, randomOffsetMax.y),
                Random.Range(randomOffsetMin.z, randomOffsetMax.z)
            );

            Vector3 destroyedPos = displacedPos + randomOffset;

            // Scale logic
            Vector3 destroyedScale = (destroyedFragmentScale >= 0f)
                ? Vector3.one * destroyedFragmentScale
                : assembledScale;
            
            if (startKinematic && frag.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;

            // Store states
            assembledPositions.Add(assembledPos);
            assembledRotations.Add(assembledRot);
            assembledScales.Add(assembledScale);

            destroyedPositions.Add(destroyedPos);
            destroyedRotations.Add(assembledRot);
            destroyedScales.Add(destroyedScale);

            fractureController.enabled = false;
        }
        
        isAssembled = false;
    }

    public void Constitute()
    {
        if (reconstituting || isAssembled) return;

        StartCoroutine(LerpFragments(
            destroyedPositions,
            assembledPositions,
            destroyedRotations,
            assembledRotations,
            destroyedScales,
            assembledScales,
            true
        ));

        isAssembled = true;
    }

    public void Reshatter()
    {
        if (reconstituting || !isAssembled) return;

        StartCoroutine(LerpFragments(
            assembledPositions,
            destroyedPositions,
            assembledRotations,
            destroyedRotations,
            assembledScales,
            destroyedScales,
            false
        ));

        isAssembled = false;
    }

    public void ResetDestroyedState()
    {
        if (reconstituting) return;
        fragments[0].transform.parent.gameObject.SetActive(true);
        destructiblePrefab.SetActive(false);
        for (int i = 0; i < fragments.Count; i++)
        {
            if (fragments[i] == null) continue;

            fragments[i].transform.position = destroyedPositions[i];
            fragments[i].transform.rotation = destroyedRotations[i];
            fragments[i].transform.localScale = destroyedScales[i];
        }

        isAssembled = false;
    }
    
    public void ResetToAssembledShape()
    {
        if (reconstituting) return;
        fragments[0].transform.parent.gameObject.SetActive(true);
        destructiblePrefab.SetActive(false);
        for (int i = 0; i < fragments.Count; i++)
        {
            if (fragments[i] == null) continue;

            fragments[i].transform.position = assembledPositions[i];
            fragments[i].transform.rotation = assembledRotations[i];
            fragments[i].transform.localScale = assembledScales[i];
        }
        fragments[0].transform.parent.gameObject.SetActive(false);
        destructiblePrefab.SetActive(true);
        isAssembled = true;
    }

    private IEnumerator LerpFragments(
        List<Vector3> fromPos,
        List<Vector3> toPos,
        List<Quaternion> fromRot,
        List<Quaternion> toRot,
        List<Vector3> fromScale,
        List<Vector3> toScale,
        bool returnParent
    )
    {
        //Startup
        if (!returnParent)
        {
            fragments[0].transform.parent.gameObject.SetActive(true);
            destructiblePrefab.SetActive(false);
        } else if (disableColliderWhenShattered)
        {
            for (int i = 0; i < fragments.Count; i++)
            {
                fragments[i].GetComponent<Collider>().enabled = true;
            }
        }
        
        reconstituting = true;

        float elapsedTime = 0f;
        var func = EasingFunction.GetEasingFunction(easingFunction);

        while (elapsedTime < easingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / easingDuration);

            for (int i = 0; i < fragments.Count; i++)
            {
                if (fragments[i] == null) continue;
                
                //Lerping Positions, Rotations and Scale
                fragments[i].transform.position = new Vector3(
                    func(fromPos[i].x, toPos[i].x, t),
                    func(fromPos[i].y, toPos[i].y, t),
                    func(fromPos[i].z, toPos[i].z, t)
                );
                
                fragments[i].transform.rotation = Quaternion.Slerp(
                    fromRot[i],
                    toRot[i],
                    t
                );
                
                fragments[i].transform.localScale = new Vector3(
                    func(fromScale[i].x, toScale[i].x, t),
                    func(fromScale[i].y, toScale[i].y, t),
                    func(fromScale[i].z, toScale[i].z, t)
                );
            }

            yield return null;
        }
        reconstituting = false;
        
        //Cleanup
        if (returnParent)
        {
            fragments[0].transform.parent.gameObject.SetActive(false);
            destructiblePrefab.SetActive(true);
        } else if (disableColliderWhenShattered)
        {
            for (int i = 0; i < fragments.Count; i++)
            {
                fragments[i].GetComponent<Collider>().enabled = false;
            }
        }
    }
}