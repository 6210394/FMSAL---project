using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class PlayerMovementController : MonoBehaviour
{
    public bool isControlled = true;
    public bool canSprint = true;
    bool isSprintingAnim;


    [Header("Field Of View & Speed Values")]
    public float normalFOV = 60f;
    public float sprintingFOV;
    public float playerRotationSpeed = 5f;


    [Header("Component References")]
    public GameObject backupCamera;
    public Transform cameraTransform;
    public Animator animator;

    private MovementScript movementScript;
    private CinemachineBrain cinemachineBrain;
    private Rigidbody rb;

#region Initialization
    void Awake()
    {
        Initialize();
    }
    
    void Start()
    {
        //characterController = GetComponent<CharacterController>();
        DebugTools();
        cinemachineBrain = FindAnyObjectByType<CinemachineBrain>();
    }

    void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        movementScript = GetComponent<MovementScript>();
    }
#endregion

    void Update()
    {
        cameraTransform = Camera.main.transform; //Find the reference for the current active camera
        if(isControlled)
        {   
            if(!movementScript.isDodging)
            {
                MovePlayer();
            }
        }
        UpdateAnimator();
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

        if(Input.GetKeyDown(KeyCode.LeftShift) && canSprint)
        {
            isSprintingAnim = true;
            Camera.main.DOFieldOfView(sprintingFOV, 0.2f);
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprintingAnim = false;
            Camera.main.DOFieldOfView(normalFOV, 0.2f);
        }


        if(Input.GetKey(KeyCode.LeftShift) && canSprint)
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
