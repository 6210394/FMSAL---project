using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.ProBuilder;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    public MovementScript movementScript;

    public bool isControlled = true;
    public bool isInCombat = false;
    bool isSprintingAnim;


    public bool lockOnMode = false;
    public float softLockDistance = 10f;
    public float softLockAngle = 80f;

    public float playerRotationSpeed = 5f;

    public GameObject backupCamera;
    public CinemachineCamera playerCamera;
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

        if(GameObject.FindGameObjectWithTag("PlayerCamera"))
        {
            playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CinemachineCamera>();
            cameraTransform = playerCamera.transform;
        }
        else
        {
            Debug.LogWarning("No camera found, creating backup camera");
            backupCamera.GetComponent<CinemachineCamera>().LookAt = transform;
            backupCamera.GetComponent<CinemachineCamera>().Follow = transform;
            playerCamera = Instantiate(backupCamera, transform.position, Quaternion.identity).GetComponent<CinemachineCamera>();
            cameraTransform = playerCamera.transform;
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
                    movementScript.FaceTowards(direction, playerRotationSpeed);
                }
                if(Input.GetKeyDown(KeyCode.Q))
                {
                    lockOnMode = !lockOnMode;        
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
            movementScript.FaceTowards(moveDirection, playerRotationSpeed);
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
    

    void LookAtTarget(GameObject target)
    {
        playerCamera.LookAt = target.transform;
    }


    void DebugTools()
    {
        if(!GameObject.FindGameObjectWithTag("GameManager"))
        {
            Debug.LogWarning("No GameManager found!!!!");
        }
    }

    void OnDrawGizmos()
    {
        // Draw the range sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, softLockDistance);

        // Draw the detection cone
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-softLockAngle / 2, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(softLockAngle / 2, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * forward * softLockDistance;
        Vector3 rightRayDirection = rightRayRotation * forward * softLockDistance;

        Gizmos.DrawRay(transform.position, leftRayDirection);
        Gizmos.DrawRay(transform.position, rightRayDirection);
    }
}
