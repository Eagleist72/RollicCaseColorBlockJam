using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Centralized manager for all User Interface elements and screen transitions.
/// Subscribes to GameManager events to dynamically update the HUD (timer, level indicator) 
/// and configures end-game screens. Utilizes CanvasGroup modifications for 
/// highly optimized, allocation-free UI toggling without triggering expensive Canvas rebuilds.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Screens (Canvas Groups)")]
    [SerializeField] private CanvasGroup startScreen;
    [SerializeField] private CanvasGroup playScreen;
    [SerializeField] private CanvasGroup endGameScreen;
    [SerializeField] private CanvasGroup gameCompleteScreen;

    [Header("Play Screen UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelIndicatorText;

    [Header("End Screen UI")]
    [SerializeField] private TextMeshProUGUI endConditionText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    [Header("Game Complete Screen UI")]
    [SerializeField] private TextMeshProUGUI ggWpText;
    [SerializeField] private Button restartAllButton;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[UIManager] CRITICAL ERROR: GameManager Instance not found!");
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChange;
        GameManager.Instance.OnTimerUpdated += UpdateTimerUI;
        GameManager.Instance.OnLevelStarted += UpdateLevelIndicator;

        HandleGameStateChange(GameManager.Instance.CurrentState);
    }

    private void HandleGameStateChange(GameState newState)
    {
        ToggleScreen(startScreen, newState == GameState.StartScreen);
        ToggleScreen(playScreen, newState == GameState.Playing);
        ToggleScreen(endGameScreen, newState == GameState.GameOver || newState == GameState.LevelComplete);
        ToggleScreen(gameCompleteScreen, newState == GameState.GameComplete);

        if (newState == GameState.GameOver)
        {
            endConditionText.text = "TIME'S UP!";
            SetupActionButton("RETRY", () => GameManager.Instance.RestartLevel());
        }
        else if (newState == GameState.LevelComplete)
        {
            endConditionText.text = "LEVEL COMPLETE!";
            SetupActionButton("NEXT LEVEL", () => GameManager.Instance.NextLevel());
        }

        if (newState == GameState.GameComplete)
        {
            ggWpText.text = "GG WP!\nALL LEVELS CLEARED";
            restartAllButton.onClick.RemoveAllListeners();
            restartAllButton.onClick.AddListener(() => GameManager.Instance.RestartEntireGame());
        }
    }

    private void SetupActionButton(string text, UnityEngine.Events.UnityAction action)
    {
        if (actionButton != null)
        {
            if (actionButtonText != null) actionButtonText.text = text;

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(action);
        }
    }

    private void UpdateTimerUI(float currentTime)
    {
        timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    public void UpdateLevelIndicator(int currentLevelNumber)
    {
        levelIndicatorText.text = $"LEVEL {currentLevelNumber}";
    }

    private void ToggleScreen(CanvasGroup screen, bool isVisible)
    {
        if (screen == null) return;
        screen.alpha = isVisible ? 1f : 0f;
        screen.interactable = isVisible;
        screen.blocksRaycasts = isVisible;
    }

    public void OnStartButtonPressed()
    {
        GameManager.Instance.StartGame();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChange;
            GameManager.Instance.OnTimerUpdated -= UpdateTimerUI;
            GameManager.Instance.OnLevelStarted -= UpdateLevelIndicator;
        }
    }
}