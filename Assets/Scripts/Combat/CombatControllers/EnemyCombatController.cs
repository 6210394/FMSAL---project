using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyCombatController : MonoBehaviour
{
    [Header("States")]
    public bool seeksRetaliation = false;

    [Header("Stun Tolerance")]
    public int maximumChainStun = 3;
    private int currentChainStun = 0;
    
    [Header("Attack Options")]
    public bool prefersShooting = false;
    public bool prefersPunching = false;
    public float comfortRange = 5f;

    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    public float deathTime = 1f;

    public bool isDead = false;

    [Header("Combat Booleans")]
    public bool isPreparingAttack = false;
    public bool isAvailableForEnemyManager = true;

    //Animations
    Animator animator;
    private Rigidbody rb;

    //References
    private EnemyManager enemyManager;
    public EnemyMovementController enemyMovementController;
    public CombatScript combatScript;
    private CharacterController characterController;

    [Header("Player References")]
    public List<GameObject> players = new List<GameObject>();
    public List<EnemyDetection> playerEnemyDetections = new List<EnemyDetection>();
    public PlayerCombatController target;

    public Coroutine PrepareAttackCoroutine;
    public Coroutine DamageCoroutine;

    public UnityEvent<EnemyCombatController> OnDamage;
    public UnityEvent<EnemyCombatController> OnStopMoving;
    public UnityEvent<EnemyCombatController> OnRetreat;


    [Header("Debug Tools")]
    public bool moveDebugBool = false;


    void Start()
    {
       Initialize();

    }

    void Update()
    {
        
    }

    void Initialize()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        
        combatScript = GetComponent<CombatScript>();
        characterController = GetComponent<CharacterController>();

        players = GameManager.instance.players;
        foreach (GameObject player in players)
        {
            playerEnemyDetections.Add(player.GetComponent<EnemyDetection>());
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            playerCombat.OnHit.AddListener((a) => OnTakeHit(a));
        }
    }

    void UpdatePlayerList()
    {
        Debug.Log("UpdatingPlayerList");
        foreach(GameObject player in players)
        {
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            Debug.Log("Adding " + player + " OnHit");
            playerCombat.OnHit.AddListener((a) => OnTakeHit(a));
        }
    }
    
    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(hitEventArgs.enemyCombatController == this)
        {
            if(Vector3.Distance(transform.position, hitEventArgs.playerCombatController.transform.position) > hitEventArgs.attackRange)
            {
                return;
            }
            else
            {
                Debug.Log("Player is in range!");
            }

            StopEnemyCoroutines();

            if(Vector3.Distance(hitEventArgs.playerCombatController.transform.position, transform.position) <= detectionRange)
            {
                target = hitEventArgs.playerCombatController;
            }
            animator.SetTrigger("RecieveHit");
            enemyMovementController.movementScript.KnockBack(0.3f, 0.1f);
            combatScript.GetStunned(hitEventArgs.stunDuration);
            currentChainStun += 1;
            combatScript.health -= hitEventArgs.damageReceived;

            if(combatScript.health <= 0)
            {
                Die();
            }

            if(currentChainStun >= maximumChainStun && combatScript.debugCanAttack)
            {
                combatScript.stunImmune = true;
                Retaliate();
            }
        }
    }

    public void Retaliate()
    {
        isAvailableForEnemyManager = false;
        Attack();
    }
    

    void StopEnemyCoroutines()
    {
        Debug.Log("Stopping enemy coroutines!");
        if (enemyMovementController.isRetreating)
        {
            if (enemyMovementController.RetreatCoroutine != null)
                StopCoroutine(enemyMovementController.RetreatCoroutine);
                enemyMovementController.isRetreating = false;
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);
            isPreparingAttack = false;

        if (enemyMovementController.PatrolDirectionCoroutine != null)
        {
            StopCoroutine(enemyMovementController.PatrolDirectionCoroutine);
        }

    }

    public void DealDamageEvent()
    {
        if(!target.isAttackingEnemy && !target.movementScript.isDashing)
            target.OnTakeHit(combatScript.BuildAttack(combatScript.attackDamage, combatScript.punchDuration,combatScript. punchRange, this, target));
            PrepareAttackCoroutine = null;
    }

    public void Attack()
    {
        if(combatScript.debugCanAttack)
        {
            isPreparingAttack = true;
        }
        else
        {
            isPreparingAttack = false;
        }
    }

    public bool CheckForPlayersInDetectionRange()
    {
        foreach (GameObject player in players)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= detectionRange)
            {
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (angleToPlayer <= fieldOfViewAngle / 2)
                {
                    Debug.Log("Player detected");
                    target = player.GetComponent<PlayerCombatController>();
                    return true;
                }
            }
            else
            {
                target = null;
                return false;
            }
        }
        return false;
    }

    void Die()
    {   
        StopEnemyCoroutines();

        isDead = true;
        target = null;
        enemyMovementController.movementScript.isAllowedToMove = false;

        foreach(EnemyDetection enemyDetection in playerEnemyDetections)
        {
            enemyDetection.SetCurrentTarget(null);
        }

        int dieAnimAnex = Random.Range(1,4);
        animator.SetFloat("deathIndex", dieAnimAnex);
        animator.SetTrigger("Die");

        enemyManager.SetEnemyAvailiability(this, false);  
    }

    public void OnDrawGizmos()
    {

        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }

    #region Public Booleans

    public bool IsAttackable()
    {
        return combatScript.health > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }


    #endregion

}
