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

    
    public bool hasCompletedDailyMission = false; //this is used in the BedScript
    public FadeInOutScript fadeScript;
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
        float fadeSpeed = 2;
        float fadeLength = 1;

        day++;
        if (day > 7)
        {
            IncreaseWeek();
        }

        foreach (PlayerMovement player in players)
        {
            player.isControlled = false;
        }
        DisplayMessageScript.instance.ImmidiatelyHideMessage();
        StartCoroutine(fadeScript.FadeOutInCycle(fadeSpeed, fadeLength));
        StartCoroutine(WakeUp(fadeSpeed + fadeLength, 1));
    }

    public IEnumerator WakeUp(float animationLength, float waitLength)
    {
        yield return new WaitForSeconds(animationLength + waitLength);
        foreach (PlayerMovement player in players)
        {
            player.isControlled = true;
        }
        
        DisplayMessageScript.instance.ChangeDisplayMessage(RandomMorningMessage(), 1, 2);
        hasCompletedDailyMission = false;
    }

    public string RandomMorningMessage()
    {
        string message;
        int random = Random.Range(0, 4);

        switch (random)
        {
            case 0:
                message = "I need to get up..";
                break;
            case 1:
                message = "My back hurts..";
                break;
            case 2:
                message = "I feel pretty good today..";
                break;
            case 3:
                message = "This bed sucks..";
                break;
            case 4:
                message = "I'm thirsty..";
                break;
            default:
                message = "I need to get up..";
                break;
        }
        return message;
    }
}
