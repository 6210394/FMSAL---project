using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerCombatController : MonoBehaviour
{
    enum CameraType 
    {
        Default, Aim, Focus
    }

#region Variables & States

    public int currentAnimationComboChain = 0;
    public float detectionRange = 5;
    private EnemyCombatController bulletHitTarget;
    
    [Header("States")]
    public bool isLockOnToggle = false;
    
    private bool isAiming = false;
    public bool isAttackingEnemy = false;
#endregion

#region Component References
    private PlayerMovementController playerMovementController;
    private CombatScript combatScript;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;

    public Image crosshairReference;
    public Transform barrelAnchorReference;
    public LayerMask playerLayermask;
#endregion

#region Camera References & Targeting
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera targetCamera;
    [SerializeField] private CinemachineCamera aimCamera;

    [Header("Combat References")]
    public EnemyCombatController currentLockedTarget;
    public EnemyCombatController lastTarget;


#endregion

    [Header("Player Combat Events")]
    public UnityEvent<CombatScript.HitEventArgs> OnHit;
    public UnityEvent OnTakeDamage;
    public UnityEvent<EnemyCombatController> OnTrajectory;

    [Header("Debug")]
    public bool debugDeadBoolean = false;

    void Awake()
    {
        enemyDetection = FindFirstObjectByType<EnemyDetection>();
        combatScript = GetComponent<CombatScript>();
        playerMovementController = GetComponent<PlayerMovementController>();
    }

    void Start()
    {
        playerCamera = GameObject.Find("DefaultPlayerCamera").GetComponent<CinemachineCamera>();
        targetCamera = GameObject.Find("TargetCamera").GetComponent<CinemachineCamera>();
        aimCamera = GameObject.Find("ThirdPersonAimCamera").GetComponent<CinemachineCamera>();

        playerLayermask = LayerMask.GetMask("Player");

        //CHANGE THIS TO SUPPLY OUR OWN CROSSHAIR BASED ON THE WEAPON HELD
        crosshairReference = GameObject.Find("Crosshair").GetComponent<Image>();
        crosshairReference.enabled = false;
    }

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

        AdjustLockOnCamera();

        PlayerAim();
        //or
        PlayerFaceTarget();

        PlayerDodge();


        //Process player inputs
        PlayerLockOn();
        
        if(Input.GetKey(KeyCode.Mouse0)) //Attack Command
        {
            if(!playerMovementController.movementScript.isDodging)
            {
                if(combatScript.diogenicInventory.currentHeldWeapon.weaponType == WeaponScript.WeaponType.Gun)
                {
                    PlayerShoot();
                }
                else if (combatScript.diogenicInventory.currentHeldWeapon.weaponType == WeaponScript.WeaponType.Melee)
                {
                    PlayerMelee();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if(!combatScript.isAttacking)
            {
                PlayerParry();
            }
        }
            
        if(debugDeadBoolean)
        {
            playerMovementController.isControlled = false;
        }
    }

#region Player Actions

    void SwitchWeapons()
    {
        if(isAiming)
        {
            return;
        }

        if(Input.GetKeyDown("1"))
        {
            combatScript.SwitchWeapons(1);
        }

        if(Input.GetKeyDown("2"))
        {
            combatScript.SwitchWeapons(2);
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

    void PlayerMelee()
    {
        if(!combatScript.meleeEquipped || !combatScript.attackIsAvailable)
        {
            return;
        }

        string animationTriggerName; //default animation trigger name
        if(currentAnimationComboChain <= combatScript.diogenicInventory.currentHeldWeapon.listOfAttacks.Count)
        {
            animationTriggerName = combatScript.diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].animationTriggerName;
        }
        else
        {
            Debug.Log("Combo restart");
            currentAnimationComboChain = 0;
            animationTriggerName = combatScript.diogenicInventory.currentHeldWeapon.listOfAttacks[currentAnimationComboChain].animationTriggerName;
        }

        #region Camera Reference
        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f; // Keep the direction horizontal
        forward.Normalize();
        right.y = 0f;
        right.Normalize();

        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        #endregion

        Vector3 direction = (forward * inputDirection.z + right * inputDirection.x).normalized;

        if (inputDirection != Vector3.zero)
        {
            direction = direction * 1f; // Move 1 meter in the input direction
        }
        else
        {
            direction = forward; // Default push towards the camera direction
        }

        transform.LookAt(transform.position + direction);

        combatScript.Attack(CombatScript.CombatActionType.LightMelee, combatScript.meleeDuration, animationTriggerName);
        playerMovementController.movementScript.TweenToPosition(transform.position + direction * 1.5f, 0.2f, 0);
        
        currentAnimationComboChain += 1;
    }

    void PlayerShoot()
    {
        if(!combatScript.gunEquipped || !combatScript.attackIsAvailable)
        {
            return;
        }
        if(currentLockedTarget != null && !isAiming)
        {
            transform.DOLookAt(currentLockedTarget.transform.position, 0.5f);
            //Hipfire shot
        }
        
        if(isAiming)
        {   
            bulletHitTarget = null;

            Vector3 crosshairScreenPosition = crosshairReference.rectTransform.position;

            Ray ray = Camera.main.ScreenPointToRay(crosshairScreenPosition);
            RaycastHit hit;

            int layerMask = ~playerLayermask;

            if (Physics.SphereCast(ray, combatScript.gunAimAssistSize, out hit, 100f, layerMask))
            {
                Debug.Log("Hit: " + hit.collider.name);
                // Save the first hit
                bulletHitTarget = hit.collider.GetComponent<EnemyCombatController>();
            }
        }      
        
        combatScript.Attack(CombatScript.CombatActionType.Shoot, combatScript.gunRateOfFireTime, "Shoot"); //change later to be a variable for different guns
    }

    void PlayerAim()
    {
        if(!combatScript.gunEquipped) //Can't aim without a gun OR A THROWABLE OBJECT -- TO ADJUST
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

                SwitchCamera(CameraType.Aim);

                combatScript.animator.SetTrigger("enterAim");
                combatScript.animator.SetBool("isAiming", true);
            }
        } 

        if(Input.GetKeyUp(KeyCode.Mouse1)) //Let go of Aim
        { 
            isAiming = false;
            playerMovementController.canSprint = true;
            crosshairReference.enabled = false;

            SwitchCamera(CameraType.Default);

            combatScript.animator.SetBool("isAiming", false);
        }
        
        if(isAiming) //While Aiming
        {
            Vector3 direction = new Vector3(aimCamera.transform.forward.x, 0, aimCamera.transform.forward.z);
            playerMovementController.movementScript.FaceTowards(direction, playerMovementController.playerRotationSpeed);
        }
    }

    void PlayerLockOn()
    {
        if(Input.GetKeyDown(KeyCode.Q) && !isLockOnToggle) //Lock On Command
        {
            if(currentLockedTarget)
            {
                isLockOnToggle = !isLockOnToggle;
                playerMovementController.canSprint = false;
                playerMovementController.animator.SetBool("Strafe", true);
            }
            //make the FOV zoom closer or farther based on LockOnMode
        }

        else if(Input.GetKeyDown(KeyCode.Q) && isLockOnToggle || currentLockedTarget == null)
        {
            isLockOnToggle = false;
            playerMovementController.canSprint = true;
            playerMovementController.animator.SetBool("Strafe", false);
        }
    }

    void PlayerFaceTarget()
    {
        if(isLockOnToggle && !isAiming && !playerMovementController.movementScript.isSprinting)
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
        
        if (Input.GetKeyDown(KeyCode.Space) && inputDirection != Vector3.zero && !playerMovementController.movementScript.isDodging)
        {
            combatScript.AttackCancel();
            playerMovementController.animator.SetTrigger("DashingTrigger");
            if(currentLockedTarget)
            {
                playerMovementController.movementScript.DodgeWithTarget(inputDirection, 0.5f, currentLockedTarget.transform);
            }
        }
    }

    void PlayerParry()
    {
        combatScript.Attack(CombatScript.CombatActionType.Parry, 0, "ParryTrigger");
    }
#endregion

#region Controller Functions

    private void RemoveControl()
    {
        combatScript.ultimateCanAttack = false;
        playerMovementController.movementScript.ultimateCanMove = false;
    }

    private void GiveControl()
    {
        combatScript.ultimateCanAttack = true;
        playerMovementController.movementScript.ultimateCanMove = true;
    }


    void SwitchCamera(CameraType cameraType)
    {
        CinemachineShake.Instance.ResetCameraShake();

        switch(cameraType)
        {
            case CameraType.Default:
            {
                CinemachineShake.Instance.cinemachineCamera = playerCamera;
                playerCamera.Priority = 1;
                targetCamera.Priority = 0;
                aimCamera.Priority = 0;
                break;
            }
            case CameraType.Aim:
            {
                CinemachineShake.Instance.cinemachineCamera = aimCamera;
                aimCamera.Priority = 1;
                playerCamera.Priority = 0;
                targetCamera.Priority = 0;
                break;
            }
            case CameraType.Focus:
            {
                CinemachineShake.Instance.cinemachineCamera = targetCamera;
                targetCamera.Priority = 1;
                aimCamera.Priority = 0;
                playerCamera.Priority = 0;
                break;
            }
        }
    }

    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(transform != hitEventArgs.damageSource)
        {
            if(playerMovementController.movementScript.isInvincibleFromDodge)
            {
                Debug.LogWarning("DODGED");
                return;
            }

            Debug.Log("Took Damage");

            playerMovementController.animator.SetTrigger("RecieveHit");
            playerMovementController.movementScript.KnockBack(0.3f, 0.1f, hitEventArgs.damageSource.position);

            combatScript.health -= hitEventArgs.damageReceived;

            if(combatScript.health <= 0)
            {
                Die();
            }
        }
        else
        {
            Debug.Log(name + ": I wasnt the target");
        }
    }

    /*
    public void DealDamageEvent()
    {
        Debug.Log("Attack!");
        GiveControl();
        if(combatScript.meleeEquipped)
        {
            if (currentLockedTarget == null)
            {
                return;
            }
            if(Vector3.Distance(transform.position, currentLockedTarget.gameObject.transform.position) > enemyDetection.autoLockOnRange)
            {
                return;
            }

            if(currentLockedTarget)
            {
                OnHit.Invoke(combatScript.BuildAttack(combatScript.attackDamage, combatScript.meleeStunDuration, combatScript.meleeRange, currentLockedTarget.transform, transform));
            }
            //punchParticle.PlayParticleAtPosition(punchPosition.position);
        }

        if(combatScript.gunEquipped)
        {
            Debug.Log("Shot fired!");
            if(bulletHitTarget == null)
            {
                return;
            }
            OnHit.Invoke(combatScript.BuildAttack(combatScript.attackDamage, combatScript.gunStunDuration, 100, bulletHitTarget.transform, transform));
        }
    }
    */

    void AdjustLockOnCamera()
    {   
        if(!isAiming) //Manage the target group by remembering the last target and comparing with the current target
        {
            CinemachineTargetGroup cinemachineTargetGroup = targetCamera.GetComponentInChildren<CinemachineTargetGroup>();

            if (currentLockedTarget != null && isLockOnToggle) 
            {
                if (cinemachineTargetGroup.FindMember(currentLockedTarget.transform) == -1)
                {
                    cinemachineTargetGroup.AddMember(currentLockedTarget.transform, 1, 2);
                }
                SwitchCamera(CameraType.Focus);
            }
            else
            {
                if(lastTarget != null)
                {
                    cinemachineTargetGroup.RemoveMember(lastTarget.transform);
                }
                isLockOnToggle = false;
                SwitchCamera(CameraType.Default);
            }
        }
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

    void Die()
    {
        Debug.Log("Player got knocked out");
        debugDeadBoolean = true;
    }
#endregion

}
