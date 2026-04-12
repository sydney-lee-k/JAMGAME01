using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class KeyPickup : MonoBehaviour
{
    public static event Action OnKeyGather;
    public static event Action OnLastKeyGather;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerInventory playerInventory))
        {
            if (GameObject.FindGameObjectsWithTag("Shard").Length <= 1)
            {
                OnLastKeyGather?.Invoke();
            }
            else OnKeyGather?.Invoke();
            Destroy(gameObject);
        }
    }
}
