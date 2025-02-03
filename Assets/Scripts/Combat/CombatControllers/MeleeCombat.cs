using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    public bool canAttack = false;

    PlayerController playerMovement;
    public float nudgeForce = 30f;
    RecieveImpact impact;

    public float animationTime = 0.1f;

    bool attackAvailable = true;    
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;

    public Vector3 attackPoint;

    bool isAttacking = false;
    public GameObject hitBoxPrefab;

    GameObject hitBoxMemory;

    Quaternion targetRotation;

    public void Start()
    {
        attackCooldownTimer = 0;
        playerMovement = GetComponent<PlayerController>();
        impact = GetComponent<RecieveImpact>();
    }

    public IEnumerator AttackRoutine()
    {
        playerMovement.isControlled = false;
        NudgeEntity();
        Vector3 spawnPosition = transform.TransformPoint(attackPoint);
        hitBoxMemory = Instantiate(hitBoxPrefab, spawnPosition, transform.rotation);
        hitBoxMemory.GetComponent<Hitbox>().damage = attackDamage;
        hitBoxMemory.GetComponent<Hitbox>().owner = gameObject;
        yield return new WaitForSeconds(animationTime);
        Destroy(hitBoxMemory);

        isAttacking = false;
        playerMovement.isControlled = true;
    }

    public void Attack()
    {
        if(!isAttacking && attackAvailable)
        {
            isAttacking = true;
            
            Transform cameraTransform = Camera.main.transform;
            Vector3 lookDirection = cameraTransform.forward;
            lookDirection.y = 0; // Keep the character upright
            transform.rotation = Quaternion.LookRotation(lookDirection);

            StartCoroutine(AttackRoutine());
        }
    }

    void Update()
    {
        if (!canAttack)
        {
            return;
        }
        
        if (isAttacking)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

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
            attackAvailable = false;
            attackCooldownTimer -= Time.deltaTime;
            
            if (attackCooldownTimer <= 0)
            {
                attackCooldownTimer = 0;
                attackAvailable = true;
            }
        }
    }

    public void NudgeEntity()
    {
         impact.AddImpact(transform.forward, nudgeForce);
    }
}
