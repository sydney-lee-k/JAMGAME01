using System;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public bool active;
    [SerializeField] private float lerpTime;
    private float remainingLerpTime;
    [SerializeField] private Vector3 spinDirections;

    private void Update()
    {
        if (!active)
        {
            remainingLerpTime = 0f;
            return;
        }
        
        float speedMultiplier;
        if (lerpTime <= 0f)
        {
            speedMultiplier = 1f;
        }
        else
        {
            remainingLerpTime += Time.deltaTime;
            speedMultiplier = Mathf.Clamp01(remainingLerpTime / lerpTime);
        }
        
        transform.localRotation *= Quaternion.Euler(spinDirections * speedMultiplier * Time.deltaTime);
    }
}
