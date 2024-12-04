using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IDataPersistence
{

#region Singleton
    public static GameManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeGameData();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
#endregion

    public string gameOverScene = "MainMenu";

    public int hungerPoints = 0;
    public HUNGER hungerState = HUNGER.FED;
    public enum HUNGER {DYING, STARVED, HUNGRY, FED, FULL}

    public int money;
    public int weekQuota;
    public int week;
    public int day;

    public float quotaMultiplier = 1.2f;

    
    public bool hasCompletedDailyMission = false; //this is used in the BedScript
    public bool interactEnabled = true;

    public List<PlayerMovement> players = new List<PlayerMovement>(); // change this to the actual player script later


    // Start is called before the first frame update
    void Start()
    {
        InitializeGameData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadMission(string missionName)
    {
        SceneManager.LoadScene(missionName);
    }

    void InitializeGameData()
    {
        hasCompletedDailyMission = false;

        foreach (PlayerMovement player in GameObject.FindObjectsOfType<PlayerMovement>())
        {
            players.Add(player);
        }
    }

    public void IncreaseWeek()
    {
        week++;
        day = 1;
        weekQuota = (int)(weekQuota * quotaMultiplier);
    }

    public void GoToNextDay()
    {
        day++;
        if (day > 7)
        {
            IncreaseWeek();
        }
        hasCompletedDailyMission = false;
        hungerPoints -= 2;
        HungerManagement();
    }   

    public void HungerManagement()
    {
        float waitTime = 2;
        float animationLength = 1;

        switch(hungerPoints)
        {
            case 2:
                hungerState = HUNGER.STARVED;
                break;
            case 4:
                hungerState = HUNGER.DYING;
                break;
            case 6:
                hungerState = HUNGER.HUNGRY;
                break;
            case 8:
                hungerState = HUNGER.FED;
                break;
            case 10:
                hungerState = HUNGER.FULL;
                break;
        }

        if(hungerPoints == 0)
        {
            StartCoroutine(IGameOver("You never woke up."));
            return;
        }

        StartCoroutine(BedScript.instance.IWakeUp(animationLength, waitTime));
    }

    public IEnumerator IGameOver(string gameOverMessage)
    {
        yield return new WaitForSeconds(3);
        DisplayMessageScript.instance.ChangeDisplayMessage(gameOverMessage, 3, 6);
        yield return new WaitForSeconds(6);
        SceneManager.LoadScene(gameOverScene);
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.week = week;
        gameData.day = day;
        gameData.money = money;
        gameData.weekQuota = weekQuota;
    }

    public void LoadData(GameData gameData)
    {
        week = gameData.week;
        day = gameData.day;
        money = gameData.money;
        weekQuota = gameData.weekQuota;
    }
}
