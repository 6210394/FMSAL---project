using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof (EnemyCombatController))]
[RequireComponent(typeof (EnemyMovementController))]

public class BrawlerEnemy : MonoBehaviour
{
    public EnemyStats _enemyStats;
    public bool _isReadyToAttack = false; //Will be set to True by the Enemy Manager
    
    public bool _hasTakenHit = false;
    public Vector3 _damageSource;
    public float _stunDuration = 0;
    public float _stunChainRecoveryRate;

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

        _combatController.OnDamage.AddListener((stunTime, damageSource) => OnTakeHit(stunTime, damageSource));

        //Create the states that will compose my AI
        var takeDamage = new TakeDamage(this, _movementController, _combatController, _animator);
        var patrol = new Patrol(this, _movementController, _animator);
        var circlingPlayer = new CirclingPlayer(this, _movementController, _combatController, _animator);
        var approachAndPunch = new ApproachAndPunch(this, _movementController, _combatController, _animator);
        var searchDamageSourceArea = new SearchDamageSourceArea(this, _movementController, _damageSource);

        //Create the transitions with their condition
        At(patrol, circlingPlayer, PlayerInDetectionRange());
        At(circlingPlayer, approachAndPunch, ReadyToPunch());
        At(takeDamage, approachAndPunch, Retaliate());
        //At(takeDamage, searchDamageSourceArea, OutOfHit());
        At(takeDamage, circlingPlayer, PlayerInDetectionRange());

        _stateMachine.AddAnyTransition(takeDamage, TookHit());

        //Begin at Patrol
        _stateMachine.SetState(patrol);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> PlayerInDetectionRange() => () => CheckForPlayersInDetectionRange();
        Func<bool> ReadyToPunch() => () => _isReadyToAttack == true;
        Func<bool> TookHit() => () => _hasTakenHit;
        Func<bool> OutOfHit() => () => !_hasTakenHit && CheckForPlayersInDetectionRange();
        Func<bool> Retaliate() => () => takeDamage.Retaliate();
    }

    // Update is called once per frame
    void Update()
    {
        if(_stateMachine != null)
        {
            _stateMachine.Tick();
        }
    }

    public void ResetAnimator()
    {
        _animator.SetBool("Strafe", false);
        _animator.SetFloat("Speed", 0);
    }

    void OnTakeHit(float stunDuration, Transform damageSource)
    {
        _hasTakenHit = true;
        _damageSource = damageSource.position;
        _stunDuration = stunDuration;
    }

    public void Death()
    {
        int dieAnimAnex = UnityEngine.Random.Range(1,4);
        _animator.SetFloat("deathIndex", dieAnimAnex);
        _animator.SetTrigger("Die");
        _stateMachine = null;
    }

    private bool CheckForPlayersInDetectionRange()
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
                    _target = player.GetComponent<PlayerCombatController>().transform;
                    return true;
                }
            }
            else
            {
                _target = null;
                return false;
            }
        }
        return false;
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
