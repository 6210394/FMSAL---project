using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EnemyStateMachineBlueprint[] enemies;
    public EnemyStruct[] allEnemies;
    private List<int> enemyIndexes;

    [Header("Main AI Loop - Settings")]
    private Coroutine AI_Loop_Coroutine;

    public int aliveEnemyCount;
    
    void Start()
    {
        enemies = GetComponentsInChildren<EnemyStateMachineBlueprint>();

        allEnemies = new EnemyStruct[enemies.Length];

        for (int i = 0; i < allEnemies.Length; i++)
        {
            allEnemies[i].enemyStateMachine = enemies[i];
            allEnemies[i].enemyAvailability = true;
        }

        StartAI();
    }

    public void InitializeEnemies()
    {
        
    }

    public void Update()
    {
        
    }

    EnemyStateMachineBlueprint CheckRetaliation()
    {
        foreach(EnemyStruct enemy in allEnemies)
        {
            EnemyCombatController enemyCombatController = enemy.enemyStateMachine.enemyCombatController;
            if(enemyCombatController.seeksRetaliation)
            {
                return enemy.enemyStateMachine;
            }
        }
        return null;
    }

    public void StartAI()
    {
        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    IEnumerator AI_Loop(EnemyStateMachineBlueprint enemy)
    {
        if (AliveEnemyCount() == 0)
        {
            StopCoroutine(AI_Loop(null));
            yield break;
        }

        
        yield return new WaitForSeconds(Random.Range(.5f,4f));

        EnemyStateMachineBlueprint attackingEnemy;

        if(!CheckRetaliation())
        {
            attackingEnemy = RandomEnemyExcludingOne(enemy);

            if (attackingEnemy == null)
                attackingEnemy = RandomEnemy();

            if (attackingEnemy == null)
                yield break;
                
            yield return new WaitUntil(() => attackingEnemy.currentState == EnemyStateMachineBlueprint.STATE.CIRCLING);
            yield return new WaitUntil(() => attackingEnemy.enemyCombatController.combatScript.isStunned == false);
        }
        else
        {
            attackingEnemy = CheckRetaliation();
        }

        attackingEnemy.enemyCombatController.Attack();

        yield return new WaitUntil(() => attackingEnemy.enemyCombatController.IsPreparingAttack() == false);

        attackingEnemy.enemyCombatController.SetRetreat();

        yield return new WaitForSeconds(Random.Range(0,.5f));

        if (AliveEnemyCount() > 0)
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
    }
    


    public EnemyStateMachineBlueprint RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyStateMachineBlueprint randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyStateMachine;

        return randomEnemy;
    }

    public EnemyStateMachineBlueprint RandomEnemyExcludingOne(EnemyStateMachineBlueprint exclude)
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyStateMachine != exclude)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyStateMachineBlueprint randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyStateMachine;

        return randomEnemy;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                count++;
        }
        return count;
    }

    public bool AnEnemyIsAttacking()
    {
        foreach (EnemyStruct enemyStruct in allEnemies)
        {
            
        }
        return false;
    }


    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyStateMachine.isActiveAndEnabled)
                count++;
        }
        aliveEnemyCount = count;
        return count;
    }

    public void SetEnemyAvailiability (EnemyCombatController enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyStateMachine == enemy)
                allEnemies[i].enemyAvailability = state;
        }

        if (FindFirstObjectByType<EnemyDetection>().CurrentTarget() == enemy)
            FindFirstObjectByType<EnemyDetection>().SetCurrentTarget(null);
    }
}

[System.Serializable]
public struct EnemyStruct
{
    public EnemyStateMachineBlueprint enemyStateMachine;
    public bool enemyAvailability;
}