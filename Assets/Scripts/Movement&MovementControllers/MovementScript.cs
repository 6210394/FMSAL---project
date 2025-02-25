using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MovementScript : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement Variable")]
    public float movementSpeed =5f;
    public bool isSprinting;

    float normalSpeed = 5f;
    float sprintSpeed = 9f;
    public bool isMoving;
    public float moveTowardsTargetOffset = 2f;


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
        movementSpeed = normalSpeed;
        characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector3 moveDirection, bool isSprinting)
    {
        SprintCheckAndSpeedSetup(isSprinting);
        if(moveDirection != Vector3.zero)
        {
            characterController.Move(moveDirection * movementSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
            movementSpeed = 0;
        }
    }

    public void MoveTowardsTarget(Transform target, float baseDuration)
    {
        transform.DOLookAt(target.transform.position, .2f);
        Vector3 targetPosition = TargetOffset(target.transform, moveTowardsTargetOffset);
        transform.DOMove(targetPosition, baseDuration); 
    }

    Vector3 TargetOffset(Transform target, float offsetDistance)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, offsetDistance);
    }

    void SprintCheckAndSpeedSetup(bool sprintInput)
    {
        if (sprintInput)
        {
            isSprinting = true;
            movementSpeed = sprintSpeed;
        }
        else
        {
            isSprinting = false;
            movementSpeed = normalSpeed;
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

    public void Dash(Vector3 dodgeDirection, float dodgeCooldownLength)
    {
        maxDodgeCooldown = dodgeCooldownLength;

        isDashing = true;
        
        transform.DOMove(transform.position + (dodgeDirection * dodgeForce), dodgeMoveDuration);
    }

    public void DodgeWithTarget(Vector3 dodgeDirection, float dodgeCooldownLength, Transform lockedTarget)
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
