using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;

    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalTimeText;

    [Header("Level Up Notification")]
    [SerializeField] private TextMeshProUGUI levelUpText;

    private float levelUpDisplayTimer;
    private const float LevelUpDisplayDuration = 2.5f;

    private void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null) { Debug.LogError("[GameUI] GameManager.Instance is null!"); return; }

        gm.OnGameStateChanged.AddListener(OnGameStateChanged);
        gm.OnBaseHealthChanged.AddListener(OnHealthChanged);
        gm.OnScoreChanged.AddListener(OnScoreChanged);

        RewardManager.OnRewardApplied += OnRewardApplied;

        if (startButton == null)   startButton   = FindButtonByName("StartButton");
        if (restartButton == null) restartButton = FindButtonByName("RestartButton");

        startButton?.onClick.AddListener(OnStartButtonClicked);
        restartButton?.onClick.AddListener(OnRestartButtonClicked);

        if (levelUpText != null) levelUpText.gameObject.SetActive(false);

        OnGameStateChanged(gm.CurrentState);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            GameManager.Instance.OnBaseHealthChanged.RemoveListener(OnHealthChanged);
            GameManager.Instance.OnScoreChanged.RemoveListener(OnScoreChanged);
        }
        RewardManager.OnRewardApplied -= OnRewardApplied;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            float t = GameManager.Instance.GameTime;
            int min = (int)(t / 60f);
            int sec = (int)(t % 60f);
            if (timeText != null) timeText.text = $"{min:00}:{sec:00}";
        }

        if (levelUpDisplayTimer > 0f)
        {
            levelUpDisplayTimer -= Time.deltaTime;
            if (levelUpDisplayTimer <= 0f && levelUpText != null)
                levelUpText.gameObject.SetActive(false);
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        startPanel?.SetActive(state == GameState.Ready);
        hudPanel?.SetActive(state == GameState.Playing);
        gameOverPanel?.SetActive(state == GameState.GameOver);

        if (state == GameState.GameOver)
        {
            float t = GameManager.Instance.GameTime;
            int min = (int)(t / 60f);
            int sec = (int)(t % 60f);
            if (finalScoreText != null) finalScoreText.text = $"Score: {GameManager.Instance.Score}";
            if (finalTimeText != null)  finalTimeText.text  = $"Survived: {min:00}:{sec:00}";
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthSlider != null) { healthSlider.maxValue = max; healthSlider.value = current; }
        if (healthText   != null) healthText.text = $"{current}/{max}";
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void OnRewardApplied(Tower tower, string message)
    {
        if (levelUpText == null) return;
        levelUpText.gameObject.SetActive(true);
        levelUpText.text = $"{tower.gameObject.name}  Lv.{tower.Level}  {message}";
        levelUpDisplayTimer = LevelUpDisplayDuration;
    }

    public void OnStartButtonClicked()   => GameManager.Instance.StartGame();
    public void OnRestartButtonClicked() => GameManager.Instance.StartGame();

    private Button FindButtonByName(string n)
    {
        foreach (var btn in FindObjectsOfType<Button>(true))
            if (btn.gameObject.name == n) return btn;
        return null;
    }
}
