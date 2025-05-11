using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DepositZoneScript : Interactable
{
    public bool isInZone = false;

    public float depositTime;
    public float depositTimer;

    public static UnityEvent onDeposit = new UnityEvent();

    void Start()
    {

    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.tag == "Player")
        {
            isInZone = true;
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isInZone = false;
        }
    }

    public override void Interact()
    {
        if (isInZone && LevelManager.instance.currentCarry > 0 && Input.GetKeyDown(KeyCode.E))
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("Deposited!", 0.5f, 1);
            LevelManager.instance.AddMoney(LevelManager.instance.pendingMoney);
            LevelManager.instance.currentCarry = 0;
            depositTime = 0;

            onDeposit.Invoke();
        }
    }

}
