using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.UI;


[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerCombatController : MonoBehaviour
{
#region Variables & States
    [Header("Stats")]
    public int health = 5;

    [Header("Attack Values")]
    public float punchRange = 4f;
    public float punchDuration = 0.5f;
    private float meleeRange;
    private float meleeDuration;

    public float gunHipFireBulletAccuracyRange = 10f;
    public float gunAimAssistSize = 1f;
    
    [Header("States")]
    private bool meleeEquipped = false;
    private bool gunEquipped = false;
    private bool junkEquipped = false;

    private bool isAiming = false;
    private bool isLockOnToggle = false;
#endregion

#region Component References
    private MovementScript movementScript;
    private PlayerMovementController playerMovementController;
    private CombatScript combatScript;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;

    public Image crosshairReference;
#endregion

#region Camera References & Targeting
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera targetCamera;
    [SerializeField] private CinemachineCamera aimCamera;

    [Header("Target References")]
    public EnemyCombatController currentLockedTarget;
    public EnemyCombatController lastTarget;
#endregion

    [Header("Player Combat Events")]
    public UnityEvent<int, EnemyCombatController> OnHit;
    public UnityEvent<EnemyCombatController> OnTrajectory;

    
    [Header("Debug")]
    public bool debugDeadBoolean = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyDetection = FindFirstObjectByType<EnemyDetection>();
        combatScript = GetComponent<CombatScript>();
        movementScript = GetComponent<MovementScript>();
        playerMovementController = GetComponent<PlayerMovementController>();

        playerCamera = GameObject.Find("DefaultPlayerCamera").GetComponent<CinemachineCamera>();
        targetCamera = GameObject.Find("TargetCamera").GetComponent<CinemachineCamera>();
        aimCamera = GameObject.Find("ThirdPersonAimCamera").GetComponent<CinemachineCamera>();
        crosshairReference.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!combatScript.isAttacking) //Between Attack Actions
        {
            if(currentLockedTarget != enemyDetection.CurrentTarget())
            {
                lastTarget = currentLockedTarget;
                currentLockedTarget = enemyDetection.CurrentTarget();
            }
            SwitchWeapons();
        }

        AdjustCamera();

        PlayerAim();
        //or
        PlayerFaceTarget();

        PlayerDodge();



        if(Input.GetKeyDown(KeyCode.Mouse0)) //Attack Command
        {
            if(playerMovementController.isControlled && !movementScript.isDodging)
            {
                if(isAiming)
                {
                    PlayerShoot();
                }
                else
                {
                    PlayerMelee(punchRange);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Q)) //Lock On Command
        {
            if(currentLockedTarget)
            {
                isLockOnToggle = !isLockOnToggle;
            }
            else
            {
                isLockOnToggle = false;
            }
            //make the FOV zoom closer or farther based on LockOnMode
        }

        if(debugDeadBoolean)
        {
            playerMovementController.isControlled = false;
        }
    }

#region Player Actions

    void SwitchWeapons()
    {
        //PLACEHOLDER FOR WHEN THE WEAPON SWITCHING IS IMPLEMENTED

        if(isAiming)
        {
            return;
        }
        if(Input.GetKeyDown("1"))
        {
            meleeEquipped = true;
            gunEquipped = false;
        }
        if(Input.GetKeyDown("2"))
        {
            meleeEquipped = false;
            gunEquipped = true;
        }

        /*
        if(gunEquipped)
        {
            enemyDetection.autoLockOnRange = 10f;
        }
        else
        {
            enemyDetection.autoLockOnRange = 5f;
        }
        */
    }

    void PlayerMelee(float range)
    {
        if(!meleeEquipped || !combatScript.attackAvailable || !combatScript.canAttack)
        {
            return;
        }
               
        combatScript.Attack(CombatScript.AttackType.Melee, punchDuration); //to change later when we have more weapons
        if(currentLockedTarget != null)
        {
            transform.DOLookAt(currentLockedTarget.transform.position, punchDuration);
            if(TargetDistance(currentLockedTarget.transform) < range)
            {
                movementScript.MoveTowardsTarget(currentLockedTarget.gameObject.transform, punchDuration);
            }
        }
    }

    void PlayerShoot()
    {
        if(!gunEquipped || !combatScript.attackAvailable || !combatScript.canAttack)
        {
            return;
        }
        if(currentLockedTarget != null && !isAiming)
        {
            transform.DOLookAt(currentLockedTarget.transform.position, punchDuration);
        }
        combatScript.Attack(CombatScript.AttackType.Shoot, 0.2f); //change later to be a variable for different guns
    }

    void PlayerAim()
    {
        if(!gunEquipped) //Can't aim without a gun OR A THROWABLE OBJECT -- TO ADJUST
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Mouse1)) //Press Aim
        {
            if(playerMovementController.isControlled)
            {
                isAiming = true;
                crosshairReference.enabled = true;

                playerMovementController.canSprint = false;
                isLockOnToggle = false;
                enemyDetection.SetCurrentTarget(null);
                
                aimCamera.enabled = true;
                playerCamera.enabled = false;
                targetCamera.enabled = false;

                combatScript.animator.SetTrigger("enterAim");
                combatScript.animator.SetBool("isAiming", true);
            }
        } 

        if(Input.GetKeyUp(KeyCode.Mouse1)) //Let go of Aim
        { 
            isAiming = false;
            playerMovementController.canSprint = true;
            crosshairReference.enabled = false;

            playerCamera.enabled = true;
            aimCamera.enabled = false;

            combatScript.animator.SetBool("isAiming", false);
        }
        
        if(isAiming) //While Aiming
        {
            Vector3 direction = new Vector3(aimCamera.transform.forward.x, 0, aimCamera.transform.forward.z);
            movementScript.FaceTowards(direction, playerMovementController.playerRotationSpeed);
        }
    }

    void PlayerFaceTarget()
    {
        if(currentLockedTarget != null && !isAiming && !movementScript.isDashing)
        {
           transform.DOLookAt(currentLockedTarget.transform.position, 0.1f);
        }
    }

    void PlayerDodge()
    {   
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 inputDirection = forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal");
        inputDirection.Normalize();
        
        if (Input.GetKeyDown(KeyCode.Space) && inputDirection != Vector3.zero && !movementScript.isDodging)
        {
            combatScript.AttackCancel();
            playerMovementController.animator.SetTrigger("DashingTrigger");
            if(currentLockedTarget)
            {
                movementScript.DodgeWithTarget(inputDirection, 0.5f, currentLockedTarget.transform);
            }
            else
            {
                movementScript.Dash(inputDirection,0.5f);
            }
        }
    }

