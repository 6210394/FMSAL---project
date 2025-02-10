using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatScript : MonoBehaviour
{
    private PlayerController playerController;
    public Animator animator;

    public enum AttackType
    {
        Melee, HeavyMelee, Shoot
    }

    public bool canAttack = false; //debug variable

    public float nudgeForce = 30f;

    public bool attackAvailable = true;
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    float attackCooldownTimer = 0f;

    public bool isAttacking = false;


    public bool canShoot = false;
    public Vector3 reticleOffset;

    public GameObject bulletVisualsPrefab;
    public Transform spawnOriginOffset;

    public void Start()
    {
        attackCooldownTimer = 0;
        playerController = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
    }

    
    public void Attack(AttackType attackType, float specificAttackCooldown)
    {
        attackCooldown = specificAttackCooldown;
        switch(attackType)
        {
            case AttackType.Melee:
            {
                StartCoroutine(IMelee());
                break;
            }
            case AttackType.Shoot:
            {
                StartCoroutine(IShoot());
                break;
            }
        }
    }

    public IEnumerator IMelee()
    {
        if(playerController != null)
        {
            playerController.isControlled = false;
        }
        isAttacking = true;

        Debug.Log("Punch");
        animator.SetTrigger("Punch");
        yield return new WaitForSeconds(attackCooldown);
        
        isAttacking = false;

        if(playerController != null)
        {
            playerController.isControlled = true;
        }
    }

    public IEnumerator IShoot()
    {
        if(playerController != null)
        {
            playerController.isControlled = false;
        }
        isAttacking = true;

        animator.SetTrigger("Shoot");
        Debug.Log("Shoot");
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
        attackAvailable = true;
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
        transform.DOMove(transform.forward, nudgeForce);
    }

    public void Shoot()
    {
        RaycastHit hit;
        Camera renderingCamera = Camera.main;
        Ray ray = renderingCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        ray.origin += renderingCamera.transform.TransformDirection(reticleOffset);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.GetComponent<EnemyCombatController>() != null)
            {
                //hit.collider.gameObject.GetComponent<EnemyCombatController>().LoseHealth(attackDamage);
            }
        }
        GameObject bulletVisuals = Instantiate(bulletVisualsPrefab, transform.position + transform.rotation * spawnOriginOffset.position, transform.rotation);
        bulletVisuals.GetComponent<AutomaticMovementScript>().target = hit.point;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * spawnOriginOffset.position, 0.1f);
    }
}
