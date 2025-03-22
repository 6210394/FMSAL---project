using Unity.Cinemachine;
using UnityEngine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance {get; private set;}
    public CinemachineCamera cinemachineCamera;
    private float shakeTimer;

    private void Awake()
    {
        Instance = this;
        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        shakeTimer = time;
    }

    public void ResetCameraShake()
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                
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
                    cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
            }
        }
    }
}
