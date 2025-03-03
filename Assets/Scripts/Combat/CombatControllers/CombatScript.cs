using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatScript : MonoBehaviour
{
    public enum AttackType
    {
        LightMelee, HeavyMelee, Shoot, Slash
    }

    [Header ("Stats")]
    public int health = 3;
    public int attackDamage = 1;

    [Header ("States")]
    public bool isStunned = false;
    public bool stunImmune = false;
    [Space]
    public bool attackIsAvailable = true;
    public bool isAttacking = false;
    float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;
    
    [Header ("Object & Component References ")]
    [SerializeField] Vector3 reticleOffset;
    [SerializeField] GameObject bulletVisualsPrefab;
    [SerializeField] Transform bulletSpawnOriginOffset;
    [Space]
    private PlayerMovementController playerController;
    public Animator animator;

    [Header ("Debug")]
    [SerializeField] bool canAttack = false; //debug variable


    public void Start()
    {
        attackCooldownTimer = 0;
        playerController = GetComponent<PlayerMovementController>();
        //animator = GetComponent<Animator>();
    }

    public void ProcessAttackList(Dictionary<AttackType, int> stringOfAttacks)
    {
        //
    }
    
    public void Attack(AttackType attackType, float specificAttackCooldown, string animationName)
    {
        if(!isStunned && canAttack)
        {
            attackCooldown = specificAttackCooldown;
            switch(attackType)
            {
                case AttackType.LightMelee:
                {
                    StartCoroutine(ILightMelee(animationName));
                    break;
                }
                case AttackType.Shoot:
                {
                    StartCoroutine(IShoot(animationName));
                    break;
                }
            }
        }
    }

    public IEnumerator ILightMelee(string animationName)
    {
        if(playerController != null)
        {
            playerController.isControlled = false;
        }
        isAttacking = true;

        animator.SetTrigger(animationName);
        yield return new WaitForSeconds(attackCooldown);
        
        isAttacking = false;

        if(playerController != null)
        {
            playerController.isControlled = true;
        }
    }

    public IEnumerator IShoot(string animationName)
    {
        if(playerController != null)
        {
            playerController.isControlled = false;
        }
        isAttacking = true;

        animator.SetTrigger(animationName);
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        if(playerController != null)
        {
            playerController.isControlled = true;
        }
    }

    void Update()
    {
        AttackCooldownCountdown();   
    }

    public void AttackCancel()
    {
        StopAllCoroutines();
        isAttacking = false;
        attackIsAvailable = true;
        if(playerController != null)
        {
            playerController.isControlled = true;
        }
        //give control back to the player/enemy that is trying to move
    }

    public void AttackCooldownCountdown()
    {
        if (isAttacking && attackCooldownTimer <= 0)
        {
            attackCooldownTimer = attackCooldown;
        }

        if (attackCooldownTimer >= 0)
        {
            attackIsAvailable = false;
            attackCooldownTimer -= Time.deltaTime;
            
            if (attackCooldownTimer <= 0)
            {
                attackCooldownTimer = 0;
                attackIsAvailable = true;
            }
        }
    }

    public IEnumerator IStunned(float time)
    {
        isStunned = true;
        yield return new WaitForSeconds(time);
        isStunned = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(bulletSpawnOriginOffset)
        {
            Gizmos.DrawSphere(transform.position + transform.rotation * bulletSpawnOriginOffset.position, 0.1f);
        }
    }
}
