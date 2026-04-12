using System;
using System.Collections;
using UnityEngine;

public class ResetManager : MonoBehaviour
{
    public static ResetManager instance;
    
    [SerializeField] private Transform playerStart;
    [SerializeField] private Transform playerRespawn;

    private GameObject player;
    private MovementController movementController;
    private FollowPath[] pathFollowers;
    private const float resetMoshTime = 1.6f;

    public static event Action OnPlayerResetStart;
    public static event Action OnPlayerResetFinish;

    private void Start()
    {
        pathFollowers = FindObjectsByType<FollowPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        GameObject[] playerParts = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerPart in playerParts)
        {
            if (playerPart.TryGetComponent(out MovementController mc))
            {
                player = playerPart;
                movementController = mc;
                break;
            }
        }
        instance = this;
        playerStart = GameObject.FindWithTag("PlayerSpawn").transform;
        SceneTransitionManager.OnSceneLoaded += () => playerStart = GameObject.FindWithTag("PlayerSpawn").transform;
        if (!playerRespawn) playerRespawn = playerStart;
    }

    public void Reset()
    {
        foreach (var pathFollower in pathFollowers)
        {
            //Null check in case asynchronous loading causes issues.
            if (pathFollower != null)
            {
                pathFollower.ResetPath(resetMoshTime + MoshManager.moshEntropyFadeTime);
            }
        }

        StartCoroutine(ResetTransition());
    }
    
    public IEnumerator ResetTransition()
    {
        movementController.lookLocked = true;
        movementController.moveLocked = true;
        MoshManager.Instance.StartMosh();
        OnPlayerResetStart?.Invoke();

        yield return new WaitForSecondsRealtime(resetMoshTime);
        movementController.ResetVelocity();
        player.transform.position = playerRespawn.position;
        player.transform.eulerAngles = playerRespawn.eulerAngles;
        movementController.KinematicOff();
        MoshManager.Instance.FadeOutMosh();
        yield return new WaitForSecondsRealtime(MoshManager.moshEntropyFadeTime);
        OnPlayerResetFinish?.Invoke();

        movementController.lookLocked = false;
        movementController.moveLocked = false;
    }
}
