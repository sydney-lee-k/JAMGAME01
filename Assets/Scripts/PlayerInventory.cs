using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int keyCount = 0;

    private void Start()
    {
        PickupItem.OnKeyGather += AddKey;
    }
    
    private void AddKey() { keyCount++; }
    public void ResetKeys() { keyCount = 0; }
}
