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

    [Header("Attack Values")]
    public float punchRange = 3f; //Range withing which the melee hits
    public float punchReach = 4f; //Range within which the attack will tween
    public float punchDuration = 0.5f; //Duration of the attack
    public float punchStunDuration = 0.3f; //Duration of the stun
    private float meleeRange;
    private float meleeReach;
    private float meleeDuration;
    private float meleeStunDuration;
    public float punchTargetDistanceOffset = 2f;
    [Space]
    public float gunHipFireBulletAccuracyRange = 10f;
    public float gunAimAssistSize = 1f;
    private float gunStunDuration;
    public float gunRateOfFireTime = 140f; //in round per minute

    private EnemyCombatController bulletHitTarget;
    
    [Header("States")]
    public bool isLockOnToggle = false;
    
    private bool isAiming = false;
    public bool isAttackingEnemy = false;

    private bool meleeEquipped = false;
    private bool gunEquipped = false;
    private bool junkEquipped;

#endregion

#region Component References
    public MovementScript movementScript;
    private PlayerMovementController playerMovementController;
    private CombatScript combatScript;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private DiogenicPlayerInventory diogenicPlayerInventory;

    public Image crosshairReference;
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
                    PlayerPunch(meleeRange);
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
                UpdateStatsBasedOnWeapon(diogenicPlayerInventory.mainWeapon);   
            }
            else if (diogenicPlayerInventory.sidearm)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.sidearm);
                UpdateStatsBasedOnWeapon(diogenicPlayerInventory.sidearm);
            }
            else
            {
                //maybe drop whatever the player is holding if it was a pickup
                diogenicPlayerInventory.HideItemInHands();
                UpdateStatsBasedOnWeapon(null);
            }
        }

        if(Input.GetKeyDown("2"))
        {
            if (diogenicPlayerInventory.sidearm)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.sidearm);
                UpdateStatsBasedOnWeapon(diogenicPlayerInventory.sidearm);
            }

            else if(diogenicPlayerInventory.mainWeapon)
            {
                diogenicPlayerInventory.ShowItemInHands(diogenicPlayerInventory.mainWeapon);
                UpdateStatsBasedOnWeapon(diogenicPlayerInventory.mainWeapon);   
            }
            else
            {
                //maybe drop whatever the player is holding if it was a pickup
                diogenicPlayerInventory.HideItemInHands();
                UpdateStatsBasedOnWeapon(null);
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
        if(!meleeEquipped || !combatScript.attackIsAvailable)
        {
            return;
        }
               
        combatScript.Attack(CombatScript.AttackType.LightMelee, meleeDuration, "Punch"); //to change later when we have more weapons
        if(currentLockedTarget != null)
        {
            transform.DOLookAt(currentLockedTarget.transform.position, meleeDuration);
            if(TargetDistance(currentLockedTarget.transform) < range)
            {
                playerMovementController.movementScript.TweenToTarget(currentLockedTarget.gameObject.transform.position, meleeDuration/1.75f, punchTargetDistanceOffset);
            }
        }
    }

    void PlayerShoot()
    {
        if(!gunEquipped || !combatScript.attackIsAvailable)
        {
            return;
        }
        if(currentLockedTarget != null && !isAiming)
        {
            transform.DOLookAt(currentLockedTarget.transform.position, punchDuration);
        }
        
        if(isAiming)
        {   
            bulletHitTarget = null;

            Vector3 crosshairScreenPosition = crosshairReference.rectTransform.position;
            Debug.Log(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Debug.Log(crosshairScreenPosition);

            Ray ray = Camera.main.ScreenPointToRay(crosshairScreenPosition);
            RaycastHit hit;

            int layerMask = ~playerLayermask;

            if (Physics.SphereCast(ray, gunAimAssistSize, out hit, 100f, layerMask))
            {
                Debug.Log("Hit: " + hit.collider.name);
                // Save the first hit
                bulletHitTarget = hit.collider.GetComponent<EnemyCombatController>();
            }
        }      
        
        combatScript.Attack(CombatScript.AttackType.Shoot, gunRateOfFireTime, "Shoot"); //change later to be a variable for different guns
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

    void UpdateStatsBasedOnWeapon(WeaponScript weapon) //THIS NEEDS TO BE A WEAPON AND MUST BE VERIFIED
    {
        if(weapon != null)
        {
            combatScript.attackDamage = weapon.damage;

            switch(weapon.weaponType)
            {
                case WeaponScript.WeaponType.Melee:
                {
                    meleeEquipped = true;
                    gunEquipped = false;

                    meleeRange = weapon.weaponReach;
                    meleeDuration = weapon.swingTime;
                    break;
                }

                case WeaponScript.WeaponType.Gun:
                {
                    gunEquipped = true;
                    gunAimAssistSize = weapon.weaponAimAssistValue;
                    gunRateOfFireTime = 60f / weapon.rateOfFire;
                    Debug.Log(gunRateOfFireTime);
                    meleeEquipped = false;

                    float animationSpeed = weapon.rateOfFire / 60f;
                    combatScript.animator.SetFloat("ShootSpeed", animationSpeed);
                    Debug.Log(combatScript.animator.GetFloat("ShootSpeed"));
                    break;
                }
            }
        }

        else
        {
            combatScript.attackDamage = 1;
            meleeEquipped = true;
            gunEquipped = false;
            meleeRange = punchRange;
            meleeDuration = punchDuration;
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

    public void DealDamageEvent()
    {
        if(meleeEquipped)
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
            
                OnHit.Invoke(combatScript.BuildAttack(combatScript.attackDamage, meleeStunDuration, 2, currentLockedTarget, this));
            }
            //punchParticle.PlayParticleAtPosition(punchPosition.position);
        }

        if(gunEquipped)
        {
            Debug.Log("Shot fired!");
            if(bulletHitTarget == null)
            {
                return;
            }
            OnHit.Invoke(combatScript.BuildAttack(combatScript.attackDamage, gunStunDuration, 100, bulletHitTarget, this));
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

    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(hitEventArgs.playerCombatController == this)
        {
            if(Vector3.Distance(hitEventArgs.playerCombatController.transform.position, hitEventArgs.enemyCombatController.transform.position) > hitEventArgs.attackRange)
            {
                return;
            }

            if(movementScript.isInvincible)
            {
                Debug.LogWarning("DODGED");
                return;
            }
            Debug.Log("Took Damage");

            playerMovementController.animator.SetTrigger("RecieveHit");
            playerMovementController.movementScript.KnockBack(0.3f, 0.1f);

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
