using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Day Display")]
    [SerializeField] private GameObject dayDisplayPanel;
    [SerializeField] private Text dayNumberText;

    [Header("State Label")]
    [SerializeField] private Text stateLabelText;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private Text winTitleText;

    [Header("Death Screen")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private Text deathResetText;

    [Header("Blackout")]
    [SerializeField] private Image blackoutImage;

    private bool isInitialised = false;

    private void Awake()
    {
        HideAllScreens();
        SetBlackout(0f);
        isInitialised = true;
    }

    private void OnEnable()
    {
        EventBus.OnGameStateChanged += HandleGameStateChanged;
        EventBus.OnDayStarted += HandleDayStarted;
        EventBus.OnPlayerDied += HandlePlayerDied;
        EventBus.OnGameWon += HandleGameWon;
    }

    private void OnDisable()
    {
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
        EventBus.OnDayStarted -= HandleDayStarted;
        EventBus.OnPlayerDied -= HandlePlayerDied;
        EventBus.OnGameWon -= HandleGameWon;
    }

    public void ShowDayDisplay(int dayNumber)
    {
        if (dayDisplayPanel != null)
        {
            dayDisplayPanel.SetActive(true);
        }

        if (dayNumberText != null)
        {
            dayNumberText.text = "Day " + dayNumber;
        }
    }

    public void HideDayDisplay()
    {
        if (dayDisplayPanel != null)
        {
            dayDisplayPanel.SetActive(false);
        }
    }

    public void ShowWinScreen()
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        if (winTitleText != null)
        {
            winTitleText.text = "THE WORLD IS SAVED";
        }

        // TODO Task 3.1: Add DOTween fade-in animation
    }

    public void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
        }

        if (deathResetText != null)
        {
            deathResetText.text = "Day 1";
        }

        // TODO Task 3.1: Add blackout hold and fade
    }

    public void HideAllScreens()
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }

        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        if (dayDisplayPanel != null)
        {
            dayDisplayPanel.SetActive(false);
        }
    }

    public void SetBlackout(float alpha)
    {
        if (blackoutImage != null)
        {
            Color color = blackoutImage.color;
            color.a = alpha;
            blackoutImage.color = color;
        }

        // Used to instantly set blackout alpha.
        // TODO Task 3.1: Replace with DOTween fade coroutine.
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (!isInitialised)
        {
            return;
        }

        if (stateLabelText != null)
        {
            stateLabelText.text = newState.ToString();
        }

        switch (newState)
        {
            case GameState.RunStart:
                HideAllScreens();
                SetBlackout(0f);
                break;
            case GameState.DayStart:
                HideAllScreens();
                break;
            case GameState.DayActive:
                HideDayDisplay();
                break;
            case GameState.DayResolved:
                break;
            case GameState.PlayerDead:
                ShowDeathScreen();
                break;
            case GameState.GameWon:
                ShowWinScreen();
                break;
            case GameState.MainMenu:
                HideAllScreens();
                break;
            default:
                break;
        }
    }

    private void HandleDayStarted(int dayNumber)
    {
        if (!isInitialised)
        {
            return;
        }

        ShowDayDisplay(dayNumber);
    }

    private void HandlePlayerDied()
    {
        if (!isInitialised)
        {
            return;
        }

        SetBlackout(1f);
        ShowDeathScreen();
    }

    private void HandleGameWon()
    {
        if (!isInitialised)
        {
            return;
        }

        SetBlackout(0f);
        ShowWinScreen();
    }
}
