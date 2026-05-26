using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button boostFireBtn;
    [SerializeField] private Button boostWaterBtn;
    [SerializeField] private Button boostLightningBtn;
    [SerializeField] private Button boostEarthBtn;
    [SerializeField] private Button addTowerBtn;

    [Header("Settings")]
    [SerializeField] private float damageBoostAmount = 0.3f;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private ConveyorBelt conveyorBelt;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

        boostFireBtn?.onClick.AddListener(() => BoostElement(Element.Fire));
        boostWaterBtn?.onClick.AddListener(() => BoostElement(Element.Water));
        boostLightningBtn?.onClick.AddListener(() => BoostElement(Element.Lightning));
        boostEarthBtn?.onClick.AddListener(() => BoostElement(Element.Earth));
        addTowerBtn?.onClick.AddListener(AddTower);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);

        boostFireBtn?.onClick.RemoveAllListeners();
        boostWaterBtn?.onClick.RemoveAllListeners();
        boostLightningBtn?.onClick.RemoveAllListeners();
        boostEarthBtn?.onClick.RemoveAllListeners();
        addTowerBtn?.onClick.RemoveAllListeners();
    }

    private void OnGameStateChanged(GameState state)
    {
        upgradePanel?.SetActive(state == GameState.Upgrading);
    }

    private void BoostElement(Element element)
    {
        Tower[] towers = FindObjectsOfType<Tower>();
        foreach (var tower in towers)
        {
            if (tower.CurrentElement == element)
                tower.BoostDamage(damageBoostAmount);
        }
        FinishUpgrade();
    }

    private void AddTower()
    {
        if (towerPrefab == null || conveyorBelt == null) return;

        Element randomElement = (Element)Random.Range(0, 4);
        GameObject obj = Instantiate(towerPrefab, Vector3.zero, Quaternion.identity);
        Tower tower = obj.GetComponent<Tower>();
        tower.SetElement(randomElement);
        conveyorBelt.RegisterTower(tower);
        FinishUpgrade();
    }

    private void FinishUpgrade()
    {
        GameManager.Instance.OnUpgradeComplete();
    }
}
