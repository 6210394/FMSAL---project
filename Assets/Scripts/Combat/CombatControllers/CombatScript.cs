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

    [Header("Attack Values")]
    public float punchRange = 3f; //Range withing which the melee hits
    public float punchReach = 4f; //Range within which the attack will tween
    public float punchDuration = 0.5f; //Duration of the attack
    public float punchStunDuration = 0.3f; //Duration of the stun
    public float meleeRange;
    public float meleeReach;
    public float meleeDuration;
    public float meleeStunDuration;
    public float punchTargetDistanceOffset = 2f;
    [Space]
    public float gunHipFireBulletAccuracyRange = 10f;
    public float gunAimAssistSize = 1f;
    public float gunStunDuration;
    public float gunRateOfFireTime = 140f; //in round per minute

    [Header ("States")]
    public bool isStunned = false;
    public bool stunImmune = false;
    float maxStunTimer;
    float currentStunTime;
    [Space]
    public bool attackIsAvailable = true;
    public bool isAttacking = false;
    float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;
    
    [Header ("Object & Component References ")]
    [SerializeField] Vector3 reticleOffset;
    [SerializeField] GameObject bulletVisualsPrefab;
    [SerializeField] Transform bulletSpawnOriginOffset;
    public GameObject hitBoxReference;

    [Space]
    private PlayerMovementController playerController;
    public Animator animator;

    [Header ("Debug")]
    [SerializeField] public bool debugCanAttack = false; //debug variable


    public void Start()
    {
        attackCooldownTimer = 0;
        playerController = GetComponent<PlayerMovementController>();
        //animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        AttackCooldownCountdown();
        StunCooldown();
    }

    public void ProcessAttackList(Dictionary<AttackType, int> stringOfAttacks)
    {
        //
    }

    public struct HitEventArgs
    {
        public int damageReceived;
        public float stunDuration;
        public float attackRange;
        public EnemyCombatController enemyCombatController;
        public PlayerCombatController playerCombatController;
    }

    public HitEventArgs BuildAttack(int damageReceived, float stunDuration, float attackRange, EnemyCombatController enemyCombatController, PlayerCombatController playerCombatController)
    {
        HitEventArgs hitEventArgs;

        hitEventArgs.damageReceived = damageReceived;
        hitEventArgs.stunDuration = stunDuration;
        hitEventArgs.attackRange = attackRange;
        hitEventArgs.enemyCombatController = enemyCombatController;
        hitEventArgs.playerCombatController = playerCombatController;

        return hitEventArgs;
    }
    
    public void Attack(AttackType attackType, float specificAttackCooldown, string animationName)
    {
        if(!isStunned && debugCanAttack)
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
        else if (!debugCanAttack)
        {
            Debug.LogWarning("debugCanAttack is set to false!!");
        }
    }

    public void GetStunned(float stunTimer)
    {
        isStunned = true;
        maxStunTimer = stunTimer;
    }

    public void StunCooldown()
    {
        if(stunImmune)
        {
            isStunned = false;
        }
        if(isStunned && currentStunTime == 0)
        {
            currentStunTime = maxStunTimer;
        }
        if(currentStunTime > 0)
        {
            currentStunTime -= Time.deltaTime;
            if(currentStunTime <= 0)
            {
                currentStunTime = 0;
                isStunned = false;
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
