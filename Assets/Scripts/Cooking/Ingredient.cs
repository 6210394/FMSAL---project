using UnityEngine;

public class Ingredient : Pickupable
{
    public enum IngredientType
    {
        Rice, Sugar, Salt
    }

    public int portions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnPickup()
    {
        //PantryInventory.instance.AddIngredient(this, portions);
        //base.OnPickup();
    }
}
