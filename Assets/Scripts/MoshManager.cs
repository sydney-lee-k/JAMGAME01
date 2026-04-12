using System.Collections;
using Kino;
using UnityEngine;
using UnityEngine.UI;

public class MoshManager : MonoBehaviour
{
    public static MoshManager Instance;
    [SerializeField] private RawImage renderImg;
    private Datamosh datamosh;
    private int moshFadeTween = -1;
    public bool IsMoshing { get; private set; } = false;
    private const float moshStartEntropy = 0.45f;
    private const float glitchEffectEntropy = 0.25f;
    private const float moshEndEntropy = 0.2f;
    public const float moshEntropyFadeTime = 0.2f;

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

    private void Start()
    {
        datamosh = Camera.main.GetComponent<Datamosh>();
        DontDestroyOnLoad(datamosh.gameObject);
        Datamosh.OnScreenResized += newSize =>
        {
            if (newSize.x / newSize.y > 16f / 9f)
            {
                renderImg.rectTransform.sizeDelta = new(Screen.width, Screen.width * 9f / 16f);
            }
            else
            {
                renderImg.rectTransform.sizeDelta = new(Screen.height * 16f / 9f, Screen.height);
            }
        };
    }

    private void OnDestroy()
    {
        if (datamosh != null) Destroy(datamosh.gameObject);
    }

    public void StartMosh()
    {
        if (IsMoshing)
        {
            StartCoroutine(RestartMosh(moshStartEntropy));
        }
        else
        {
            datamosh.entropy = moshStartEntropy;
            datamosh.Glitch();
            IsMoshing = true;
        }
    }

    public void StartCalmGlitch()
    {
        if (IsMoshing)
        {
            StartCoroutine(RestartMosh(glitchEffectEntropy));
        }
        else
        {
            datamosh.entropy = glitchEffectEntropy;
            datamosh.Glitch();
            IsMoshing = true;
        }
    }

    private IEnumerator RestartMosh(float usedEntropy)
    {
        datamosh.Reset();
        yield return null;
        datamosh.entropy = usedEntropy;
        datamosh.Glitch();
        IsMoshing = true;
    }

    public void FadeOutMosh()
    {
        if (moshFadeTween != -1) LeanTween.cancel(moshFadeTween);
        moshFadeTween = LeanTween.value(gameObject, value => datamosh.entropy = value, moshStartEntropy, moshEndEntropy, moshEntropyFadeTime)
            .setOnComplete(() =>
            {
                datamosh.Reset();
                IsMoshing = false;
                moshFadeTween = -1;
            }).id;
    }
}
