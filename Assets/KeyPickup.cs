using UnityEngine;

public class KeyPickup : Pickupable
{
    public override void OnPickup()
    {
        LevelManager.instance.hasKey = true;
        base.OnPickup();
        DisplayMessageScript.instance.ChangeDisplayMessage("Access to locked doors.", 0.5f, 0.5f);
    }
}
