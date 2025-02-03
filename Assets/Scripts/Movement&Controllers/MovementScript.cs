using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public CharacterController characterController;
    public float movementSpeed =5f;

    public bool isSprinting;

    public float normalSpeed = 5f;
    public float sprintSpeed = 9f;
    public bool isMoving;

    public bool isGrounded;
    public float gravityScale = 9.8f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void FixedUpdate()
    {
        ApplyGravity();
    }

    public void Move(Vector3 moveDirection)
    {
        Sprinting();
        characterController.Move(moveDirection * movementSpeed * Time.deltaTime);
    }

    void Sprinting()
    {
        if (isSprinting)
        {
            movementSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            movementSpeed = normalSpeed;
            isSprinting = false;
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
}
