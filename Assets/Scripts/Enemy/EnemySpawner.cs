using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnDistance = 12f;

    private WaveManager waveManager;

    private void Awake()
    {
        waveManager = GetComponent<WaveManager>();
    }

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
    }

    public void SpawnEnemy(EnemyData data, Element element)
    {
        Vector3 pos = GetRandomEdgePosition();
        GameObject obj = Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.Init(data, element);
    }

    public void SpawnProceduralEnemy(int wave, Element element)
    {
        Vector3 pos = GetRandomEdgePosition();
        GameObject obj = Instantiate(enemyPrefab, pos, Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.InitProcedural(wave, element);
    }

    private Vector3 GetRandomEdgePosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 basePos = GameManager.Instance.BasePosition;
        return basePos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnDistance;
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        if (waveManager != null)
            waveManager.OnEnemyDestroyed();
    }
}
