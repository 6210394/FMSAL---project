using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecieveImpact : MonoBehaviour
{
    public float characterMass = 1f;
    Vector3 impactDirection;
    private CharacterController character;

    void Awake()
    {
        character = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (impactDirection.magnitude > 0.2F) character.Move(impactDirection * Time.deltaTime);
        impactDirection = Vector3.Lerp(impactDirection, Vector3.zero, 5 * Time.deltaTime);
    }

    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y;
        impactDirection += dir.normalized * force / characterMass;
    }
}
