using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int hungerPoints;

    public int week;
    public int day;
    public int money;
    public int weekQuota;

    public GameData()
    {
        this.week = 1;
        this.day = 1;
        this.money = 10;
        this.weekQuota = 150;
    }
}
