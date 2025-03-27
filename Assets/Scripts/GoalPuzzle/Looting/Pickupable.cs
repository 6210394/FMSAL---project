using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pickupable : Interactable
{
    public virtual void OnPickup()
    {
        Destroy(gameObject);
    }

    public override void Interact()
    {
        base.Interact();
        OnPickup();
    }
}
