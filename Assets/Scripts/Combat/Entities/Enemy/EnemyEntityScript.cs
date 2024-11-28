using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntityScript : CombatEntity
{
    public Animator anim;
    public Rigidbody rb;

    public AutomaticMovementScript autoMove;

    public bool moveDebugBool = false;



    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool tiedPatrol = true;
    public float patrolRangeFromSpawn = 10f;

    public float deathTime = 1f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        autoMove = GetComponent<AutomaticMovementScript>();
    }

    void Update()
    {
        if(moveDebugBool)
        {
            if(autoMove.canMove)
            {
                autoMove.MoveToTarget();
            }
        }
    }

    public override void LoseHealth(float amount)
    {
        base.LoseHealth(amount);
        anim.SetTrigger("TakeDamage");
    }

    public override void Die()
    {
        rb.constraints = RigidbodyConstraints.None; //fun basic ragdoll
        Destroy(gameObject, deathTime);
    }


}
