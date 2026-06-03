using UnityEngine;

// Renamed role: no longer manages waves, just spawns enemies continuously.
public class WaveManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Spawn Settings")]
    [SerializeField] private float initialInterval = 2.5f;
    [SerializeField] private float minInterval = 0.4f;
    [SerializeField] private float intervalDecayRate = 0.008f; // per second elapsed

    private bool spawning;
    private float gameTimer;
    private float spawnTimer;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameState state)
    {
        spawning = (state == GameState.Playing);
        if (spawning)
        {
            gameTimer = 0f;
            spawnTimer = 0f;
        }
    }

    private void Update()
    {
        if (!spawning) return;

        gameTimer += Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            float interval = Mathf.Max(minInterval, initialInterval - gameTimer * intervalDecayRate);
            spawnTimer = interval;
        }
    }

    private void SpawnEnemy()
    {
        if (enemySpawner == null) return;
        Element element = (Element)Random.Range(0, 4);
        int difficulty = Mathf.Max(1, (int)(gameTimer / 30f) + 1);
        enemySpawner.SpawnProceduralEnemy(difficulty, element);
    }

    // Kept so EnemySpawner's existing reference compiles
    public void OnEnemyDestroyed() { }
}
