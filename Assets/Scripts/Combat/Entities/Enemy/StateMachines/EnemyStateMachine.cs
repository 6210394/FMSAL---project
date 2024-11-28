using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public enum STATE { PATROL, CHASING, ATTACKING}
    public enum EVENT { ENTER, UPDATE, EXIT }
    
    public STATE currentState;
    public EVENT currentEvent;
    protected STATE nextState;

    public EnemyStateMachine()
    {
        currentState = STATE.PATROL;
        currentEvent = EVENT.ENTER;
    }

    public EnemyEntityScript enemyEntityScript;
    public Animator animator;
    public GameObject target;

    public virtual void Update()
    {
        RunStateMachine();
    }

    public virtual void Init()
    {
        enemyEntityScript = GetComponent<EnemyEntityScript>();
        animator = GetComponentInChildren<Animator>();
    }

    public virtual void Patrol()
    {
        
    }

    public virtual void Chasing()
    {
    }

    public virtual void Attacking()
    {

    }


    protected void RunStateMachine()
    {
            switch (currentState)
            {
                case STATE.PATROL:
                    Patrol();
                    break;

                case STATE.CHASING:
                    Chasing();
                    break;

                case STATE.ATTACKING:
                    Attacking();
                    break;
            }
    }

    protected void SwitchToNextState(STATE nextState)
    {
        currentState = nextState;
    }
    protected void SwitchToNextEvent(EVENT nextEvent)
    {
        currentEvent = nextEvent;
    }

    public void MeleeAttack()
    {
        animator.SetTrigger("MeleeAttack");
    }

    protected bool CheckIfObjectInSight(GameObject gameObject)
        {
            
            Vector3 directionToTargetObject = gameObject.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, directionToTargetObject, out hit))
            {
                if (hit.collider.gameObject.tag != "Player")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

}
