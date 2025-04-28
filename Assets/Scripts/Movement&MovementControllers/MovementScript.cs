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
    public bool usesGravity = true;
    public bool isGrounded;
    float gravityScale = 9.8f;

    /*
    [Header("Dodging/Dashing Values")]
    public float dodgeMoveDuration;
    public float dodgeForce; // to be made private

    [Header("Dodging/Dashing Cooldown")]
    public float maxDodgeCooldown = 0.5f;
    float dodgeCooldownRemaining = 0;
    public bool isDodging = false;

    public bool isInvincibleFromDodge = false;
    */

    [Header("Component References")]
    public Animator animator;
    public Rigidbody rb;

    [Header("Coroutines")]
    Coroutine KnockBackCoroutine;
    Coroutine TweenCoroutine;
    Coroutine DodgeCoroutine;

    [Header("Ultimate Bool")]
    private bool ultimateCanMove = true;


    void Start()
    {
        Initizialize();
        sprintSpeed = currentMovementSpeed * 2;
    }

    void Update()
    {
        if(usesGravity)
        {
            ApplyGravity();
        }
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
        Vector3 targetDirection = TargetOffset(target, moveTowardsTargetOffset);
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            characterController.Move(targetDirection * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetDirection;
        TweenCoroutine = null;
    }

    private IEnumerator ILerpToTransform(Transform target, float moveDuration, float moveTowardsTargetOffset)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = TargetOffset(target.position, moveTowardsTargetOffset);
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            Vector3 interpolatedPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);

            Vector3 moveDirection = interpolatedPosition - transform.position;
            moveDirection.y = 0;
            characterController.Move(moveDirection);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

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

    public void Knockback(float knockBackTime, Vector3 knockBackOrigin, float knockbackStrength)
    {
        if(KnockBackCoroutine!=null)
        {
            StopCoroutine(KnockBackCoroutine);
            KnockBackCoroutine = null;
        }
        KnockBackCoroutine = StartCoroutine(IKnockBack(knockBackTime, knockBackOrigin, knockbackStrength));
    }

    IEnumerator IKnockBack(float knockBackTime, Vector3 knockBackOrigin, float knockbackStrength)
    {
        float currentTime = 0;
        Vector3 knockBackDirection = (transform.position - knockBackOrigin).normalized;
        Vector3 knockbackVector = knockBackDirection * knockbackStrength;
        knockbackVector.y = 0;
        
        Vector3 knockbackVelocity = knockbackVector / knockBackTime;

        // Knockback
        while (currentTime < knockBackTime)
        {
            characterController.Move(knockbackVelocity * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return null;
    }

    public void FaceTowards(Vector3 orientation, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(orientation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
