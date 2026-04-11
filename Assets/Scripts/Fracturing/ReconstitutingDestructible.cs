using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ReconstitutingDestructible : MonoBehaviour
{
    [Header("Basics")]
    [SerializeField] private GameObject destructiblePrefab;
    [SerializeField] private float lifetime = 1f;
    private float curLifetime;
    
    [Header("Easing")]
    [SerializeField] private float easingDuration = 1f; 
    [SerializeField] private EasingFunction.Ease easingFunction;

    [Header("Extras")] [SerializeField]
    private Transform transformAndFinaleSpot;
    [SerializeField] private GameObject transformParticle;
    [SerializeField] private bool parentFragmentParticle;
    [SerializeField] private GameObject fragmentParticle;
    [SerializeField] private GameObject finaleParticle;
    
    private List<GameObject> fragments = new List<GameObject>();
    private List<Vector3> fragmentPositions = new List<Vector3>();
    private List<Vector3> fragmentRotations = new List<Vector3>();
    private CustomFracture fractureController;
    private bool reconstituting;
    private bool delaying;

    private void Start()
    {
        fractureController = destructiblePrefab.GetComponent<CustomFracture>();
        if (!transformAndFinaleSpot)
        {
            transformAndFinaleSpot = transform;
        }
    }

    private void Update()
    {
        if (!destructiblePrefab.activeSelf && !reconstituting)
        {
            foreach (Transform child in fractureController.fragmentRoot.transform)
            {
                fragments.Add(child.gameObject);
                fragmentPositions.Add(child.position);
                fragmentRotations.Add(child.rotation.eulerAngles);
            }
            reconstituting = true;
            delaying = true;
            curLifetime = lifetime;
        }

        if (delaying && curLifetime > 0f)
        {
            curLifetime -= Time.deltaTime;
        } else if (reconstituting && delaying)
        {
            delaying = false;
            StartCoroutine(LerpFragmentsToPosition());
            if (transformParticle)
            {
                Instantiate(transformParticle, transformAndFinaleSpot.position, transformAndFinaleSpot.rotation);
            }
        }
    }

    private IEnumerator LerpFragmentsToPosition()
    {
        float elapsedTime = 0f;
        List<Vector3> initialPositions = new List<Vector3>();
        List<Vector3> initialRotations = new List<Vector3>();

        EasingFunction.Function func = EasingFunction.GetEasingFunction(easingFunction);

        //Prep
        for (int i = 0; i < fragments.Count; i++)
        {
            initialPositions.Add(fragments[i].transform.position);
            initialRotations.Add(fragments[i].transform.rotation.eulerAngles);
            fragments[i].GetComponent<Rigidbody>().isKinematic = true;
            
            if (fragmentParticle)
            {
                GameObject fragPart = Instantiate(fragmentParticle, fragments[i].transform.position, quaternion.identity);
                if (parentFragmentParticle)
                {
                    fragPart.transform.parent = fragments[i].transform;
                }
            }
        }
        
        //Transition
        while (elapsedTime < easingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / easingDuration);

            for (int i = 0; i < fragments.Count; i++)
            {
                Vector3 newPosition = new Vector3(
                    func(initialPositions[i].x, fragmentPositions[i].x, t),
                    func(initialPositions[i].y, fragmentPositions[i].y, t),
                    func(initialPositions[i].z, fragmentPositions[i].z, t)
                );
                fragments[i].transform.position = newPosition;
                
                Vector3 newRotation = new Vector3(
                    func(initialRotations[i].x, fragmentRotations[i].x, t),
                    func(initialRotations[i].y, fragmentRotations[i].y, t),
                    func(initialRotations[i].z, fragmentRotations[i].z, t)
                );
                fragments[i].transform.rotation = Quaternion.Euler(newRotation);
            }

            yield return null;
        }
        
        //Once in position, destroy all fragments and clear stuff, then re-enable original object
        for (int i = 0; i < fragments.Count; i++)
        {
            Destroy(fragments[i]);
        }
        fragments.Clear();
        fragmentPositions.Clear();
        fragmentRotations.Clear();
        Destroy(fractureController.fragmentRoot);
        destructiblePrefab.SetActive(true);
        reconstituting = false;
        
        if (finaleParticle)
        {
            Instantiate(finaleParticle, transformAndFinaleSpot.position, transformAndFinaleSpot.rotation);
        }
    }

}
