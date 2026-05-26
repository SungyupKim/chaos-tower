using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Ready,
    Playing,
    WaveComplete,
    Upgrading,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Base Settings")]
    [SerializeField] private int maxBaseHealth = 20;
    [SerializeField] private Transform baseTransform;

    [Header("Events")]
    public UnityEvent<GameState> OnGameStateChanged;
    public UnityEvent<int, int> OnBaseHealthChanged;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnWaveChanged;

    public GameState CurrentState { get; private set; }
    public int CurrentWave { get; private set; }
    public int Score { get; private set; }
    public int BaseHealth { get; private set; }
    public Vector3 BasePosition => baseTransform != null ? baseTransform.position : Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        BaseHealth = maxBaseHealth;
        CurrentWave = 0;
        Score = 0;
        SetState(GameState.Ready);
    }

    public void StartGame()
    {
        BaseHealth = maxBaseHealth;
        CurrentWave = 0;
        Score = 0;
        OnBaseHealthChanged?.Invoke(BaseHealth, maxBaseHealth);
        OnScoreChanged?.Invoke(Score);
        StartNextWave();
    }

    public void StartNextWave()
    {
        CurrentWave++;
        OnWaveChanged?.Invoke(CurrentWave);
        SetState(GameState.Playing);
    }

    public void OnWaveCleared()
    {
        SetState(GameState.WaveComplete);
        Invoke(nameof(EnterUpgrade), 1.5f);
    }

    private void EnterUpgrade()
    {
        SetState(GameState.Upgrading);
    }

    public void OnUpgradeComplete()
    {
        StartNextWave();
    }

    public void DamageBase(int damage)
    {
        if (CurrentState == GameState.GameOver) return;

        BaseHealth = Mathf.Max(0, BaseHealth - damage);
        OnBaseHealthChanged?.Invoke(BaseHealth, maxBaseHealth);

        if (BaseHealth <= 0)
        {
            SetState(GameState.GameOver);
        }
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
