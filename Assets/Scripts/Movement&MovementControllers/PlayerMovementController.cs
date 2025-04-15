using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Booleans")]
    public bool isControlled = true;
    public bool canSprint = true;
    bool isSprinting;

    public bool isFocused = false;

    [Header("Field Of View & Speed Values")]
    public float normalFOV = 60f;
    public float sprintingFOV;
    public float playerRotationSpeed = 5f;

    [Header("Component References")]
    public Transform cameraTransform;
    
    public Animator animator;

    public MovementScript movementScript;

#region Initialization
    void Awake()
    {
        Initialize();
    }
    
    void Start()
    {
        DebugTools();
    }

    void Initialize()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<MovementScript>();
    }
#endregion

    void Update()
    {
        cameraTransform = Camera.main.transform; //Find the reference for the current active camera
        if(isControlled)
        {   
            MovePlayer();
        }
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", movementScript.currentMovementSpeed);
        animator.SetBool("Sprinting", isSprinting);
    }

    void MovePlayer()
    {

        if(!isFocused && canSprint)
        {
            if(Input.GetKeyDown(KeyCode.LeftShift))
            {
                isSprinting = true;
            }
            if(Input.GetKeyUp(KeyCode.LeftShift))
            {
                isSprinting = false;
            }
            
            if(Input.GetKey(KeyCode.LeftShift))
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }
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

        #region Movement Animation Modifiers
        if(isFocused)
        {
            animator.SetFloat("StrafeDirection", moveDirection.z);

            bool isWalkingBack = Vector3.Dot(transform.forward, moveDirection.normalized) < 0;
            animator.SetBool("WalkBack", isWalkingBack);
        }
        #endregion

  
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
