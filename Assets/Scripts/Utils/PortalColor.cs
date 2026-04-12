using UnityEngine;

public class PortalColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.material.SetColor("_Color", ColorUtil.Instance.LerpedColor);
    }
}
