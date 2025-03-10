using UnityEngine;

public class SearchDamageSourceArea : IState
{
    private BrawlerEnemy _brawlerEnemy;
    private EnemyMovementController _movementController;

    private Vector3 _searchPosition;
    private bool _hasArrived = false;

    public SearchDamageSourceArea(BrawlerEnemy brawlerEnemy, EnemyMovementController movementController, Vector3 damageSource)
    {
        _brawlerEnemy = brawlerEnemy;
        _movementController = movementController;
        _searchPosition = damageSource + Random.insideUnitSphere * 5f;
    }

    public void OnEnter()
    {
        _searchPosition.y = _brawlerEnemy._damageSource.y; // Keep the same height
        _hasArrived = false;
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
        if (_hasArrived)
        {
            return;
        }
        else
        {
            MoveToSearchPosition();
        }
    }

    private void MoveToSearchPosition()
    {
        Vector3 direction = (_searchPosition - _movementController.transform.position).normalized;
        _movementController.MoveEnemyInDirection(direction, true);

        if (Vector3.Distance(_movementController.transform.position, _searchPosition) < 1f)
        {
            _hasArrived = true;
        }
    }

    public Color GizmoColor()
    {
        return Color.magenta;
    }
}
