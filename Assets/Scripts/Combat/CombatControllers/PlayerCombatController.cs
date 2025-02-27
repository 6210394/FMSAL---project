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
    [Space]
    public float gunHipFireBulletAccuracyRange = 10f;
    public float gunAimAssistSize = 1f;
    public float gunRateOfFireTime = 140f; //in round per minute

    private EnemyCombatController bulletHitTarget;
    
    [Header("States")]
    public bool isLockOnToggle = false;
    
    private bool isAiming = false;

    private bool meleeEquipped = false;
    private bool gunEquipped = false;
    private bool junkEquipped;

#endregion

#region Component References
    private MovementScript movementScript;
    private PlayerMovementController playerMovementController;
    private CombatScript combatScript;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private DiogenicPlayerInventory diogenicPlayerInventory;

    public Image crosshairReference;
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
    public UnityEvent<int, EnemyCombatController> OnHit; //damage, target
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
            if(playerMovementController.isControlled && !movementScript.isDodging)
            {
                if(isAiming)
                {
                    PlayerShoot();
                }
                else
                {
                    PlayerMelee(meleeRange);
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

    void PlayerMelee(float range)
    {
        if(!meleeEquipped || !combatScript.attackAvailable || !combatScript.canAttack)
        {
            return;
        }
               
        combatScript.Attack(CombatScript.AttackType.Melee, meleeDuration); //to change later when we have more weapons
        if(currentLockedTarget != null)
        {
            transform.DOLookAt(currentLockedTarget.transform.position, meleeDuration);
            if(TargetDistance(currentLockedTarget.transform) < range)
            {
                movementScript.MoveTowardsTarget(currentLockedTarget.gameObject.transform, meleeDuration/1.75f);
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
        
        if(isAiming)
        {   
            bulletHitTarget = null;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.SphereCast(ray, gunAimAssistSize, out hit, 100f))
            {
                Debug.Log("Hit: " + hit.collider.name);
                // Save the first hit
                bulletHitTarget = hit.collider.GetComponent<EnemyCombatController>();
            }
        }

        
        
        combatScript.Attack(CombatScript.AttackType.Shoot, gunRateOfFireTime); //change later to be a variable for different guns
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

    void PlayerLockOn()
    {
        if(Input.GetKeyDown(KeyCode.Q) && !isLockOnToggle) //Lock On Command
        {
            if(currentLockedTarget)
            {
                isLockOnToggle = !isLockOnToggle;
                playerMovementController.canSprint = false;
            }
            //make the FOV zoom closer or farther based on LockOnMode
        }

        else if(Input.GetKeyDown(KeyCode.Q) && isLockOnToggle)
        {
            isLockOnToggle = false;
            playerMovementController.canSprint = true;
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

                    float animationSpeed = gunRateOfFireTime / 60f;
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

            Debug.Log(currentLockedTarget);
            if(currentLockedTarget)
            {
                OnHit.Invoke(combatScript.attackDamage, currentLockedTarget);
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
            OnHit.Invoke(combatScript.attackDamage, bulletHitTarget);
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
