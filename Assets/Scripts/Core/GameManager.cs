using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Ready,
    Playing,
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

    public GameState CurrentState { get; private set; }
    public int Score { get; private set; }
    public int BaseHealth { get; private set; }
    public float GameTime { get; private set; }
    public Vector3 BasePosition => baseTransform != null ? baseTransform.position : Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void Start()
    {
        BaseHealth = maxBaseHealth;
        Score = 0;
        SetState(GameState.Ready);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
            GameTime += Time.deltaTime;
    }

    public void StartGame()
    {
        BaseHealth = maxBaseHealth;
        Score = 0;
        GameTime = 0f;
        OnBaseHealthChanged?.Invoke(BaseHealth, maxBaseHealth);
        OnScoreChanged?.Invoke(Score);
        SetState(GameState.Playing);
    }

    public void DamageBase(int damage)
    {
        if (CurrentState == GameState.GameOver) return;

        BaseHealth = Mathf.Max(0, BaseHealth - damage);
        OnBaseHealthChanged?.Invoke(BaseHealth, maxBaseHealth);

        if (BaseHealth <= 0)
            SetState(GameState.GameOver);
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.GameOver);
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
