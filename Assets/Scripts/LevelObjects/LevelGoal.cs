using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LevelGoal : MonoBehaviour
{
    [SerializeField] private SceneTransitionManager.SceneName newScene;
    private int levelKeysCount;

    public static event Action OnNotEnoughKeys;

    private void Start()
    {
        levelKeysCount = FindObjectsByType<KeyPickup>(FindObjectsSortMode.None).Length;
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