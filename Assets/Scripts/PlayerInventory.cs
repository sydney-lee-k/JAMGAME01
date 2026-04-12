using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int keyCount = 0;

    private void Start()
    {
        KeyPickup.OnKeyGather += AddKey;
        KeyPickup.OnLastKeyGather += AddKey;
    }

    public int KeyCount => keyCount;
    private void AddKey() { keyCount++; }
    public void ResetKeys() { keyCount = 0; }
}
