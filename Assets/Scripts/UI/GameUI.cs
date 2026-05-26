using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject waveCompletePanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;

    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalWaveText;

    private void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("[GameUI] GameManager.Instance is null!");
            return;
        }
        gm.OnGameStateChanged.AddListener(OnGameStateChanged);
        gm.OnBaseHealthChanged.AddListener(OnHealthChanged);
        gm.OnScoreChanged.AddListener(OnScoreChanged);
        gm.OnWaveChanged.AddListener(OnWaveChanged);

        if (startButton == null)
            startButton = FindButtonByName("StartButton");
        if (restartButton == null)
            restartButton = FindButtonByName("RestartButton");

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        else
            Debug.LogError("[GameUI] StartButton not found!");

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        OnGameStateChanged(gm.CurrentState);
    }

    private Button FindButtonByName(string name)
    {
        foreach (var btn in FindObjectsOfType<Button>(true))
        {
            if (btn.gameObject.name == name)
                return btn;
        }
        return null;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        var gm = GameManager.Instance;
        gm.OnGameStateChanged.RemoveListener(OnGameStateChanged);
        gm.OnBaseHealthChanged.RemoveListener(OnHealthChanged);
        gm.OnScoreChanged.RemoveListener(OnScoreChanged);
        gm.OnWaveChanged.RemoveListener(OnWaveChanged);
    }

    private void OnGameStateChanged(GameState state)
    {
        startPanel?.SetActive(state == GameState.Ready);
        hudPanel?.SetActive(state == GameState.Playing || state == GameState.WaveComplete);
        gameOverPanel?.SetActive(state == GameState.GameOver);
        waveCompletePanel?.SetActive(state == GameState.WaveComplete);

        if (state == GameState.GameOver)
        {
            if (finalScoreText != null) finalScoreText.text = $"Score: {GameManager.Instance.Score}";
            if (finalWaveText != null) finalWaveText.text = $"Wave: {GameManager.Instance.CurrentWave}";
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null) healthText.text = $"{current}/{max}";
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void OnWaveChanged(int wave)
    {
        if (waveText != null) waveText.text = $"Wave {wave}";
    }

    public void OnStartButtonClicked()
    {
        GameManager.Instance.StartGame();
    }

    public void OnRestartButtonClicked()
    {
        GameManager.Instance.StartGame();
    }
}
