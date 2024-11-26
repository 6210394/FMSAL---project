using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelZoneScript : MonoBehaviour
{
    public bool lose;

    private void OnTriggerEnter(Collider other)
    {
        if (lose)
        {
            LevelManager.instance.FailMission();
        }
        else
        {
            LevelManager.instance.CompleteMission();
        }
    }
}
