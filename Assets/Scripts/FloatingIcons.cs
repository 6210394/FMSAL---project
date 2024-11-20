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
        transform.LookAt(GameObject.FindGameObjectWithTag("PlayerCamera").transform);
    }

    public void SetIconActive(bool active)
    {
        GetComponent<SpriteRenderer>().enabled = active;
    }
}
