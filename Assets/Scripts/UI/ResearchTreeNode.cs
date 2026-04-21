using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ResearchTreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string elementSymbol;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;

    [SerializeField] private Color lockedColor = new Color(0.17f, 0.15f, 0.22f, 1f);
    [SerializeField] private Color availableColor = new Color(0.55f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.13f, 0.83f, 0.93f, 1f);

    private Button button;
    private Color researchedColor;
    private string currentState = "locked";

    public string Symbol => elementSymbol;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void Setup(string sym, Color elemColor)
    {
        elementSymbol = sym;
        researchedColor = elemColor;
        Refresh();
    }

    public void Refresh()
    {
        var gs = GameState.Instance;
        if (gs == null) return;

        if (gs.IsResearched(elementSymbol))
        {
            currentState = "researched";
            background.color = researchedColor;
            label.text = elementSymbol;
            label.color = Color.black;
            button.interactable = true;
        }
        else if (gs.CanResearch(elementSymbol))
        {
            currentState = "available";
            background.color = availableColor;
            label.text = elementSymbol;
            label.color = Color.white;
            button.interactable = true;
        }
        else
        {
            currentState = "locked";
            background.color = lockedColor;
            label.text = "?";
            label.color = new Color(1, 1, 1, 0.3f);
            button.interactable = false;
        }
    }

    public void SetSelected(bool sel)
    {
        if (sel && currentState != "locked")
            background.color = selectedColor;
        else
            Refresh();
    }

    private void OnClick()
    {
        var tree = GetComponentInParent<ResearchTreeManager>();
        if (tree != null) tree.SelectNode(this);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (currentState != "locked")
            background.color = Color.Lerp(background.color, Color.white, 0.15f);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (currentState != "locked") Refresh();
    }
}
