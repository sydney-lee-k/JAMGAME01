using System.Collections;
using Kino;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private RectTransform fadeImageRect;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private Canvas renderCanvas;
    [SerializeField] private EventSystem eventSystem;
    private bool isTransitionActive = false;
    private bool persistentsLoaded = false;
    private Datamosh datamosh;
    private SceneName activeScene;
    private const float moshStartEntropy = 0.45f;
    private const float moshEndEntropy = 0.2f;
    private const float moshEntropyFadeTime = 0.2f;
    private const float minimumMoshTime = 1.6f;
    private const float sceneFadeOutTime = 0.32f;
    private const float sceneFadeInTime = 0.32f;

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
        DontDestroyOnLoad(renderCamera.gameObject);
        DontDestroyOnLoad(renderCanvas.gameObject);
        DontDestroyOnLoad(eventSystem.gameObject);
        activeScene = SceneName.MainMenu;
    }

    private void OnDestroy()
    {
        if (datamosh.gameObject != null) Destroy(datamosh.gameObject);
        if (renderCamera.gameObject != null) Destroy(renderCamera.gameObject);
        if (renderCanvas.gameObject != null) Destroy(renderCanvas.gameObject);
        if (eventSystem.gameObject != null) Destroy(eventSystem.gameObject);
    }

    public void StartSceneTransition(SceneName sceneName, TransitionStyle style)
    {
        if (isTransitionActive) return;
        StartCoroutine(SceneTransition(sceneName, style));
    }

    private IEnumerator SceneTransition(SceneName sceneName, TransitionStyle style)
    {
        Debug.Log("Started scenetransition");
        isTransitionActive = true;

        switch (style)
        {
            case TransitionStyle.FadeIn:
                LeanTween.alpha(fadeImageRect, 1f, sceneFadeOutTime).setEaseInCubic();
                yield return new WaitForSecondsRealtime(sceneFadeOutTime);
                break;
            case TransitionStyle.Datamosh:
                datamosh.entropy = moshStartEntropy;
                datamosh.Glitch();
                break;
        }

        float timeTaken = Time.unscaledTime;
        if (!persistentsLoaded)
        {
            yield return SceneManager.LoadSceneAsync(SceneName.PersistentObjectsScene.ToString(), LoadSceneMode.Additive);
            persistentsLoaded = true;
        }
        yield return SceneManager.UnloadSceneAsync(activeScene.ToString());
        yield return SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);
        activeScene = sceneName;
        timeTaken = Time.unscaledTime - timeTaken;

        switch (style)
        {
            case TransitionStyle.FadeIn:
                LeanTween.alpha(fadeImageRect, 0f, sceneFadeInTime).setEaseOutCubic();
                yield return new WaitForSecondsRealtime(sceneFadeOutTime);
                break;
            case TransitionStyle.Datamosh:
                if (timeTaken < minimumMoshTime)
                    yield return new WaitForSecondsRealtime(minimumMoshTime - timeTaken);
                LeanTween.value(gameObject, value => datamosh.entropy = value, moshStartEntropy, moshEndEntropy, moshEntropyFadeTime)
                    .setOnComplete(datamosh.Reset);
                break;
        }

        
        
        
        isTransitionActive = false;
    }

    public enum SceneName
    {
        MainMenu = 0,
        FirstStage,
        SecondStage,
        ThirdStage,
        FourthStage,
        FifthStage,
        FinalStage,
        PersistentObjectsScene,
        SampleScene
    }

    public enum TransitionStyle
    {
        None = 0,
        FadeIn,
        Datamosh,
    }
}
