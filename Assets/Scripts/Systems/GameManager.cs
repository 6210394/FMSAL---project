using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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

    int defaultPlayerMoney = 0;
    int defaultWeekQuota = 150;
    float defaultQuotaMultiplier = 1.1f;
    int defaultWeek = 1;
    int defaultDay = 1;

    int hungerPoints = 0;
    public HUNGER hungerState = HUNGER.FED;
    public enum HUNGER {STARVED, HUNGRY, FED}

    public int money;
    public int weekQuota;
    public float quotaMultiplier;
    public int week;
    public int day;

    
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
        if(Input.GetKeyDown(KeyCode.K))
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("This is a test message", 1, 2);
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            DisplayMessageScript.instance.ImmidiatelyHideMessage();
        }
    }

    public void LoadMission(string missionName)
    {
        SceneManager.LoadScene(missionName);
    }

    void InitializeGameData()
    {
        money = defaultPlayerMoney;
        weekQuota = defaultWeekQuota;
        quotaMultiplier = defaultQuotaMultiplier;
        week = defaultWeek;
        day = defaultDay;

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
    }   
}
