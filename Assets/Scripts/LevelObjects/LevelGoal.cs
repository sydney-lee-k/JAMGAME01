using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(SpriteRenderer))]
public class LevelGoal : MonoBehaviour
{
    [SerializeField] private SceneTransitionManager.SceneName newScene;
    private SphereCollider sphereCollider;
    private SpriteRenderer spriteRenderer;
    private int levelKeysCount;

    public static event Action OnNotEnoughKeys;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sphereCollider.enabled = false;
        spriteRenderer.enabled = false;
        levelKeysCount = FindObjectsByType<KeyPickup>(FindObjectsSortMode.None).Length;
        KeyPickup.OnLastKeyGather += () =>
        {
            sphereCollider.enabled = true;
            spriteRenderer.enabled = true;
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerInventory playerInventory))
        {
            if (playerInventory.KeyCount >= levelKeysCount)
            {
                playerInventory.ResetKeys();
                SceneTransitionManager.Instance.StartSceneTransition(newScene, SceneTransitionManager.TransitionStyle.Datamosh);
            }
            else
            {
                OnNotEnoughKeys?.Invoke();
            }
        }
    }
}