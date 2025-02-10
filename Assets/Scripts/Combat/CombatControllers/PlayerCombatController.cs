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

    public UnityEvent<EnemyScript> OnPunch;
    public UnityEvent<EnemyScript> OnTrajectory;

    private bool isAiming = false;

    public float punchRange = 4f;
    public float punchDuration = 0.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyDetection = GetComponent<EnemyDetection>();
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
                PlayerPunch(punchRange);
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

    void PlayerPunch(float range)
    {
        if(!combatScript.canAttack || combatScript.isAttacking || combatScript.isAttacking)
        {
            Debug.Log("Can't attack");
            return;
        }
        
        lockedTarget = enemyDetection.CurrentTarget();
        combatScript.Attack(CombatScript.AttackType.Punch);
        if(lockedTarget != null)
        {
            Debug.Log("Punching");
            transform.DOLookAt(lockedTarget.transform.position, punchDuration);
            if(TargetDistance(lockedTarget) < range)
            {
                movementScript.MoveTowardsTarget(lockedTarget, punchDuration);
                OnPunch.Invoke(lockedTarget);
            }
        }
    }

    void PlayerShoot()
    {
        if(!combatScript.canShoot || combatScript.isAttacking)
        {
            return;
        }
        lockedTarget = enemyDetection.CurrentTarget();
        if(lockedTarget != null && !isAiming)
        {
            transform.DOLookAt(lockedTarget.transform.position, punchDuration);
        }
        combatScript.Attack(CombatScript.AttackType.Shoot);

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
        // Implementation of DamageEvent
    }
    

    float TargetDistance(EnemyScript target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }
}
