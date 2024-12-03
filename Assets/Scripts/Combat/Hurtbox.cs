using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public CombatEntity combatEntity;
    public bool takesKnockback = true;
    public float entityWeight = 1f;


    public bool isPlayer = false;
    public RecieveImpact recieveImpact;

    void Awake()
    {
        combatEntity = GetComponent<CombatEntity>();
        if(isPlayer)
        {
            recieveImpact = GetComponent<RecieveImpact>();
        }
        if (GetComponent<Rigidbody>() == null)
        {
            takesKnockback = false;
        }
    }

    public void OnHit(float damage, float knockbackForce, Vector3 incomingHitDirection)
    {
        Knockback(knockbackForce, incomingHitDirection);
        combatEntity.LoseHealth(damage);
    }

    void Knockback(float knockbackForce, Vector3 knockbackDirection)
    {
        if (takesKnockback)
        {
            if (isPlayer)
            {
                recieveImpact.AddImpact(knockbackDirection, knockbackForce);
            }
            else
            {
                GetComponent<Rigidbody>().AddForce(knockbackForce * knockbackDirection, ForceMode.Impulse);
            }
        }

    }
}
