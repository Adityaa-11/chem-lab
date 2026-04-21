using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PeriodicTableUI : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject cellPrefab;

    [Header("Popup")]
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI popupTitle;
    [SerializeField] private TextMeshProUGUI popupDesc;
    [SerializeField] private TextMeshProUGUI popupShells;
    [SerializeField] private TextMeshProUGUI popupValence;
    [SerializeField] private Button popupClose;

    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI rpText;
    [SerializeField] private Button backButton;

    private static readonly int[,] layout = {
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
        {3,4,0,0,0,0,0,0,0,0,0,0,5,6,7,8,9,10},
        {11,12,0,0,0,0,0,0,0,0,0,0,13,14,15,16,17,18},
        {19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36},
        {37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54},
        {55,56,0,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86},
        {87,88,0,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118},
    };

    private static readonly string[] symbols = {
        "","H","He","Li","Be","B","C","N","O","F","Ne",
        "Na","Mg","Al","Si","P","S","Cl","Ar",
        "K","Ca","Sc","Ti","V","Cr","Mn","Fe","Co","Ni","Cu","Zn","Ga","Ge","As","Se","Br","Kr",
        "Rb","Sr","Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn","Sb","Te","I","Xe",
        "Cs","Ba","","","","","","","","","","","","","","","","",
        "","","","Hf","Ta","W","Re","Os","Ir","Pt","Au","Hg","Tl","Pb","Bi","Po","At","Rn",
        "Fr","Ra","","","","","","","","","","","","","","","","",
        "","","","Rf","Db","Sg","Bh","Hs","Mt","Ds","Rg","Cn","Nh","Fl","Mc","Lv","Ts","Og"
    };

    private void Start()
    {
        if (popup != null) popup.SetActive(false);
        if (popupClose != null) popupClose.onClick.AddListener(ClosePopup);
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        // Wait a frame for GameState to initialize
        StartCoroutine(DelayedBuild());
    }

    private IEnumerator DelayedBuild()
    {
        yield return null; // wait one frame
        BuildTable();
        UpdateRP();
    }

    private void BuildTable()
    {
        if (gridParent == null || cellPrefab == null) return;

        // Clear any existing children first
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        int rows = layout.GetLength(0);
        int cols = layout.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int z = layout[r, c];
                var cell = Instantiate(cellPrefab, gridParent);
                cell.SetActive(true);

                var img = cell.GetComponent<Image>();
                var texts = cell.GetComponentsInChildren<TextMeshProUGUI>(true);
                TextMeshProUGUI zText = texts.Length > 0 ? texts[0] : null;
                TextMeshProUGUI symText = texts.Length > 1 ? texts[1] : null;
                var btn = cell.GetComponent<Button>();

                if (z == 0)
                {
                    if (img != null) img.color = Color.clear;
                    if (zText != null) zText.text = "";
                    if (symText != null) symText.text = "";
                    if (btn != null) btn.interactable = false;
                    continue;
                }

                string sym = z < symbols.Length ? symbols[z] : "";
                if (zText != null) zText.text = z.ToString();
                if (symText != null) symText.text = sym;

                // Only highlight if GameState exists AND element is researched
                bool researched = false;
                if (GameState.Instance != null)
                    researched = GameState.Instance.IsResearched(sym);

                if (researched)
                {
                    if (img != null)
                    {
                        img.color = new Color(0.06f, 0.46f, 0.32f, 0.3f);
                        // Add a visible border by making the element stand out
                    }
                    if (btn != null)
                    {
                        btn.interactable = true;
                        // Capture sym in local variable for the lambda
                        string capturedSym = sym;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => ShowPopup(capturedSym));
                    }
                }
                else
                {
                    if (img != null) img.color = new Color(0.1f, 0.13f, 0.21f, 1f);
                    if (btn != null) btn.interactable = false;
                }
            }
        }
    }

    private void ShowPopup(string sym)
    {
        if (GameState.Instance == null) return;

        var el = GameState.Instance.GetElement(sym);
        if (el == null)
        {
            Debug.LogWarning($"No ElementData found for symbol: {sym}");
            return;
        }

        if (popup == null)
        {
            Debug.LogWarning("Popup reference is null");
            return;
        }

        popup.SetActive(true);

        if (popupTitle != null) popupTitle.text = $"{el.elementName} ({el.symbol})";
        if (popupDesc != null) popupDesc.text = el.description;
        if (popupShells != null) popupShells.text = $"Electron shells: {el.shellCount}";
        if (popupValence != null) popupValence.text = $"Valence electrons: {el.valenceElectrons}";

        Debug.Log($"Showing popup for {sym}");
    }

    private void ClosePopup()
    {
        if (popup != null) popup.SetActive(false);
    }

    private void UpdateRP()
    {
        if (rpText != null && GameState.Instance != null)
            rpText.text = $"{GameState.Instance.RP} RP";
    }
}