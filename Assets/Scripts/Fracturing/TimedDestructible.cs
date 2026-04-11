using System;
using UnityEngine;

public class TimedDestructible : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private ConstitutingDestructible target; 
    public bool active = true;

    [SerializeField] private float aliveTime;
    [SerializeField] private float deadTime;

    //Kept public because it's something another script MIGHT want to check
    [NonSerialized] public bool alive;
    [Header("Debugging")] [SerializeField] private float remainingSwapTime;

    private void Start()
    {
        alive = target.startConstructed;
        remainingSwapTime = target.startConstructed ? aliveTime: deadTime;
    }

    private void Update()
    {
        if (!active) return;
        remainingSwapTime -= Time.deltaTime;
        if (remainingSwapTime <= 0)
        {
            if (alive)
            {
                target.Reshatter();
                remainingSwapTime = deadTime + target.easingDuration;
            }
            else
            {
                target.Constitute();
                remainingSwapTime = aliveTime + target.easingDuration;
            }

            alive = !alive;
        }
    }
}