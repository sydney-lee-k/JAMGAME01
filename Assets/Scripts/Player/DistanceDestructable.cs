using System;
using UnityEngine;

public class DistanceDestructable : MonoBehaviour
{
    [Header("Settings")] [SerializeField]
    private ConstitutingDestructible target;
    [SerializeField] private float distance;

    //Kept public because it's something another script MIGHT want to check
    [NonSerialized] public bool alive;
    private Transform player;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        alive = target.startConstructed;
    }

    private void Update()
    {
        bool playerInArea = Vector3.Distance(transform.position, player.position) <= distance;

        bool shouldBeAlive = playerInArea ? !target.startConstructed : target.startConstructed;

        if (alive != shouldBeAlive && !target.reconstituting)
        {
            alive = shouldBeAlive;

            if (alive)
            {
                target.Constitute();
            }
            else
            {
                target.Reshatter();
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Range shower
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
