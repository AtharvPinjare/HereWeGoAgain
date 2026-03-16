using UnityEngine;

public class ClockIdleBehaviour : MonoBehaviour
{
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private float minuteHandDegreesPerSecond = 0.1f;
    [SerializeField] private float hourHandDegreesPerSecond = 0.00833f;
    [SerializeField] public bool isOverriddenByAnomaly = false;

    private void Update()
    {
        if (isOverriddenByAnomaly)
        {
            return;
        }

        if (hourHand != null)
        {
            hourHand.Rotate(Vector3.forward, hourHandDegreesPerSecond * Time.deltaTime, Space.Self);
        }

        if (minuteHand != null)
        {
            minuteHand.Rotate(Vector3.forward, minuteHandDegreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
