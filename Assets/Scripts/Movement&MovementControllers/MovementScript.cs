using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.Events;

public class MovementScript : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement Variable")]
    [HideInInspector]
    public float currentMovementSpeed = 5f;
    public bool isSprinting;

    public float normalSpeed = 5f;
    public float sprintSpeed = 9f;
    public bool isMoving;

    [Header("Gravity")]
    public bool isGrounded;
    float gravityScale = 9.8f;

    [Header("Dodging/Dashing Values")]
    public float dodgeMoveDuration;
    public float dodgeForce; // to be made private

    [Header("Dodging/Dashing Cooldown")]
    public float maxDodgeCooldown = 0.5f;
    float dodgeCooldownRemaining = 0;
    public bool isDodging = false;

    public bool isInvincibleFromDodge = false;

    [Header("Component References")]
    public Animator animator;
    public Rigidbody rb;

    [Header("Coroutines")]
    Coroutine TweenCoroutine;
    Coroutine DodgeCoroutine;

    [Header("Ultimate Bool")]
    public bool ultimateCanMove = true;


    void Start()
    {
        Initizialize();
        sprintSpeed = currentMovementSpeed * 2;
    }

    void Update()
    {
        ApplyGravity();
        DodgeTimer();
    }

    void Initizialize()
    {
        currentMovementSpeed = normalSpeed;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public void StopMovementCoroutines()
    {
        StopCoroutine(TweenCoroutine);
        TweenCoroutine = null;

        StopCoroutine(DodgeCoroutine);
        DodgeCoroutine = null;
    }

    public void Move(Vector3 moveDirection, bool isSprinting)
    {
        if(ultimateCanMove && TweenCoroutine == null)
        {
            SprintCheckAndSpeedSetup(isSprinting);
            if(moveDirection != Vector3.zero)
            {
                characterController.Move(moveDirection * currentMovementSpeed * Time.deltaTime);
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

    public void TweenToPosition(Vector3 target, float moveDuration, float moveTowardsTargetOffset)
    {
        if(TweenCoroutine == null)
        {
            Vector3 targetPosition = TargetOffset(target, moveTowardsTargetOffset);
            TweenCoroutine = StartCoroutine(ILerpToPosition(targetPosition, moveDuration, moveTowardsTargetOffset));
        }
    }

    public void LerpToTransform(Transform target, float moveDuration, float moveTowardsTargetOffset)
    {
        if(TweenCoroutine == null)
        {
            TweenCoroutine = StartCoroutine(ILerpToTransform(target, moveDuration, moveTowardsTargetOffset));
        }
    }

    private IEnumerator ILerpToPosition(Vector3 target, float moveDuration, float moveTowardsTargetOffset)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = TargetOffset(target, moveTowardsTargetOffset);
        float distance = Vector3.Distance(startPosition, targetPosition);
        float speed = distance / moveDuration;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = (speed * elapsedTime) / distance;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        TweenCoroutine = null;
    }

    private IEnumerator ILerpToTransform(Transform target, float moveDuration, float moveTowardsTargetOffset)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = TargetOffset(target.position, moveTowardsTargetOffset);
        float distance = Vector3.Distance(startPosition, targetPosition);
        float speed = distance / moveDuration;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = (speed * elapsedTime) / distance;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        TweenCoroutine = null;
    }

    Vector3 TargetOffset(Vector3 target, float offsetDistance)
    {
        Vector3 position = target;
        return Vector3.MoveTowards(position, transform.position, offsetDistance);
    }

    void SprintCheckAndSpeedSetup(bool sprintInput)
    {
        if (sprintInput)
        {
            isSprinting = true;
            currentMovementSpeed = sprintSpeed;
        }
        else
        {
            isSprinting = false;
            currentMovementSpeed = normalSpeed;
        }
    }

    void ApplyGravity()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.01f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        if(!isGrounded)
        {
            Vector3 gravityForce = Vector3.down * gravityScale * Time.deltaTime;
            characterController.Move(gravityForce);
        }
    }

    public void DodgeInvincibilityFrame()
    {
        isInvincibleFromDodge = !isInvincibleFromDodge;
    }

    public void DodgeWithTarget(Vector3 dodgeDirection, float dodgeCooldownLength, Transform lockedTarget)
    {   
        if(ultimateCanMove && DodgeCoroutine == null)
        {
            maxDodgeCooldown = dodgeCooldownLength;

            isDodging = true;

            DodgeCoroutine = StartCoroutine(DodgeAround(lockedTarget, dodgeDirection, 5, dodgeMoveDuration));
        }
    }

    IEnumerator DodgeAround(Transform axisPoint, Vector3 orbitDirection, float orbitDistance, float orbitDuration)
    {
        float elapsedTime = 0f;
        float direction = orbitDirection.x > 0 ? -1 : 1;
        float radius = Vector3.Distance(transform.position, axisPoint.position);

        float initialDodgeAwayDistance = 0f;
        float finalDodgeAwayDistance = 0.5f;

        while (elapsedTime < orbitDuration)
        {
            elapsedTime += Time.deltaTime;
            float angle = (orbitDistance / radius) * (360f / (2 * Mathf.PI)) * Time.deltaTime * direction;
            Vector3 offset = transform.position - axisPoint.position;
            offset = Quaternion.Euler(0, angle, 0) * offset.normalized * radius;

            float currentDodgeAwayDistance = Mathf.Lerp(initialDodgeAwayDistance, finalDodgeAwayDistance, elapsedTime / orbitDuration);
            offset += offset.normalized * currentDodgeAwayDistance;

            transform.position = axisPoint.position + offset;
            DodgeCoroutine = null;
            yield return null;
        }
    }

    void DodgeTimer()
    {
        if (isDodging && dodgeCooldownRemaining <= 0)
        {
            dodgeCooldownRemaining = maxDodgeCooldown;
        }

        if (dodgeCooldownRemaining >= 0)
        {
            dodgeCooldownRemaining -= Time.deltaTime;
            
            if (dodgeCooldownRemaining <= 0)
            {
                dodgeCooldownRemaining = 0;
                isDodging = false;
            }
        }
    }

    public void KnockBack(float knockBackTime, float knockBackDelay, Vector3 knockBackOrigin)
    {
        Vector3 knockBackDirection = (transform.position - knockBackOrigin).normalized;
        transform.DOMove(transform.position + knockBackDirection / 2, knockBackTime).SetDelay(knockBackDelay);
    }

    public void FaceTowards(Vector3 orientation, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(orientation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
