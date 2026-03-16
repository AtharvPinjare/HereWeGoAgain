using System.Collections;
using UnityEngine;

public abstract class AnomalyBase : MonoBehaviour
{
    [SerializeField] protected AnomalyData data;

    private HealthSystem healthSystem;

    public bool IsActive => data != null && data.isActive;

    public AnomalyData Data => data;

    protected virtual void Awake()
    {
        healthSystem = FindObjectOfType<HealthSystem>();

        if (healthSystem == null)
            Debug.LogError("AnomalyBase: HealthSystem not found in scene.DealDamageToPlayer will not work. Ensure HealthSystem component exists on a GameObject in the scene.");
        else
            Debug.Log("AnomalyBase: HealthSystem cached successfully on " + gameObject.name);

        if (healthSystem == null)
        {
            Debug.LogWarning($"{nameof(AnomalyBase)} could not find a {nameof(HealthSystem)} in the scene.");
        }

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    public abstract void Activate();

    public abstract void Resolve();

    public abstract void TriggerFailState();

    public abstract void ResetAnomaly();

    protected void DealDamageToPlayer()
    {
        if (healthSystem == null)
        {
            return;
        }

        healthSystem.TakeDamage();
    }

    protected IEnumerator BlackoutAndReset(float delay)
    {
        yield return new WaitForSeconds(delay);
        EventBus.OnPlayerDied?.Invoke();
    }
}
