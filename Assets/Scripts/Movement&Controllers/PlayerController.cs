using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public MovementScript movementScript;

    public bool isControlled = true;
    

    public float movementSpeed =5f;
    

    public float dashForce = 40f;
    
    public float maxDashTime = 0.5f;
    public float currentDashTime;
    public bool isDashing;

    public GameObject backupCamera;
    public Transform cameraTransform; 
    public Rigidbody rb;
    public Animator animator;

    public RecieveImpact recieveImpact;

    // Start is called before the first frame update
    void Awake()
    {
        Initizialize();
    }
    
    void Start()
    {
        //characterController = GetComponent<CharacterController>();
        DebugTools();

        if (GameObject.FindGameObjectWithTag("MainCamera"))
        {
            cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
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
            if(!isDashing)
            {
                MovePlayer();
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

    void Initizialize()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        recieveImpact = GetComponent<RecieveImpact>();
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", movementSpeed);
        animator.SetBool("Sprinting", movementScript.isSprinting);
        animator.SetBool("Dashing", isDashing);
    }

    void MovePlayer()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();

        if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            movementScript.isMoving = true;
        }
        else
        {
            movementScript.isMoving = false;
            movementSpeed = 0;
        }

        if(Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1))
        {
            movementScript.isSprinting = true;
        }
        else
        {
            movementScript.isSprinting = false;
        }

        Vector3 moveDirection = forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");
        moveDirection = moveDirection.normalized;

        if (moveDirection != Vector3.zero && !Input.GetMouseButton(1))
        {
            FaceTowards(moveDirection);
        }

        movementScript.Move(moveDirection);
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


        if (!isDashing && dashDirection != Vector3.zero)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                animator.SetTrigger("DashingTrigger");

                isDashing = true;
                recieveImpact.AddImpact(dashDirection, dashForce);
                
            }
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
