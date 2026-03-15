using System.Collections;
using UnityEngine;

public class FalsePositiveController : MonoBehaviour
{
    [SerializeField] private GameObject devilPrefab;
    [SerializeField] private Transform devilSpawnPoint;
    [SerializeField] private float silenceDuration = 5.0f;
    [SerializeField] private float devilMoveSpeed = 3.5f;

    private GameObject activeDevil = null;
    private Coroutine falsePositiveCoroutine = null;
    private bool sequenceActive = false;

    private void OnEnable()
    {
        EventBus.OnFalsePositive += HandleFalsePositive;
        EventBus.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        EventBus.OnFalsePositive -= HandleFalsePositive;
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
    }

    private IEnumerator FalsePositiveSequence()
    {
        EventBus.OnInputLocked?.Invoke();

        yield return new WaitForSeconds(silenceDuration);

        if (devilPrefab != null && devilSpawnPoint != null)
        {
            activeDevil = Instantiate(devilPrefab, devilSpawnPoint.position, devilSpawnPoint.rotation);
        }

        Transform playerTransform = null;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        while (activeDevil != null && playerTransform != null)
        {
            activeDevil.transform.position = Vector3.MoveTowards(
                activeDevil.transform.position,
                playerTransform.position,
                devilMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(activeDevil.transform.position, playerTransform.position) <= 0.5f)
            {
                break;
            }

            yield return null;
        }

        if (activeDevil != null)
        {
            Destroy(activeDevil);
            activeDevil = null;
        }

        falsePositiveCoroutine = null;
        EventBus.OnPlayerDied?.Invoke();
        sequenceActive = false;
    }

    private void HandleFalsePositive()
    {
        if (sequenceActive)
        {
            return;
        }

        sequenceActive = true;

        if (falsePositiveCoroutine != null)
        {
            StopCoroutine(falsePositiveCoroutine);
        }

        falsePositiveCoroutine = StartCoroutine(FalsePositiveSequence());
    }

    private void HandleRunStarted()
    {
        if (falsePositiveCoroutine != null)
        {
            StopCoroutine(falsePositiveCoroutine);
            falsePositiveCoroutine = null;
        }

        if (activeDevil != null)
        {
            Destroy(activeDevil);
            activeDevil = null;
        }

        sequenceActive = false;
        EventBus.OnInputUnlocked?.Invoke();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.RunStart)
        {
            HandleRunStarted();
        }
    }
}
