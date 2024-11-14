using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField]
    bool lockMode = false;

    bool isAiming = false;

    [Range(40, 100)]
    public float baseFOV = 60;
    [Range(20, 60)]
    public float aimFOV = 40;

    public Camera cam;
    public Transform target;


    [Range(-5.0f, 5.0f)]
    public float baseDistanceX;
    [Range(-5.0f, 5.0f)]
    public float baseDistanceY;
    [Range(-5.0f, 5.0f)]
    public float baseDistanceZ;

    [Range(-5.0f, 5.0f)]
    public float aimDistanceX;
    [Range(-5.0f, 5.0f)]
    public float aimDistanceY;
    [Range(-5.0f, 5.0f)]
    public float aimDistanceZ;


    public float rotationSpeed = 5.0f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        lockMode = false;

        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }

    // Update is called once per frame

    void Update()
    {
        AimCheckAndFOV();

        if(!lockMode)
        {
            rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            rotationY = Mathf.Clamp(rotationY, -90, 90);
        }
        if(lockMode)
        {
            if(Input.GetMouseButton(1))
            {
                rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
                rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
                rotationY = Mathf.Clamp(rotationY, -90, 90);
            }
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            lockMode = !lockMode;
        }
        if(!lockMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    void LateUpdate()
    {   
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        transform.rotation = rotation;

        if(!isAiming)
        {
            transform.position = target.position + rotation * new Vector3(baseDistanceX, baseDistanceY, baseDistanceZ);
        }
        if(isAiming)
        {
            transform.position = target.position + rotation * new Vector3(aimDistanceX, aimDistanceY, aimDistanceZ);
        }
    }

    void AimCheckAndFOV()
    {
        if(Input.GetMouseButton(1))
        {
            isAiming = true;
            cam.fieldOfView = aimFOV;
        }
        else
        {
            isAiming = false;
            cam.fieldOfView = baseFOV;
        }
    }
}
