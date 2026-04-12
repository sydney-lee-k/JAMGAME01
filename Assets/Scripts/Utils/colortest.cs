using UnityEngine;

public class colortest : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    // Update is called once per frame
    void Update()
    {
        meshRenderer.material.color = ColorUtil.Instance.LerpedColor;
    }
}
