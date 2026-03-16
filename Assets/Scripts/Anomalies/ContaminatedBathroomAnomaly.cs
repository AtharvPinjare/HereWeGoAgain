using System.Collections;
using UnityEngine;

public class ContaminatedBathroomAnomaly : AnomalyBase
{
    public enum FailReason
    {
        TimerExpired,
        GreenPressed
    }

    [SerializeField] private Renderer[] bathroomSurfaces;
    [SerializeField] private Material stainedMaterial;
    [SerializeField] private AudioSource crackingAudioSource;
    [SerializeField] private float failStateTimer = 45.0f;
    [SerializeField] private float stainSpreadRate = 0.02f;
    [SerializeField] private float crackAudioFadeInTime = 10.0f;
    [SerializeField] private float contaminationOverlayDuration = 1.5f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.8f;

    private Coroutine stainCoroutine = null;
    private Coroutine countdownCoroutine = null;
    private Coroutine shakeCoroutine = null;
    private bool isFailStateTriggered = false;
    private float currentStainAmount = 0f;
    private Material[] originalMaterials;

    protected override void Awake()
    {
        base.Awake();

        if (bathroomSurfaces != null && bathroomSurfaces.Length > 0)
        {
            originalMaterials = new Material[bathroomSurfaces.Length];

            for (int i = 0; i < bathroomSurfaces.Length; i++)
            {
                if (bathroomSurfaces[i] == null)
                {
                    continue;
                }

                originalMaterials[i] = bathroomSurfaces[i].sharedMaterial;
            }
        }

        if (crackingAudioSource != null)
        {
            crackingAudioSource.volume = 0f;
            crackingAudioSource.loop = true;
            crackingAudioSource.Stop();
        }
    }

    public override void Activate()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = true;
        data.isResolved = false;
        isFailStateTriggered = false;
        currentStainAmount = 0f;

        ResetBathroomVisuals();
        StopAllRunningCoroutines();

        stainCoroutine = StartCoroutine(SpreadStain());
        countdownCoroutine = StartCoroutine(FailStateCountdown());

