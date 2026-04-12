using System;
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
    private SceneName activeScene;
    private const float sceneFadeOutTime = 0.32f;
    private const float sceneFadeInTime = 0.32f;
    private const float minimumMoshTransitionTime = 1.6f;

    public static event Action<SceneName> OnSceneLoadStart;
    public static event Action OnSceneLoaded;
    public static event Action OnSceneTransitionOver;

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
        DontDestroyOnLoad(renderCamera.gameObject);
        DontDestroyOnLoad(renderCanvas.gameObject);
        DontDestroyOnLoad(eventSystem.gameObject);
        activeScene = SceneName.MainMenu;
    }

    private void OnDestroy()
    {
        if (renderCamera != null) Destroy(renderCamera.gameObject);
        if (renderCanvas != null) Destroy(renderCanvas.gameObject);
        if (eventSystem != null) Destroy(eventSystem.gameObject);
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
                MoshManager.Instance.StartMosh();
                break;
        }

        OnSceneLoadStart?.Invoke(sceneName);

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
        OnSceneLoaded?.Invoke();
        MovementController movementController = FindFirstObjectByType<MovementController>();
        Transform playerSpawn = GameObject.FindWithTag("PlayerSpawn").transform;
        GameObject player = movementController.gameObject;

        switch (style)
        {
            case TransitionStyle.FadeIn:
                LeanTween.alpha(fadeImageRect, 0f, sceneFadeInTime).setEaseOutCubic();
                yield return new WaitForSecondsRealtime(sceneFadeOutTime);
                break;
            case TransitionStyle.Datamosh:
                if (timeTaken < minimumMoshTransitionTime)
                    yield return new WaitForSecondsRealtime(minimumMoshTransitionTime - timeTaken);
                MoshManager.Instance.FadeOutMosh();
                movementController.SetTransform(playerSpawn);
                break;
        }
        OnSceneTransitionOver?.Invoke();
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
        LevelBlockouts
    }

    public enum TransitionStyle
    {
        None = 0,
        FadeIn,
        Datamosh,
    }
}
