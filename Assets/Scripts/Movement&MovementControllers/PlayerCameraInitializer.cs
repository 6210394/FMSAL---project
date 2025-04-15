using UnityEngine;
using UnityEngine.Events;

public class PlayerCameraInitializer : MonoBehaviour
{
    public GameObject defaultPlayerCamera;
    public GameObject targetCamera;
    public GameObject aimCamera;

    public UnityEvent camerasInitialized;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(!GameObject.Find(defaultPlayerCamera.name))
        {
            Instantiate(defaultPlayerCamera, gameObject.transform);
        }
        if(!GameObject.Find(targetCamera.name))
        {
            Instantiate(targetCamera, gameObject.transform);
        }
        if(!GameObject.Find(aimCamera.name))
        {
            Instantiate(aimCamera, gameObject.transform);
        }
    }

}
