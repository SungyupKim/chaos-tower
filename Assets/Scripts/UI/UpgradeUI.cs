using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Reward Buttons")]
    [SerializeField] private Button btnDamage;
    [SerializeField] private Button btnFireRate;
    [SerializeField] private Button btnRange;
    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnRepair;
    [SerializeField] private Button btnAddTower;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI selectedLabel;

    private Button[]     allButtons;
    private RewardType[] buttonTypes;
    private int          currentIndex;

    private static readonly Color SelectedColor   = new Color(0.9f, 0.75f, 0.1f, 1f);
    private static readonly Color UnselectedColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);

    private void Start()
    {
        allButtons  = new[] { btnDamage, btnFireRate, btnRange, btnAttack, btnRepair, btnAddTower };
        buttonTypes = new[]
        {
            RewardType.DamageBoost,
            RewardType.FireRateBoost,
            RewardType.RangeBoost,
            RewardType.AttackUpgrade,
            RewardType.RepairTower,
            RewardType.AddTower,
        };

        btnDamage?.onClick.AddListener(()   => SelectReward(RewardType.DamageBoost));
        btnFireRate?.onClick.AddListener(() => SelectReward(RewardType.FireRateBoost));
        btnRange?.onClick.AddListener(()    => SelectReward(RewardType.RangeBoost));
        btnAttack?.onClick.AddListener(()   => SelectReward(RewardType.AttackUpgrade));
        btnRepair?.onClick.AddListener(()   => SelectReward(RewardType.RepairTower));
        btnAddTower?.onClick.AddListener(() => SelectReward(RewardType.AddTower));

        RewardManager.OnQueuedRewardChanged += RefreshSelection;

        RewardType initial = RewardManager.Instance != null
            ? RewardManager.Instance.QueuedReward
            : RewardType.DamageBoost;
        RefreshSelection(initial);
    }

    private void OnDisable()
    {
        RewardManager.OnQueuedRewardChanged -= RefreshSelection;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.downArrowKey.wasPressedThisFrame)
            MoveSelection(+1);
        else if (kb.upArrowKey.wasPressedThisFrame)
            MoveSelection(-1);
    }

    private void MoveSelection(int delta)
    {
        currentIndex = (currentIndex + delta + allButtons.Length) % allButtons.Length;
        SelectReward(buttonTypes[currentIndex]);
    }

    private void SelectReward(RewardType type)
    {
        RewardManager.Instance?.SetQueuedReward(type);
    }

    private void RefreshSelection(RewardType selected)
    {
        for (int i = 0; i < allButtons.Length; i++)
        {
            if (allButtons[i] == null) continue;
            var img = allButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = (buttonTypes[i] == selected) ? SelectedColor : UnselectedColor;

            if (buttonTypes[i] == selected)
                currentIndex = i;
        }

        if (selectedLabel != null)
            selectedLabel.text = "Select: " + GetRewardLabel(selected);
    }

    private static string GetRewardLabel(RewardType type) => type switch
    {
        RewardType.DamageBoost   => "Damage +20%",
        RewardType.FireRateBoost => "Fire Rate +15%",
        RewardType.RangeBoost    => "Range +15%",
        RewardType.AttackUpgrade => "Attack Gacha",
        RewardType.RepairTower   => "Repair HP 50%",
        RewardType.AddTower      => "Add Tower",
        _                        => ""
    };
}
