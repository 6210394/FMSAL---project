using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public CharacterController characterController;
    public RecieveImpact recieveImpact;


    //Movement variables
    public float movementSpeed =5f;
    public bool isSprinting;

    public float normalSpeed = 5f;
    public float sprintSpeed = 9f;
    public bool isMoving;

    //Gravity
    public bool isGrounded;
    public float gravityScale = 9.8f;

    //Dashing Varaibles
    public float dashForce = 40f;
    
    public float maxDashTime = 0.5f;
    public float currentDashTime;
    public bool isDashing;

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
        recieveImpact = GetComponent<RecieveImpact>();
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
            isDashing = true;
            recieveImpact.AddImpact(dashDirection, dashForce);
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
}
