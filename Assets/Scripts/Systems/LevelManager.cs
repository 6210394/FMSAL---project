using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public TextMeshProUGUI timerText;
    public float timerDuration = 120f;
    float timer;

    public GameObject playerPrefab;

    public int carryLimit = 3;
    public int currentCarry = 0;
    
    /*
    public List<Transform> enemySpawnPoints;
    */
    public Transform playerSpawnPoint;
    
    public int rewardMoney = 0;
    public string endOfMissionDestination = "Home";

    public static UnityEvent onMissionInitialize = new UnityEvent();


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
        StartCoroutine(StartTimer());
        TreasureScript.onPickup.AddListener(AddCarryWeight);
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
        Destroy(GameObject.Find("MainCamera"));
        /*
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("EnemySpawnPoint"))
        {
            enemySpawnPoints.Add(spawnPoint.transform);
        }
        */
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;
        onMissionInitialize.Invoke(); 
    }
    

    public void AddMoney(int money)
    {
        rewardMoney += money;
    }

    public void AddCarryWeight(int rewardMoney, int weight, int dropTime)
    {
        currentCarry += weight;
    }

    public void CompleteMission()
    {
        Debug.Log("Players count: " + GameManager.instance.players.Count);
        Debug.Log("Mission Complete");
        Debug.Log("You have earned " + rewardMoney + " money");
        GameManager.instance.money += rewardMoney;
        StartCoroutine(ILeaveMission());
    }

    public void FailMission()
    {

        Debug.Log("Mission Failed");
        StartCoroutine(ILeaveMission());
    }

    IEnumerator ILeaveMission()
    {
        StartCoroutine(FadeInOutScript.instance.IFadeOut(1));
        yield return new WaitForSeconds(1);
        foreach(GameObject player in GameManager.instance.players)
        {
            Destroy(player);
        }
        SceneManager.LoadScene(endOfMissionDestination);
        GameManager.instance.hasCompletedDailyMission = true;
        instance = null;
    }

    IEnumerator StartTimer()
    {
        timer = timerDuration;
        while (timerDuration > 0)
        {
            timerDuration -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }
        TimerEnded();
    }

    void UpdateTimerUI()
    {
        timerText.text = "Time: " + Mathf.Floor(timerDuration / 60).ToString("00") + ":" + (timerDuration % 60).ToString("00");
    }

    void TimerEnded()
    {
        Debug.Log("Time's up!");
        FailMission();
    }
}
