using UnityEngine;

public class LockedDoorScript : Interactable
{
    public GameObject door;

    public override void Interact()
    {
        base.Interact();
        if(LevelManager.instance.hasKey)
        {
            Destroy(door);
            Destroy(gameObject);
            DisplayMessageScript.instance.ChangeDisplayMessage("Unlocked!", 0.5f, 0.5f);
        }
        else
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("Locked.", 0.5f, 0.5f);
        }
    }
}
