using UnityEngine;
using System;
using URandom = UnityEngine.Random;

public enum RewardType
{
    DamageBoost,
    FireRateBoost,
    RangeBoost,
    AttackUpgrade,
    RepairTower,
    AddTower,
}

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private ConveyorBelt conveyorBelt;

    private int initialTowerCount = 3;

    public RewardType QueuedReward { get; private set; } = RewardType.DamageBoost;

    public static event Action<Tower, string> OnRewardApplied;
    public static event Action<RewardType> OnQueuedRewardChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        Tower.OnTowerLevelUp += HandleTowerLevelUp;
    }

    private void OnDisable()
    {
        Tower.OnTowerLevelUp -= HandleTowerLevelUp;
    }

    public void SetInitialTowerCount(int count) => initialTowerCount = count;

    public void SetQueuedReward(RewardType type)
    {
        QueuedReward = type;
        OnQueuedRewardChanged?.Invoke(type);
    }

    private void HandleTowerLevelUp(Tower tower)
    {
        string message = ApplyReward(tower, QueuedReward);
        OnRewardApplied?.Invoke(tower, message);
    }

    private string ApplyReward(Tower tower, RewardType type)
    {
        switch (type)
        {
            case RewardType.DamageBoost:
                tower.BoostDamage(0.2f);
                return "Damage +20%";

            case RewardType.FireRateBoost:
                tower.BoostFireRate(0.15f);
                return "Fire Rate +15%";

            case RewardType.RangeBoost:
                tower.BoostRange(0.15f);
                return "Range +15%";

            case RewardType.AttackUpgrade:
                var mode = AttackMode.RollGacha();
                tower.SetAttackMode(mode);
                return $"{mode.GetRarityTag()} {mode.displayName}";

            case RewardType.RepairTower:
                tower.RepairHP(0.5f);
                return "Repair HP 50%";

            case RewardType.AddTower:
                int current = FindObjectsOfType<Tower>().Length;
                float prob = 0.5f * Mathf.Pow(0.5f, current - initialTowerCount);
                if (URandom.value < prob)
                {
                    SpawnNewTower();
                    return $"+Tower! ({prob * 100f:F0}% chance)";
                }
                else
                {
                    tower.BoostDamage(0.2f);
                    return $"+Tower failed ({prob * 100f:F0}%) -> Damage +20%";
                }

            default:
                return "";
        }
    }

    private void SpawnNewTower()
    {
        if (towerPrefab == null || conveyorBelt == null) return;
        Element element = (Element)URandom.Range(0, 4);
        GameObject obj = Instantiate(towerPrefab, Vector3.zero, Quaternion.identity);
        Tower t = obj.GetComponent<Tower>();
        if (t != null) t.SetElement(element);
        conveyorBelt.RegisterTower(t);
    }
}
