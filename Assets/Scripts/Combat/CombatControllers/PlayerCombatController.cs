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

#region Variables & States

    private EnemyCombatController bulletHitTarget;
    
    [Header("States")]
    
    private bool isAiming = false;
#endregion

#region Component References
    public PlayerMovementController playerMovementController {get; private set;}
    public CombatScript combatScript {get; private set;}
    private EnemyDetectionManager enemyDetection;

    [SerializeField] Image crosshairReference;
    [SerializeField] Transform barrelAnchorReference;
    [SerializeField] LayerMask playerLayermask;

    PostProcessManager postProcessManager;
#endregion

#region Camera References & Targeting


    [Header("Combat References")]
    private Queue<CombatScript.CombatActionType> attackQueue = new Queue<CombatScript.CombatActionType>();
    public EnemyCombatController currentLockedTarget;

#endregion

    [Header("Player Combat Events")]
    public UnityEvent<CombatScript.HitEventArgs> OnHit;

    [Header("Debug")]
    public bool debugDeadBoolean = false;

    void Awake()
    {
        enemyDetection = FindFirstObjectByType<EnemyDetectionManager>();
        combatScript = GetComponent<CombatScript>();
        playerMovementController = GetComponent<PlayerMovementController>();
    }

    void Start()
    {
        postProcessManager = FindFirstObjectByType<PostProcessManager>();
        playerLayermask = LayerMask.GetMask("Player");

        //CHANGE THIS TO SUPPLY OUR OWN CROSSHAIR BASED ON THE WEAPON HELD
        if(GameObject.Find("Crosshair"))
        {
            crosshairReference = GameObject.Find("Crosshair").GetComponent<Image>();
            crosshairReference.enabled = false;
        }
        
        combatScript.healthScript.OnTakeDamage.AddListener((CombatScript.HitEventArgs hitEventArgs) => OnTakeHit(hitEventArgs));
        combatScript.healthScript.OnDeath.AddListener(Die);

        enemyDetection.OnTargetSelected.AddListener((EnemyCombatController currentTarget) => GetTarget(currentTarget));

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

        if(currentLockedTarget)
        {   
            if(!currentLockedTarget.IsAttackable())
            {
                currentLockedTarget = null;
            }
        }
        

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

            SwitchWeapons();
        }
    }

    void GetTarget(EnemyCombatController target)
    {
        currentLockedTarget = target;
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
            if(TargetDistance(currentLockedTarget.transform) > combatScript.targetDistanceOffset)
            {
                if(TargetDistance(currentLockedTarget.transform) < combatScript.meleeReach)
                {
                    playerMovementController.movementScript.LerpToTransform(currentLockedTarget.gameObject.transform, combatScript.meleeDuration/1.75f, combatScript.targetDistanceOffset);
                }
            }
        }
        else
        {
            Vector3 direction = (forward * inputDirection.z + right * inputDirection.x).normalized;

            if (direction != Vector3.zero)
            {
                transform.LookAt(transform.position + direction);
            }
        }
        playerMovementController.movementScript.FaceTowards(transform.forward, playerMovementController.playerRotationSpeed);    
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

                playerMovementController.SwitchCamera(PlayerMovementController.CameraType.Aim);

                combatScript.animator.SetTrigger("enterAim");
                combatScript.animator.SetBool("isAiming", true);
            }
        } 

        if(Input.GetKeyUp(KeyCode.Mouse1)) //Let go of Aim
        { 
            isAiming = false;
            playerMovementController.canSprint = true;
            crosshairReference.enabled = false;

            playerMovementController.SwitchCamera(PlayerMovementController.CameraType.Default);

            combatScript.animator.SetBool("isAiming", false);
        }
        
        if(isAiming) //While Aiming
        {
            Vector3 direction = new Vector3(playerMovementController.aimCamera.transform.forward.x, 0, playerMovementController.aimCamera.transform.forward.z);
            playerMovementController.movementScript.FaceTowards(direction, playerMovementController.playerRotationSpeed);
        }
    }

    /*                           DOESN'T FEEL GOOD
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
    */

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

    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        playerMovementController.animator.SetTrigger("RecieveHit");
        playerMovementController.animator.SetBool("IsStunned", false);

        if(hitEventArgs.damageSource != null)
        {
            playerMovementController.movementScript.Knockback(0.2f, hitEventArgs.damageSource.position, 1);
            StartCoroutine(ITakeHitSequence());
        }
    }

    IEnumerator ITakeHitSequence()
    {
        postProcessManager.VignetteFadeIn(0.6f, 0.05f);
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        postProcessManager.VignetteFadeOut(0.5f);
    }

    void AdjustLockOnCamera()
    {   
        if(!isAiming && playerMovementController.targetCamera != null) //Manage the target group by remembering the last target and comparing with the current target
        {
            CinemachineTargetGroup cinemachineTargetGroup = playerMovementController.targetCamera.GetComponentInChildren<CinemachineTargetGroup>();

            if (currentLockedTarget != null && playerMovementController.isFocused) 
            {
                if (cinemachineTargetGroup.FindMember(currentLockedTarget.transform) == -1)
                {
                    cinemachineTargetGroup.AddMember(currentLockedTarget.transform, 1, 2);
                }
                playerMovementController.SwitchCamera(PlayerMovementController.CameraType.Focus);
            }
            else
            {
                if(enemyDetection.lastTarget != null)
                {
                    cinemachineTargetGroup.FindMember(enemyDetection.lastTarget.gameObject.transform);
                    {
                        cinemachineTargetGroup.RemoveMember(enemyDetection.lastTarget.transform);
                    }
                }
                playerMovementController.isFocused = false;
                playerMovementController.SwitchCamera(PlayerMovementController.CameraType.Default);
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

        int dieAnimAnex = Random.Range(1,4);
        combatScript.animator.SetFloat("deathIndex", dieAnimAnex);
        combatScript.animator.SetTrigger("Die");

        if(LevelManager.instance)
        {
            LevelManager.instance.FailMission();
        }
    }
#endregion

}
