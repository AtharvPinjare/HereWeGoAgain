using DG.Tweening;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    [System.Serializable]
    public class PlantSlot
    {
        public GameObject deadPlant;
        public GameObject alivePlant;
    }

    [SerializeField] private PlantSlot[] plantSlots;

    private int currentPlantIndex = 0;

    public void BloomPlant(int dayIndex)
    {
        if (plantSlots == null || dayIndex < 0 || dayIndex >= plantSlots.Length)
        {
            return;
        }

        PlantSlot slot = plantSlots[dayIndex];
        if (slot == null)
        {
            return;
        }

        if (slot.deadPlant != null)
        {
            slot.deadPlant.SetActive(false);
        }

        if (slot.alivePlant != null)
        {
            slot.alivePlant.SetActive(true);
            slot.alivePlant.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 0.5f);
        }

        currentPlantIndex = dayIndex + 1;
    }

    public void ResetAll()
    {
        if (plantSlots == null)
        {
            currentPlantIndex = 0;
            return;
        }

        foreach (PlantSlot slot in plantSlots)
        {
            if (slot == null)
            {
                continue;
            }

            if (slot.deadPlant != null)
            {
                slot.deadPlant.SetActive(true);
            }

            if (slot.alivePlant != null)
            {
                slot.alivePlant.SetActive(false);
            }
        }

        currentPlantIndex = 0;
    }

    private void OnEnable()
    {
        EventBus.OnDayResolved += HandleDayResolved;
        EventBus.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        EventBus.OnDayResolved -= HandleDayResolved;
        EventBus.OnPlayerDied -= HandlePlayerDied;
    }

    private void HandleDayResolved(int dayNumber)
    {
        BloomPlant(dayNumber - 1);
    }

    private void HandlePlayerDied()
    {
        ResetAll();
    }
}
