using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResearchTreeManager : MonoBehaviour
{
    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoDescription;
    [SerializeField] private TextMeshProUGUI infoShells;
    [SerializeField] private TextMeshProUGUI infoValence;
    [SerializeField] private TextMeshProUGUI infoStatus;
    [SerializeField] private Button researchButton;
    [SerializeField] private TextMeshProUGUI researchButtonText;

    [Header("RP Display")]
    [SerializeField] private TextMeshProUGUI rpText;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private ResearchTreeNode[] allNodes;
    private ResearchTreeNode selectedNode;

    private void Start()
    {
        allNodes = GetComponentsInChildren<ResearchTreeNode>(true);
        if (infoPanel != null) infoPanel.SetActive(false);
        if (researchButton != null) researchButton.onClick.AddListener(OnResearchClick);
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        if (GameState.Instance != null)
        {
            GameState.Instance.OnElementResearched += (_) => RefreshAll();
            GameState.Instance.OnRPChanged += (_) => UpdateRP();
        }

        RefreshAll();
        UpdateRP();
    }

    public void SelectNode(ResearchTreeNode node)
    {
        if (selectedNode != null) selectedNode.SetSelected(false);
        selectedNode = node;
        node.SetSelected(true);
        ShowInfo(node.Symbol);
    }

    private void ShowInfo(string sym)
    {
        var el = GameState.Instance?.GetElement(sym);
        if (el == null || infoPanel == null) return;

        infoPanel.SetActive(true);
        infoTitle.text = $"{el.elementName} ({el.symbol})";
        infoDescription.text = el.description;
        if (infoShells != null) infoShells.text = $"Shells: {el.shellCount}";
        if (infoValence != null) infoValence.text = $"Valence e\u207B: {el.valenceElectrons}";

        bool isRes = GameState.Instance.IsResearched(sym);
        bool canRes = GameState.Instance.CanResearch(sym);

        if (infoStatus != null)
        {
            infoStatus.text = isRes ? "Researched" : "Not researched";
            infoStatus.color = isRes ? new Color(0.06f, 0.73f, 0.51f) : new Color(0.96f, 0.62f, 0.04f);
        }

        if (researchButton != null)
        {
            researchButton.gameObject.SetActive(!isRes && canRes);
            if (researchButtonText != null) researchButtonText.text = $"Research {sym}";
        }
    }

    private void OnResearchClick()
    {
        if (selectedNode == null) return;
        GameState.Instance?.Research(selectedNode.Symbol);
        ShowInfo(selectedNode.Symbol);
    }

    public void RefreshAll()
    {
        if (allNodes == null) return;
        foreach (var n in allNodes) n.Refresh();
        UpdateRP();
    }

    private void UpdateRP()
    {
        if (rpText != null && GameState.Instance != null)
            rpText.text = $"{GameState.Instance.RP} RP";
    }
}
