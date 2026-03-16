using System.Collections;
using UnityEngine;

public class BackwardClockAnomaly : AnomalyBase
{
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private AudioSource clockAudioSource;
    [SerializeField] private AudioClip forwardTickClip;
    [SerializeField] private AudioClip reverseTickClip;
    [SerializeField] private GameObject clockGameObject;
    [SerializeField] private ClockIdleBehaviour clockIdleBehaviour;
    [SerializeField] private EnvironmentController environmentController;
    [SerializeField] private float reverseTickSpeed = -1.0f;
    [SerializeField] private float tempoEscalationRate = 0.08f;
    [SerializeField] private float maxReverseSpeed = -12.0f;
    [SerializeField] private float audioPitchMin = 1.0f;
    [SerializeField] private float audioPitchMax = 3.0f;
    [SerializeField] private ParticleSystem shatterParticles;

    private Coroutine reverseCoroutine = null;
    private bool isFailStateTriggered = false;
    private float currentReverseSpeed = 0f;
    private Quaternion hourHandOriginalRotation;
    private Quaternion minuteHandOriginalRotation;

    protected override void Awake()
    {
        base.Awake();

        if (hourHand != null)
        {
            hourHandOriginalRotation = hourHand.localRotation;
        }

        if (minuteHand != null)
        {
            minuteHandOriginalRotation = minuteHand.localRotation;
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
        currentReverseSpeed = reverseTickSpeed;

        if (clockIdleBehaviour != null)
        {
            clockIdleBehaviour.isOverriddenByAnomaly = true;
        }

        if (clockAudioSource != null && reverseTickClip != null)
        {
            clockAudioSource.clip = reverseTickClip;
            clockAudioSource.pitch = audioPitchMin;
            clockAudioSource.loop = true;
            clockAudioSource.Play();
        }

        StopReverseCoroutine();
        reverseCoroutine = StartCoroutine(ReverseClockTick());
    }

    public override void Resolve()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = false;
        data.isResolved = true;

        StopReverseCoroutine();
        StopAllCoroutines();
        isFailStateTriggered = false;
        ResetClockHands();
        RestoreClockAudio();
        ReturnClockToWall();

        if (clockIdleBehaviour != null)
        {
            clockIdleBehaviour.isOverriddenByAnomaly = false;
        }

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

        StopReverseCoroutine();
        StartCoroutine(ShatterSequence());
    }

    public override void ResetAnomaly()
    {
        StopReverseCoroutine();
        StopAllCoroutines();
        isFailStateTriggered = false;
        currentReverseSpeed = reverseTickSpeed;
        ResetClockHands();
        RestoreClockAudio();
        ReturnClockToWall();

        if (clockIdleBehaviour != null)
        {
            clockIdleBehaviour.isOverriddenByAnomaly = false;
        }

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    private IEnumerator ReverseClockTick()
    {
        while (data != null && data.isActive)
        {
            currentReverseSpeed -= tempoEscalationRate * Time.deltaTime;
            currentReverseSpeed = Mathf.Max(currentReverseSpeed, maxReverseSpeed);

            if (hourHand != null)
            {
                hourHand.Rotate(Vector3.forward, currentReverseSpeed * 100 * Time.deltaTime, Space.Self);
            }

            if (minuteHand != null)
            {
                minuteHand.Rotate(Vector3.forward, currentReverseSpeed * 120f * Time.deltaTime, Space.Self);
            }

            if (clockAudioSource != null)
            {
                float speedRatio = Mathf.InverseLerp(reverseTickSpeed, maxReverseSpeed, currentReverseSpeed);
                clockAudioSource.pitch = Mathf.Lerp(audioPitchMin, audioPitchMax, speedRatio);
            }

            if (Mathf.Abs(currentReverseSpeed) >= Mathf.Abs(maxReverseSpeed))
            {
                TriggerFailState();
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator ShatterSequence()
    {
        if (clockAudioSource != null)
        {
            clockAudioSource.pitch = audioPitchMax;
        }

        if (clockAudioSource != null)
        {
            clockAudioSource.Stop();
        }

        if (shatterParticles != null)
        {
            shatterParticles.Play();
        }

        if (clockGameObject != null)
        {
            clockGameObject.SetActive(false);
        }

        if (environmentController != null)
        {
            environmentController.ScatterAllObjects();
        }

        EventBus.OnAnomalyFailState?.Invoke(data?.id ?? string.Empty);
        EventBus.OnInputLocked?.Invoke();

        yield return null;

        StartCoroutine(BlackoutAndReset(0.0f));
    }

    private void StopReverseCoroutine()
    {
        if (reverseCoroutine != null)
        {
            StopCoroutine(reverseCoroutine);
            reverseCoroutine = null;
        }
    }

    private void ResetClockHands()
    {
        if (hourHand != null)
        {
            hourHand.localRotation = hourHandOriginalRotation;
        }

        if (minuteHand != null)
        {
            minuteHand.localRotation = minuteHandOriginalRotation;
        }
    }

    private void RestoreClockAudio()
    {
        if (clockAudioSource == null)
        {
            return;
        }

        clockAudioSource.Stop();
        clockAudioSource.pitch = audioPitchMin;

        if (forwardTickClip != null)
        {
            clockAudioSource.clip = forwardTickClip;
        }
    }

    private void ReturnClockToWall()
    {
        if (clockGameObject != null)
        {
            clockGameObject.SetActive(true);
        }
    }
}
