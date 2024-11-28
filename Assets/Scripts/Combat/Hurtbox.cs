using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Hurtbox : MonoBehaviour
{
    public CombatEntity combatEntity;
    public bool takesKnockback = true;
    public float entityWeight = 1f;

    void Awake()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            takesKnockback = false;
        }
        combatEntity = GetComponent<CombatEntity>();
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
            GetComponent<Rigidbody>().AddForce(knockbackDirection * (knockbackForce / entityWeight), ForceMode.Impulse);
        }

    }
}
