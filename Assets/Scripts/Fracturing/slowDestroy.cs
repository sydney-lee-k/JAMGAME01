using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDestroy : MonoBehaviour
{
    public float destroyTime;
    private float elapsedTime;
    private Vector3 initialScale;

    private void Start()
    {
        initialScale = transform.localScale;
        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / destroyTime;
        
        transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
    }
}
