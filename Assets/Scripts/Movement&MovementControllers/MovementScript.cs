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
    public float dashTime = 40f;
    
    public float maxDashTime = 0.5f;
    float currentDashTime;
    public bool isDashing;

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
        Debug.Log("Target Offset");
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

    public void Dash(Vector3 dashDirection)
    {   
        if (!isDashing && dashDirection != Vector3.zero)
        {
            dashDirection.y = 0;
            isDashing = true;
            transform.DOMove(dashDirection, dashTime);
        }
    }

    void DashTimer()
    {
        if (isDashing && currentDashTime <= 0)
        {
            currentDashTime = maxDashTime;
        }

        if (currentDashTime >= 0)
        {
            currentDashTime -= Time.deltaTime;
            
            if (currentDashTime <= 0)
            {
                currentDashTime = 0;
                isDashing = false;
            }
        }
    }

    public void FaceTowards(Vector3 orientation, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(orientation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
