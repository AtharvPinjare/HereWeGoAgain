using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private Transform[] redButtonPositions;
    [SerializeField] private Transform greenButtonTransform;
    [SerializeField] private GameObject redButtonObject;
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private GameObject playerObject;

    private int currentPoolSize = 3;
    private bool isListening = false;

    private void OnEnable()
    {
        EventBus.OnGameStateChanged += HandleGameStateChanged;
        EventBus.OnAnomalyResolved += HandleAnomalyResolved;
        EventBus.OnDayStarted += HandleDayStarted;
    }

    private void OnDisable()
    {
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
        EventBus.OnAnomalyResolved -= HandleAnomalyResolved;
        EventBus.OnDayStarted -= HandleDayStarted;
    }

    public void InitialisePool()
    {
        currentPoolSize = 3;
    }

    public void ExpandPool()
    {
        currentPoolSize = Mathf.Min(currentPoolSize + 1, 8);
    }

    public void PlaceButtonForDay()
    {
        if (redButtonObject == null || redButtonPositions == null || redButtonPositions.Length == 0)
        {
            return;
        }

        int selectableCount = Mathf.Clamp(currentPoolSize, 1, redButtonPositions.Length);
        int randomIndex = Random.Range(0, selectableCount);
        Transform selectedAnchor = redButtonPositions[randomIndex];

        if (selectedAnchor == null)
        {
            return;
        }

        Transform redTransform = redButtonObject.transform;
        redTransform.position = selectedAnchor.position;
        redTransform.rotation = selectedAnchor.rotation;
    }

    private void Update()
    {
        if (!isListening)
        {
            return;
        }

        if (playerObject == null || redButtonObject == null || greenButtonTransform == null)
        {
            return;
        }

        float redDist = Vector3.Distance(
            playerObject.transform.position,
            redButtonObject.transform.position);
        float greenDist = Vector3.Distance(
            playerObject.transform.position,
            greenButtonTransform.position);

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("red Dist: " + redDist + ", greenDist: " + greenDist);
            if (redDist <= interactionRadius)
            {
                Debug.Log("RED button pressed");
                OnREDPressed();
            }
            else if (greenDist <= interactionRadius)
            {
                Debug.Log("GREEN button pressed");
                OnGREENPressed();
            }
        }
    }

    private void OnREDPressed()
    {
        EventBus.OnREDButtonPressed?.Invoke();
    }

    private void OnGREENPressed()
    {
        EventBus.OnGREENButtonPressed?.Invoke();
    }

    private void HandleGameStateChanged(GameState newState)
    {
        isListening = newState == GameState.DayActive;
    }

    private void HandleAnomalyResolved(string anomalyId)
    {
        ExpandPool();
    }

    private void HandleDayStarted(int dayNumber)
    {
        PlaceButtonForDay();
    }
}
