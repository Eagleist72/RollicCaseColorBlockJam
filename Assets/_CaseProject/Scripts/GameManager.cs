using System;
using UnityEngine;

/// <summary>
/// Core Singleton manager that controls the primary game loop and state machine.
/// Orchestrates interactions between the Grid, Block, and Sorting managers.
/// Handles level progression (saving/loading via PlayerPrefs), timer mechanics, and win/loss evaluations.
/// </summary>
public enum GameState
{
    StartScreen,
    Playing,
    LevelComplete,
    GameOver,
    GameComplete
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private SortingManager sortingManager;

    [Header("Level Progression")]
    [Tooltip("Drag and drop all LevelData objects here in sequential order.")]
    [SerializeField] private LevelData[] allLevels;
    public int currentLevelIndex = 0;

    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnTimerUpdated;
    public event Action<int> OnLevelStarted;

    public GameState CurrentState { get; private set; }

    private float _currentTimer;
    private LevelData _activeLevelData;
    private bool _isTimerStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

#if UNITY_STANDALONE_WIN
        Screen.SetResolution(540, 960, false);
#endif
    }

    private void Start()
    {
        currentLevelIndex = PlayerPrefs.GetInt("SavedLevel", 0);
        ChangeState(GameState.StartScreen);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            currentLevelIndex = 0;
            Debug.Log("<color=cyan>Save data cleared! Game will start from index 0.</color>");
            ChangeState(GameState.StartScreen);
        }

        if (CurrentState == GameState.Playing)
        {
            HandleGameplayTimer();
        }
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                InitializeGameplay();
                OnLevelStarted?.Invoke(currentLevelIndex + 1);
                break;
            case GameState.StartScreen:
                CleanUpScene();
                break;
            case GameState.LevelComplete:
                currentLevelIndex++;
                PlayerPrefs.SetInt("SavedLevel", currentLevelIndex);
                PlayerPrefs.Save();
                break;
            case GameState.GameOver:
                Debug.LogWarning("[GameManager] Game Over triggered.");
                break;
        }
    }

    private void InitializeGameplay()
    {
        _isTimerStarted = false;

        if (allLevels == null || allLevels.Length == 0)
        {
            Debug.LogError("[GameManager] CRITICAL ERROR: All Levels array is completely empty!");
            return;
        }

        if (currentLevelIndex >= allLevels.Length) currentLevelIndex = 0;

        _activeLevelData = allLevels[currentLevelIndex];

        if (_activeLevelData == null) return;

        CleanUpScene();

        InitializeLevelTimer(_activeLevelData.EditableTimer);
        gridManager.GenerateGrid(_activeLevelData);
        blockManager.SpawnBlocksForLevel(_activeLevelData);
    }

    private void CleanUpScene()
    {
        if (gridManager != null) gridManager.ClearGrid();
        if (blockManager != null) blockManager.ClearBlocks();
        if (sortingManager != null) sortingManager.ClearDock();
    }

    private void HandleGameplayTimer()
    {
        if (!_isTimerStarted) return;
        _currentTimer -= Time.deltaTime;
        OnTimerUpdated?.Invoke(_currentTimer);

        if (_currentTimer <= 0f)
        {
            _currentTimer = 0f;
            ChangeState(GameState.GameOver);
        }
    }

    public void StartGame() => ChangeState(GameState.Playing);
    public void NextLevel() => ChangeState(GameState.Playing);
    public void RestartLevel() => ChangeState(GameState.Playing);
    public void ReturnToMenu() => ChangeState(GameState.StartScreen);
    private void InitializeLevelTimer(float time)
    {
        _currentTimer = time;
        OnTimerUpdated?.Invoke(_currentTimer);
    }
    public void StartTimer() => _isTimerStarted = true;

    public void CheckWinCondition()
    {
        if (CurrentState != GameState.Playing) return;

        Debug.Log($"<color=yellow>[WIN CHECK]</color> Blocks on Field: {blockManager.ActiveBlockCount}");

        if (blockManager.ActiveBlockCount == 0)
        {
            Debug.Log("<color=green>[WIN TRIGGERED!] Field completely cleared, calling UI screen!</color>");
            ChangeState(GameState.LevelComplete);
        }

        if (blockManager.ActiveBlockCount == 0)
        {
            if (currentLevelIndex >= allLevels.Length - 1)
            {
                Debug.Log("<color=gold>[GAME COMPLETE]</color> Player finished all level!");
                ChangeState(GameState.GameComplete);
            }
            else
            {
                ChangeState(GameState.LevelComplete);
            }
        }
    }

    public void RestartEntireGame()
    {
        currentLevelIndex = 0;
        PlayerPrefs.SetInt("SavedLevel", 0);
        PlayerPrefs.Save();
        ChangeState(GameState.Playing);
    }
}