using UnityEngine;

public class ColorUtil : MonoBehaviour
{
    public static ColorUtil Instance;
    public Color LerpedColor { get; private set; }
    [SerializeField] private float timeBetweenColors = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LerpedColor = Color.red;
        TweenToGreen();
    }

    private void TweenToGreen()
    {
        LeanTween.value(gameObject, SetNewColorValue, LerpedColor, Color.green, timeBetweenColors).setOnComplete(TweenToBlue);
    }

    private void TweenToBlue()
    {
        LeanTween.value(gameObject, SetNewColorValue, LerpedColor, Color.blue, timeBetweenColors).setOnComplete(TweenToRed);
    }

    private void TweenToRed()
    {
        LeanTween.value(gameObject, SetNewColorValue, LerpedColor, Color.red, timeBetweenColors).setOnComplete(TweenToGreen);
    }
    
    private void SetNewColorValue(Color color)
    {
        LerpedColor = color;
    }
}
