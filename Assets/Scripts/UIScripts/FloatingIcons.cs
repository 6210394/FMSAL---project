using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingIcons : MonoBehaviour
{
    void Start()
    {
        SetIconActive(false);   
    }

    void Update()
    {
        transform.LookAt(GameObject.FindGameObjectWithTag("MainCamera").transform);
    }

    public void SetIconActive(bool active)
    {
        if(GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().enabled = active;
        }
    }
}
