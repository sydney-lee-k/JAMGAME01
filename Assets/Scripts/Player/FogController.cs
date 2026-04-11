using System;
using UnityEngine;

public class FogController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResetManager.instance.Reset();
            particles.Clear();
        }
    }
}
