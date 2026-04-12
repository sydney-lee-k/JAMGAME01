using System;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    [Header("Standard Settings")]
    [SerializeField] private BezierPath path;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Vector3 rotationMask = new Vector3(1, 1, 1);

    [Space(10)]
    [SerializeField] private float delayBeforeStart;
    private float remainingStartDelay;
    [SerializeField][Range(0,1f)] private float progress;

    [NonSerialized] public bool endReached = false;
    private Vector3 currentPos;

    private float startProgress;
    private Vector3 startAngle;

    private void Start()
    {
        currentPos = path.GetPoint(progress);
        transform.position = currentPos;
        startProgress = progress;
        startAngle = transform.eulerAngles;
        remainingStartDelay = delayBeforeStart;
    }

    public void ResetPath(float additionalDelay = 0f)
    {
        progress = startProgress;
        currentPos = path.GetPoint(progress);
        transform.position = currentPos;
        transform.eulerAngles = startAngle;
        remainingStartDelay = delayBeforeStart + additionalDelay;
    }

    private void Update()
    {
        //Start Delay
        if (remainingStartDelay > 0)
        {
            remainingStartDelay -= Time.deltaTime;
            return;
        }
        
        float remainingMove = speed * Time.deltaTime;
        while (remainingMove > 0f)
        {
            //Check next "step" between the point youre going toward
            float step = 0.01f;

            float nextStep = progress + step;
            
            //Loop around if path loops
            if (nextStep > 1f)
                nextStep = path.loop ? nextStep - 1f : 1f;

            Vector3 nextPos = path.GetPoint(nextStep);
            float dist = Vector3.Distance(currentPos, nextPos);
            
            //
            if (dist > remainingMove)
            {
                float ratio = remainingMove / dist;
                currentPos = Vector3.Lerp(currentPos, nextPos, ratio);
                if (path.loop && nextStep < progress)
                {
                    float wrappedT = Mathf.Lerp(progress, nextStep + 1f, ratio);
                    progress = wrappedT >= 1f ? wrappedT - 1f : wrappedT;
                }
                else
                {
                    progress = Mathf.Lerp(progress, nextStep, ratio);
                }
                break;
            }

            remainingMove -= dist;
            currentPos = nextPos;
            progress = nextStep;

            if (progress >= 1f && !path.loop)
            {
                endReached = true;
                break;
            }
        }
        
        float multiplier = path.GetSpeedMultiplier(progress);
        currentPos = transform.position + (currentPos - transform.position) * multiplier;
        Vector3 movement = currentPos - transform.position;
        
        //Using the direction of movement, turn the object according to rotationMask
        if (movement.sqrMagnitude > 0.0001f)
        {
            Vector3 direction = movement.normalized;
            
            direction = new Vector3(
                rotationMask.x != 0 ? direction.x : 0,
                rotationMask.y != 0 ? direction.y : 0,
                rotationMask.z != 0 ? direction.z : 0
            );

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        
        //Final position
        transform.position = currentPos;
    }
}