using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] Volume postProcessingVolume;
    private Vignette vignetteEffect;
    ClampedFloatParameter defaultVignette;

    void Awake()
    {
        postProcessingVolume = GetComponent<Volume>();
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out vignetteEffect))
        {
            defaultVignette = new ClampedFloatParameter(vignetteEffect.intensity.value, 0f, 1f); // Store the value
            Debug.Log(defaultVignette.value);
        }
    }

    public IEnumerator IVignetteFadeInOut(float value, float fadeInLength, float stayDuration, float fadeOutLength)
    {
        VignetteFadeIn(value, fadeInLength);
        yield return new WaitForSeconds(stayDuration);
        VignetteFadeOut(fadeOutLength);
    }

    public void VignetteFadeIn(float value, float fadeInLength)
    {
        if (vignetteEffect != null)
        {
            StartCoroutine(IFadeVignetteIntensity(vignetteEffect.intensity.value, value, fadeInLength));
        }
    }

    public void VignetteFadeOut(float fadeOutLength)
    {
         if (vignetteEffect != null)
        {
            StartCoroutine(IFadeVignetteIntensity(vignetteEffect.intensity.value, defaultVignette.value, fadeOutLength));
        }
    }

        private IEnumerator IFadeVignetteIntensity(float startValue, float endValue, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newValue = Mathf.Lerp(startValue, endValue, elapsed / duration);
                vignetteEffect.intensity.value = newValue;
                yield return null;
            }
            vignetteEffect.intensity.value = endValue;
        }
}
