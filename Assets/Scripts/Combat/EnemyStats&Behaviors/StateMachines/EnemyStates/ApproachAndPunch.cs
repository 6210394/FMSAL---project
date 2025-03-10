using UnityEngine;

public class ApproachAndPunch : IState
{
    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;
    private BrawlerEnemy _brawlerEnemy;

    public ApproachAndPunch(BrawlerEnemy brawlerEnemy, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
        _brawlerEnemy = brawlerEnemy;
    }

    public void Tick()
    {
        // Implement Tick logic here
    }

    public void OnEnter()
    {
        // Implement OnEnter logic here
    }

    public void OnExit()
    {
        // Implement OnExit logic here
    }

    public Color GizmoColor()
    {
        return Color.red;
    }
}
