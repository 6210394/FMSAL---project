using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatScript : MonoBehaviour
{
    public Animator animator;

    public enum AttackType
    {
        Punch, HeavyPunch, Shoot
    }

    public bool canAttack = false; //debug variable

    public float nudgeForce = 30f;
    RecieveImpact impact;

    bool attackAvailable = true;
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
        animator = GetComponentInChildren<Animator>();
    }

    
    public void Attack(AttackType attackType)
    {
        switch(attackType)
        {
            case AttackType.Punch:
            StartCoroutine(IPunch());
            break;
        }
    }

    public IEnumerator IPunch()
    {
        isAttacking = true;

        //take control away from the player/enemy that is trying to move
        animator.SetTrigger("Punch");
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        //give control back to the player/enemy that is trying to move
    }

    void Update()
    {
        AttackCooldownCountdown();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            AttackCancel();
        }
    }

    public void AttackCancel()
    {
        StopAllCoroutines();
        isAttacking = false;
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
         impact.AddImpact(transform.forward, nudgeForce);
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
