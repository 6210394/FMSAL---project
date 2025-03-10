using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.UI;
using System.Collections;


[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerCombatController : MonoBehaviour
{
#region Variables & States

    public float detectionRange = 5;

    private EnemyCombatController bulletHitTarget;
    
    [Header("States")]
    public bool isLockOnToggle = false;
    
    private bool isAiming = false;
    public bool isAttackingEnemy = false;
#endregion

#region Component References
    public MovementScript movementScript;
    private PlayerMovementController playerMovementController;
    private CombatScript combatScript;
    private DepractedEnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private DiogenicPlayerInventory diogenicPlayerInventory;

    public Image crosshairReference;
    public Transform barrelAnchorReference;
    public LayerMask playerLayermask;
#endregion

#region Camera References & Targeting
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera targetCamera;
    [SerializeField] private CinemachineCamera aimCamera;

    [Header("Target References")]
    public EnemyCombatController currentLockedTarget;
    public EnemyCombatController lastTarget;
#endregion

    [Header("Player Combat Events")]
    public UnityEvent<CombatScript.HitEventArgs> OnHit;
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
        diogenicPlayerInventory = GetComponent<DiogenicPlayerInventory>();

        playerCamera = GameObject.Find("DefaultPlayerCamera").GetComponent<CinemachineCamera>();
        targetCamera = GameObject.Find("TargetCamera").GetComponent<CinemachineCamera>();
        aimCamera = GameObject.Find("ThirdPersonAimCamera").GetComponent<CinemachineCamera>();
        playerLayermask = LayerMask.GetMask("Player");
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

        AdjustLockOnCamera();

        PlayerAim();
        //or
        PlayerFaceTarget();

        PlayerDodge();


        //Process player inputs
        PlayerLockOn();
        
        if(Input.GetKey(KeyCode.Mouse0)) //Attack Command
        {
            if(playerMovementController.isControlled && !playerMovementController.movementScript.isDodging)
            {
                if(isAiming)
                {
                    PlayerShoot();
                }
                else
                {
                    PlayerPunch(combatScript.meleeRange);
                }
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
        //PLACEHOLDER FOR WHEN THE WEAPON SWITCHING IS IMPLEMENTED

        if(isAiming)
        {
            return;
        }

        if(Input.GetKeyDown("1"))
        {
            if(diogenicPlayerInventory.mainWeapon)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.mainWeapon);
                combatScript.UpdateStatsBasedOnWeapon(diogenicPlayerInventory.mainWeapon);   
            }
            else if (diogenicPlayerInventory.sidearm)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.sidearm);
                combatScript.UpdateStatsBasedOnWeapon(diogenicPlayerInventory.sidearm);
            }
            else
            {
                //maybe drop whatever the player is holding if it was a pickup
                diogenicPlayerInventory.HideItemInHands();
                combatScript.UpdateStatsBasedOnWeapon(null);
            }
        }

        if(Input.GetKeyDown("2"))
        {
            if (diogenicPlayerInventory.sidearm)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.sidearm);
                combatScript.UpdateStatsBasedOnWeapon(diogenicPlayerInventory.sidearm);
            }

            else if(diogenicPlayerInventory.mainWeapon)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.mainWeapon);
                combatScript.UpdateStatsBasedOnWeapon(diogenicPlayerInventory.mainWeapon);   
            }
            else
            {
                //maybe drop whatever the player is holding if it was a pickup
                diogenicPlayerInventory.HideItemInHands();
                combatScript.UpdateStatsBasedOnWeapon(null);
            }
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

    void PlayerPunch(float range)
    {
        if(!combatScript.meleeEquipped || !combatScript.attackIsAvailable)
        {
            return;
        }
               
        combatScript.Attack(CombatScript.AttackType.LightMelee, combatScript.meleeDuration, "Punch");
        if(currentLockedTarget != null)
        {
            transform.LookAt(currentLockedTarget.transform.position);
            if(TargetDistance(currentLockedTarget.transform) < range)
            {
                playerMovementController.movementScript.TweenToTarget(currentLockedTarget.gameObject.transform.position, combatScript.meleeDuration/1.75f, combatScript.punchTargetDistanceOffset);
            }
        }
        else
        {
            var camera = Camera.main;
            var forward = camera.transform.forward;
            var right = camera.transform.right;

            forward.y = 0f; // Keep the direction horizontal
            forward.Normalize();
            right.y = 0f;
            right.Normalize();

            Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
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
            Debug.Log(inputDirection);

            playerMovementController.movementScript.TweenToTarget(transform.position + direction * 1.5f, 0.5f, 0);
                //playerMovementController.movementScript.TweenToTarget(direction.normalized * 1.5f, 0.5f, 0);
        }
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
        
        combatScript.Attack(CombatScript.AttackType.Shoot, combatScript.gunRateOfFireTime, "Shoot"); //change later to be a variable for different guns
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
                
                aimCamera.Priority = 1;
                playerCamera.Priority = 0;
                targetCamera.Priority = 0;

                combatScript.animator.SetTrigger("enterAim");
                combatScript.animator.SetBool("isAiming", true);
            }
        } 

        if(Input.GetKeyUp(KeyCode.Mouse1)) //Let go of Aim
        { 
            isAiming = false;
            playerMovementController.canSprint = true;
            crosshairReference.enabled = false;

            playerCamera.Priority = 1;
            
            aimCamera.Priority = 0;

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
        if(isLockOnToggle && !isAiming && !movementScript.isDashing && !movementScript.isSprinting)
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
            else
            {
                playerMovementController.movementScript.Dash(inputDirection,0.5f);
            }
        }
    }

#endregion

#region Controller Functions

    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(hitEventArgs.target == transform)
        {
            if(Vector3.Distance(hitEventArgs.target.position, hitEventArgs.damageSource.transform.position) > hitEventArgs.attackRange)
            {
                Debug.Log("Too far");
                return;
            }

            if(movementScript.isInvincible)
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

    public void DealDamageEvent()
    {
        Debug.Log("Attack!");
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

    public IEnumerator IDamageRecievedCoroutine()
    {
        playerMovementController.isControlled = false;
        yield return new WaitForSeconds(1f);
        playerMovementController.isControlled = true;
    }

    void Die()
    {
        Debug.Log("Player got knocked out");
        debugDeadBoolean = true;
    }
#endregion

}
