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

    public override void Update()
    {
        base.Update();
        if(player)
        {
            
        }
        else
        {
            
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    private void MarkOutline()
    {
        if (!highlighted)
        {
            highlighted = true;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_OutlineOpacity"))
                {
                    material.SetFloat("_OutlineOpacity", 3f);
                }
            }
        }
    }

    private void ClearOutline()
    {
        if (highlighted)
        {
            highlighted = false;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_OutlineOpacity"))
                {
                    material.SetFloat("_OutlineOpacity", 0f); 
                }
            }
        }
    }
}
