using UnityEngine;

public class SearchAroundGivenArea : IState
{
    private EnemyBlueprint _enemyStates;
    private EnemyMovementController _movementController;

    private Vector3 _searchPosition;
    private bool _hasArrived = false;

    float _maxSearchTime = 4f;
    float _currentSearchTime = 0f;
    bool _timerReachedMax = false;

    public SearchAroundGivenArea(EnemyBlueprint enemyStates, EnemyMovementController movementController)
    {
        _enemyStates = enemyStates;
        _movementController = movementController;
    }

    public void OnEnter()
    {
        _searchPosition = _enemyStates._damageSource + new Vector3(Random.Range(-2,3), 0, Random.Range(-2,3));

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
            _enemyStates.ResetAnimator();
            SearchTimer();
            return;
        }
        else
        {
            Vector3 lookAtTarget = _enemyStates._damageSource;
            lookAtTarget.y = _enemyStates.transform.position.y;
            _movementController.transform.LookAt(lookAtTarget);
            
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
        Vector3 moveVector = (_enemyStates._damageSource - _movementController.transform.position).normalized;
        _movementController.MoveEnemyInDirection(moveVector, true);

        if(Vector3.Distance(_movementController.transform.position, _searchPosition) < 1f)
        {
            _hasArrived = true;
        }
    }

    public Color GizmoColor()
    {
        return Color.magenta;
    }
}
