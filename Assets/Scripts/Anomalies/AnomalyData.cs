using UnityEngine;

[System.Serializable]
public class AnomalyData
{
    public string id;
    public AnomalyType type;
    public string displayName;
    public int assignedDay;
    public bool isActive;
    public bool isResolved;
    [Range(0.1f, 3.0f)] public float escalationRate = 1.0f;
    public int hitCountToKill = 2;
}
