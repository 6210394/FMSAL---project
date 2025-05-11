using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovementScript : MovementScript
{
    public NavMeshAgent navMeshAgent;
    public NavMeshPath CurrentPath;

    //Handle knockback so that the agent stops at obstacles instead of going around them
    NavMeshHit pushHit;
    
    protected override void Initizialize()
    {
        currentMovementSpeed = normalSpeed;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected override void Update()
    {
        
    }

    public void NavMeshMove(Vector3 moveDestination, bool isSprinting)
    {
        if(ultimateCanMove && KnockBackCoroutine == null)
        {
            SprintCheckAndSpeedSetup(isSprinting);
            if(moveDestination != Vector3.zero)
            {
                navMeshAgent.speed = currentMovementSpeed;
                navMeshAgent.SetDestination(moveDestination);

                CurrentPath = navMeshAgent.path;
                isMoving = true;
            }
            else
            {
                isMoving = false;
                currentMovementSpeed = 0;
            }
        }
        else
        {
            if(!ultimateCanMove)
            {
                Debug.Log("Prevented from moving");
            }
            if(KnockBackCoroutine != null)
            {
                Debug.Log("Knockback isn't over");
            }
            isMoving = false;
            currentMovementSpeed = 0;
            isSprinting = false;

            CurrentPath = null;
        }

        animator.SetFloat("Speed", currentMovementSpeed);
        animator.SetBool("Sprinting", isSprinting);
    }

    public override void Move(Vector3 moveDirection, bool isSprinting)
    {
        if(ultimateCanMove)
        {
            if(navMeshAgent.hasPath)
            {
                navMeshAgent.ResetPath();
            }
            SprintCheckAndSpeedSetup(isSprinting);
            if(moveDirection != Vector3.zero)
            {
                navMeshAgent.Move(moveDirection * currentMovementSpeed * Time.deltaTime);
                isMoving = true;
            }
            else
            {
                isMoving = false;
                currentMovementSpeed = 0;
            }
        }
        else
        {
            if(!ultimateCanMove)
            {
                Debug.Log("Movement has been disabled");
            }
            
            isMoving = false;
            isSprinting = false;
            currentMovementSpeed = 0;
        }

        animator.SetFloat("Speed", currentMovementSpeed);
        animator.SetBool("Sprinting", isSprinting);
    }

    public override void Knockback(float knockBackTime, Vector3 knockBackOrigin, float knockbackStrength)
    {        
        if(KnockBackCoroutine!=null)
        {
            StopCoroutine(KnockBackCoroutine);
            KnockBackCoroutine = null;
        }
        KnockBackCoroutine = StartCoroutine(IKnockBack(knockBackTime, knockBackOrigin, knockbackStrength));
    }

    protected override IEnumerator IKnockBack(float knockBackTime, Vector3 knockBackOrigin, float knockbackStrength)
    {
        ultimateCanMove = false;

        Vector3 knockBackDirection = (transform.position - knockBackOrigin).normalized;
        float elapsedTime = 0f;

        while (elapsedTime < knockBackTime)
        {
            if (navMeshAgent.Raycast(transform.position + knockBackDirection * knockbackStrength * Time.deltaTime, out pushHit))
            {
                break;
            }

            float knockBackStep = (knockbackStrength / knockBackTime) * Time.deltaTime;
            navMeshAgent.Move(knockBackDirection * knockBackStep);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ultimateCanMove = true;
        KnockBackCoroutine = null;
        yield return null;
    }
}
