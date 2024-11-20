using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorScript : Interactable
{
    public bool isMissionSelected = true;

    public override void Interact()
    {
        base.Interact();
        if (isMissionSelected)
        {
            GameManager.instance.LoadMission("Mission");
        }
        else
        {
            
        }
    }
}
