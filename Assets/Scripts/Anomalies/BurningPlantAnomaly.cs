using System.Collections;
using UnityEngine;

public class BurningPlantAnomaly : AnomalyBase
{
    [SerializeField] private ParticleSystem fireParticleSystem;
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float fireSpreadRate = 0.5f;
    [SerializeField] private float damageProximityRadius = 1.5f;
    [SerializeField] private float startRadius = 0.5f;
    [SerializeField] private float roomFillRadius = 8.0f;
    [SerializeField] private float damageInterval = 1.0f;

    private float currentFireRadius = 0f;
    private Coroutine fireSpreadCoroutine = null;
    private Coroutine damageTickCoroutine = null;
    private bool isFailStateTriggered = false;
    private Vector3 initialFireOriginScale = Vector3.one;

    protected override void Awake()
    {
        base.Awake();

        if (fireOrigin != null)
        {
            initialFireOriginScale = fireOrigin.localScale;
        }
    }

    public override void Activate()
    {
        if (data == null)
        {
            return;
        }

        StopAllRunningCoroutines();

        data.isActive = true;
        data.isResolved = false;
        isFailStateTriggered = false;
        currentFireRadius = startRadius;

        if (fireParticleSystem != null)
        {
            fireParticleSystem.gameObject.SetActive(true);
            fireParticleSystem.Play();
        }

        UpdateFireVisualScale();

        fireSpreadCoroutine = StartCoroutine(SpreadFire());
        damageTickCoroutine = StartCoroutine(DamageTick());
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

        if (fireParticleSystem != null)
        {
            fireParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireParticleSystem.gameObject.SetActive(false);
        }

        currentFireRadius = 0f;
        isFailStateTriggered = false;
        ResetFireVisualScale();

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

        if (fireParticleSystem != null)
        {
            fireParticleSystem.Stop(
        true,
        ParticleSystemStopBehavior.StopEmittingAndClear);
            fireParticleSystem.gameObject.SetActive(false);
        }

        ResetFireVisualScale();

        if (data != null)
        {
            EventBus.OnAnomalyFailState?.Invoke(data.id);
        }

        StartCoroutine(BlackoutAndReset(1.5f));
    }

    public override void ResetAnomaly()
    {
        StopAllRunningCoroutines();
        StopAllCoroutines();

        if (fireParticleSystem != null)
        {
            fireParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireParticleSystem.gameObject.SetActive(false);
        }

        currentFireRadius = 0f;
        isFailStateTriggered = false;
        ResetFireVisualScale();

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    private IEnumerator SpreadFire()
    {
        while (data != null && data.isActive)
        {
            currentFireRadius += fireSpreadRate * Time.deltaTime;
            UpdateFireVisualScale();

            if (currentFireRadius >= roomFillRadius)
            {
                TriggerFailState();
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator DamageTick()
    {
        while (data != null && data.isActive)
        {
            yield return new WaitForSeconds(damageInterval);

            if (playerObject == null || fireOrigin == null || data == null || !data.isActive)
            {
                continue;
            }

            float dist = Vector3.Distance(playerObject.transform.position, fireOrigin.position);
            float effectiveRadius = Mathf.Max(currentFireRadius, damageProximityRadius);

            if (dist <= effectiveRadius)
            {
                DealDamageToPlayer();
            }
        }
    }

    private void StopAllRunningCoroutines()
    {
        if (fireSpreadCoroutine != null)
        {
            StopCoroutine(fireSpreadCoroutine);
            fireSpreadCoroutine = null;
        }

        if (damageTickCoroutine != null)
        {
            StopCoroutine(damageTickCoroutine);
            damageTickCoroutine = null;
        }
    }

    private void UpdateFireVisualScale()
    {
        if (fireOrigin == null)
        {
            return;
        }

        float safeStartRadius = Mathf.Max(startRadius, 0.01f);
        float scale = currentFireRadius / safeStartRadius;
        fireOrigin.localScale = initialFireOriginScale * scale;
    }

    private void ResetFireVisualScale()
    {
        if (fireOrigin == null)
        {
            return;
        }

        fireOrigin.localScale = initialFireOriginScale;
    }
}
