using TMPro;
using UnityEngine;

public class CurrentLoadScript : MonoBehaviour
{
    
    public TextMeshProUGUI loadText;

    void Start()
    {
        UpdateLoadText();
        LevelManager.instance.onTreasurePickup.AddListener(UpdateLoadText);
        DepositZoneScript.onDeposit.AddListener(UpdateLoadText);
    }

    public void UpdateLoadText()
    {
        loadText.text = LevelManager.instance.currentCarry + "/" + LevelManager.instance.carryLimit;
    }
    
}
