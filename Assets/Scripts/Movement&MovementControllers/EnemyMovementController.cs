using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementController : MonoBehaviour
{
    [Header("Stats")]
    public Vector3 givenMoveDirection;
    public Vector3 givenMoveDestination;

    [Header("Booleans")]
    public bool isControlled = true;
    public bool canSprint = true;

    [Header("Patrol Options and Detection")]
    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool doesPatrol = true;
    public bool tiedPatrol = true;
    public Vector3 spawnPoint;
    public float patrolRangeFromSpawn = 10f;

    [Header("Component References")]
    public Animator animator;

    public MovementScript movementScript;
    [SerializeField] NavMeshAgent navMeshAgent;


    void Awake()
    {
        movementScript = GetComponent<MovementScript>();
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPoint = transform.position;
    }

    #region Enemy Movement

    public void MoveEnemyUntilReached(Vector3 desiredPositionFromOrigin, bool isSprinting)
    {   
        if(Vector3.Distance(transform.position, desiredPositionFromOrigin) > 0.5f)
        {
            Vector3 moveDir = (desiredPositionFromOrigin - transform.position).normalized;
            MoveEnemyInDirection(moveDir, isSprinting);
        }
    }

    public void MoveEnemyInDirection(Vector3 targetDirection, bool isSprinting)
    {
        Vector3 moveDir = targetDirection.normalized;
        movementScript.Move(moveDir, isSprinting);
        moveDir.y = 0;
        transform.LookAt(moveDir + transform.position);
    }

    public void EnemyCirclingMovement(Vector3 axisPoint, Vector3 direction)
    {
        //Set Animator values
        animator.SetBool("Strafe", direction.normalized == Vector3.right || direction.normalized == Vector3.left);
        animator.SetFloat("StrafeDirection", direction.normalized.x);

        Vector3 dir = (axisPoint - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction

        Vector3 finalDirection = pDir * direction.normalized.x;

        movementScript.Move(finalDirection, false);

        finalDirection.y = 0;
        transform.LookAt(axisPoint);
    }

    public void StopMoving()
    {
        movementScript.ultimateCanMove = false;
        givenMoveDirection = Vector3.zero;
    }
    
    #endregion
}
