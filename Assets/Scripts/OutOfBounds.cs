using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResetManager.instance.Reset();
        }
    }
}
