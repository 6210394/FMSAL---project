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
    public int maximumChainStun = 2;
    
    [Header("Attack Options")]
    public float comfortRange = 5f;
    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    [Header("Combat Booleans")]
    public bool isDead = false;
    public bool isPreparingAttack = false;
    public bool isAvailableForEnemyManager = true;

    //Animations
    Animator animator;
    private Rigidbody rb;

    //References
    private DepractedEnemyManager enemyManager;
    public EnemyMovementController enemyMovementController;
    public CombatScript combatScript;
    private CharacterController characterController;

    [Header("Player References")]
    public List<GameObject> players = new List<GameObject>();
    public List<EnemyDetection> playerEnemyDetections = new List<EnemyDetection>();
    public PlayerCombatController target;

    public Coroutine PrepareAttackCoroutine;
    public Coroutine DamageCoroutine;

    public UnityEvent<float, Transform> OnDamage; //stunTime
    public UnityEvent OnDeath;



    [Header("Debug Tools")]
    public bool moveDebugBool = false;

    void Awake()
    {
    }

    void Start()
    {
        GameManager.onPlayersListed.AddListener(() => UpdatePlayerList());
        Initialize();
        UpdatePlayerList();
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
    }

    void UpdatePlayerList()
    {
        Debug.Log("getting players!");
        players = GameManager.instance.players;
        foreach(GameObject player in players)
        {
            playerEnemyDetections.Add(player.GetComponent<EnemyDetection>());
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            playerCombat.OnHit.AddListener((a) => OnTakeHit(a));
        }
    }
    
    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(hitEventArgs.target == transform)
        {
            Debug.Log("isTarget");

            if (Vector3.Distance(transform.position, hitEventArgs.damageSource.position) > hitEventArgs.attackRange)
            {
                return;
            }
            else
            {
                Debug.Log("Player is in range!");
            }

            if(Vector3.Distance(hitEventArgs.damageSource.position, transform.position) <= detectionRange)
            {
                if(hitEventArgs.damageSource.GetComponent<PlayerCombatController>())
                {
                    target = hitEventArgs.damageSource.GetComponent<PlayerCombatController>();
                }
            }

            OnDamage.Invoke(hitEventArgs.stunDuration, hitEventArgs.damageSource);
            combatScript.health -= hitEventArgs.damageReceived;

            if(combatScript.health <= 0)
            {
                Die();
            }
        }
    }

    public void Retaliate()
    {
        isAvailableForEnemyManager = false;
        Attack();
    }

    /*
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
    */

    public void DealDamageEvent()
    {
        if(!target.isAttackingEnemy && !target.movementScript.isDashing)
            target.OnTakeHit(combatScript.BuildAttack(combatScript.attackDamage, combatScript.punchDuration,combatScript. punchRange, transform, target.transform));
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

    void Die()
    {   
        isDead = true;

        foreach(EnemyDetection enemyDetection in playerEnemyDetections)
        {
            enemyDetection.SetCurrentTarget(null);
        }

  

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
