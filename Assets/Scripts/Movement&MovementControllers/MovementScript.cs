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
    public bool isDashing = false;

    public bool isInvincible = false;

    [Header("Component References")]
    public Animator animator;

    [Header("Ultimate Bool")]
    public bool isAllowedToMove = true;


    void Start()
    {
        Initizialize();
    }

    void Update()
    {
        DodgeTimer();
    }

     void FixedUpdate()
    {
        ApplyGravity();
    }

    void Initizialize()
    {
        currentMovementSpeed = normalSpeed;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void Move(Vector3 moveDirection, bool isSprinting)
    {
        if(isAllowedToMove)
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

    public void TweenToTarget(Vector3 target, float moveDuration, float moveTowardsTargetOffset)
    {
        if(isAllowedToMove)
        {
            //transform.DOLookAt(target, .2f);
            Vector3 targetPosition = TargetOffset(target, moveTowardsTargetOffset);
            transform.DOMove(targetPosition, moveDuration);
        }
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
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.2f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        if(!isGrounded)
        {
            transform.position += Vector3.down * gravityScale * Time.deltaTime;
        }
    }

    public void DodgeInvincibilityFrame()
    {
        isInvincible = !isInvincible;
    }

    public void Dash(Vector3 dodgeDirection, float dodgeCooldownLength)
    {
        if(isAllowedToMove)
        {
            maxDodgeCooldown = dodgeCooldownLength;

            isDashing = true;
            
            transform.DOMove(transform.position + (dodgeDirection * dodgeForce), dodgeMoveDuration);
        }
    }

    public void DodgeWithTarget(Vector3 dodgeDirection, float dodgeCooldownLength, Transform lockedTarget)
    {   
        if(isAllowedToMove)
        {
             maxDodgeCooldown = dodgeCooldownLength;

            isDodging = true;
            
            if(dodgeDirection.z < 0)
            {
                Dash(dodgeDirection, dodgeCooldownLength);
            }
            else
            {
                StartCoroutine(DodgeAround(lockedTarget, dodgeDirection, 5, dodgeMoveDuration));
            }
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
            yield return null;
        }
    }

    void DodgeTimer()
    {
        if (isDodging && !isDashing && dodgeCooldownRemaining <= 0)
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
                isDashing = false;
            }
        }
    }

    public void KnockBack(float knockBackTime, float knockBackDelay)
    {
        transform.DOMove(transform.position - (transform.forward / 2), knockBackTime).SetDelay(knockBackDelay);  

    }

    public void FaceTowards(Vector3 orientation, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(orientation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
