using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    int defaultPlayerMoney = 0;
    int defaultWeekQuota = 150;
    int defaultWeek = 1;
    int defaultDay = 1;

    public int week;
    public int day;
    public int money;
    public int weekQuota;

    public GameData()
    {
        this.week = defaultWeek;
        this.day = defaultDay;
        this.money = defaultPlayerMoney;
        this.weekQuota = defaultWeekQuota;
    }
}
