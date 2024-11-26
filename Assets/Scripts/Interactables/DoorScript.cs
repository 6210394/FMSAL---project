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
        if (isMissionSelected)
        {
            GameManager.instance.LoadMission(missionName);
        }
        else
        {
            
        }
    }
}
