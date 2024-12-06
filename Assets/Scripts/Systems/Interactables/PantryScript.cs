using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PantryScript : Interactable
{
    public static PantryScript instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public bool isPantryOpen = false;
    public GameObject pantryUI;
    public CurrentMoneyDisplayScript moneyDisplay;
    public BuyFoodScript buyFoodScript;

    public override void Update()
    {   
        if (isPantryOpen)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ClosePantry();
            }
            return;
        }
        base.Update();
    }

    override public void Interact()
    {
        base.Interact();
        if (GameManager.instance.interactEnabled)
        {
            OpenPantry();
        }
    }

    public void OpenPantry()
    {   
        buyFoodScript.isPantryOpen = true;
        moneyDisplay.UpdateMoneyText(GameManager.instance.money);
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().lockMode = true;
        pantryUI.SetActive(true);
        isPantryOpen = true;
        player.GetComponent<PlayerMovement>().isControlled = false;
    }

    public void ClosePantry()
    {
        buyFoodScript.isPantryOpen = false;
        GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().lockMode = false;
        pantryUI.SetActive(false);
        isPantryOpen = false;
        player.GetComponent<PlayerMovement>().isControlled = true;
    }
}
