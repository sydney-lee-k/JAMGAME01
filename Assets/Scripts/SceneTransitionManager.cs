using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    private const float loadCheckFrequency = 0.1f;
    private WaitForSeconds loadCheckWait;
    private bool isTransitionActive = false;

    private void Start()
    {
        loadCheckWait = new(loadCheckFrequency);
    }

    public void StartSceneTransition(SceneName sceneName, bool doDatamosh = false)
    {
        if (isTransitionActive) return;
        StartCoroutine(SceneTransition(sceneName, doDatamosh));
    }

    private IEnumerator SceneTransition(SceneName sceneName, bool doDatamosh)
    {
        isTransitionActive = true;
        AsyncOperation transitionOperation = SceneManager.LoadSceneAsync(sceneName.ToString());

        //Start DataMosh effect here

        while (true)
        {
            yield return loadCheckWait;
            if (transitionOperation.isDone) // Add check for if DataMosh thing is done
            {
                break;
            }
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
        FinalStage
    }
}
