using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.Cinemachine;

[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerController))]
public class PlayerCombatController : MonoBehaviour
{
    private MovementScript movementScript;
    private PlayerController playerController;
    private CombatScript combatScript;

    private CinemachineCamera playerCamera;
    private CinemachineCamera aimCamera;

    public EnemyScript lockedTarget;

    private EnemyManager enemyManager;
    EnemyDetection enemyDetection;

    public UnityEvent<int, EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnTrajectory;

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
        playerController = GetComponent<PlayerController>();

        playerCamera = GameObject.Find("PlayerCamera").GetComponent<CinemachineCamera>();
        aimCamera = GameObject.Find("ThirdPersonAimCamera").GetComponent<CinemachineCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
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
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            PlayerAim();
        }
        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            isAiming = false;
            combatScript.animator.SetBool("isAiming", false);
            playerCamera.enabled = true;
            aimCamera.enabled = false;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            combatScript.AttackCancel();
        }
    }

    void SwitchWeapons()
    {
        //PLACEHOLDER FOR WHEN THE WEAPON SWITCHING IS IMPLEMENTED

        if(combatScript.canShoot)
        {
            enemyDetection.autoLockOnRange = 10f;
        }
        else
        {
            enemyDetection.autoLockOnRange = 5f;
        }
    }

    void PlayerMelee(float range)
    {
        if(!combatScript.attackAvailable || !combatScript.canAttack)
        {
            return;
        }
               
        lockedTarget = enemyDetection.CurrentTarget();
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
        if(!combatScript.canShoot || !combatScript.attackAvailable || !combatScript.canAttack)
        {
            return;
        }
        lockedTarget = enemyDetection.CurrentTarget();
        if(lockedTarget != null && !isAiming)
        {
            transform.DOLookAt(lockedTarget.transform.position, punchDuration);

        }
        combatScript.Attack(CombatScript.AttackType.Shoot, 0.2f); //change later to be a variable for different guns
    }

    void PlayerAim()
    {
        isAiming = true;
        combatScript.animator.SetTrigger("enterAim");
        combatScript.animator.SetBool("isAiming", true);
        playerCamera.enabled = false;
        aimCamera.enabled = true;
    }

    public void DamageEvent()
    {
        if (lockedTarget == null)
        {
            Debug.Log("No target");
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
