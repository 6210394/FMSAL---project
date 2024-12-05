using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Treasure", menuName = "Treasure")]
public class TreasureScriptable : ScriptableObject
{
    public int rewardMoney = 100;
    public int carryWeight = 1;
    public int dropTime = 1;
}
