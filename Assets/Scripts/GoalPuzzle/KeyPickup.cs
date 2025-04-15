using UnityEngine;

public class KeyPickup : Pickupable
{
    public int keyID;

    public override void OnPickup()
    {
        LevelManager.instance.keys.Add(this);
        base.OnPickup();
        DisplayMessageScript.instance.ChangeDisplayMessage("Picked up key", 0.5f, 0.5f);
    }
}
