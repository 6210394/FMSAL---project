using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class PlayerMovementController : MonoBehaviour
{
    public MovementScript movementScript;

    public bool isControlled = true;
    bool isSprintingAnim;

    public float playerRotationSpeed = 5f;

    public float sprintingFOV;
    public float normalFOV = 60f;

    public GameObject backupCamera;
    public CinemachineBrain cinemachineBrain;
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
        movementScript = GetComponent<MovementScript>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        cameraTransform = Camera.main.transform;
        if(isControlled)
        {   
            if(!movementScript.isDodging)
            {
                MovePlayer();
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
        animator.SetBool("Dashing", movementScript.isDodging);
    }

    void MovePlayer()
    {
        bool isSprinting = false;


        if(Input.GetKeyDown(KeyCode.LeftShift) && !Input.GetMouseButton(1))
        {
            isSprintingAnim = true;
            Camera.main.DOFieldOfView(sprintingFOV, 0.2f);
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprintingAnim = false;
            Camera.main.DOFieldOfView(normalFOV, 0.2f);
        }


        if(Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
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
        movementScript.Move(moveDirection, isSprinting);
    }

    void DebugTools()
    {
        if(!GameObject.FindGameObjectWithTag("GameManager"))
        {
            Debug.LogWarning("No GameManager found!!!!");
        }
    }
}
