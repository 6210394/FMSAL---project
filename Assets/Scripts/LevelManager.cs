using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public GameObject playerPrefab;
    public List<Transform> enemySpawnPoints;
    public Transform playerSpawnPoint;

    public int rewardMoney = 100;
    public string endOfMissionDestination = "Home";


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeLevel();
        Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            EndMission();
        }
    }

    void InitializeLevel()
    {
        Destroy(GameObject.FindGameObjectWithTag("MainCamera"));

        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("EnemySpawnPoint"))
        {
            enemySpawnPoints.Add(spawnPoint.transform);
        }
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;
        
    }

    public void EndMission()
    {
        Debug.Log("Mission Complete");
        Debug.Log("You have earned " + rewardMoney + " money");
        SceneManager.LoadScene(endOfMissionDestination);
        GameManager.instance.money += rewardMoney;
        instance = null;
    }   
}
