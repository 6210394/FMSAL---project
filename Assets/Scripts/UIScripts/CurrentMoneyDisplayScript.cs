using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrentMoneyDisplayScript : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    public void UpdateMoneyText(int money)
    {
        moneyText.text = money.ToString() + "$";
    }
}
