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
        else
        {
            Destroy(gameObject);
        }
    }

    public bool isPantryOpen = false;
    public GameObject pantryUI;

    void Update()
    {
        if (isPantryOpen)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ClosePantry();
            }
        }
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
        pantryUI.SetActive(true);
        isPantryOpen = true;
        player.GetComponent<PlayerMovement>().isControlled = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ClosePantry()
    {
        pantryUI.SetActive(false);
        isPantryOpen = false;
        player.GetComponent<PlayerMovement>().isControlled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
