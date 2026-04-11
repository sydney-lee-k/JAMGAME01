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
            remainingLerpTime = 0;
        }

        if (remainingLerpTime <= lerpTime)
        {
            remainingLerpTime += Time.deltaTime;
            transform.localEulerAngles += spinDirections * (remainingLerpTime/lerpTime) * Time.deltaTime;
        }
        else
        {
            transform.localEulerAngles += spinDirections * Time.deltaTime;
        }
    }
}
