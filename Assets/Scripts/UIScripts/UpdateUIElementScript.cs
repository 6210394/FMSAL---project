using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateUIElementScript : MonoBehaviour
{    
    public static UpdateUIElementScript instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hungerText = GameObject.Find("HungerText").GetComponent<TMPro.TextMeshProUGUI>();
        dayText = GameObject.Find("DayText").GetComponent<TMPro.TextMeshProUGUI>();
        quotaText = GameObject.Find("QuotaText").GetComponent<TMPro.TextMeshProUGUI>();
        UpdateUIElementsDebug();
    }

    public TMPro.TextMeshProUGUI hungerText;
    public TMPro.TextMeshProUGUI dayText;
    public TMPro.TextMeshProUGUI quotaText;

    public void UpdateUIElementsDebug()
    {
        hungerText.text = GameManager.instance.hungerPoints.ToString();
        dayText.text = GameManager.instance.day.ToString();
        quotaText.text = GameManager.instance.weekQuota.ToString();
    }
}
