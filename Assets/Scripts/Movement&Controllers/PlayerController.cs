using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public MovementScript movementScript;

    public bool isControlled = true;
    bool isSprintingAnim;


    public GameObject backupCamera;
    public Transform cameraTransform; 
    public Rigidbody rb;
    public Animator animator;


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
            if(!movementScript.isDashing)
            {
                MovePlayer();
                if(Input.GetMouseButton(1))
                {
                    Vector3 direction = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z);
                    FaceTowards(direction);
                }
                Dash();
            }
        }
        UpdateAnimator();
    }

    void Initizialize()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", movementScript.movementSpeed);
        animator.SetBool("Sprinting", isSprintingAnim);
        animator.SetBool("Dashing", movementScript.isDashing);
    }

    void MovePlayer()
    {
        if(Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1))
        {
            isSprintingAnim = true;
        }
        else
        {
            isSprintingAnim = false;
        }

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

        movementScript.Move(moveDirection, isSprintingAnim);
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

        if (Input.GetKeyDown(KeyCode.Space) && dashDirection != Vector3.zero)
        {
            animator.SetTrigger("DashingTrigger");
            movementScript.Dash(dashDirection);
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
