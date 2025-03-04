using UnityEngine;
using UnityEngine.AI;


public class DefaultEnemyStates : EnemyStateMachineBlueprint
{
    
    void Start()
    {
        
    }

    protected override void RunStateMachine()
    {
        switch (currentState)
        {
            case STATE.PATROL:
                Patrol();
                break;

            case STATE.MOVING:
                MoveToPlayer();
                break;

            case STATE.ATTACKING:
                Attacking();
                break;
            
            case STATE.CIRCLING:
                Circling();
                break;

            case STATE.RETREATING:
                Retreat();
                break;
        }
    }

    public void Patrol()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                Debug.Log("Patroling");
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(!enemyCombatController.CheckForPlayersInDetectionRange())
                {   
                    if(!enemyCombatController.tiedPatrol)
                    {
                        enemyCombatController.WalkRandomly(1, 5, 1);
                    }
                }
                else
                {
                    SwitchToNextEvent(EVENT.EXIT);
                }
                break;
            }
            case EVENT.EXIT:
            {
                if(enemyCombatController.target)
                {
                    SwitchToNextState(STATE.MOVING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                else
                {
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
        }
    }

    public void MoveToPlayer()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                Debug.Log("MovingToPlayer");
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }

            case EVENT.UPDATE:
            {
                if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) < detectionRange)
                {
                    if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) > enemyCombatController.comfortRange - 1)
                    {
                        enemyCombatController.ApproachPlayer(true);
                        transform.LookAt(enemyCombatController.target.transform);
                    }
                    else
                    {
                        SwitchToNextEvent(EVENT.EXIT);
                    }
                }
                else
                {
                    SwitchToNextState(STATE.PATROL);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
            
            case EVENT.EXIT:
            {
                SwitchToNextState(STATE.CIRCLING);
                SwitchToNextEvent(EVENT.ENTER);
                break;
            }
        }
    }

    public void Attacking()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                Debug.Log("Attacking");
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(enemyCombatController.isRetreating)
                {
                    SwitchToNextState(STATE.RETREATING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
            case EVENT.EXIT:
            {
                
                
                break;
            }
        }
    }

    public void Retreat()
    {
        switch(currentEvent)
        {
            case EVENT.ENTER:
            {   
                Debug.Log("Retreating");
                enemyCombatController.SetRetreat();
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(!enemyCombatController.isRetreating)
                {
                    SwitchToNextEvent(EVENT.EXIT);
                }
                else
                {
                    //enemyCombatController.movementScript.Move();
                }
                break;
            }
            case EVENT.EXIT:
            {
                if(enemyCombatController.target)
                {
                    SwitchToNextState(STATE.CIRCLING);
                }
                else
                {
                    SwitchToNextState(STATE.PATROL);
                }
                SwitchToNextEvent(EVENT.ENTER);
                break;
            }
        }
    }

    public void Circling()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                Debug.Log("Circling");
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) > comfortDistance)
                {
                    Debug.Log("Player outside of comfort range!");
                    SwitchToNextState(STATE.MOVING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                if(enemyCombatController.target)
                {
                    enemyCombatController.EnemyCirclingMovement();
                }
                if(enemyCombatController.isPreparingAttack)
                {
                    SwitchToNextState(STATE.ATTACKING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
            case EVENT.EXIT:
            {
                break;
            }
        }
    }
}
