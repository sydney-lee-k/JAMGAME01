using UnityEngine;
using UnityEngine.UI;

public class Die : MonoBehaviour
{
    public Image image;

    // Update is called once per frame
    void Start()
    {
        LeanTween.alpha(image.rectTransform, 1f, 30f).setOnComplete(() =>
        {
            Application.Quit();
        });
    }
}
