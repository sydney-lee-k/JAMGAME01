using UnityEngine;

public class Breathe : MonoBehaviour
{
    [SerializeField] private Vector3 size;
    [SerializeField] private float breatheTime;

    private void Start()
    {
        LeanTween.scale(transform.gameObject, size, breatheTime).setEaseOutQuad().setLoopPingPong();
    }
}