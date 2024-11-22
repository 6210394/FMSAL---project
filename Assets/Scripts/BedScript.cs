using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedScript : Interactable
{
    public bool confirmSleep = false;

    public override void Interact()
    {
        base.Interact();
        Sleep();
        if(!GameManager.instance.hasCompletedDailyMission)
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("Can I just sleep without working?..", 1, 2);
            confirmSleep = true;
        }
    }

    public void Sleep()
    {
        if(confirmSleep || GameManager.instance.hasCompletedDailyMission)
        {
            GameManager.instance.GoToNextDay();
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        confirmSleep = false;
    }
    
}
