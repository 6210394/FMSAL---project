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
    private Vector3 givenMoveDirection;
    private Vector3 givenMoveDestination;

    [Header("States")]
    public bool seeksRetaliation = false;

    [Header("Stun Tolerance")]
    public int maximumChainStun = 3;
    private int currentChainStun = 0;
    
    [Header("Attack Options")]
    public bool prefersShooting = false;
    public bool prefersPunching = false;
    public float comfortRange = 5f;

    [Header("Attack Values")]
    public float punchRange = 3f;
    public float punchReach = 4f;
    public float punchDuration = 0.5f;
    public float punchStunDuration = 0.3f;
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
    public CombatScript combatScript;
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
            playerCombat.OnHit.AddListener((a) => OnTakeHit(a));
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
            playerCombat.OnHit.AddListener((a) => OnTakeHit(a));
        }
    }



    #region MyCode
    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(hitEventArgs.enemyCombatController == this)
        {
            if(Vector3.Distance(target.transform.position, hitEventArgs.playerCombatController.transform.position) > hitEventArgs.attackRange)
            {
                return;
            }

            StopEnemyCoroutines();

            if(Vector3.Distance(hitEventArgs.playerCombatController.transform.position, transform.position) <= detectionRange)
            {
                target = hitEventArgs.playerCombatController;
            }
            else
            {
                Panic();
            }
            anim.SetTrigger("RecieveHit");
            movementScript.KnockBack(0.3f, 0.1f);
            combatScript.GetStunned(hitEventArgs.stunDuration);
            currentChainStun += 1;
            combatScript.health -= hitEventArgs.damageReceived;

            if(combatScript.health <= 0)
            {
                Die();
            }

            if(currentChainStun >= maximumChainStun)
            {
                combatScript.stunImmune = true;
                seeksRetaliation = true;
            }
        }
    }

    public void Panic()
    {

    }

    public void Retaliate()
    {
        Attack();

    }

    public void WalkInRandomDirectionRandomly(float distance)
    {            

        if (!isMoving && !isPaused)
        {
            Vector3 randomDirection = GenerateRandomDirection();
            givenMoveDestination = randomDirection * distance;
            givenMoveDirection = randomDirection;
            isMoving = true;
            anim.SetFloat("Speed", moveSpeed);
        }

        if (isMoving)
        {
            movementScript.Move(givenMoveDirection, false);
            transform.LookAt(givenMoveDirection + transform.position);

            if(transform.position == givenMoveDestination)
            {
                isMoving = false;
                anim.SetFloat("Speed", 0);
                StartCoroutine(IWaitForRandomRange(1, 3));
            }
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
        if(!isDead)
        {
            anim.SetFloat("Speed", 1);
            anim.SetBool("Sprinting", isSprinting);
            
            Vector3 moveDir = (target.transform.position - transform.position).normalized;
            movementScript.Move(moveDir, isSprinting);
        }
    }

    public void RetreatAwayFromPlayer(float targetDistance)
    {
        if(!isDead)
        {
            anim.SetFloat("Speed", 1);

            if(Vector3.Distance(target.transform.position, transform.position) <= targetDistance)
            {
                transform.LookAt(target.transform);
                movementScript.Move(-transform.forward, false);
            }
        }
    }

    
    public void SetRetreat()
    {
        RetreatCoroutine = StartCoroutine(PrepRetreat());

        IEnumerator PrepRetreat()
        {
            isRetreating = true;
            
            yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) > comfortRange);
            Debug.LogWarning("Reached the end of the retreat");
            isRetreating = false;
            StopMoving();
        }
    }
    
    public IEnumerator IWaitForRandomRange(int a, int b)
    {
        if(!isPaused)
        {
            isPaused = true;

            float waitTime = Random.Range(a, b); // Random delay between 1 and 3 seconds
            yield return new WaitForSeconds(waitTime);

            isPaused = false;
        }
    }
    #endregion

    void StopEnemyCoroutines()
    {
        Debug.Log("Stopping enemy coroutines!");
        if (isRetreating)
        {
            if (RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
                isRetreating = false;
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);
            isPreparingAttack = false;

    }


    IEnumerator IRandomMovementDirection()
    {
        //Waits until the enemy is not assigned to no action like attacking or retreating
        //yield return new WaitUntil(() => isWaiting == true);

        int randomChance = Random.Range(0, 2);

        if (randomChance == 1)
        {
            int randomDir = Random.Range(0, 2);
            givenMoveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
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

    public void SetCircling()
    {
        MovementCoroutine = StartCoroutine(IGenerateCirclingDirection());
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
        anim.SetBool("Strafe", givenMoveDirection == Vector3.right || givenMoveDirection == Vector3.left);
        anim.SetFloat("StrafeDirection", givenMoveDirection.normalized.x, .2f, Time.deltaTime);

        //Don't do anything if isMoving is false
        if (!isMoving)
            return;

        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction

        Vector3 finalDirection = pDir * givenMoveDirection.normalized.x;

        dir += finalDirection * currentMoveSpeed * Time.deltaTime;

        movementScript.Move(dir, false);

        if (!isPreparingAttack)
            return;

        if(Vector3.Distance(transform.position, target.transform.position) < 2)
        {
            StopMoving();
            if (!combatScript.isStunned)
                Attack();
        }
    }

    public IEnumerator IGenerateCirclingDirection()
    {
        switch(Random.Range(1,4))
        {
            case 1:
            {
                givenMoveDirection = new Vector3(1,0,0);
                break;
            }
            case 2:
            {
                givenMoveDirection = new Vector3(-1,0,0);
                break;
            }
            case 3:
            {
                givenMoveDirection = new Vector3(0,0,0);
                break;
            }
            default:
            {
                givenMoveDirection = new Vector3(0,0,0);
                break;
            }
        }
        yield return new WaitForSeconds(3);
        Debug.Log("Generating new direction");
        MovementCoroutine = StartCoroutine(IGenerateCirclingDirection());
    }


    public void DealDamageEvent()
    {
        if(!target.isAttackingEnemy && !target.movementScript.isDashing)
            target.OnTakeHit(combatScript.BuildAttack(combatScript.attackDamage, punchDuration, punchRange, this, target));

        //PrepareAttack(false);
    }

    public void StopMoving()
    {
        isMoving = false;
        
        givenMoveDirection = Vector3.zero;
        if(characterController.enabled)
            characterController.Move(givenMoveDirection);
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
        if(!seeksRetaliation)
        {
            yield return new WaitForSeconds(0.2f);
            movementScript.TweenToTarget(target.transform.position, 0.5f, 1f);
            yield return new WaitForSeconds(0.2f);
        }

        
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
        movementScript.isAllowedToMove = false;

        foreach(EnemyDetection enemyDetection in playerEnemyDetections)
        {
            enemyDetection.SetCurrentTarget(null);
        }

        int dieAnimAnex = Random.Range(1,4);
        anim.SetFloat("deathIndex", dieAnimAnex);
        anim.SetTrigger("Die");

        enemyManager.SetEnemyAvailiability(this, false);  
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
            Gizmos.DrawWireSphere(givenMoveDestination, 0.5f); // Adjust the size as needed
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

    public bool IsRetreating()
    {
        return isRetreating;
    }

    

    #endregion

}
