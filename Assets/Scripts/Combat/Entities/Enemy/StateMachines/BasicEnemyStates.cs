using UnityEngine;


public class BasicEnemyStates : EnemyStateMachine
{
    public float attackRange = 2;
    public float detectionRange = 20;

    public AutomaticMovementScript automaticMovementScript;

    void Start()
    {
        
    }

    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        //base.Init();
        //automaticMovementScript = GetComponent<AutomaticMovementScript>();
        //attackRange = enemyEntityScript.enemyStats.attackRange;
        //detectionRange = enemyEntityScript.enemyStats.detectionRange;
    }

    public override void Patrol()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                CheckForPlayersInDetectionRange();
                break;
            }
                
        }
    }

    public override void Chasing()
    {
        
    }

    public override void Attacking()
    {
        
    }

    void CheckForPlayersInDetectionRange()
    {
        foreach (PlayerMovement player in GameManager.instance.players)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= detectionRange)
            {
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (angleToPlayer <= 22.5f) // 45 degrees total, 22.5 degrees on each side
                {
                    Debug.Log("Player detected");
                    target = player.gameObject;
                    return;
                }
            }
            else
            {
                target = null;
            }
        }
    }

}
