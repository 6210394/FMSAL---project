using UnityEngine;
using UnityEngine.AI;

public class NavmeshAgentMoveOnMesh : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent navAgent;

    [SerializeField]
    private float speed;

    public GameObject targetObject;

    public bool push = false;
    public bool isGettingPushed = false;
    NavMeshHit pushHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGettingPushed)
        {
            MoveTowards(targetObject);
        }

        if (push)
        {
            isGettingPushed = true;
            Vector3 pushDirection = (targetObject.transform.position - transform.position).normalized;
            float length = 5.0f;

            if (navAgent.Raycast(transform.position - pushDirection * length, out pushHit))
            {
                navAgent.SetDestination(pushHit.position);
                Debug.Log("WORKS");
            }

            push = false;
        }

        if (isGettingPushed)
        {
            PushAgent(pushHit);
        }
    }

    void MoveTowards(GameObject destination)
    {
        navAgent.SetDestination(destination.transform.position);
    }

    void PushAgent(NavMeshHit destination)
    {
        Vector3 direction = destination.position - transform.position;
        navAgent.Move(direction.normalized * 2 * Time.deltaTime);

        if(direction.magnitude <= 1.5f)
        {
            isGettingPushed = false;
        }
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.DrawSphere(pushHit.position, 1);
            Gizmos.DrawRay(transform.position, pushHit.normal);
        }
    }
}
