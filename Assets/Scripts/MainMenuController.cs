using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    
    private void Start()
    {
        startGameButton.onClick.AddListener(() =>
            SceneTransitionManager.Instance.StartSceneTransition(SceneTransitionManager.SceneName.SampleScene, SceneTransitionManager.TransitionStyle.FadeIn));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
