using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;

[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerCombatController : MonoBehaviour
{
    private MovementScript movementScript;
    private PlayerMovementController playerController;
    private CombatScript combatScript;

    private CinemachineCamera playerCamera;
    private CinemachineCamera aimCamera;

    public EnemyScript lockedTarget;

    private EnemyManager enemyManager;
    EnemyDetection enemyDetection;

    public UnityEvent<int, EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnTrajectory;

    private bool meleeEquipped = false;
    private bool gunEquipped = false;
    private bool junkEquipped = false;

    private bool isAiming = false;

    public float punchRange = 4f;
    public float punchDuration = 0.5f;

    public float gunHipFireBulletAccuracyRange = 10f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyDetection = FindFirstObjectByType<EnemyDetection>();
        combatScript = GetComponent<CombatScript>();
        movementScript = GetComponent<MovementScript>();
        playerController = GetComponent<PlayerMovementController>();

        playerCamera = GameObject.Find("PlayerCamera").GetComponent<CinemachineCamera>();
        aimCamera = GameObject.Find("ThirdPersonAimCamera").GetComponent<CinemachineCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!combatScript.isAttacking)
        {
            lockedTarget = enemyDetection.CurrentTarget();
            SwitchWeapons();
        }

        PlayerAim();
        //or
        PlayerFaceTarget();

        PlayerDodge();


        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(playerController.isControlled && !movementScript.isDodging)
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

        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            combatScript.AttackCancel();
        }
    }

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
        if(lockedTarget != null)
        {
            transform.DOLookAt(lockedTarget.transform.position, punchDuration);
            if(TargetDistance(lockedTarget) < range)
            {
                movementScript.MoveTowardsTarget(lockedTarget, punchDuration);
            }
        }
    }

    void PlayerShoot()
    {
        if(!gunEquipped || !combatScript.attackAvailable || !combatScript.canAttack)
        {
            return;
        }
        if(lockedTarget != null && !isAiming)
        {
            transform.DOLookAt(lockedTarget.transform.position, punchDuration);
        }
        combatScript.Attack(CombatScript.AttackType.Shoot, 0.2f); //change later to be a variable for different guns
    }

    void PlayerAim()
    {
        if(!gunEquipped)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            if(playerController.isControlled)
            {
                isAiming = true;
                combatScript.animator.SetTrigger("enterAim");
                combatScript.animator.SetBool("isAiming", true);
                playerCamera.enabled = false;
                aimCamera.enabled = true;
            }
        } 
        if(Input.GetKeyUp(KeyCode.Mouse1))
        { 
            isAiming = false;
            combatScript.animator.SetBool("isAiming", false);
            playerCamera.enabled = true;
            aimCamera.enabled = false;
        }
        
        if(isAiming)
        {
            Vector3 direction = new Vector3(aimCamera.transform.forward.x, 0, aimCamera.transform.forward.z);
            movementScript.FaceTowards(direction, playerController.playerRotationSpeed);
        }
    }

    void PlayerFaceTarget()
    {
        if(lockedTarget != null && !isAiming && !movementScript.isDashing)
        {
           transform.DOLookAt(lockedTarget.transform.position, 0.1f);
        }
    }

    void PlayerDodge()
    {   
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 inputDirection = forward * Input.GetAxis("Vertical") + Vector3.right * Input.GetAxis("Horizontal");
        inputDirection = inputDirection.normalized;
        
        if (Input.GetKeyDown(KeyCode.Space) && inputDirection != Vector3.zero && !movementScript.isDodging)
        {
            playerController.animator.SetTrigger("DashingTrigger");
            if(lockedTarget)
            {
                movementScript.DodgeWithTarget(inputDirection, 0.5f, lockedTarget.transform);
            }
            else
            {
                movementScript.Dash(inputDirection,0.5f);
            }
        }
    }

    public void DamageEvent()
    {
        if (lockedTarget == null)
        {
            return;
        }
        if(Vector3.Distance(transform.position, lockedTarget.gameObject.transform.position) > enemyDetection.autoLockOnRange)
        {
            return;
        }

        OnHit.Invoke(combatScript.attackDamage, lockedTarget);
        //punchParticle.PlayParticleAtPosition(punchPosition.position);
    }
    

    float TargetDistance(EnemyScript target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    float CalculateOddOfHipFire()
    {
        float result = TargetDistance(lockedTarget);

        if((100f / result) > 90)
        {
            return 90f;
        }
        else
        {
            return 100f/result;
        }
    }
}
