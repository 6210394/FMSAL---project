using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : IState
{
    EnemyBlueprint _enemyStates;

    private EnemyMovementController _movementController;
    private Animator _animator;

    private bool _reachedPosition = false;

    public Vector3 Destination;

    bool _isPaused;
    float randomTime;

    public Patrol(EnemyBlueprint enemyStates, EnemyMovementController movementController, Animator animator)
    {
        _enemyStates = enemyStates;
        _movementController = movementController;
        _animator = animator;
    }

    public void Tick()
    {
        if(_movementController.doesPatrol && _movementController.navMeshAgent.path != null)
        {
            WaitRandomTime();
            if(_movementController.MoveEnemyUntilReached(Destination, false))
            {
                _reachedPosition = true;
                _animator.SetFloat("Speed", 0);
            }
            else
            {
                return;
            }
        }
    }

    public void OnEnter()
    {
        ChangeDirection();
        if(_movementController.doesPatrol)
        {
            _movementController.MoveEnemyUntilReached(Destination, false);
        }
    }

    public void OnExit()
    {
        _enemyStates.ResetAnimator();
    }

    private void WaitRandomTime()
    {
        if(!_isPaused && _reachedPosition)
        {
            randomTime = Random.Range(3, 7);
            _isPaused = true;
        }
        if(_isPaused)
        {
            if(randomTime > 0)
            {
                randomTime -= Time.deltaTime;
            }
            else
            {
                randomTime = 0;
                _isPaused = false;
                _reachedPosition = false;
                ChangeDirection();
            }
        }
    }

    private void ChangeDirection()
    {
        Destination = _enemyStates.transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));

        NavMeshHit navMeshHit;
        if(_movementController.navMeshAgent.Raycast(Destination, out navMeshHit))
        {
            Destination = navMeshHit.position;
        }
    }

    public Color GizmoColor()
    {
        return Color.white;
    }
}
