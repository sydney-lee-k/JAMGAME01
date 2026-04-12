using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeOptionsButton;
    [SerializeField] private Button closeCreditsButton;
    [SerializeField] private CanvasGroup optionsGroup;
    [SerializeField] private CanvasGroup creditsGroup;
    private const float menusFadeTime = 0.32f;
    
    private void Start()
    {
        startGameButton.onClick.AddListener(() =>
            SceneTransitionManager.Instance.StartSceneTransition(SceneTransitionManager.SceneName.FirstStage, SceneTransitionManager.TransitionStyle.FadeIn));

        optionsButton.onClick.AddListener(() =>
        {
            optionsGroup.alpha = 0f;
            optionsGroup.gameObject.SetActive(true);
            LeanTween.value(gameObject, value => optionsGroup.alpha = value, 0f, 1f, menusFadeTime).setEaseOutCubic();
        });

        creditsButton.onClick.AddListener(() =>
        {
            creditsGroup.alpha = 0f;
            creditsGroup.gameObject.SetActive(true);
            LeanTween.value(gameObject, value => creditsGroup.alpha = value, 0f, 1f, menusFadeTime).setEaseOutCubic();
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        closeOptionsButton.onClick.AddListener(() =>
        {
            LeanTween.value(gameObject, value => optionsGroup.alpha = value, optionsGroup.alpha, 0f, menusFadeTime)
                .setEaseInCubic()
                .setOnComplete(() => optionsGroup.gameObject.SetActive(false));
        });

        closeCreditsButton.onClick.AddListener(() =>
        {
            LeanTween.value(gameObject, value => creditsGroup.alpha = value, creditsGroup.alpha, 0f, menusFadeTime)
                .setEaseInCubic()
                .setOnComplete(() => creditsGroup.gameObject.SetActive(false));
        });
    }
}