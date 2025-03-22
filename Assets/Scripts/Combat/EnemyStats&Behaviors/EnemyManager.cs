using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private EnemyBlueprint[] enemies;
    public EnemyStruct[] allEnemies;
    private List<int> enemyIndexes;

    [Header("Main AI Loop - Settings")]
    private Coroutine AI_Loop_Coroutine;

    public int aliveEnemyCount;
    
    void Start()
    {
        enemies = GetComponentsInChildren<EnemyBlueprint>();

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

    public void StartAI()
    {
        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    IEnumerator AI_Loop(EnemyBlueprint enemy)
    {
        if (AliveEnemyCount() == 0)
        {
            StopCoroutine(AI_Loop(null));
            yield break;
        }
        
        yield return new WaitForSeconds(Random.Range(.5f,4f));

        EnemyBlueprint attackingEnemy;

        attackingEnemy = RandomEnemyExcludingOne(enemy);

        if (attackingEnemy == null)
            attackingEnemy = RandomEnemy();

        if (attackingEnemy == null)
            yield break;
            
        yield return new WaitUntil(() => attackingEnemy._isReadyToAttack);
        Debug.Log(attackingEnemy + " Is Ready to Attack!");
        
        if(attackingEnemy._combatController.combatScript.isStunned)
        {
           //yield break; 
        }

        attackingEnemy._combatController.Attack();
        
        yield return new WaitUntil(() => attackingEnemy._combatController.IsPreparingAttack() == false);

        yield return new WaitForSeconds(Random.Range(2, 4));

        if (AliveEnemyCount() > 0)
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
    }
    


    public EnemyBlueprint RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyBlueprint randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyStateMachine;

        return randomEnemy;
    }

    public EnemyBlueprint RandomEnemyExcludingOne(EnemyBlueprint exclude)
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyStateMachine != exclude)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyBlueprint randomEnemy;
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
    public EnemyBlueprint enemyStateMachine;
    public bool enemyAvailability;
}