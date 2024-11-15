using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

#region Singleton
    public static GameManager instance;
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
#endregion

    public int defaultPlayerMoney = 0;
    public int defaultWeekQuota = 150;
    public float defaultQuotaMultiplier = 1.1f;
    public int defaultWeek = 1;
    public int defaultDay = 1;

    public gameInfo gameData = new gameInfo();

    public struct gameInfo
    {
        public int playerMoney;
        public int weekQuota;
        public float quotaMultiplier;
        public int week;
        public int day;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(SaveData.instance != null)
        {
            
        }
        else
        {
            InitializeGameData();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeGameData()
    {
        gameData.playerMoney = defaultPlayerMoney;
        gameData.weekQuota = defaultWeekQuota;
        gameData.quotaMultiplier = defaultQuotaMultiplier;
        gameData.week = defaultWeek;
        gameData.day = defaultDay;
    }
}
