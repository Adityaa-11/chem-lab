using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Compendium : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private DiscoverableItem[] allItems;

    [Header("Detail Popup")]
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI popupTitle;
    [SerializeField] private TextMeshProUGUI popupBiome;
    [SerializeField] private TextMeshProUGUI popupDesc;
    [SerializeField] private TextMeshProUGUI popupElements;
    [SerializeField] private Button popupClose;

    [Header("Top Bar")]
    [SerializeField] private TextMeshProUGUI rpText;
    [SerializeField] private Button backButton;

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        if (popup != null) popup.SetActive(false);
        if (popupClose != null) popupClose.onClick.AddListener(() => popup.SetActive(false));
        StartCoroutine(DelayedBuild());
    }

    private IEnumerator DelayedBuild()
    {
        yield return null;
        BuildCompendium();
        UpdateRP();
    }

    private void BuildCompendium()
    {
        if (cardParent == null || cardPrefab == null || allItems == null) return;

        for (int i = cardParent.childCount - 1; i >= 0; i--)
            Destroy(cardParent.GetChild(i).gameObject);

        var biomeGroups = new Dictionary<string, List<DiscoverableItem>>();
        foreach (var item in allItems)
        {
            if (!biomeGroups.ContainsKey(item.biome))
                biomeGroups[item.biome] = new List<DiscoverableItem>();
            biomeGroups[item.biome].Add(item);
        }

        var biomeOrder = new[] { "Caverns", "Forest", "Wasteland" };
        foreach (var biome in biomeOrder)
        {
            if (!biomeGroups.ContainsKey(biome)) continue;

            // Biome header
            var header = Instantiate(cardPrefab, cardParent);
            header.SetActive(true);
            var headerImg = header.GetComponent<Image>();
            if (headerImg != null) headerImg.color = new Color(0.08f, 0.11f, 0.18f);
            var headerTexts = header.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (headerTexts.Length > 0) { headerTexts[0].text = biome.ToUpper(); headerTexts[0].color = GetBiomeColor(biome); headerTexts[0].fontSize = 14; }
            if (headerTexts.Length > 1) headerTexts[1].text = "";
            if (headerTexts.Length > 2) headerTexts[2].text = "";
            var headerBtn = header.GetComponent<Button>();
            if (headerBtn != null) headerBtn.interactable = false;

            foreach (var item in biomeGroups[biome])
            {
                //bool discovered = GameState.Instance != null && GameState.Instance.IsItemDiscovered(item.itemName);
                bool discovered = true;
                
                var card = Instantiate(cardPrefab, cardParent);
                card.SetActive(true);
                var img = card.GetComponent<Image>();
                var texts = card.GetComponentsInChildren<TextMeshProUGUI>(true);
                var btn = card.GetComponent<Button>();

                if (discovered)
                {
                    if (img != null) img.color = new Color(0.067f, 0.094f, 0.153f);
                    if (texts.Length > 0) { texts[0].text = item.itemName; texts[0].color = new Color(0.91f, 0.93f, 0.96f); }
                    if (texts.Length > 1)
                    {
                        string elemList = "";
                        foreach (var ey in item.elementYields)
                            elemList += ey.amount + "x " + ey.elementSymbol + "  ";
                        texts[1].text = elemList.Trim();
                        texts[1].color = GetBiomeColor(biome);
                    }
                    if (texts.Length > 2) { texts[2].text = item.description; texts[2].color = new Color(0.39f, 0.45f, 0.53f); }
                    if (btn != null)
                    {
                        btn.interactable = true;
                        string capturedName = item.itemName;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => ShowPopup(capturedName));
                    }
                }
                else
                {
                    if (img != null) img.color = new Color(0.06f, 0.08f, 0.12f);
                    if (texts.Length > 0) { texts[0].text = "???"; texts[0].color = new Color(0.25f, 0.27f, 0.32f); }
                    if (texts.Length > 1) { texts[1].text = "Undiscovered"; texts[1].color = new Color(0.2f, 0.22f, 0.27f); }
                    if (texts.Length > 2) { texts[2].text = "Find this in the " + biome + " to reveal its contents."; texts[2].color = new Color(0.18f, 0.2f, 0.25f); }
                    if (btn != null) btn.interactable = false;
                }
            }
        }
    }

    private void ShowPopup(string itemName)
    {
        if (popup == null || allItems == null) return;
        DiscoverableItem found = null;
        foreach (var item in allItems)
            if (item.itemName == itemName) { found = item; break; }
        if (found == null) return;

        popup.SetActive(true);
        if (popupTitle != null) popupTitle.text = found.itemName;
        if (popupBiome != null) { popupBiome.text = found.biome; popupBiome.color = GetBiomeColor(found.biome); }
        if (popupDesc != null) popupDesc.text = found.description;
        if (popupElements != null)
        {
            string elemText = "Contains:\n";
            foreach (var ey in found.elementYields)
            {
                var el = GameState.Instance != null ? GameState.Instance.GetElement(ey.elementSymbol) : null;
                string ename = el != null ? el.elementName : ey.elementSymbol;
                elemText += "  " + ey.amount + "x " + ename + " (" + ey.elementSymbol + ")\n";
            }
            popupElements.text = elemText.TrimEnd();
        }
    }

    private Color GetBiomeColor(string biome)
    {
        if (biome == "Caverns") return new Color(0.18f, 0.83f, 0.63f);
        if (biome == "Forest") return new Color(0.29f, 0.87f, 0.5f);
        if (biome == "Wasteland") return new Color(0.96f, 0.62f, 0.04f);
        return new Color(0.66f, 0.55f, 0.98f);
    }

    private void UpdateRP()
    {
        if (rpText != null && GameState.Instance != null)
            rpText.text = GameState.Instance.RP + " RP";
    }
}