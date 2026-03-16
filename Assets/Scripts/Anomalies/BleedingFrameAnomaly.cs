using System.Collections;
using UnityEngine;

public class BleedingFrameAnomaly : AnomalyBase
{
    [SerializeField] private GameObject frameObject;
    [SerializeField] private Renderer frameRenderer;
    [SerializeField] private string _BaseColor = "  ";
    [SerializeField] private float stage1Duration = 60.0f;
    [SerializeField] private float shakeIntensity = 0.04f;
    [SerializeField] private float shakeFrequency = 18.0f;
    [SerializeField] private float floatSpeed = 2.0f;
    [SerializeField] private float contactRadius = 0.8f;
    [SerializeField] private float hitCooldown = 1.2f;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float bounceAmplitude = 1.5f;
    [SerializeField] private float bounceFrequency = 1.2f;

    private Coroutine stage1Coroutine = null;
    private Coroutine stage2Coroutine = null;
    private Coroutine shakeCoroutine = null;
    private bool isStage2Active = false;
    private bool isFailStateTriggered = false;
    private float lastHitTime = -999f;
    private Vector3 frameOriginalPosition;
    private Quaternion frameOriginalRotation;
    private Material frameMaterialInstance;

    protected override void Awake()
    {
        base.Awake();

        if (frameObject != null)
        {
            frameOriginalPosition = frameObject.transform.position;
            frameOriginalRotation = frameObject.transform.rotation;
        }

        if (frameRenderer != null)
        {
            frameMaterialInstance = frameRenderer.material;

            if (frameMaterialInstance.HasProperty(_BaseColor))
            {
                frameMaterialInstance.SetFloat(_BaseColor, 0f);
            }
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
        isStage2Active = false;
        isFailStateTriggered = false;
        lastHitTime = -999f;

        ResetFrameVisuals();
        StopAllRunningCoroutines();

        stage1Coroutine = StartCoroutine(Stage1Sequence());
        shakeCoroutine = StartCoroutine(ShakeFrame());
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
        ResetFrameToWall();
        ResetFrameVisuals();
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
        ResetFrameToWall();
        ResetFrameVisuals();
        EventBus.OnAnomalyFailState?.Invoke(data.id);
        StartCoroutine(BlackoutAndReset(1.0f));
    }

    public override void ResetAnomaly()
    {
        StopAllRunningCoroutines();
        StopAllCoroutines();
        isStage2Active = false;
        isFailStateTriggered = false;
        lastHitTime = -999f;
        ResetFrameToWall();
        ResetFrameVisuals();

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    private IEnumerator Stage1Sequence()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, stage1Duration);

        while (elapsed < duration)
        {
            if (data == null || !data.isActive)
            {
                stage1Coroutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float acceleratedFill = normalizedTime * normalizedTime;

            if (frameMaterialInstance != null &&
                frameMaterialInstance.HasProperty(_BaseColor))
            {
                frameMaterialInstance.SetFloat(_BaseColor, acceleratedFill);
            }

            yield return null;
        }

        stage1Coroutine = null;

        if (data != null && data.isActive)
        {
            StopShake();
            ResetFrameToWall();
            isStage2Active = true;
            stage2Coroutine = StartCoroutine(Stage2Float());
        }
    }

    private IEnumerator ShakeFrame()
    {
        while (data != null && data.isActive && !isStage2Active)
        {
            float offsetX = Mathf.Sin(Time.time * shakeFrequency) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency * 1.3f) * shakeIntensity;

            if (frameObject != null)
            {
                frameObject.transform.position = frameOriginalPosition + new Vector3(offsetX, offsetY, 0f);
            }

            yield return null;
        }

        shakeCoroutine = null;
    }

    private IEnumerator Stage2Float()
    {
        while (data != null && data.isActive)
        {
            if (frameObject == null || playerObject == null)
            {
                yield return null;
                continue;
            }

            Vector3 playerPos = playerObject.transform.position;
            Vector3 framePos = frameObject.transform.position;
            float bounce = Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
            Vector3 targetPos = new Vector3(
                playerPos.x,
                playerPos.y + 1.5f + bounce,
                playerPos.z);

            frameObject.transform.position = Vector3.MoveTowards(
                framePos,
                targetPos,
                floatSpeed * Time.deltaTime);

            float dist = Vector3.Distance(frameObject.transform.position, playerPos);
            if (dist <= contactRadius)
            {
                if (Time.time - lastHitTime >= hitCooldown)
                {
                    lastHitTime = Time.time;
                    DealDamageToPlayer();
                }
            }

            yield return null;
        }

        stage2Coroutine = null;
    }

    private void StopAllRunningCoroutines()
    {
        if (stage1Coroutine != null)
        {
            StopCoroutine(stage1Coroutine);
            stage1Coroutine = null;
        }

        if (stage2Coroutine != null)
        {
            StopCoroutine(stage2Coroutine);
            stage2Coroutine = null;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    private void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    private void ResetFrameToWall()
    {
        if (frameObject != null)
        {
            frameObject.transform.position = frameOriginalPosition;
            frameObject.transform.rotation = frameOriginalRotation;
        }
    }

    private void ResetFrameVisuals()
    {
        if (frameMaterialInstance != null &&
            frameMaterialInstance.HasProperty(_BaseColor))
        {
            frameMaterialInstance.SetFloat(_BaseColor, 0f);
        }
    }
}
