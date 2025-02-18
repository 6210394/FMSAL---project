using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PantryInventory : MonoBehaviour
{
#region Singleton
    public static PantryInventory instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
#endregion

    public Dictionary<Ingredient, int> pantry;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddIngredient(Ingredient ingredient, int portions)
    {
        if(pantry.ContainsKey(ingredient))
        {
            pantry[ingredient] = pantry[ingredient] + portions;
        }
        else
        {
            pantry.Add(ingredient, portions);
        }
    }

    public bool RemoveIngredient(Ingredient ingredient, int portions)
    {
        if(pantry.ContainsKey(ingredient))
        {
            pantry[ingredient] = pantry[ingredient] - portions;
            if(pantry[ingredient] <= 0)
            {
                pantry.Remove(ingredient);
            }
            return true;
        }
        else
        {
            Debug.Log("You're missing something!!");
            return false;
        }
    }
}
