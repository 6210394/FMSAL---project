using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovementScript : MovementScript
{
    private NavMeshAgent navMeshAgent;

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

    public override void Move(Vector3 moveDirection, bool isSprinting)
    {
        if(ultimateCanMove && KnockBackCoroutine == null)
        {
            SprintCheckAndSpeedSetup(isSprinting);
            if(moveDirection != Vector3.zero)
            {
                navMeshAgent.Move(moveDirection.normalized * currentMovementSpeed * Time.deltaTime);
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
            isMoving = false;
            currentMovementSpeed = 0;
        }

        animator.SetFloat("Speed", currentMovementSpeed);
        animator.SetBool("Sprinting", isSprinting);
    }

    public override void Knockback(float knockBackTime, Vector3 knockBackOrigin, float knockbackStrength)
    {        
        Debug.Log("Knocking back!");

        if(KnockBackCoroutine!=null)
        {
            StopCoroutine(KnockBackCoroutine);
            KnockBackCoroutine = null;
        }
        KnockBackCoroutine = StartCoroutine(IKnockBack(knockBackTime, knockBackOrigin, knockbackStrength));
    }

    protected override IEnumerator IKnockBack(float knockBackTime, Vector3 knockBackOrigin, float knockbackStrength)
    {
        Debug.Log("Knocking back ENUMERATOR!");
        float currentTime = 0;
        Vector3 knockBackDirection = (transform.position - knockBackOrigin).normalized;
        Vector3 knockbackVector = knockBackDirection * knockbackStrength;
        knockbackVector.y = 0;

        if (navMeshAgent.Raycast(knockbackVector, out pushHit))
        {
            knockbackVector = transform.position - pushHit.position;
        }

        Vector3 knockbackVelocity = knockbackVector / knockBackTime;

        // Knockback
        while (currentTime < knockBackTime)
        {
            navMeshAgent.Move(knockbackVelocity * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        KnockBackCoroutine = null;
        yield return null;
    }
}