        if (crackingAudioSource != null)
        {
            crackingAudioSource.Play();
        }
    }

    public override void Resolve()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = false;
        data.isResolved = true;

        StopAllRunningCoroutines();
        ResetBathroomVisuals();
        StopCrackingAudio();
        EventBus.OnAnomalyResolved?.Invoke(data.id);
    }

    public override void TriggerFailState()
    {
        TriggerFailState(FailReason.TimerExpired);
    }

    public void TriggerFailState(FailReason reason)
    {
        if (isFailStateTriggered)
        {
            return;
        }

        isFailStateTriggered = true;

        if (data != null)
        {
            data.isActive = false;
        }

        StopAllRunningCoroutines();
        StopCrackingAudio();

        if (data != null)
        {
            EventBus.OnAnomalyFailState?.Invoke(data.id);
        }

        EventBus.OnInputLocked?.Invoke();

        if (reason == FailReason.TimerExpired)
        {
            StartCoroutine(TimerFailSequence());
        }
        else if (reason == FailReason.GreenPressed)
        {
            StartCoroutine(GreenPressFailSequence());
        }
    }

    public override void ResetAnomaly()
    {
        StopAllRunningCoroutines();
        StopAllCoroutines();
        isFailStateTriggered = false;
        currentStainAmount = 0f;
        ResetBathroomVisuals();
        StopCrackingAudio();

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    public void TriggerGreenFail()
    {
        TriggerFailState(FailReason.GreenPressed);
    }

    private IEnumerator SpreadStain()
    {
        while (data != null && data.isActive)
        {
            currentStainAmount += stainSpreadRate * Time.deltaTime;
            currentStainAmount = Mathf.Clamp01(currentStainAmount);
            ApplyStainToSurfaces(currentStainAmount);
            yield return null;
        }

        stainCoroutine = null;
    }

    private IEnumerator FailStateCountdown()
    {
        float elapsed = 0f;

        while (elapsed < failStateTimer)
        {
            if (data == null || !data.isActive)
            {
                countdownCoroutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;

            if (crackingAudioSource != null)
            {
                float fadeDuration = Mathf.Max(0.01f, crackAudioFadeInTime);
                crackingAudioSource.volume = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            }

            yield return null;
        }

        countdownCoroutine = null;
        TriggerFailState(FailReason.TimerExpired);
    }

    private IEnumerator TimerFailSequence()
    {
        if (crackingAudioSource != null)
        {
            crackingAudioSource.volume = 1f;
        }

        if (bathroomSurfaces != null)
        {
            for (int i = 0; i < bathroomSurfaces.Length; i++)
            {
                if (bathroomSurfaces[i] == null || stainedMaterial == null)
                {
                    continue;
                }

                bathroomSurfaces[i].sharedMaterial = stainedMaterial;
            }
        }

        yield return new WaitForSeconds(0.3f);
        ResetBathroomVisuals(); 
        StartCoroutine(BlackoutAndReset(0.5f));
    }

    private IEnumerator GreenPressFailSequence()
    {
        ApplyStainToSurfaces(1f);
        yield return new WaitForSeconds(0.2f);

        shakeCoroutine = StartCoroutine(ShakeCamera(shakeDuration));

        float sequenceDuration = Mathf.Max(shakeDuration, contaminationOverlayDuration);
        yield return new WaitForSeconds(sequenceDuration);
        ResetBathroomVisuals();
        StartCoroutine(BlackoutAndReset(0.5f));
    }

    private IEnumerator ShakeCamera(float duration)
    {
        if (playerCamera == null)
        {
            shakeCoroutine = null;
            yield break;
        }

        Vector3 originalCamPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            playerCamera.transform.localPosition = originalCamPos + new Vector3(x, y, 0f);
            yield return null;
        }

        playerCamera.transform.localPosition = originalCamPos;
        shakeCoroutine = null;
    }

    private void ApplyStainToSurfaces(float stainAmount)
    {
        if (bathroomSurfaces == null ||
            stainedMaterial == null) return;
        for (int i = 0; i < bathroomSurfaces.Length; i++)
        {
            if (bathroomSurfaces[i] == null) continue;
            if (stainAmount >= 1f)
            {
                bathroomSurfaces[i].sharedMaterial =
                    stainedMaterial;
            }
            else if (stainAmount <= 0f)
            {
                if (originalMaterials != null &&
                    i < originalMaterials.Length &&
                    originalMaterials[i] != null)
                    bathroomSurfaces[i].sharedMaterial =
                        originalMaterials[i];
            }
            else
            {
                // Lerp via material property block
                // to avoid creating runtime instances
                MaterialPropertyBlock mpb =
                    new MaterialPropertyBlock();
                bathroomSurfaces[i].GetPropertyBlock(mpb);
                Color originalColor = originalMaterials[i] != null
                    ? originalMaterials[i].color
                    : Color.white;
                Color stainColor = stainedMaterial.color;
                mpb.SetColor("_BaseColor",
                    Color.Lerp(originalColor, stainColor, stainAmount));
                bathroomSurfaces[i].SetPropertyBlock(mpb);
            }
        }
    }

    private void ResetBathroomVisuals()
    {
        currentStainAmount = 0f;

        if (bathroomSurfaces == null)
        {
            return;
        }

        for (int i = 0; i < bathroomSurfaces.Length; i++)
        {
            if (bathroomSurfaces[i] == null)
            {
                continue;
            }

            if (originalMaterials != null &&
                i < originalMaterials.Length &&
                originalMaterials[i] != null)
            {
                bathroomSurfaces[i].sharedMaterial = originalMaterials[i];
            }

            bathroomSurfaces[i].SetPropertyBlock(null);
        }
    }

    private void StopCrackingAudio()
    {
        if (crackingAudioSource != null)
        {
            crackingAudioSource.Stop();
            crackingAudioSource.volume = 0f;
        }
    }

    private void StopAllRunningCoroutines()
    {
        if (stainCoroutine != null)
        {
            StopCoroutine(stainCoroutine);
            stainCoroutine = null;
        }

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }
}