#endregion

#region Controller Functions

    void AdjustCamera()
    {   
        if(!isAiming)
        {
            CinemachineTargetGroup cinemachineTargetGroup = targetCamera.GetComponentInChildren<CinemachineTargetGroup>();

            if (currentLockedTarget != null && isLockOnToggle) 
            {
                if (cinemachineTargetGroup.FindMember(currentLockedTarget.transform) == -1)
                {
                    cinemachineTargetGroup.AddMember(currentLockedTarget.transform, 1, 2);
                }
                playerCamera.enabled = false;
                targetCamera.enabled = true;
            }
            else
            {
                if(lastTarget != null)
                {
                    cinemachineTargetGroup.RemoveMember(lastTarget.transform);
                }
                isLockOnToggle = false;
                playerCamera.enabled = true;
                targetCamera.enabled = false;
            }
        }
    }

    public void DealDamageEvent()
    {
        if (currentLockedTarget == null)
        {
            return;
        }
        if(Vector3.Distance(transform.position, currentLockedTarget.gameObject.transform.position) > enemyDetection.autoLockOnRange)
        {
            return;
        }

        Debug.Log(currentLockedTarget);
        OnHit.Invoke(combatScript.attackDamage, currentLockedTarget);
        //punchParticle.PlayParticleAtPosition(punchPosition.position);
    }
    
    float TargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    float CalculateOddOfHipFire()
    {
        float result = TargetDistance(currentLockedTarget.transform);

        if((100f / result) > 90)
        {
            return 90f;
        }
        else
        {
            return 100f/result;
        }
    }

    public void OnTakeHit(int damageReceived, PlayerCombatController target)
    {
        if(target == this)
        {
            Debug.Log("Took Damage");

            playerMovementController.animator.SetTrigger("RecieveHit");
            movementScript.KnockBack(0.3f, 0.1f);

            health -= damageReceived;

            if(health <= 0)
            {
                Die();
            }
        }
        else
        {
            Debug.Log(name + ": I wasnt the target");
        }
    }

    void Die()
    {
        Debug.Log("Player got knocked out");
        debugDeadBoolean = true;
    }
#endregion

}
