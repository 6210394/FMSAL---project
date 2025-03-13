using UnityEngine;

public class SearchDamageSourceArea : IState
{
    private EnemyBlueprint _brawlerEnemy;
    private EnemyMovementController _movementController;

    private Vector3 _searchPosition;
    private bool _hasArrived = false;

    bool _searching;
    float _maxSearchTime = 4f;
    float _currentSearchTime = 0f;
    bool _timerReachedMax = false;

    public SearchDamageSourceArea(EnemyBlueprint brawlerEnemy, EnemyMovementController movementController)
    {
        _brawlerEnemy = brawlerEnemy;
        _movementController = movementController;
    }

    public void OnEnter()
    {
        _movementController.transform.LookAt(_brawlerEnemy._damageSource);
        _searchPosition = _brawlerEnemy._damageSource + Random.insideUnitSphere * 2;

        _hasArrived = false;
        _timerReachedMax = false;
    }

    public void OnExit()
    {
    }

    public void Tick()
    {
        if (_hasArrived)
        {
            _brawlerEnemy.ResetAnimator();
            SearchTimer();
            return;
        }
        else
        {
            MoveToSearchPosition();
        }
    }

    public void SearchTimer()
    {
        if (!_timerReachedMax)
        {
            _currentSearchTime += Time.deltaTime;
            if (_currentSearchTime > _maxSearchTime)
            {
                _timerReachedMax = true;
                _currentSearchTime = 0f;
            }
        }
    }

    public bool HasTimerReachedMax()
    {
        return _timerReachedMax;
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
