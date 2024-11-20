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

    // Start is called before the first frame update
    void Start()
    {

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
        money = defaultPlayerMoney;
        weekQuota = defaultWeekQuota;
        quotaMultiplier = defaultQuotaMultiplier;
        week = defaultWeek;
        day = defaultDay;
    }
}
