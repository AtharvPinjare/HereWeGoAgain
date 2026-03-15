using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int buttonPoolSize = 3;
    [SerializeField] private int safeDayCount;
    [SerializeField] private bool[] dayCompletionRecord = new bool[7];
    [SerializeField] private float dayResolvedPauseDuration = 1.5f;
    [SerializeField] private float deathScreenHoldDuration = 2.0f;

    public GameState CurrentState => currentState;
    public int CurrentDay => currentDay;
    public int ButtonPoolSize => buttonPoolSize;

    private Coroutine nextDayCoroutine;
    private Coroutine resetCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // TEMP Sprint 1 test only — remove before Sprint 3
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Return key pressed - starting game");
            StartGame();
        }
    }


    private void OnEnable()
    {
        EventBus.OnPlayerDied += OnPlayerDied;
        EventBus.OnAnomalyResolved += HandleAnomalyResolved;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerDied -= OnPlayerDied;
        EventBus.OnAnomalyResolved -= HandleAnomalyResolved;
    }

    public void StartGame()
    {
        TransitionToState(GameState.RunStart);
        BeginRun();
    }

    public void BeginRun()
    {
        StopTrackedCoroutines();

        currentDay = 1;
        buttonPoolSize = 3;

        if (dayCompletionRecord == null || dayCompletionRecord.Length != 7)
        {
            dayCompletionRecord = new bool[7];
        }
        else
        {
            for (int i = 0; i < dayCompletionRecord.Length; i++)
            {
                dayCompletionRecord[i] = false;
            }
        }

        TransitionToState(GameState.DayStart);
        EventBus.OnDayStarted?.Invoke(currentDay);
    }

    public void OnDayResolved()
    {
        if (currentDay < 1 || currentDay > 7)
        {
            return;
        }

        dayCompletionRecord[currentDay - 1] = true;
        EventBus.OnDayResolved?.Invoke(currentDay);

        if (currentDay == 7)
        {
            OnGameWon();
            return;
        }

        if (nextDayCoroutine != null)
        {
            StopCoroutine(nextDayCoroutine);
        }

        nextDayCoroutine = StartCoroutine(WaitThenNextDay());
    }

    public void OnPlayerDied()
    {
        StopTrackedCoroutines();
        TransitionToState(GameState.PlayerDead);
        resetCoroutine = StartCoroutine(WaitThenReset());
    }

    public void OnGameWon()
    {
        StopTrackedCoroutines();
        TransitionToState(GameState.GameWon);
        EventBus.OnGameWon?.Invoke();
    }

    public void ExpandButtonPool()
    {
        buttonPoolSize = Mathf.Min(buttonPoolSize + 1, 8);
    }

    public void TransitionToState(GameState newState)
    {
        currentState = newState;
        EventBus.OnGameStateChanged?.Invoke(newState);
    }

    private IEnumerator WaitThenNextDay()
    {
        yield return new WaitForSeconds(dayResolvedPauseDuration);

        currentDay = Mathf.Clamp(currentDay + 1, 1, 7);
        TransitionToState(GameState.DayStart);
        EventBus.OnDayStarted?.Invoke(currentDay);
        nextDayCoroutine = null;
    }

    private IEnumerator WaitThenReset()
    {
        yield return new WaitForSeconds(deathScreenHoldDuration);
        resetCoroutine = null;
        BeginRun();
    }

    private void HandleAnomalyResolved(string _)
    {
        ExpandButtonPool();
    }

    private void StopTrackedCoroutines()
    {
        if (nextDayCoroutine != null)
        {
            StopCoroutine(nextDayCoroutine);
            nextDayCoroutine = null;
        }

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }
    }
}
