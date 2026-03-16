using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    [SerializeField] private AnomalyBase[] allAnomalies;

    private AnomalyBase currentActiveAnomaly = null;
    private int[] dayAssignments;
    private int safeDayCount;

    public AnomalyBase CurrentActiveAnomaly => currentActiveAnomaly;

    private void OnEnable()
    {
        EventBus.OnDayStarted += HandleDayStarted;
        EventBus.OnREDButtonPressed += HandleREDPressed;
        EventBus.OnGREENButtonPressed += HandleGREENPressed;
        EventBus.OnGameStateChanged += HandleGameStateChanged;
        EventBus.OnHealthDepleted += HandleHealthDepleted;
    }

    private void OnDisable()
    {
        EventBus.OnDayStarted -= HandleDayStarted;
        EventBus.OnREDButtonPressed -= HandleREDPressed;
        EventBus.OnGREENButtonPressed -= HandleGREENPressed;
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
        EventBus.OnHealthDepleted -= HandleHealthDepleted;
    }

    public void InitialiseRun()
    {
        dayAssignments = new int[7];
        for (int i = 0; i < dayAssignments.Length; i++)
        {
            dayAssignments[i] = -1;
        }

        safeDayCount = Random.Range(0, 2) == 0 ? 2 : 3;

        int[] daySlots = new int[7];
        for (int i = 0; i < daySlots.Length; i++)
        {
            daySlots[i] = i + 1;
        }

        ShuffleArray(daySlots);

        bool[] safeDayLookup = new bool[7];
        for (int i = 0; i < safeDayCount && i < daySlots.Length; i++)
        {
            int safeDay = daySlots[i];
            safeDayLookup[safeDay - 1] = true;
            dayAssignments[safeDay - 1] = -1;
        }

        List<int> anomalyDaySlots = new List<int>();
        for (int i = 0; i < safeDayLookup.Length; i++)
        {
            if (!safeDayLookup[i])
            {
                anomalyDaySlots.Add(i + 1);
            }
        }

        List<int> anomalyPool = new List<int>();
        int corruptedTextIndex = -1;

        if (allAnomalies != null)
        {
            for (int i = 0; i < allAnomalies.Length; i++)
            {
                AnomalyBase anomaly = allAnomalies[i];
                if (anomaly == null || anomaly.Data == null)
                {
                    continue;
                }

                if (anomaly.Data.type == AnomalyType.CorruptedText)
                {
                    corruptedTextIndex = i;
                    continue;
                }

                anomalyPool.Add(i);
            }
        }

        int[] shuffledPool = anomalyPool.ToArray();
        ShuffleArray(shuffledPool);

        int assignmentCount = Mathf.Min(anomalyDaySlots.Count, shuffledPool.Length);
        for (int i = 0; i < assignmentCount; i++)
        {
            int dayNumber = anomalyDaySlots[i];
            int anomalyIndex = shuffledPool[i];

            dayAssignments[dayNumber - 1] = anomalyIndex;

            if (allAnomalies != null &&
                anomalyIndex >= 0 &&
                anomalyIndex < allAnomalies.Length &&
                allAnomalies[anomalyIndex] != null &&
                allAnomalies[anomalyIndex].Data != null)
            {
                allAnomalies[anomalyIndex].Data.assignedDay = dayNumber;
            }
        }

        if (corruptedTextIndex >= 0 && anomalyDaySlots.Count > 0)
        {
            List<int> eligibleCorruptedDays = new List<int>();
            for (int i = 0; i < anomalyDaySlots.Count; i++)
            {
                if (anomalyDaySlots[i] >= 3)
                {
                    eligibleCorruptedDays.Add(anomalyDaySlots[i]);
                }
            }

            int targetDay;
            if (eligibleCorruptedDays.Count > 0)
            {
                int randomIndex = Random.Range(0, eligibleCorruptedDays.Count);
                targetDay = eligibleCorruptedDays[randomIndex];
            }
            else
            {
                targetDay = anomalyDaySlots[0];
                for (int i = 1; i < anomalyDaySlots.Count; i++)
                {
                    if (anomalyDaySlots[i] > targetDay)
                    {
                        targetDay = anomalyDaySlots[i];
                    }
                }
            }

            int replacedIndex = dayAssignments[targetDay - 1];
            dayAssignments[targetDay - 1] = corruptedTextIndex;

            if (allAnomalies != null &&
                corruptedTextIndex < allAnomalies.Length &&
                allAnomalies[corruptedTextIndex] != null &&
                allAnomalies[corruptedTextIndex].Data != null)
            {
                allAnomalies[corruptedTextIndex].Data.assignedDay = targetDay;
            }

            if (replacedIndex >= 0 &&
                allAnomalies != null &&
                replacedIndex < allAnomalies.Length &&
                allAnomalies[replacedIndex] != null &&
                allAnomalies[replacedIndex].Data != null)
            {
                allAnomalies[replacedIndex].Data.assignedDay = 0;
            }
        }

        LogAssignments();
    }

    public void ActivateDayAnomaly(int day)
    {
        currentActiveAnomaly = null;

        if (dayAssignments == null || day < 1 || day > dayAssignments.Length)
        {
            return;
        }

        int anomalyIndex = dayAssignments[day - 1];
        if (anomalyIndex < 0 ||
            allAnomalies == null ||
            anomalyIndex >= allAnomalies.Length ||
            allAnomalies[anomalyIndex] == null)
        {
            return;
        }

        currentActiveAnomaly = allAnomalies[anomalyIndex];
        currentActiveAnomaly.Activate();

        if (currentActiveAnomaly.Data != null)
        {
            EventBus.OnAnomalyActivated?.Invoke(currentActiveAnomaly.Data.id);
        }
    }

    public void ResolveCurrentAnomaly()
    {
        if (currentActiveAnomaly == null)
        {
            return;
        }

        currentActiveAnomaly.Resolve();
        currentActiveAnomaly = null;
        if (GameManager.Instance != null)
            GameManager.Instance.OnDayResolved();
    }

    public void TriggerGreenOnAnomalyDay()
    {
        if (currentActiveAnomaly == null)
        {
            return;
        }

        ContaminatedBathroomAnomaly bathroomAnomaly = currentActiveAnomaly as ContaminatedBathroomAnomaly;
        CorruptedTextAnomaly textAnomaly = currentActiveAnomaly as CorruptedTextAnomaly;
        if (bathroomAnomaly != null)
        {
            bathroomAnomaly.TriggerGreenFail();
        }
        else if (textAnomaly != null)
        {
            textAnomaly.TriggerGreenFail();
        }
        else
        {
            currentActiveAnomaly.TriggerFailState();
        }
    }

    public void ResetAll()
    {
        currentActiveAnomaly = null;

        if (allAnomalies != null)
        {
            for (int i = 0; i < allAnomalies.Length; i++)
            {
                if (allAnomalies[i] != null)
                {
                    allAnomalies[i].ResetAnomaly();
                }
            }
        }

        InitialiseRun();
    }

    private void HandleDayStarted(int dayNumber)
    {
        ActivateDayAnomaly(dayNumber);
    }

    private void HandleREDPressed()
    {
        if (currentActiveAnomaly != null)
        {
            ResolveCurrentAnomaly();
            return;
        }

        EventBus.OnFalsePositive?.Invoke();
    }

    private void HandleGREENPressed()
    {
        if (currentActiveAnomaly != null)
        {
            TriggerGreenOnAnomalyDay();
            return;
        }

        EventBus.OnSafeDayCleared?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayResolved();
        }
    }
    private void HandleHealthDepleted()
    {
        if (currentActiveAnomaly != null)
        {
            currentActiveAnomaly.TriggerFailState();
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.RunStart)
        {
            ResetAll();
        }
    }

    private void ShuffleArray(int[] array)
    {
        if (array == null)
        {
            return;
        }

        for (int i = array.Length - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[swapIndex];
            array[swapIndex] = temp;
        }
    }

    private void LogAssignments()
    {
        if (dayAssignments == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < dayAssignments.Length; i++)
        {
            string anomalyName = "SAFE";
            int anomalyIndex = dayAssignments[i];

            if (anomalyIndex >= 0 &&
                allAnomalies != null &&
                anomalyIndex < allAnomalies.Length &&
                allAnomalies[anomalyIndex] != null)
            {
                if (allAnomalies[anomalyIndex].Data != null &&
                    !string.IsNullOrWhiteSpace(allAnomalies[anomalyIndex].Data.displayName))
                {
                    anomalyName = allAnomalies[anomalyIndex].Data.displayName;
                }
                else
                {
                    anomalyName = allAnomalies[anomalyIndex].name;
                }
            }

            builder.Append("Day ")
                .Append(i + 1)
                .Append(": ")
                .Append(anomalyName);

            if (i < dayAssignments.Length - 1)
            {
                builder.AppendLine();
            }
        }

        Debug.Log(builder.ToString());
    }

    //TESTING Anomaly
    public void SetTestAnomaly(AnomalyBase anomaly)
    {
        currentActiveAnomaly = anomaly;
    }

}
