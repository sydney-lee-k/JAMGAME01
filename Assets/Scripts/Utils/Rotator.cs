using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAmount;

    private void Update()
    {
        transform.Rotate(rotationAmount * Time.deltaTime);
    }
}