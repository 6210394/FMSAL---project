using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    Animator anim;
    private Rigidbody rb;

    private EnemyScript enemyScript;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private MovementScript movementScript;

    public bool moveDebugBool = false;
    public List<GameObject> players = new List<GameObject>();


    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool tiedPatrol = true;
    Vector3 spawnPoint;
    public float patrolRangeFromSpawn = 10f;

    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    public float deathTime = 1f;

    [Header("Stats")]
    public int health = 3;
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting = true;

    public enum BehaviorState
    {
        Patrol,
        Fighting,
        Searching
    }

    public BehaviorState currentState = BehaviorState.Patrol;

    void Start()
    {
       Initialize();
       GameManager.onPlayersListed.AddListener(UpdatePlayerList);
    }

    void Update()
    {
        
    }

    void Initialize()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        enemyScript = GetComponent<EnemyScript>();
        movementScript = GetComponent<MovementScript>();
        players = GameManager.instance.players;
        spawnPoint = transform.position;
    }

    void UpdatePlayerList()
    {
        foreach(GameObject player in players)
        {
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            Debug.Log("Adding " + player + " OnHit");
            playerCombat.OnHit.AddListener((x, y) => OnTakeHit(x, y));
        }
    }

    bool IsPlayerInDetectionRange(Transform playerPosition)
    {   
        return Vector3.Distance(transform.position, playerPosition.position) <= detectionRange;
    }

    bool IsPlayerInFront(Transform playerPosition)
    {
        Vector3 directionToPlayer = (playerPosition.position- transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle <= fieldOfViewAngle / 2;
    }

    public void Patrol()
    {
        
    }

    public void OnTakeHit(int damageReceived, EnemyScript target)
    {
        if(target == enemyScript)
        {
            Debug.Log("Took Damage");
            StopEnemyCoroutines();

            anim.SetTrigger("RecieveHit");
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

    void StopEnemyCoroutines()
    {
        
    }

    void Die()
    {
        
    }


    /*
        public override void LoseHealth(float amount)
        {
            base.LoseHealth(amount);
            anim.SetTrigger("TakeDamage");
        }

        public override void Die()
        {
            rb.constraints = RigidbodyConstraints.None; //fun basic ragdoll
            Destroy(gameObject, deathTime);
        }

    */
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 frontRay = transform.position + transform.forward * patrolRangeFromSpawn;
        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }

}
