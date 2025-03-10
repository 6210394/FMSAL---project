using UnityEngine;

public class CirclingPlayer : IState
{
    BrawlerEnemy _brawlerEnemy;

    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;

    private Vector3 _perpendicularDirection;

    float _randomTime;
    float _randomTimeTimer;

    public CirclingPlayer(BrawlerEnemy brawlerEnemy, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
        _brawlerEnemy = brawlerEnemy;
    }

    public void Tick()
    {
        DirectionChangeDelay(); //Timer to give time between direction switches
        if(_brawlerEnemy._target != null)
        {
            if(IsTooClose())
            {
                _movementController.MoveEnemyInDirection(Vector3.back, false);
                _movementController.transform.LookAt(_brawlerEnemy._target);
                return;
            }
            if(IsInComfortRange())
            {
                _movementController.EnemyCirclingMovement(_brawlerEnemy._target.position, _perpendicularDirection);
            }
            
            else
            {
                Debug.Log("MOVIN OVER TO YA");
                _animator.SetBool("Strafe", false);
                Vector3 moveDir = (_brawlerEnemy._target.position - _movementController.transform.position).normalized;
                _movementController.MoveEnemyInDirection(moveDir, true);
            }
        }
        else
        {
            Debug.Log("NO TARGET WUT DA HELL");
        }
    }

    public void OnEnter()
    {
        _randomTime = Random.Range(3f, 5f);
        _randomTimeTimer = _randomTime;
        ChangeDirection();
    }

    public void OnExit()
    {
        _brawlerEnemy.ResetAnimator();
        _brawlerEnemy._isReadyToAttack = false;
    }

    private bool IsTooClose()
    {
        if(Vector3.Distance(_brawlerEnemy._target.position, _movementController.transform.position) < 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsInComfortRange()
    {
        if(Vector3.Distance(_brawlerEnemy._target.position, _movementController.transform.position) > _combatController.comfortRange)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DirectionChangeDelay()
    {
        if (_randomTimeTimer > 0)
        {
            _randomTimeTimer -= Time.deltaTime;
        }
        else
        {
            ChangeDirection();
            _randomTime = Random.Range(1f, 5f);
            _randomTimeTimer = _randomTime;
        }
    }

    private void ChangeDirection()
    {
        /* FOR WHEN WE HAVE A VISUAL DIFFERENCE BETWEEN CHILLING VS AWARE
        float randomChance = Random.Range(1, 11);

         
        if(randomChance >= 9)
        {
            _perpendicularDirection = Vector3.zero;
            return;
        }
        if(randomChance >= 4)
        {
            _perpendicularDirection = Vector3.right;
            return;
        }
        else
        {
            _perpendicularDirection = Vector3.left;
            return;
        }
        */

        switch(Random.Range(0,2))
        {
            case 0:
            {
                _perpendicularDirection = Vector3.right;
                break;
            }
            case 1:
            {
                _perpendicularDirection = Vector3.left;
                break;
            }
        }
    }

    public Color GizmoColor()
    {
        return Color.blue;
    }
}
