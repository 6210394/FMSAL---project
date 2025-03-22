using UnityEngine;

public class Retreat : IState
{
    EnemyBlueprint _brawlerEnemy;

    private EnemyMovementController _movementController;
    private Animator _animator;

    bool hasRetreated = false;

    public Retreat(EnemyBlueprint brawlerEnemy, EnemyMovementController movementController, Animator animator)
    {
        _movementController = movementController;
        _animator = animator;
        _brawlerEnemy = brawlerEnemy;
    }

    public void OnEnter()
    {
        hasRetreated = false;
        _brawlerEnemy.wantsToRetreat = false;

        if(Random.Range(1, 4) == 1) //25% chance of the brawler enemy sticking to you
        {
            hasRetreated = true;
        }
        
    }

    public void OnExit()
    {
        _brawlerEnemy.ResetAnimator();
        hasRetreated = false;
    }

    public void Tick()
    {
        if(IsTooClose())
        {
            Vector3 retreatDirection = -_movementController.transform.forward; // Move backwards locally
            _movementController.MoveEnemyInDirection(retreatDirection, false);
            _movementController.transform.LookAt(_brawlerEnemy._target);
            return;
        }
    }

    bool IsTooClose()
    {
        if(Vector3.Distance(_brawlerEnemy._target.position, _movementController.transform.position) < 3)
        {
            return true;
        }
        else
        {
            Debug.LogWarning("HAS RETREATED!");
            hasRetreated = true;
            return false;
        }
    }

    public bool HasRetreated()
    {
        return hasRetreated;
    }

    public Color GizmoColor()
    {
        return Color.yellow;
    }
}
