using System;
using UnityEngine;

public class BrawlerEnemy : EnemyBlueprint
{
    protected override void Start()
    {
        base.Start();

        //Create the states that will compose my AI
        var takeDamage = new TakeDamage(this, _movementController, _combatController, _animator);
        var patrol = new Patrol(this, _movementController, _animator);
        var circlingPlayer = new CirclingPlayer(this, _movementController, _combatController, _animator);
        var approachAndAttack = new ApproachAndAttack(this, _movementController, _combatController, _animator);
        var searchDamageSourceArea = new SearchDamageSourceArea(this, _movementController);
        var retreat = new Retreat(this, _movementController, _animator);

        //Create the transitions with their condition
        At(patrol, circlingPlayer, PlayerInDetectionRange());
        At(circlingPlayer, approachAndAttack, PreparingAttack());
        At(takeDamage, approachAndAttack, Retaliate());
        At(takeDamage, searchDamageSourceArea, OutOfHit());
        At(searchDamageSourceArea, circlingPlayer, PlayerInDetectionRange());
        At(searchDamageSourceArea, patrol, FinishedSearching());
        At(circlingPlayer, approachAndAttack, PreparingAttack());
        At(retreat, circlingPlayer, FinishedRetreating());

        _stateMachine.AddAnyTransition(takeDamage, TookHit());
        _stateMachine.AddAnyTransition(retreat,WantsToRetreat());
        
        _combatController.OnDamage.AddListener((stunTime, damageSource) => OnTakeHit(stunTime, damageSource));
        _combatController.OnHit.AddListener(approachAndAttack.CompleteAttack);
        _combatController.OnHit.AddListener(() => RetreatAfterHit());

        //Begin at Patrol
        _stateMachine.SetState(patrol);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> PlayerInDetectionRange() => () => CheckForPlayersInDetectionRange();
        Func<bool> PreparingAttack() => () => _combatController.isPreparingAttack == true;
        Func<bool> TookHit() => () => _hasTakenHit;
        Func<bool> OutOfHit() => () => !_hasTakenHit;
        Func<bool> Retaliate() => () => takeDamage.Retaliate();
        Func<bool> FinishedSearching() => () => searchDamageSourceArea.HasTimerReachedMax();
        Func<bool> WantsToRetreat() => () => wantsToRetreat;
        Func<bool> FinishedRetreating() => () => retreat.HasRetreated();
    }

    // Update is called once per frame
    void Update()
    {
        if(_stateMachine != null)
        {            
            _stateMachine.Tick();
        }
    }

    private void RetreatAfterHit()
    {
        wantsToRetreat = true;
    }

    private void OnDrawGizmos()
    {
        if(_stateMachine != null)
        {
            Gizmos.color = _stateMachine.GetGizmoColor();
            Gizmos.DrawSphere(transform.position + Vector3.up * 3, 0.5f);
        }
    }
}
