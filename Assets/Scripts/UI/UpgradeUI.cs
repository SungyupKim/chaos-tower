using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button addTowerBtn;

    [Header("Settings")]
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private ConveyorBelt conveyorBelt;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

        addTowerBtn?.onClick.AddListener(AddTower);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged.RemoveListener(OnGameStateChanged);

        addTowerBtn?.onClick.RemoveAllListeners();
    }

    private void OnGameStateChanged(GameState state)
    {
        upgradePanel?.SetActive(state == GameState.Upgrading);
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
