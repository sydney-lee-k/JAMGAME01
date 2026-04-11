using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class KeyPickup : MonoBehaviour
{
    public static event Action OnKeyGather;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerInventory playerInventory))
        {
            OnKeyGather?.Invoke();
            Destroy(gameObject);
        }
    }
}
