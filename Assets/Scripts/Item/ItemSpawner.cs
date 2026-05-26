using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float spawnChance = 0.6f;
    [SerializeField] private float beltRadius = 3f;
    [SerializeField] private float spawnRadiusOffset = 1.5f;

    [Header("Item Weights")]
    [SerializeField] [Range(0, 1)] private float powerUpWeight = 0.4f;
    [SerializeField] [Range(0, 1)] private float elementChangeWeight = 0.35f;
    [SerializeField] [Range(0, 1)] private float trajectoryChangeWeight = 0.25f;

    private float timer;

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (Random.value <= spawnChance)
                SpawnRandomItem();
        }
    }

    private void SpawnRandomItem()
    {
        ItemEffect effect = GenerateRandomEffect();
        Vector3 position = GetSpawnPosition();

        GameObject obj = Instantiate(itemPrefab, position, Quaternion.identity);
        FloatingItem item = obj.GetComponent<FloatingItem>();
        item.Init(effect);
    }

    private ItemEffect GenerateRandomEffect()
    {
        float total = powerUpWeight + elementChangeWeight + trajectoryChangeWeight;
        float roll = Random.value * total;

        Element randomElement = (Element)Random.Range(0, 4);

        if (roll < powerUpWeight)
            return ItemEffect.CreatePowerUp(randomElement);

        roll -= powerUpWeight;
        if (roll < elementChangeWeight)
            return ItemEffect.CreateElementChange(randomElement);

        return ItemEffect.CreateTrajectoryChange();
    }

    private Vector3 GetSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float minR = beltRadius - spawnRadiusOffset;
        float maxR = beltRadius + spawnRadiusOffset;
        float r = Random.Range(minR, maxR);

        Vector3 center = GameManager.Instance.BasePosition;
        return center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * r;
    }
}
