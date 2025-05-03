using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerCombatController : MonoBehaviour
{
    enum CameraType 
    {
        Default, Aim, Focus
    }

#region Variables & States

    private EnemyCombatController bulletHitTarget;
    
    [Header("States")]
    
    private bool isAiming = false;
#endregion

#region Component References
    public PlayerMovementController playerMovementController {get; private set;}
    public CombatScript combatScript {get; private set;}
    private EnemyDetection enemyDetection;

    [SerializeField] Image crosshairReference;
    [SerializeField] Transform barrelAnchorReference;
    [SerializeField] LayerMask playerLayermask;
#endregion

#region Camera References & Targeting
    [Header("Camera References")]
    public PlayerCameraInitializer PlayerCameras;

    private CinemachineCamera defaultCamera;
    private CinemachineCamera targetCamera;
    private CinemachineCamera aimCamera;

    [Header("Combat References")]
    private Queue<CombatScript.CombatActionType> attackQueue = new Queue<CombatScript.CombatActionType>();
    public EnemyCombatController currentLockedTarget;
    public EnemyCombatController lastTarget;

#endregion

    [Header("Player Combat Events")]
    public UnityEvent<CombatScript.HitEventArgs> OnHit;

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
        playerLayermask = LayerMask.GetMask("Player");

        //CHANGE THIS TO SUPPLY OUR OWN CROSSHAIR BASED ON THE WEAPON HELD
        crosshairReference = GameObject.Find("Crosshair").GetComponent<Image>();
        crosshairReference.enabled = false;

        GetCameraReferences();
        combatScript.healthScript.OnTakeDamage.AddListener((CombatScript.HitEventArgs hitEventArgs) => OnTakeHit(hitEventArgs));
        combatScript.healthScript.OnDeath.AddListener(Die);

        combatScript.SwitchWeapons(1);
    }

    void Update()
    {      
        StatusCheck();

        AdjustLockOnCamera();

        ProcessAttackQueue();

        PlayerAim();
        //or
        PlayerFaceTarget();

        //Process player inputs
        //PlayerLockOn();
        
        if(Input.GetKeyDown(KeyCode.Mouse0)) //Attack Command
        {
            if(combatScript.diogenicInventory.currentHeldWeapon.weaponType == WeaponScript.WeaponType.Gun)
            {
                QueueAttack(CombatScript.CombatActionType.Shoot);
            }
            else if (combatScript.diogenicInventory.currentHeldWeapon.weaponType == WeaponScript.WeaponType.Melee)
            {
                QueueAttack(CombatScript.CombatActionType.LightMelee);
            }
            else
            {
                return;
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

    void StatusCheck()
    {
        if(!combatScript.isAttacking)
        {
            GiveControl();

            if(currentLockedTarget != enemyDetection.CurrentTarget())
            {
                lastTarget = currentLockedTarget;
                currentLockedTarget = enemyDetection.CurrentTarget();
            }
            SwitchWeapons();
        }
    }

    void QueueAttack(CombatScript.CombatActionType attackType)
    {
        if (attackQueue.Count == 0)
        {
            Debug.Log("Attack queued");
            attackQueue.Enqueue(attackType);
        }
    }

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
    }

    void PlayerAttack(CombatScript.CombatActionType combatActionType)
    {
        switch(combatActionType)
        {
            case CombatScript.CombatActionType.LightMelee:
            {
                PlayerMelee();
                break;
            }

            case CombatScript.CombatActionType.Shoot:
            {
                PlayerShoot();
                break;
            }
        }
    }

    void PlayerMelee()
    {
        if(!combatScript.meleeEquipped || !combatScript.attackIsAvailable)
        {
            return;
        }

        #region Camera Reference
        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f;
        forward.Normalize();
        right.y = 0f;
        right.Normalize();

        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        #endregion
        
        if(currentLockedTarget)
        {
            Vector3 lookAtTarget = currentLockedTarget.transform.position;
            lookAtTarget.y = transform.position.y;
            transform.LookAt(lookAtTarget);
            if(TargetDistance(currentLockedTarget.transform) > combatScript.punchTargetDistanceOffset)
            {
                if(TargetDistance(currentLockedTarget.transform) < combatScript.meleeReach)
                {
                    playerMovementController.movementScript.LerpToTransform(currentLockedTarget.gameObject.transform, combatScript.meleeDuration/1.75f, combatScript.punchTargetDistanceOffset);
                }
            }
        }
        else
        {
            Vector3 direction;

            if (inputDirection == Vector3.zero)
            {
                direction = (forward + right).normalized; // Default push towards the camera direction
                direction.y = 0;
            }
            else
            {
                direction = (forward * inputDirection.z + right * inputDirection.x).normalized;
            }

            transform.LookAt(transform.position + direction);
        }
        
        combatScript.Attack(CombatScript.CombatActionType.LightMelee);
        RemoveControl();
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

            if(bulletHitTarget != null)
            {
                if(bulletHitTarget.GetComponent<HealthScript>() != null)
                {
                    //bulletHitTarget.GetComponent<HealthScript>().TakeDamage(combatScript.BuildAttack(combatScript.attackDamage, combatScript.gunStunDuration, transform));
                }
            }
        }
        
        combatScript.Attack(CombatScript.CombatActionType.Shoot); //change later to be a variable for different guns
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
                playerMovementController.isFocused = false;
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
        if(Input.GetKeyDown(KeyCode.Q) && !playerMovementController.isFocused) //Lock On Command
        {
            if(currentLockedTarget)
            {
                playerMovementController.isFocused = !playerMovementController.isFocused;
                playerMovementController.animator.SetBool("Strafe", true);
            }
            //make the FOV zoom closer or farther based on LockOnMode
        }

        else if(Input.GetKeyDown(KeyCode.Q) && playerMovementController.isFocused || currentLockedTarget == null)
        {
            playerMovementController.isFocused = false;
            playerMovementController.animator.SetBool("Strafe", false);
        }
    }

    void PlayerFaceTarget()
    {
        if(playerMovementController.isFocused && !isAiming && !playerMovementController.movementScript.isSprinting)
        {
           transform.DOLookAt(currentLockedTarget.transform.position, 0.1f);
        }
    }

    void PlayerParry()
    {
        combatScript.Attack(CombatScript.CombatActionType.Parry);
    }
#endregion

#region Controller Functions

    private void GetCameraReferences()
    {
        defaultCamera = PlayerCameras.defaultPlayerCamera.GetComponent<CinemachineCamera>();
        targetCamera = PlayerCameras.targetCamera.GetComponent<CinemachineCamera>();
        aimCamera = PlayerCameras.aimCamera.GetComponent<CinemachineCamera>();
    }

    private void RemoveControl()
    {
        playerMovementController.isControlled = false;
    }

    private void GiveControl()
    {
        playerMovementController.isControlled = true;
    }

    void ProcessAttackQueue()
    {
        if (combatScript.attackIsAvailable && attackQueue.Count > 0)
        {
            var nextAttack = attackQueue.Dequeue();
            PlayerAttack(nextAttack);
        }
    }

    void SwitchCamera(CameraType cameraType)
    {
        switch(cameraType)
        {
            case CameraType.Default:
            {
                CinemachineShake.Instance.SetActiveCamera(0);
                defaultCamera.Priority = 1;
                targetCamera.Priority = 0;
                aimCamera.Priority = 0;
                break;
            }
            case CameraType.Aim:
            {
                CinemachineShake.Instance.SetActiveCamera(1);
                aimCamera.Priority = 1;
                defaultCamera.Priority = 0;
                targetCamera.Priority = 0;
                break;
            }
            case CameraType.Focus:
            {
                CinemachineShake.Instance.SetActiveCamera(2);
                targetCamera.Priority = 1;
                aimCamera.Priority = 0;
                defaultCamera.Priority = 0;
                break;
            }
        }
    }

    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        playerMovementController.animator.SetTrigger("RecieveHit");
        if(hitEventArgs.damageSource != null)
        {
            playerMovementController.movementScript.Knockback(0.2f, hitEventArgs.damageSource.position, 1);
            StartCoroutine(ITakeHitSequence());
        }
    }

    IEnumerator ITakeHitSequence()
    {
        PostProcessManager.instance.VignetteFadeIn(0.6f, 0.05f);
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        PostProcessManager.instance.VignetteFadeOut(0.5f);
    }

    void AdjustLockOnCamera()
    {   
        if(!isAiming) //Manage the target group by remembering the last target and comparing with the current target
        {
            CinemachineTargetGroup cinemachineTargetGroup = targetCamera.GetComponentInChildren<CinemachineTargetGroup>();

            if (currentLockedTarget != null && playerMovementController.isFocused) 
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
                playerMovementController.isFocused = false;
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

        int dieAnimAnex = UnityEngine.Random.Range(1,4);
        combatScript.animator.SetFloat("deathIndex", dieAnimAnex);
        combatScript.animator.SetTrigger("Die");

        if(LevelManager.instance)
        {
            LevelManager.instance.FailMission();
        }
    }
#endregion

}
