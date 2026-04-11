using System;
using UnityEngine;

public class ResetManager : MonoBehaviour
{
    public static ResetManager instance;
    
    [SerializeField] private Transform playerStart;
    
    private GameObject player;
    private FollowPath[] pathFollowers;

    private void Start()
    {
        pathFollowers = FindObjectsByType<FollowPath>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        GameObject[] playerParts = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerPart in playerParts)
        {
            if (playerPart.GetComponent<MovementController>())
            {
                player = playerPart;
                break;
            }
        }
        instance = this;
    }

    public void Reset()
    {
        foreach (var pathFollower in pathFollowers)
        {
            //Null check in case asynchronous loading causes issues.
            if (pathFollower != null)
            {
                pathFollower.Reset();
            }
        }
        player.transform.position = playerStart.position;
        player.transform.eulerAngles = playerStart.eulerAngles;
        
    }
}
