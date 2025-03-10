using System.Collections;
using UnityEngine;

public class Patrol : IState
{
    BrawlerEnemy _brawlerEnemy;

    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;

    private bool _reachedPosition = false;

    public Vector3 Destination;

    bool _isPaused;
    float randomTime;

    public Patrol(BrawlerEnemy brawlerEnemy, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _brawlerEnemy = brawlerEnemy;
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
    }

    public void Tick()
    {
        if(_movementController.doesPatrol)
        {
            WaitRandomTime();
            if(Vector3.Distance(_movementController.transform.position, Destination) > 0.5f && !_reachedPosition)
            {
                _movementController.MoveEnemyUntilReached(Destination, false);
            }
            else
            {
                _reachedPosition = true;
                _animator.SetFloat("Speed", 0);
            }
        }
    }

    public void OnEnter()
    {
        ChangeDirection();
    }

    public void OnExit()
    {
        _brawlerEnemy.ResetAnimator();
    }

    public bool CheckForPlayersInDetectionRange()
    {
        foreach (GameObject player in _combatController.players)
        {
            Vector3 directionToPlayer = player.transform.position - _combatController.transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= _combatController.detectionRange)
            {
                float angleToPlayer = Vector3.Angle(_combatController.transform.forward, directionToPlayer);

                if (angleToPlayer <= _combatController.fieldOfViewAngle / 2)
                {
                    Debug.Log("Player detected");
                    _brawlerEnemy._target = player.GetComponent<PlayerCombatController>().transform;
                    return true;
                }
            }
            else
            {
                _brawlerEnemy._target = null;
                return false;
            }
        }
        return false;
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
        Destination = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
    }

    public Color GizmoColor()
    {
        return Color.white;
    }
}
