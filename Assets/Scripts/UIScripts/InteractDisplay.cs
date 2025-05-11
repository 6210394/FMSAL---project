using UnityEngine;

public class InteractDisplay : Interactable
{
    int uses = 0;

    public override void Interact()
    {
        Debug.Log("Interacting with " + gameObject.name);
        if(uses >= 5)
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("I can't stand how I look.", 0.5f, 1);
            return;
        }
        DisplayMessage();
    }

    void DisplayMessage()
    {
        int randomIndex = Random.Range(0, 5);

        switch (randomIndex)
        {
            case 0:
                DisplayMessageScript.instance.ChangeDisplayMessage("Maybe try makeup", 0.5f, 1);
                break;
            case 1:
                DisplayMessageScript.instance.ChangeDisplayMessage("You look like you're lacking iron.", 0.5f, 1);
                break;
            case 2:
                DisplayMessageScript.instance.ChangeDisplayMessage("It's you!", 0.5f, 1);
                break;
            case 3:
                DisplayMessageScript.instance.ChangeDisplayMessage("There's little water stains on the mirror.", 0.5f, 1);
                break;
            case 4:
                DisplayMessageScript.instance.ChangeDisplayMessage("Why does it hurt to breathe?", 0.5f, 1);
                break;
        }
        uses++;
    }
}
