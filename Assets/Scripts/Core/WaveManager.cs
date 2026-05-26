using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private WaveData[] waveDatas;

    [Header("Auto-generate waves if no WaveData")]
    [SerializeField] private int baseEnemyCount = 5;
    [SerializeField] private float enemyCountGrowth = 1.5f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float intervalDecayPerWave = 0.05f;
    [SerializeField] private float minSpawnInterval = 0.3f;

    private int activeEnemyCount;
    private bool waveInProgress;

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
        if (state == GameState.Playing)
            StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        waveInProgress = true;
        int wave = GameManager.Instance.CurrentWave;

        if (waveDatas != null && wave - 1 < waveDatas.Length)
        {
            yield return SpawnFromData(waveDatas[wave - 1]);
        }
        else
        {
            yield return SpawnProceduralWave(wave);
        }
    }

    private IEnumerator SpawnFromData(WaveData data)
    {
        foreach (var group in data.groups)
        {
            for (int i = 0; i < group.count; i++)
            {
                enemySpawner.SpawnEnemy(group.enemyData, group.element);
                activeEnemyCount++;
                yield return new WaitForSeconds(group.spawnInterval);
            }
            yield return new WaitForSeconds(data.groupDelay);
        }
    }

    private IEnumerator SpawnProceduralWave(int wave)
    {
        int count = Mathf.RoundToInt(baseEnemyCount + (wave - 1) * enemyCountGrowth);
        float interval = Mathf.Max(minSpawnInterval, spawnInterval - (wave - 1) * intervalDecayPerWave);

        for (int i = 0; i < count; i++)
        {
            Element element = (Element)Random.Range(0, 4);
            enemySpawner.SpawnProceduralEnemy(wave, element);
            activeEnemyCount++;
            yield return new WaitForSeconds(interval);
        }
    }

    public void OnEnemyDestroyed()
    {
        activeEnemyCount--;
        if (activeEnemyCount <= 0 && waveInProgress)
        {
            waveInProgress = false;
            GameManager.Instance.OnWaveCleared();
        }
    }
}
