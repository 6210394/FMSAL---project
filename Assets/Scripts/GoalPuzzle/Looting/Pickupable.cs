using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pickupable : Interactable
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickup();
        }
    }

    public virtual void OnPickup()
    {
        Destroy(gameObject);
    }
}
