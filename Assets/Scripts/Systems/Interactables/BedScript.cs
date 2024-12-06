using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BedScript : Interactable
{

    public static BedScript instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public bool confirmSleep = false;
    public FadeInOutScript fadeScript = FadeInOutScript.instance;


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
        float waitTime = 2;
        float animationLength = 1;

        foreach (GameObject player in GameManager.instance.players)
        {
            player.GetComponent<PlayerMovement>().isControlled = false;
        }
        GameManager.instance.interactEnabled = false;

        StartCoroutine(FadeInOutScript.instance.IFadeOut(3));
        DisplayMessageScript.instance.ImmidiatelyHideMessage();
        GameManager.instance.GoToNextDay();
        if(GameManager.instance.hungerState == GameManager.HUNGER.DEAD)
        {
            StartCoroutine(GameManager.instance.IGameOver("You never woke up."));
        }
        else
        {
            StartCoroutine(IWakeUp(animationLength, waitTime));
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        confirmSleep = false;
    }

    public IEnumerator IWakeUp(float animationLength, float waitLength)
    {
        yield return new WaitForSeconds(waitLength);

        StartCoroutine(FadeInOutScript.instance.IFadeIn(waitLength));

        yield return new WaitForSeconds(animationLength + waitLength);
        foreach (GameObject player in GameManager.instance.players)
        {
            player.GetComponent<PlayerMovement>().isControlled = true;
        }

        GameManager.instance.interactEnabled = true;
        DisplayMessageScript.instance.ChangeDisplayMessage(RandomMorningMessage(), 1, 2);
        confirmSleep = false;
    }

    public string RandomMorningMessage()
    {
        string message;
        int random = Random.Range(0, 4);

        switch(GameManager.instance.hungerState)
        {
            case GameManager.HUNGER.STARVED:
                message = "... My stomach..";
                return message;
            
            default:
            {
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


        
        
    }
    
}
