using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositZoneScript : MonoBehaviour
{
    public bool isInZone = false;

    public int totalRewardMoney;
    public int totalCarryWeight;

    public float depositTime;
    public float depositTimer;

    void Start()
    {
        TreasureScript.onPickup.AddListener(MatchDepositTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            isInZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isInZone = false;
        }
    }

    void Update()
    {
        Deposit();
    }

    public void Deposit()
    {
        if (isInZone && LevelManager.instance.currentCarry > 0)
        {
            if(Input.GetKey(KeyCode.E) && depositTimer > 0)
            {
                depositTimer -= Time.deltaTime;
                return;
            }
            else if (depositTimer > 0 && depositTimer < depositTime) //reset the timer
            {
                depositTimer = depositTime;
            }
            if(depositTimer <= 0)
            {
                DisplayMessageScript.instance.ChangeDisplayMessage("Deposited!", 0.5f, 1);
                LevelManager.instance.AddMoney(totalRewardMoney);
                LevelManager.instance.currentCarry -= totalCarryWeight;
                totalRewardMoney = 0;
                totalCarryWeight = 0;
                depositTime = 0;
            }
        }
    }

    public void MatchDepositTime(int rewardMoney, int carryWeight, int dropTime)
    {
        depositTime += dropTime;
        totalRewardMoney += rewardMoney;
        totalCarryWeight += carryWeight;
        depositTimer = depositTime;
    }
}
