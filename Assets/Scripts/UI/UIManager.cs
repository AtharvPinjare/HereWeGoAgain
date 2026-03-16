using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Day Display")]
    [SerializeField] private GameObject dayDisplayPanel;
    [SerializeField] private Text dayNumberText;
    [SerializeField] private float dayDisplayDuration = 2.5f;
    [SerializeField] private CanvasGroup dayDisplayCanvasGroup;

    [Header("State Label")]
    [SerializeField] private Text stateLabelText;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private Text winTitleText;
    [SerializeField] private CanvasGroup winScreenCanvasGroup;

    [Header("Death Screen")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private Text deathResetText;
    [SerializeField] private CanvasGroup deathScreenCanvasGroup;

    [Header("Blackout")]
    [SerializeField] private Image blackoutImage;
    [SerializeField] private AudioSource deathStingAudio;

    private Coroutine dayDisplayCoroutine = null;
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
        if (dayDisplayPanel == null)
        {
            return;
        }

        if (dayDisplayCoroutine != null)
        {
            StopCoroutine(dayDisplayCoroutine);
        }

        dayDisplayCoroutine = StartCoroutine(DayDisplaySequence(dayNumber));
    }

    public void ShowWinScreen()
    {
        HideAllScreens();

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        if (winTitleText != null)
        {
            winTitleText.text = "THE WORLD IS SAVED";
        }

        if (winScreenCanvasGroup != null)
        {
            winScreenCanvasGroup.alpha = 0f;
            winScreenCanvasGroup.DOFade(1f, 1.5f);
        }
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

        if (deathScreenCanvasGroup != null)
        {
            deathScreenCanvasGroup.alpha = 0f;
            deathScreenCanvasGroup.DOFade(1f, 0.5f);
        }
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
        if (blackoutImage == null)
        {
            return;
        }

        Color color = blackoutImage.color;
        color.a = alpha;
        blackoutImage.color = color;
    }

    public void FadeBlackout(float targetAlpha, float duration, Action onComplete = null)
    {
        if (blackoutImage == null)
        {
            onComplete?.Invoke();
            return;
        }

        blackoutImage.DOFade(targetAlpha, duration)
            .OnComplete(() => onComplete?.Invoke());
    }

    private IEnumerator DayDisplaySequence(int dayNumber)
    {
        if (dayDisplayCanvasGroup != null)
        {
            dayDisplayCanvasGroup.alpha = 0f;
        }

        if (dayNumberText != null)
        {
            dayNumberText.text = "Day " + dayNumber;
        }

        if (dayDisplayPanel != null)
        {
            dayDisplayPanel.SetActive(true);
        }

        if (dayDisplayCanvasGroup != null)
        {
            dayDisplayCanvasGroup.DOFade(1f, 0.5f);
        }

        yield return new WaitForSeconds(dayDisplayDuration);

        if (dayDisplayCanvasGroup != null)
        {
            yield return dayDisplayCanvasGroup.DOFade(0f, 0.5f).WaitForCompletion();
        }

        if (dayDisplayPanel != null)
        {
            dayDisplayPanel.SetActive(false);
        }

        dayDisplayCoroutine = null;
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
                FadeBlackout(0f, 0.5f);
                break;
            case GameState.DayStart:
                HideAllScreens();
                break;
            case GameState.PlayerDead:
                ShowDeathScreen();
                break;
            case GameState.GameWon:
                FadeBlackout(0f, 1.0f, ShowWinScreen);
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
        if (deathStingAudio != null) deathStingAudio.Play();

        if (!isInitialised)
        {
            return;
        }

        FadeBlackout(1f, 0.3f);
    }

    private void HandleGameWon()
    {
        if (!isInitialised)
        {
            return;
        }

        FadeBlackout(0f, 1.0f, ShowWinScreen);
    }
}
