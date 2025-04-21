using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorScript : Interactable
{
    public bool isMissionSelected = true;
    public string missionName = "Mission";


    public override void Interact()
    {
        base.Interact();
        if (!GameManager.instance.hasCompletedDailyMission)
        {
            if (isMissionSelected)
            {
                player.GetComponent<PlayerMovementController>().isControlled = false;
                StartCoroutine(FadeInOutScript.instance.IFadeOut(1f));
                StartCoroutine(StartMission());
            }
            else
            {

            }
        }
        else
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("I'm way too tired..", 0.6f, 1f );
        }
        
    }

    IEnumerator StartMission()
    {
        yield return new WaitForSeconds(3);
        GameManager.instance.LoadMission(missionName);
    }
}
