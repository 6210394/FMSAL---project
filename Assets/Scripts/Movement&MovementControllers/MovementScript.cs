using UnityEngine;
using DG.Tweening;

public class MovementScript : MonoBehaviour
{
    private CharacterController characterController;

    //Movement variables
    public float movementSpeed =5f;
    public bool isSprinting;

    float normalSpeed = 5f;
    float sprintSpeed = 9f;
    public bool isMoving;

    //Gravity
    public bool isGrounded;
    float gravityScale = 9.8f;

    //Dashing Varaibles
    public float dodgeTime = 40f;
    public float dodgeForce; // to be made private

    //Dashing Timer Variables
    public float maxDodgeTimer = 0.5f;
    float currentDodgeTime;
    public bool isDodging;

    public float offsetDistanceToTarget = 2f;

    void Start()
    {
        Initizialize();
    }

    void Update()
    {
        DashTimer();
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

    public void MoveTowardsTarget(EnemyScript target, float baseDuration)
    {
        transform.DOLookAt(target.transform.position, .2f);
        Vector3 targetPosition = TargetOffset(target.transform, offsetDistanceToTarget);
        transform.DOMove(targetPosition, baseDuration); 
        
    }
    Vector3 TargetOffset(Transform target, float offsetDistance)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, offsetDistance);
    }

    void SprintCheckAndSpeedSetup(bool isSprinting)
    {
        if (isSprinting)
        {
            movementSpeed = sprintSpeed;
        }
        else
        {
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

    public void Dodge(Vector3 dashDirection)
    {   
        if (!isDodging && dashDirection != Vector3.zero)
        {
            dashDirection.y = 0;
            isDodging = true;
            transform.DOMove(transform.position + (dashDirection * dodgeForce), dodgeTime);
        }
    }

    void DashTimer()
    {
        if (isDodging && currentDodgeTime <= 0)
        {
            currentDodgeTime = maxDodgeTimer;
        }

        if (currentDodgeTime >= 0)
        {
            currentDodgeTime -= Time.deltaTime;
            
            if (currentDodgeTime <= 0)
            {
                currentDodgeTime = 0;
                isDodging = false;
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
