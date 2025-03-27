using UnityEngine;

public class LockedDoorScript : Interactable
{
    public GameObject door;
    public int keyID;

    public override void Interact()
    {
        base.Interact();
        foreach(KeyPickup key in LevelManager.instance.keys)
        {
            if(key.keyID == keyID)
            {
                Destroy(door);
                Destroy(gameObject);
                DisplayMessageScript.instance.ChangeDisplayMessage("Unlocked!", 0.5f, 0.5f);
                return;
            }
        }
        DisplayMessageScript.instance.ChangeDisplayMessage("Locked.", 0.5f, 0.5f);
    }
}
