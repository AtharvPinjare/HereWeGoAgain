using UnityEngine;

public class EntranceTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.CurrentState != GameState.DayStart)
        {
            return;
        }

        GameManager.Instance.TransitionToState(GameState.DayActive);
    }
}
