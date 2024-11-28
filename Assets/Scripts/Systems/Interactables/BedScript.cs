using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedScript : Interactable
{

    static BedScript instance;
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

    public bool confirmSleep = false;
    public FadeInOutScript fadeScript;


    public override void Interact()
    {
        base.Interact();
        if(GameManager.instance.interactEnabled)
        {
            
            if(GameManager.instance.hasCompletedDailyMission)
            {
                Sleep();
                return;
            }
            if(confirmSleep) { Sleep(); return; }

            DisplayMessageScript.instance.ChangeDisplayMessage("Can I just sleep without working?..", 1, 2);
            confirmSleep = true;
        }
        
    }

    public void Sleep()
    {
        float fadeSpeed = 2;
        float fadeLength = 1;

        GameManager.instance.GoToNextDay();

        foreach (PlayerMovement player in GameManager.instance.players)
        {
            player.isControlled = false;
        }
        GameManager.instance.interactEnabled = false;

        DisplayMessageScript.instance.ImmidiatelyHideMessage();
        StartCoroutine(fadeScript.FadeOutInCycle(fadeSpeed, fadeLength));
        StartCoroutine(WakeUp(fadeSpeed + fadeLength, 0.5f));
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        confirmSleep = false;
    }

    public IEnumerator WakeUp(float animationLength, float waitLength)
    {
        yield return new WaitForSeconds(animationLength + waitLength);
        foreach (PlayerMovement player in GameManager.instance.players)
        {
            player.isControlled = true;
        }

        GameManager.instance.interactEnabled = true;
        DisplayMessageScript.instance.ChangeDisplayMessage(RandomMorningMessage(), 1, 2);
        confirmSleep = false;
    }

    public string RandomMorningMessage()
    {
        string message;
        int random = Random.Range(0, 4);

        switch (random)
        {
            case 0:
                message = "I need to get up..";
                break;
            case 1:
                message = "My back hurts..";
                break;
            case 2:
                message = "I feel pretty good today..";
                break;
            case 3:
                message = "This bed sucks..";
                break;
            case 4:
                message = "I'm thirsty..";
                break;
            default:
                message = "I need to get up..";
                break;
        }
        return message;
    }
    
}
