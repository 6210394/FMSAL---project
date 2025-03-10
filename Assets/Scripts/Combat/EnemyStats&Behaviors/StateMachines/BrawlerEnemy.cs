using System;
using System.Collections;
using UnityEngine;

public class BrawlerEnemy : MonoBehaviour
{
    public EnemyStats _enemyStats;
    public bool isReadyToAttack = false; //Will be set to True by the Enemy Manager

    public EnemyCombatController _combatController;
    public EnemyMovementController _movementController;
    public Animator _animator;

    private StateMachine _stateMachine;

    public Transform _target;

    void Awake()
    {
        _combatController = GetComponent<EnemyCombatController>();
        _movementController = GetComponent<EnemyMovementController>();
        _animator = GetComponent<Animator>();
        _stateMachine = new StateMachine();

        //Create the states that will compose my AI
        var patrol = new Patrol(this, _movementController, _combatController, _animator);
        var circlingPlayer = new CirclingPlayer(this, _movementController, _combatController, _animator);
        var approachAndPunch = new ApproachAndPunch(this, _movementController, _combatController, _animator);

        //Create the transitions with their condition
        At(patrol, circlingPlayer, PlayerInDetectionRange());
        At(circlingPlayer, approachAndPunch, ReadyToPunch());

        //Begin at Patrol
        _stateMachine.SetState(patrol);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> PlayerInDetectionRange() => () => patrol.CheckForPlayersInDetectionRange();
        Func<bool> ReadyToPunch() => () => isReadyToAttack == true;
    }

    // Update is called once per frame
    void Update()
    {
        _stateMachine.Tick();
    }

    public void ResetAnimator()
    {
        _animator.SetBool("Strafe", false);
        _animator.SetFloat("Speed", 0);
    }

    private void OnDrawGizmos()
    {
        if(_stateMachine != null)
        {
            Gizmos.color = _stateMachine.GetGizmoColor();
            Gizmos.DrawSphere(transform.position + Vector3.up * 3, 0.5f);
        }
        else
        {
            Debug.Log("No state machine!");
        }
    }
}
