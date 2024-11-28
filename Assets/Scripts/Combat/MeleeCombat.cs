using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    PlayerMovement playerMovement;
    public float nudgeForce = 30f;

    public float animationTime = 0.1f;

    bool canAttack = true;
    public float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;

    public Vector3 attackPoint;

    bool isAttacking = false;
    public GameObject hitBoxPrefab;

    GameObject hitBoxMemory;

    public void Start()
    {
        attackCooldownTimer = 0;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public IEnumerator AttackRoutine()
    {
        playerMovement.isControlled = false;
        NudgeEntity();
        Vector3 spawnPosition = transform.TransformPoint(attackPoint);
        hitBoxMemory = Instantiate(hitBoxPrefab, spawnPosition, transform.rotation);
        hitBoxMemory.GetComponent<Hitbox>().owner = gameObject;
        yield return new WaitForSeconds(animationTime);
        Destroy(hitBoxMemory);
        isAttacking = false;
        playerMovement.isControlled = true;
    }

    public void Attack()
    {
        if(!isAttacking && canAttack)
        {
            isAttacking = true;
            StartCoroutine(AttackRoutine());
        }
    }

    void Update()
    {
        AttackCooldownCountdown();

        if(Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            AttackCancel();
        }
    }

    public void AttackCancel()
    {
        StopAllCoroutines();
        Destroy(hitBoxMemory);
        isAttacking = false;
        playerMovement.isControlled = true;
    }

    public void AttackCooldownCountdown()
    {
        if (isAttacking && attackCooldownTimer <= 0)
        {
            attackCooldownTimer = attackCooldown;
        }

        if (attackCooldownTimer >= 0)
        {
            canAttack = false;
            attackCooldownTimer -= Time.deltaTime;
            
            if (attackCooldownTimer <= 0)
            {
                attackCooldownTimer = 0;
                canAttack = true;
            }
        }
    }

    public void NudgeEntity()
    {
        playerMovement.rb.AddForce(transform.forward * nudgeForce, ForceMode.Impulse);
    }
}
