using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UpgradesUI : MonoBehaviour
{
    [Header("Battery Upgrade")]
    [SerializeField] private TextMeshProUGUI batteryLevelText;
    [SerializeField] private TextMeshProUGUI batteryTimerText;
    [SerializeField] private TextMeshProUGUI ironCountText;
    [SerializeField] private TextMeshProUGUI copperCountText;
    [SerializeField] private TextMeshProUGUI sodiumCountText;
    [SerializeField] private TextMeshProUGUI ironReqText;
    [SerializeField] private TextMeshProUGUI copperReqText;
    [SerializeField] private TextMeshProUGUI sodiumReqText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private TextMeshProUGUI upgradeStatusText;

    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI rpText;
    [SerializeField] private Button backButton;
    InventorySystem inv;

    private void Start()
    {
        inv = InventorySystem.instance;
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClick);
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        yield return null;
        Refresh();
    }

    private void Refresh()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        // RP
        if (rpText != null) rpText.text = $"{gs.RP} RP";

        // Battery info
        if (batteryLevelText != null) batteryLevelText.text = $"Level {gs.BatteryLevel}";
        if (batteryTimerText != null) batteryTimerText.text = $"{gs.ExploreTime}s explore time";

        // Material counts
        int iron = inv == null ? 0 : inv.GetAmount("Iron");
        int copper = inv == null ? 0 : inv.GetAmount("Copper");
        int sodium = inv == null ? 0 : inv.GetAmount("Sodium");

        if (ironCountText != null) ironCountText.text = $"{iron}";
        if (copperCountText != null) copperCountText.text = $"{copper}";
        if (sodiumCountText != null) sodiumCountText.text = $"{sodium}";


        // Requirement colors (green if enough, red if not)
        if (ironReqText != null)
        {
            ironReqText.text = $"/ 10 Fe";
            ironReqText.color = iron >= 10 ? new Color(0.06f, 0.73f, 0.51f) : new Color(0.86f, 0.15f, 0.15f);
        }
        if (copperReqText != null)
        {
            copperReqText.text = $"/ 5 Cu";
            copperReqText.color = copper >= 5 ? new Color(0.06f, 0.73f, 0.51f) : new Color(0.86f, 0.15f, 0.15f);
        }
        if (sodiumReqText != null)
        {
            sodiumReqText.text = $"/ 5 Na";
            sodiumReqText.color = sodium >= 5 ? new Color(0.06f, 0.73f, 0.51f) : new Color(0.86f, 0.15f, 0.15f);
        }

        // Upgrade button
        bool canUpgrade = gs.CanUpgradeBattery();
        if (upgradeButton != null) upgradeButton.interactable = canUpgrade;
        if (upgradeButtonText != null) upgradeButtonText.text = canUpgrade ? "Upgrade Battery" : "Not enough materials";

        if (upgradeStatusText != null)
            upgradeStatusText.text = gs.BatteryLevel > 0
                ? $"Battery upgraded {gs.BatteryLevel} time{(gs.BatteryLevel > 1 ? "s" : "")}! +{gs.BatteryLevel * 10}s explore time"
                : "Upgrade your scanner battery to explore longer.";
    }

    private void OnUpgradeClick()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        if (gs.UpgradeBattery())
        {
            if (upgradeStatusText != null)
            {
                upgradeStatusText.text = $"Upgraded! Battery now level {gs.BatteryLevel}. Explore time: {gs.ExploreTime}s";
                upgradeStatusText.color = new Color(0.06f, 0.73f, 0.51f);
            }
            Refresh();
        }
    }
}