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

    public int money;
    public int weekQuota;
    public float quotaMultiplier;
    public int week;
    public int day;

    
    public bool hasCompletedDailyMission = false;

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
            DisplayMessageScript.instance.ChangeDisplayMessage("Hello World", 1, 1);
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
    }

    public void IncreaseWeek()
    {
        week++;
        day = 1;
        weekQuota = (int)(weekQuota * quotaMultiplier);
    }

    public void GoToNextDay()
    {
        if(hasCompletedDailyMission)
        {
            day++;
            if (day > 7)
            {
                IncreaseWeek();
            }
        }
    }
}
