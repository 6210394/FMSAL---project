using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isControlled = true;
    public bool isGrounded;
    public float gravityScale = 9.8f;

    public float currentSpeed =5f;
    public float normalSpeed = 5f;
    public float sprintSpeed = 9f;
    public bool isMoving;

    public float dashForce = 40f;
    
    public float maxDashTime = 0.5f;
    public float currentDashTime;
    public bool isDashing;

    public GameObject backupCamera;
    public Transform cameraTransform; 
    public Rigidbody rb;
    public Animator animator;

    //public CharacterController characterController;

    // Start is called before the first frame update
    void Awake()
    {
        Intizialize();
    }
    
    void Start()
    {
        //characterController = GetComponent<CharacterController>();
        DebugTools();

        if (GameObject.FindGameObjectWithTag("PlayerCamera"))
        {
            cameraTransform = GameObject.FindGameObjectWithTag("PlayerCamera").transform;
        }
        else
        {
            Debug.LogWarning("No camera found, creating backup camera");
            backupCamera.GetComponent<CameraMovement>().target = transform;
            cameraTransform = Instantiate(backupCamera, transform.position, Quaternion.identity).transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isControlled)
        {   
            Sprinting();
            if(!isDashing)
            {
                Move();
                if(Input.GetMouseButton(1))
                {
                    Vector3 direction = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
                    FaceTowards(direction);
                }
            }
            Dash();
        }
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

        void Intizialize()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", currentSpeed);
    }

    void Move()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();
        
        Vector3 moveDirection = forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");
        moveDirection = moveDirection.normalized;
        
        if (moveDirection != Vector3.zero && !Input.GetMouseButton(1))
        {
            FaceTowards(moveDirection);
        }
        
        transform.position += moveDirection * currentSpeed * Time.deltaTime;
        //characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    void Sprinting()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }
    }

    void Dash()
    {   
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();
        
        Vector3 dashDirection = forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");
        dashDirection = dashDirection.normalized;

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


        if (!isDashing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isDashing = true;
                rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
            }
        }
    }


    void ApplyGravity()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f))
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

    void FaceTowards(Vector3 orientation)
    {
        transform.rotation = Quaternion.LookRotation(orientation);
    }


    void DebugTools()
    {
        if(!GameObject.FindGameObjectWithTag("GameManager"))
        {
            Debug.LogWarning("No GameManager found!!!!");
        }
    }

    

}
