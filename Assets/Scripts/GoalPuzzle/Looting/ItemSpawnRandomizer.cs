using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSpawnRandomizer : MonoBehaviour
{
    public List<Transform> itemSpawnPoints;
    public GameObject[] itemPrefabs;

    void Start()
    {
        InitializeLevel();
    }

    void InitializeLevel()
    {
         foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("ItemSpawnPoint"))
        {
            itemSpawnPoints.Add(spawnPoint.transform);
        }
        SetUpItems();
    }
   
    void SetUpItems()
    {
        Shuffle(itemPrefabs);
        int randomSpawnPointSelection = Random.Range(itemSpawnPoints.Count - 5, itemSpawnPoints.Count);

        int index = 0;

        foreach (Transform spawnPoint in itemSpawnPoints)
        {
            if( index <= randomSpawnPointSelection)
            {
                int randomItem = Random.Range(0, itemPrefabs.Length);
                Instantiate(itemPrefabs[randomItem], spawnPoint.position, spawnPoint.rotation);
            }
            index++;
        }
    }

    void Shuffle(GameObject[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
