using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHits = 3;
    [SerializeField] private Image vignetteImage;
    [SerializeField] private Color[] vignetteStages;

    private int currentHits = 0;
    private bool isDead = false;

    private void OnEnable()
    {
        EventBus.OnDayStarted += HandleDayStarted;
    }

    private void OnDisable()
    {
        EventBus.OnDayStarted -= HandleDayStarted;
    }

    public void TakeDamage()
    {
        if (isDead)
        {
            return;
        }

        currentHits++;

        if (vignetteImage != null && currentHits < vignetteStages.Length)
        {
            vignetteImage.color = vignetteStages[currentHits];
        }

        EventBus.OnPlayerHit?.Invoke();

        if (currentHits >= maxHits)
        {
            SetDead();
        }
    }

    public void Reset()
    {
        currentHits = 0;
        isDead = false;

        if (vignetteImage != null && vignetteStages.Length > 0)
        {
            vignetteImage.color = vignetteStages[0];
        }
    }

    public void SetDead()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        EventBus.OnPlayerDied?.Invoke();
    }

    private void HandleDayStarted(int dayNumber)
    {
        Reset();
    }
}
