using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 1;
    public bool isMoving;
    private Vector3 moveDirection;
    private Vector3 moveDestination;

    [Header("States")]
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;

    [Header("Stun Tolerance")]
    public int maximumChainStun = 3;
    private int currentChainStun = 0;
    
    [Header("Attack Options")]
    public bool prefersShooting = false;
    public bool prefersPunching = false;
    public float comfortRange = 5f;

    [Header("Patrol Options and Detection")]
    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool tiedPatrol = true;
    public bool isPaused = false;
    public Vector3 spawnPoint;
    public float patrolRangeFromSpawn = 10f;

    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    public float deathTime = 1f;

    bool isDead = false;

    [Header("Combat Booleans")]
    public bool isPreparingAttack;
    public bool isRetreating;
    public bool isCircling;

    //Animations
    Animator anim;
    private Rigidbody rb;

    //References
    private EnemyManager enemyManager;
    public MovementScript movementScript;
    private CombatScript combatScript;
    private CharacterController characterController;

    [Header("Player References")]
    public List<GameObject> players = new List<GameObject>();
    public List<EnemyDetection> playerEnemyDetections = new List<EnemyDetection>();
    public PlayerCombatController target;

    private Coroutine MovementCoroutine;
    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;
    private Coroutine DamageCoroutine;

    public UnityEvent<EnemyCombatController> OnDamage;
    public UnityEvent<EnemyCombatController> OnStopMoving;
    public UnityEvent<EnemyCombatController> OnRetreat;


    [Header("Debug Tools")]
    public bool moveDebugBool = false;


    void Start()
    {
       Initialize();

       MovementCoroutine = StartCoroutine(IRandomMovementDirection());
    }

    void Update()
    {
        
    }

    void Initialize()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        
        movementScript = GetComponent<MovementScript>();
        combatScript = GetComponent<CombatScript>();
        characterController = GetComponent<CharacterController>();

        players = GameManager.instance.players;
        foreach (GameObject player in players)
        {
            playerEnemyDetections.Add(player.GetComponent<EnemyDetection>());
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            playerCombat.OnHit.AddListener((x, y) => OnTakeHit(x, y));
        }
        spawnPoint = transform.position;
    }

    void UpdatePlayerList()
    {
        Debug.Log("UpdatingPlayerList");
        foreach(GameObject player in players)
        {
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            Debug.Log("Adding " + player + " OnHit");
            playerCombat.OnHit.AddListener((x, y) => OnTakeHit(x, y));
        }
    }

    #region MyCode
    public void OnTakeHit(int damageReceived, EnemyCombatController target)
    {
        if(target == this)
        {
            StopEnemyCoroutines();

            anim.SetTrigger("RecieveHit");
            movementScript.KnockBack(0.3f, 0.1f);

            combatScript.health -= damageReceived;

            if(combatScript.health <= 0)
            {
                Die();
            }
        }
    }

    public void WalkRandomly(float distanceMin, float distanceMax, float movementDuration)
    {
        if (!isMoving && !isPaused)
        {
            float randomDistance = Random.Range(distanceMin, distanceMax);
            Vector3 randomDirection = GenerateRandomDirection();
            moveDestination = randomDirection * randomDistance + transform.position;
            movementScript.TweenToTarget(moveDestination, randomDistance / moveSpeed, 0);
            isMoving = true;
            anim.SetFloat("Speed", moveSpeed);
        }

        if (isMoving && transform.position == moveDestination)
        {
            isMoving = false;
            anim.SetFloat("Speed", 0);
            StartCoroutine(IWaitForRandomRange(1, 3));
        }
    }

    public Vector3 GenerateRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        Vector3 randomDirection = new Vector3(randomX, 0, randomZ).normalized;
        return randomDirection;
    }

    public void ApproachPlayer(bool isSprinting)
    {
        anim.SetFloat("Speed", 1);
        anim.SetBool("Sprinting", isSprinting);
        
        Vector3 moveDir = (target.transform.position - transform.position).normalized;
        movementScript.Move(moveDir, isSprinting);
        
    }
    
    public IEnumerator IWaitForRandomRange(int a, int b)
    {
        if(!isPaused)
        {
            Debug.Log("waiting now");
            isPaused = true;

            float waitTime = Random.Range(a, b); // Random delay between 1 and 3 seconds
            yield return new WaitForSeconds(waitTime);

            isPaused = false;
        }
    }
    #endregion

    void StopEnemyCoroutines()
    {
        if (isRetreating)
        {
            if (RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

        if(DamageCoroutine != null)
            StopCoroutine(DamageCoroutine);

        if (MovementCoroutine != null)
            StopCoroutine(MovementCoroutine);
    }


    IEnumerator IRandomMovementDirection()
    {
        //Waits until the enemy is not assigned to no action like attacking or retreating
        //yield return new WaitUntil(() => isWaiting == true);

        int randomChance = Random.Range(0, 2);

        if (randomChance == 1)
        {
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            isMoving = true;
        }
        else
        {
            StopMoving();
        }

        anim.SetFloat("Speed", 0);
        yield return new WaitForSeconds(1);
        anim.SetFloat("Speed", moveSpeed);

        MovementCoroutine = StartCoroutine(IRandomMovementDirection());
    }

    public void EnemyCirclingMovement()
    {
        if(!target.movementScript.isDodging)
        {
            transform.LookAt(target.transform);
        }

        //Set movespeed based on direction
        float currentMoveSpeed = moveSpeed;

        //Set Animator values
        anim.SetBool("Strafe", moveDirection == Vector3.right || moveDirection == Vector3.left);
        anim.SetFloat("StrafeDirection", moveDirection.normalized.x, .2f, Time.deltaTime);

        //Don't do anything if isMoving is false
        if (!isMoving)
            return;

        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction
        Vector3 movedir = Vector3.zero;

        Vector3 finalDirection = Vector3.zero;

        if (moveDirection == Vector3.forward)
            finalDirection = dir;
        if (moveDirection == Vector3.right || moveDirection == Vector3.left)
            finalDirection = pDir * moveDirection.normalized.x;
        if (moveDirection == -Vector3.forward)
            finalDirection = -transform.forward;

        if (moveDirection == Vector3.right || moveDirection == Vector3.left)
            currentMoveSpeed /= 1.5f;

        movedir += finalDirection * currentMoveSpeed * Time.deltaTime;

        characterController.Move(movedir);

        if (!isPreparingAttack)
            return;

        if(Vector3.Distance(transform.position, target.transform.position) < 2)
        {
            StopMoving();
            if (!isStunned)
                Attack();
        }
    }

    public void SetRetreat()
    {
        StopEnemyCoroutines();

        RetreatCoroutine = StartCoroutine(PrepRetreat());

        IEnumerator PrepRetreat()
        {
            yield return new WaitForSeconds(1.4f);
            OnRetreat.Invoke(this);
            isRetreating = true;
            moveDirection = -Vector3.forward;
            isMoving = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) > 4);
            isRetreating = false;
            StopMoving();

            //Free 
            isPaused = true;
            MovementCoroutine = StartCoroutine(IRandomMovementDirection());
        }
    }


    public void DealDamageEvent()
    {
        if(!target.isAttackingEnemy && !target.movementScript.isDashing)
            target.OnTakeHit(1, this);

        //PrepareAttack(false);
    }

    public void StopMoving()
    {
        isMoving = false;
        
        moveDirection = Vector3.zero;
        if(characterController.enabled)
            characterController.Move(moveDirection);
    }

    public void Attack()
    {
        if(prefersPunching)
        {
            PrepareAttackCoroutine = StartCoroutine(IPrepareAttack());
        }
        else
        {
            Debug.Log("Shooting not implemented for enemies");
        }
    }

    IEnumerator IPrepareAttack()
    {
        isPreparingAttack = true;
        yield return new WaitForSeconds(0.2f);
        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        anim.SetTrigger("Punch");
        yield return new WaitForSeconds(0.2f);
        isPreparingAttack = false;
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
        characterController.enabled = false;

        foreach(EnemyDetection enemyDetection in playerEnemyDetections)
        {
            enemyDetection.SetCurrentTarget(null);
        }

        int dieAnimAnex = Random.Range(1,4);
        anim.SetFloat("deathIndex", dieAnimAnex);
        anim.SetTrigger("Die");

        enemyManager.SetEnemyAvailiability(this, false);  
        enabled = false;
    }

    public void OnDrawGizmos()
    {

        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);

        if (isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(moveDestination, 0.5f); // Adjust the size as needed
        }
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

    public bool IsLockedTarget()
    {
        return isLockedTarget;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    #endregion

}
