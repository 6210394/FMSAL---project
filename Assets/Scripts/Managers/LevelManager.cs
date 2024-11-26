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
            CompleteMission();
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

    public void CompleteMission()
    {
        Debug.Log("Mission Complete");
        Debug.Log("You have earned " + rewardMoney + " money");
        SceneManager.LoadScene(endOfMissionDestination);
        GameManager.instance.money += rewardMoney;
        GameManager.instance.hasCompletedDailyMission = true;
        Debug.Log(GameManager.instance.hasCompletedDailyMission);
        instance = null;
    }

    public void FailMission()
    {
        Debug.Log("Mission Failed");
        SceneManager.LoadScene(endOfMissionDestination);
        GameManager.instance.hasCompletedDailyMission = true;
        instance = null;
    }
}
