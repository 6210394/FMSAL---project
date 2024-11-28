using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AutomaticMovementScript : MonoBehaviour
{
    public bool canMove = true;

    public Vector3 target;
    public GameObject targetObject;
    public float speed;

    public float movementRemaining;
    public NavMeshAgent navMeshAgent;
    public bool hasReachedTarget = true;


    public bool destroyable = false;
    public float lifeTime = 1f;

    void Awake()
    {
        Init();
    }

    // Update is called once per frame

    public void MoveToTarget()
    {
        transform.position = Vector3.Lerp(transform.position, target, speed * Time.deltaTime);
        if (destroyable)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    public void MoveWithNavMesh(Vector3 target)
    {
        if (movementRemaining <= 0)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        navMeshAgent.SetDestination(target);

        Vector3 movement = navMeshAgent.velocity.normalized;
        movementRemaining -= movement.magnitude * navMeshAgent.acceleration * Time.deltaTime;   

        if (movementRemaining <= 0)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            hasReachedTarget = true;
        }
    }

    public void Init()
    {
        hasReachedTarget = true;
        if(GetComponent<NavMeshAgent>() != null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        navMeshAgent.speed = speed;
    }

    public void MoveRandomly()
    {
        if(hasReachedTarget)
        {
            //Multiplied by 0.5 for flavor
            Vector3 randomDirection = new Vector3(Random.Range(-1 * movementRemaining, movementRemaining * 0.5f), 0, Random.Range(-1 * movementRemaining, movementRemaining * 0.5f ));
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10, NavMesh.AllAreas);
            target = hit.position;
            hasReachedTarget = false;
            return;
        }
        MoveToTarget();
        //MoveWithNavMesh(target);
    }
}
