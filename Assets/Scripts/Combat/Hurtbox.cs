using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Hurtbox : MonoBehaviour
{
    public CombatEntity combatEntity;
    public bool takesKnockback = true;
    public float entityWeight = 1f;

    NavMeshAgent agent;
    Rigidbody rb;


    public bool isPlayer = false;
    public RecieveImpact recieveImpact;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

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

/*
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "HitBox")
        {
            StartCoroutine(Pushback((other.transform.position - transform.position).normalized, 1)); 
        }
    }

    IEnumerator Pushback(Vector3 direction, float force)
    {
        float pushbackStartTime = Time.time;

        agent.velocity = Vector3.zero;

        while(Time.time - pushbackStartTime < 1)
        {
            //agent.velocity += direction * force * Time.deltaTime;
            agent.velocity = (direction * force) / Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        agent.velocity = Vector3.zero;
    }
    */
}
