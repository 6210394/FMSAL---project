using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyBlueprint> enemiesInScene;
    public List<EnemyBlueprint> availableEnemies = new List<EnemyBlueprint>();
    private List<int> enemyIndexes;

    [Header("Main AI Loop - Settings")]
    private Coroutine AI_Loop_Coroutine;

    public int aliveEnemyCount;
    
    void Start()
    {
        availableEnemies.Clear(); // Clear the list before populating

        StartAI();
    }

    public void InitializeEnemies()
    {
        
    }

    public void StartAI()
    {
        foreach(EnemyBlueprint enemy in GetComponentsInChildren<EnemyBlueprint>())
        {
            enemiesInScene.Add(enemy);
        }
        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }

    IEnumerator AI_Loop(EnemyBlueprint enemy)
    {
        Debug.Log("Meta AI start!");

        if (AliveEnemyCount() == 0)
        {
            StopCoroutine(AI_Loop(null));
            Debug.Log("No enemies available");
            yield break;
        }
        
        yield return new WaitForSeconds(Random.Range(.5f,4f));

        EnemyBlueprint attackingEnemy;

        attackingEnemy = RandomEnemyExcludingOne(enemy);

        if (attackingEnemy == null)
            attackingEnemy = RandomEnemy();

        if (attackingEnemy == null)
        {
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
            yield break;
        }

        Debug.Log(attackingEnemy + " will attack!");

        if(attackingEnemy._combatController.combatScript.isStunned)
        {
           yield break; 
        }

        if (!attackingEnemy.isActiveAndEnabled)
        {
            Debug.Log(attackingEnemy + " is no longer active. Resetting AI loop.");
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
            yield break;
        }
            
        yield return new WaitUntil(() => attackingEnemy._isReadyToAttack);
        Debug.Log(attackingEnemy + " Is Ready to Attack!");
        
        attackingEnemy._combatController.Attack();

        if (!attackingEnemy.isActiveAndEnabled)
        {
            Debug.Log(attackingEnemy + " is no longer active. Resetting AI loop.");
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
            yield break;
        }
        
        yield return new WaitUntil(() => attackingEnemy._combatController.IsPreparingAttack() == false);

        yield return new WaitForSeconds(Random.Range(2, 4));

        if (AliveEnemyCount() > 0)
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
    }
    
    public void AddEnemy(EnemyBlueprint enemyBlueprint)
    {
        if(enemyBlueprint._combatController.isAvailableForEnemyManager && !availableEnemies.Contains(enemyBlueprint))
        {
            availableEnemies.Add(enemyBlueprint);
        }
        else
        {
            return;
        }
        aliveEnemyCount = AliveEnemyCount();
    }

    public void RemoveEnemy(EnemyBlueprint enemyBlueprint)
    {
        if(availableEnemies.Contains(enemyBlueprint))
        {
            availableEnemies.Remove(enemyBlueprint);
        }
        else
        {
            return;
        }
        aliveEnemyCount = AliveEnemyCount();
    }

    public EnemyBlueprint RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < availableEnemies.Count; i++)
        {
            if (availableEnemies[i]._combatController.isAvailableForEnemyManager)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyBlueprint randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = availableEnemies[enemyIndexes[randomIndex]];

        return randomEnemy;
    }

    public EnemyBlueprint RandomEnemyExcludingOne(EnemyBlueprint exclude)
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < availableEnemies.Count; i++)
        {
            if (availableEnemies[i]._combatController.isAvailableForEnemyManager && availableEnemies[i] != exclude)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemyBlueprint randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = availableEnemies[enemyIndexes[randomIndex]];

        return randomEnemy;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < availableEnemies.Count; i++)
        {
            if (availableEnemies[i]._combatController.isAvailableForEnemyManager)
                count++;
        }
        return count;
    }


    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < enemiesInScene.Count; i++)
        {
            if (enemiesInScene[i].isActiveAndEnabled)
                count++;
        }
        aliveEnemyCount = count;
        return count;
    }

    public void SetEnemyAvailiability (EnemyCombatController enemy, bool state)
    {
        StopCoroutine(AI_Loop_Coroutine);

        for (int i = 0; i < availableEnemies.Count; i++)
        {
            if (availableEnemies[i] == enemy)
            {
                availableEnemies[i]._combatController.isAvailableForEnemyManager = state;
            }
        }

        if (FindFirstObjectByType<EnemyDetection>().CurrentTarget() == enemy)
            FindFirstObjectByType<EnemyDetection>().SetCurrentTarget(null);

        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
    }
}