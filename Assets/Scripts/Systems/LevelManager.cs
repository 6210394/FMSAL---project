using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public GameObject playerPrefab;

    public enum MissionType
    {
        Freeroam, Loot, Assassination
    }


    public int carryLimit = 20;
    public int currentCarry = 0;
    public List<KeyPickup> keys;

    [SerializeField] MissionType missionType;

    public TextMeshProUGUI timerText;
    public float timerDuration = 120f;
    float timer;
    
    public List<Transform> enemySpawnPoints;
    public Transform playerSpawnPoint;
    
    public int pendingMoney = 0;
    public int securedMoney = 0;
    public int maxMoney = 0;
    public string endOfMissionDestination = "Home";
    public string deathDestination = "Home";

    public static UnityEvent onMissionInitialize = new UnityEvent();
    public UnityEvent onTreasurePickup;
    public GameObject enemyPrefab;

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
        
        if(missionType == MissionType.Loot)
        {
            TreasureScript.onPickup.AddListener(PickUpLoot);
            foreach(TreasureScript treasure in FindObjectsByType<TreasureScript>(FindObjectsSortMode.None))
            {
                maxMoney += treasure.rewardMoney;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) //Debug to auto complete mission
        {
            CompleteMission();
        }
    }

    void InitializeLevel()
    {
        //Destroy(GameObject.Find("MainCamera"));
        //FindSpawnPoints();
        //Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation).GetComponent<CombatScript>().enabled = true;
        

        if(missionType == MissionType.Loot)
        {
            StartCoroutine(IStartTimer());
        }
        onMissionInitialize.Invoke();
    }    

    #region Loot Mission Functions
    public void AddMoney(int money)
    {
        securedMoney += money;
    }

    public void PickUpLoot(int rewardMoney, int weight, int dropTime)
    {
        currentCarry += weight;
        pendingMoney += rewardMoney;
        onTreasurePickup.Invoke();
    }
    #endregion

    #region End Mission Functions

    public void CompleteMission()
    {
        Debug.Log("Players count: " + GameManager.instance.players.Count);
        Debug.Log("Mission Complete");
        Debug.Log("You have earned " + securedMoney + " money");
        GameManager.instance.money += securedMoney;
        StartCoroutine(ILeaveMission(endOfMissionDestination));
    }

    public void FailMission()
    {
        Debug.Log("Mission Failed");
        StartCoroutine(ILeaveMission(deathDestination));
    }

    IEnumerator ILeaveMission(string destination)
    {
        StartCoroutine(FadeInOutScript.instance.IFadeOut(0.5f));
        yield return new WaitForSeconds(2);
        foreach(GameObject player in GameManager.instance.players)
        {
            Destroy(player);
        }
        SceneManager.LoadScene(destination);
        GameManager.instance.hasCompletedDailyMission = true;
        instance = null;
    }
    #endregion

    #region Timer Functions
    IEnumerator IStartTimer()
    {
        timer = timerDuration;
        UpdateTimerUI();
        StartCoroutine(FadeInOutScript.instance.IFadeIn(0.5f));
        yield return new WaitForSeconds(1f);
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
        timerText.text = Mathf.Floor(timerDuration / 60).ToString("0") + ":" + (timerDuration % 60).ToString("00");
    }

    void TimerEnded()
    {
        CompleteMission();
        Debug.Log("Time's up!");
    }
    #endregion
    
    #region UNUSED
    /*                                      
    void FindSpawnPoints()
    {
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("EnemySpawnPoint"))
        {
            enemySpawnPoints.Add(spawnPoint.transform);
        }
        //playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;
    }

    
    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            if (enemySpawnPoints.Count > 0)
            {
                // Select a random spawn point
                Transform randomSpawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Count)];
                
                // Spawn the enemy at the selected spawn point
                GameObject enemyManager = FindFirstObjectByType<EnemyManager>().gameObject;

                Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation, enemyManager.transform);
            }

            // Wait for 1 minute before spawning the next enemy
            yield return new WaitForSeconds(60f);
        }
    }
    */
    #endregion
}
