using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    public Camera mainCam;

    [Header("FOV Settings")]
    public float slideFOV = 70f;
    public float fovLerpSpeed = 5f;

    private float baseFOV = 60f;
    private Quaternion baseRotation;

    private Coroutine fovRoutine;
    private Coroutine tiltRoutine;

    void Start()
    {
        if (!mainCam)
            mainCam = Camera.main;

        baseFOV = mainCam.fieldOfView;
        baseRotation = mainCam.transform.localRotation;
    }

    public void PlaySlideEffect()
    {
        if (fovRoutine != null) StopCoroutine(fovRoutine);
        if (tiltRoutine != null) StopCoroutine(tiltRoutine);

        fovRoutine = StartCoroutine(DoSlideFOV());

        StartCoroutine(CameraShake(0.1f, 0.05f));
    }

    public void ResetFOV()
    {
        if (fovRoutine != null) StopCoroutine(fovRoutine);
        if (tiltRoutine != null) StopCoroutine(tiltRoutine);

        fovRoutine = StartCoroutine(ResetFOVCoroutine());
    }

    private IEnumerator DoSlideFOV()
    {
        float t = 0f;
        while (Mathf.Abs(mainCam.fieldOfView - slideFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, slideFOV, t);
            t += Time.deltaTime * fovLerpSpeed;
            yield return null;
        }
    }

    private IEnumerator ResetFOVCoroutine()
    {
        float t = 0f;
        while (Mathf.Abs(mainCam.fieldOfView - baseFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, baseFOV, t);
            t += Time.deltaTime * fovLerpSpeed;
            yield return null;
        }
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = originalPos;
    }
}