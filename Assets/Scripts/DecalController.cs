using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[ExecuteAlways]
public class DecalController : MonoBehaviour
{
    private DecalProjector _decalProjector;
    private bool isOpaque = false;  // Track if the decal is currently visible

    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public float scaleUpDuration = 1f;
    public float scaleDownDuration = 1f;
    public float maxSize = 2.8f;

    private Coroutine fadeInCoroutine;
    private Coroutine fadeOutCoroutine;

    void Awake()
    {
        _decalProjector = GetComponent<DecalProjector>();
    }

    public bool IsOpaque => isOpaque;  // Public property to check if the decal is visible

    public void StartFadeInAndScaleUp()
    {
        if (!isOpaque)
        {
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
            }
            fadeInCoroutine = StartCoroutine(FadeInAndScaleUp());
        }
    }

    public void StartFadeOutAndScaleDown()
    {
        if (isOpaque)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            fadeOutCoroutine = StartCoroutine(FadeOutAndScaleDown());
        }
    }

    private IEnumerator FadeInAndScaleUp()
    {
        float startOpacity = 0f;
        float endOpacity = 1f;
        float startSize = 0f;
        float endSize = maxSize;

        float elapsedTime = 0f;

        while (elapsedTime < Mathf.Max(fadeInDuration, scaleUpDuration))
        {
            elapsedTime += Time.deltaTime;

            // Transition opacity
            if (elapsedTime <= fadeInDuration)
            {
                _decalProjector.fadeFactor = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / fadeInDuration);
            }

            // Transition size
            if (elapsedTime <= scaleUpDuration)
            {
                float currentSize = Mathf.Lerp(startSize, endSize, elapsedTime / scaleUpDuration);
                _decalProjector.size = new Vector3(currentSize, currentSize, _decalProjector.size.z);
            }

            yield return null;
        }

        _decalProjector.fadeFactor = endOpacity;
        _decalProjector.size = new Vector3(endSize, endSize, _decalProjector.size.z);
        isOpaque = true;
    }

    private IEnumerator FadeOutAndScaleDown()
    {
        float startOpacity = 1f;
        float endOpacity = 0f;
        float startSize = maxSize;
        float endSize = 0f;

        float elapsedTime = 0f;

        while (elapsedTime < Mathf.Max(fadeOutDuration, scaleDownDuration))
        {
            elapsedTime += Time.deltaTime;

            // Transition opacity
            if (elapsedTime <= fadeOutDuration)
            {
                _decalProjector.fadeFactor = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / fadeOutDuration);
            }

            // Transition size
            if (elapsedTime <= scaleDownDuration)
            {
                float currentSize = Mathf.Lerp(startSize, endSize, elapsedTime / scaleDownDuration);
                _decalProjector.size = new Vector3(currentSize, currentSize, _decalProjector.size.z);
            }

            yield return null;
        }

        _decalProjector.fadeFactor = endOpacity;
        _decalProjector.size = new Vector3(endSize, endSize, _decalProjector.size.z);
        isOpaque = false;
    }
}