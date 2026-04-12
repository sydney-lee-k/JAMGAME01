using System;
using UnityEngine;

public class FogController : MonoBehaviour
{
    public bool active;
    private bool hasActivated;
    private bool startingState;
    [SerializeField] private ParticleSystem[] particles;
    [SerializeField] private FollowPath pathFollower;
    [SerializeField] private GameObject hitbox;

    private void Start()
    {
        if(!pathFollower) pathFollower = GetComponent<FollowPath>();
        pathFollower.active = active;
        startingState = active;
        hasActivated = !active;
    }

    private void Update()
    {
        hitbox.SetActive(active);
        if (active && !hasActivated)
        {
            pathFollower.active = active;
            if (particles.Length > 0)
            {
                foreach (var particle in particles)
                {
                    particle.Play();
                }
            }
            hasActivated = true;
        } else if (!active && hasActivated)
        {
            pathFollower.active = active;
            if (particles.Length > 0)
            {
                foreach (var particle in particles)
                {
                    particle.Clear();
                    particle.Stop();
                }
            }
            hasActivated = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ResetManager.instance.Reset();
            if (particles.Length > 0)
            {
                foreach (var particle in particles)
                {
                    particle.Clear();
                    active = startingState;
                }
            }
        }
    }
}
