using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BuyFoodScript : MonoBehaviour
{
    public int price;
    public int foodFullness = 2;
    public CurrentMoneyDisplayScript moneyDisplay;

    public bool isPantryOpen = false;

    public void BuyFood(int price, int foodFullness)
    {
        if (GameManager.instance.money >= price && GameManager.instance.hungerState != GameManager.HUNGER.FULL)
        {
            GameManager.instance.money -= price;
            GameManager.instance.hungerPoints += foodFullness;
            GameManager.instance.HungerManagement();

            DisplayMessageScript.instance.ChangeDisplayMessage("Delicious", 0.5f, 1);
            moneyDisplay.UpdateMoneyText(GameManager.instance.money);
        }
        else if (GameManager.instance.hungerState == GameManager.HUNGER.FULL)
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("I'm stuffed.", 0.5f, 0.5f);
        }
        else
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("Not enough money", 0.5f, 0.5f);
        }
        UpdateUIElementScript.instance.UpdateUIElementsDebug();
    }

    //EMERGENCY
    void Update()
    {
        if (isPantryOpen)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                BuyFood(50, 2);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                BuyFood(100, 5);
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                BuyFood(150, 8);
            }
            return;
        }
    }

}
