using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TreasureScript : Pickupable
{
    public static UnityEvent<int, int, int> onPickup = new UnityEvent<int, int, int>();

    public TreasureScriptable treasureData;
    int rewardMoney = 100;
    int carryWeight = 1;
    int dropTime = 1;

    void Awake()
    {
        rewardMoney = treasureData.rewardMoney;
        carryWeight = treasureData.carryWeight;
        dropTime = treasureData.dropTime;
    }

    public override void OnPickup()
    {
        if(LevelManager.instance.currentCarry + carryWeight <= LevelManager.instance.carryLimit)
        {
            onPickup.Invoke(rewardMoney, carryWeight, dropTime);
            base.OnPickup();
        }
        else
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("It's too heavy! Go deposit!", 0.5f, 1);
        }
    }

    
}
