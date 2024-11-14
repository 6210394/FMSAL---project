using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class JorgeMovement : MonoBehaviour
{
    public float playerSpeed;
    public Camera playerCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        Vector3 orientation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        orientation = orientation.normalized;

        transform.position += orientation * playerSpeed * Time.deltaTime;
    }
}
