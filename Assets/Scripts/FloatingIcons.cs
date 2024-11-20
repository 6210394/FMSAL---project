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
        if (Camera.current != null)
        {
            transform.LookAt(Camera.current.transform);
        }
    }

    public void SetIconActive(bool active)
    {
        GetComponent<SpriteRenderer>().enabled = active;
    }
}
