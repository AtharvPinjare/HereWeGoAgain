using System;

/// <summary>
/// All callers must use null-conditional invocation: EventBus.OnEvent?.Invoke()
/// </summary>
public static class EventBus
{
    // Game State
    public static Action<GameState> OnGameStateChanged;
    public static Action<int> OnDayStarted;
    public static Action<int> OnDayResolved;

    // Anomaly
    public static Action<string> OnAnomalyActivated;
    public static Action<string> OnAnomalyResolved;
    public static Action<string> OnAnomalyFailState;

    // Button
    public static Action OnREDButtonPressed;
    public static Action OnGREENButtonPressed;
    public static Action OnSafeDayCleared;
    public static Action OnFalsePositive;

    // Player
    public static Action OnPlayerHit;
    public static Action OnPlayerDied;
    public static Action OnInputLocked;
    public static Action OnInputUnlocked;

    // Run
    public static Action OnRunStarted;
    public static Action OnGameWon;
}
