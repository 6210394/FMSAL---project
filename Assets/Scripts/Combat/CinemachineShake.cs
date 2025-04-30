using Unity.Cinemachine;
using UnityEngine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance {get; private set;}
    [SerializeField] private CinemachineCamera[] cinemachineCameras;
    private CinemachineCamera currentActiveCamera;
    private float shakeTimer;

    private void Awake()
    {
        Instance = this;
        cinemachineCameras = GetComponentsInChildren<CinemachineCamera>();
    }

    void Start()
    {
        currentActiveCamera = cinemachineCameras[0];
    }

    public void SetActiveCamera(int activeCameraIndex)
    {
        if(currentActiveCamera != cinemachineCameras[activeCameraIndex])
        {
            ResetCameraShake();
        }
        currentActiveCamera = cinemachineCameras[activeCameraIndex];
    }

    public void ShakeCamera(float intensity, float time)
    {
        Debug.Log("SHAKING THAT " + currentActiveCamera + " CAM!!");
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            currentActiveCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        shakeTimer = time;
    }

    public void ResetCameraShake()
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    currentActiveCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if(shakeTimer <= 0)
            {
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    currentActiveCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
            }
        }
    }
}
