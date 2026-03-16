using System.Collections;
using UnityEngine;

public class ElectricityShortageAnomaly : AnomalyBase
{
    [SerializeField] private Light[] roomLights;
    [SerializeField] private float flickerWindowDuration = 25.0f;
    [SerializeField] private float flickerOnDuration = 1.2f;
    [SerializeField] private float flickerOffDuration = 0.8f;
    [SerializeField] private float ambientIntensity = 0.05f;
    [SerializeField] private float normalIntensity = 1.0f;

    private Coroutine flickerCoroutine = null;
    private Coroutine countdownCoroutine = null;
    private bool isFailStateTriggered = false;
    private float[] originalIntensities;

    protected override void Awake()
    {
        base.Awake();

        if (roomLights == null || roomLights.Length == 0)
        {
            return;
        }

        originalIntensities = new float[roomLights.Length];
        for (int i = 0; i < roomLights.Length; i++)
        {
            if (roomLights[i] == null)
            {
                continue;
            }

            originalIntensities[i] = roomLights[i].intensity;
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

        StopAllRunningCoroutines();

        flickerCoroutine = StartCoroutine(FlickerLights());
        countdownCoroutine = StartCoroutine(FailStateCountdown());
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
        RestoreLights();
        EventBus.OnAnomalyResolved?.Invoke(data.id);
    }

    public override void TriggerFailState()
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
        SetAllLightsTo(0f);

        if (data != null)
        {
            EventBus.OnAnomalyFailState?.Invoke(data.id);
        }

        StartCoroutine(BlackoutAndReset(0.5f));
    }

    public override void ResetAnomaly()
    {
        StopAllRunningCoroutines();
        StopAllCoroutines();
        RestoreLights();
        isFailStateTriggered = false;

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    private IEnumerator FlickerLights()
    {
        while (data != null && data.isActive)
        {
            SetAllLightsTo(normalIntensity);
            yield return new WaitForSeconds(flickerOnDuration);

            if (data == null || !data.isActive)
            {
                yield break;
            }

            SetAllLightsTo(ambientIntensity);
            yield return new WaitForSeconds(flickerOffDuration);
        }
    }

    private IEnumerator FailStateCountdown()
    {
        float elapsed = 0f;

        while (elapsed < flickerWindowDuration)
        {
            if (data == null || !data.isActive)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        TriggerFailState();
    }

    private void SetAllLightsTo(float intensity)
    {
        if (roomLights == null)
        {
            return;
        }

        for (int i = 0; i < roomLights.Length; i++)
        {
            if (roomLights[i] == null)
            {
                continue;
            }

            roomLights[i].intensity = intensity;
        }
    }

    private void RestoreLights()
    {
        if (roomLights == null)
        {
            return;
        }

        for (int i = 0; i < roomLights.Length; i++)
        {
            if (roomLights[i] == null)
            {
                continue;
            }

            if (originalIntensities != null && i < originalIntensities.Length)
            {
                roomLights[i].intensity = originalIntensities[i];
            }
            else
            {
                roomLights[i].intensity = normalIntensity;
            }
        }
    }

    private void StopAllRunningCoroutines()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }
}
