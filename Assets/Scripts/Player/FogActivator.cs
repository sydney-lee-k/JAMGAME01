using System;
using UnityEngine;

public class FogActivator : MonoBehaviour
{
    [SerializeField] private FogController fogController;
    [SerializeField] private bool SetFog;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fogController.active = SetFog;
        }
    }
}
