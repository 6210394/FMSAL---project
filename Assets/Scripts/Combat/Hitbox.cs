using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 10f;
    public GameObject owner;

    public LayerMask hitMask;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != owner)
        {
            Hurtbox hurtbox = other.GetComponent<Hurtbox>();
            if (hurtbox != null)
            {
                hurtbox.OnHit(damage, knockbackForce, transform.forward);

            }
        }
    }
}
