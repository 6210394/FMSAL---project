using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovementController : MonoBehaviour
{
    [Header("Booleans")]
    public bool canSprint = true;

    [Header("Patrol Options and Detection")]
    public bool doesPatrol = true;
    public Vector3 spawnPoint;

    [Header("Component References")]
    public Animator animator;

    public NavMeshMovementScript movementScript;
    public NavMeshAgent navMeshAgent {get; private set;}

    void Awake()
    {
        movementScript = GetComponent<NavMeshMovementScript>();
        animator = GetComponent<Animator>();
        navMeshAgent = movementScript.navMeshAgent;
    }

    void Start()
    {
        spawnPoint = transform.position;
    }

    #region Enemy Movement

    public void StopNavmeshMovement()
    {
        if(navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }

        animator.SetFloat("Speed", 0);
        animator.SetBool("Sprinting", false);
    }

    public bool MoveEnemyUntilReached(Vector3 desiredPositionFromOrigin, bool isSprinting) //Meant to be run in a loop until it returns true. Creates navMesh paths.
    {   
        if(movementScript.CurrentPath == null) //Create new path to destination
        {
            movementScript.NavMeshMove(desiredPositionFromOrigin, isSprinting);
        }
        if(movementScript.CurrentPath != navMeshAgent.path) //Destination changed: Recalculating path
        {
            movementScript.NavMeshMove(desiredPositionFromOrigin, isSprinting);
        }

        if(Vector3.Distance(transform.position, desiredPositionFromOrigin) < 0.5f)
        {
            return true; //Reached destination!
        }
        return false;
    }

    public void MoveEnemyInDirection(Vector3 targetDirection, Vector3 lookAtDirection, bool isSprinting) //Simply move in a direction and look forward along the navMesh. No pathing.
    {
        Vector3 moveDir = targetDirection.normalized;
        movementScript.Move(moveDir, isSprinting);
        transform.LookAt(lookAtDirection);
    }

    public void EnemyCirclingMovement(Vector3 axisPoint, Vector3 direction) //Simply move around an axisPoint and look at it along the navMesh. No pathing.
    {
        //Set Animator values
        animator.SetBool("Strafe", direction.normalized == Vector3.right || direction.normalized == Vector3.left);
        animator.SetFloat("StrafeDirection", direction.normalized.x);

        Vector3 dir = (axisPoint - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir;

        Vector3 finalDirection = pDir * direction.normalized.x;

        movementScript.Move(finalDirection, false);

        finalDirection.y = 0;
        transform.LookAt(axisPoint);
    }
    
    #endregion
}
