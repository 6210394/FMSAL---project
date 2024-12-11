using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelZoneScript : MonoBehaviour
{
    public bool lose;
    public bool win;
    public bool display;
    public bool fadeOut;

    private void OnTriggerEnter(Collider other)
    {
        if (lose)
        {
            LevelManager.instance.FailMission();
        }
        if (win)
        {
            LevelManager.instance.CompleteMission();
        }

        if (display)
        {
            DisplayMessageScript.instance.ChangeDisplayMessage("Sample Text", 1, 1);
        }
        if(fadeOut)
        {
            StartCoroutine(FadeInOutScript.instance.IFadeOutInCycle(1, 2));
        }
    }
}
