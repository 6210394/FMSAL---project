using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 1;
    public Vector3 givenMoveDirection;
    public Vector3 givenMoveDestination;

    [Header("Booleans")]
    public bool isControlled = true;
    public bool canSprint = true;
    bool isSprintingAnim;

    [Header("Patrol Options and Detection")]
    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool doesPatrol = true;
    public bool tiedPatrol = true;
    public Vector3 spawnPoint;
    public float patrolRangeFromSpawn = 10f;

    [Header("Component References")]
    public Animator animator;

    public MovementScript movementScript;
    private Rigidbody rb;
    [SerializeField] NavMeshAgent navMeshAgent;

    public Coroutine RetreatCoroutine;
    public Coroutine PatrolDirectionCoroutine;
    public Coroutine CircleDirectionCoroutine;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movementScript = GetComponent<MovementScript>();
        animator = GetComponent<Animator>();
        spawnPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Enemy Movement

    public void MoveEnemyUntilReached(Vector3 desiredPositionFromOrigin, bool isSprinting)
    {   
        if(Vector3.Distance(transform.position, desiredPositionFromOrigin) > 0.5f)
        {
            Debug.Log("moving to destination");
            Vector3 moveDir = (desiredPositionFromOrigin - transform.position).normalized;
            MoveEnemyInDirection(moveDir, isSprinting);
        }
    }

    public Vector3 GenerateRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        Vector3 randomDirection = new Vector3(randomX, 0, randomZ).normalized;
        return randomDirection;
    }

    public void MoveEnemyInDirection(Vector3 targetDirection, bool isSprinting)
    {
        Vector3 moveDir = targetDirection.normalized;
        movementScript.Move(moveDir, isSprinting);
        moveDir.y = 0;
        transform.LookAt(moveDir + transform.position);
    }

    public void RetreatAwayUntilDistance(float targetDistance, Vector3 axisOfRetreat)
    {
        if(Vector3.Distance(axisOfRetreat, transform.position) <= targetDistance)
        {
            transform.LookAt(axisOfRetreat);
            movementScript.Move(-transform.forward, false);
        }
        else if(Vector3.Distance(axisOfRetreat, transform.position) >= targetDistance)
        {
            return;
        }
    }

    public void EnemyCirclingMovement(Vector3 axisPoint, Vector3 direction)
    {
        //Set Animator values
        Debug.Log("Circling...");
        animator.SetBool("Strafe", direction == Vector3.right || direction == Vector3.left);
        animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);

        Vector3 dir = (axisPoint - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction

        Vector3 finalDirection = pDir * direction.normalized.x;
        Debug.Log(finalDirection);

        movementScript.Move(finalDirection, false);

        finalDirection.y = 0;
        transform.LookAt(axisPoint);
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
        CircleDirectionCoroutine = StartCoroutine(IGenerateCirclingDirection());
    }
    
    public void StopMoving()
    {
        movementScript.isAllowedToMove = false;
        givenMoveDirection = Vector3.zero;
    }
    
    #endregion
}
