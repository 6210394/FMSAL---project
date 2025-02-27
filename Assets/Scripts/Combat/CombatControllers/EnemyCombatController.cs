using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    [Header("Stats")]
    public int health = 3;
    public float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRetreating;
    [SerializeField] private bool isLockedTarget;
    [SerializeField] private bool isStunned;
    [SerializeField] private bool isWaiting;

    bool isDead = false;

    //Animations
    Animator anim;
    private Rigidbody rb;

    //References
    private EnemyManager enemyManager;
    private MovementScript movementScript;
    private CharacterController characterController;

    [Header("Player References")]
    public List<GameObject> players = new List<GameObject>();
    public List<EnemyDetection> playerEnemyDetections = new List<EnemyDetection>();
    public PlayerCombatController target;

    [Header("Debug Tools")]
    public bool moveDebugBool = false;

    [Header("Patrol Options and Detection")]
    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool tiedPatrol = true;
    Vector3 spawnPoint;
    public float patrolRangeFromSpawn = 10f;

    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    public float deathTime = 1f;

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
    }

    void Update()
    {
        if(!isStunned && !isDead)
        {
            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
        }
    }

    void Initialize()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        
        movementScript = GetComponent<MovementScript>();
        players = GameManager.instance.players;
        foreach (GameObject player in players)
        {
            playerEnemyDetections.Add(player.GetComponent<EnemyDetection>());
            PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
            Debug.Log("Adding " + player + " OnHit");
            playerCombat.OnHit.AddListener((x, y) => OnTakeHit(x, y));
        }
        spawnPoint = transform.position;
        characterController = GetComponent<CharacterController>();
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

    public void OnTakeHit(int damageReceived, EnemyCombatController target)
    {
        if(target == this)
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

    public void SetAttack()
    {

    }

    public void SetRetreat()
    {

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

     #region Public Booleans

    public bool IsAttackable()
    {
        return health > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    public bool IsRetreating()
    {
        return isRetreating;
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
