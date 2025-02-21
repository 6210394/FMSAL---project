using UnityEngine;

public class Food : Pickupable
{
    public int portions;

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
