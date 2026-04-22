#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class BuildCompendium
{
    [MenuItem("ChemGame/Build Compendium")]
    public static void Build()
    {
        CreateItemAssets();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var gmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameManager.prefab");
        if (gmPrefab != null) PrefabUtility.InstantiatePrefab(gmPrefab);

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Background
        var bg = MakeUI("Background", canvasGO.transform);
        bg.AddComponent<Image>().color = new Color(0.04f, 0.055f, 0.09f);
        Stretch(bg);

        // Top Bar
        var topBar = MakeUI("TopBar", canvasGO.transform);
        topBar.AddComponent<Image>().color = new Color(0.067f, 0.094f, 0.153f);
        var topRT = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 1); topRT.anchorMax = new Vector2(1, 1);
        topRT.pivot = new Vector2(0.5f, 1); topRT.sizeDelta = new Vector2(0, 60);

        var backBtn = MakeButton("BackButton", topBar.transform, "Back", new Vector2(70, 36));
        var backBtnRT = backBtn.GetComponent<RectTransform>();
        backBtnRT.anchorMin = backBtnRT.anchorMax = new Vector2(0, 0.5f);
        backBtnRT.anchoredPosition = new Vector2(60, 0);

        var title = MakeText("Title", topBar.transform, "Compendium", 18, TextAlignmentOptions.Center);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.3f, 0); titleRT.anchorMax = new Vector2(0.7f, 1);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var rpText = MakeText("RPText", topBar.transform, "5 RP", 14, TextAlignmentOptions.Right);
        var rpRT = rpText.GetComponent<RectTransform>();
        rpRT.anchorMin = rpRT.anchorMax = new Vector2(1, 0.5f);
        rpRT.sizeDelta = new Vector2(120, 36); rpRT.anchoredPosition = new Vector2(-80, 0);
        rpText.GetComponent<TextMeshProUGUI>().color = new Color(0.96f, 0.62f, 0.04f);

        // Card template
        var cardTemplate = CreateCardTemplate();
        cardTemplate.SetActive(false);
        cardTemplate.transform.SetParent(canvasGO.transform, false);

        // Scroll area
        var scrollGO = MakeUI("ScrollArea", canvasGO.transform);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.02f, 0.02f);
        scrollRT.anchorMax = new Vector2(0.98f, 0.9f);
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scrollGO.AddComponent<RectMask2D>();

        var contentGO = MakeUI("Content", scrollGO.transform);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        var contentFitter = contentGO.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var gridLayout = contentGO.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(380, 100);
        gridLayout.spacing = new Vector2(16, 8);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.padding = new RectOffset(20, 20, 10, 20);
        scroll.content = contentRT;

        // Popup
        var popupOverlay = MakeUI("PopupOverlay", canvasGO.transform);
        popupOverlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        Stretch(popupOverlay);

        var popupPanel = MakeUI("PopupPanel", popupOverlay.transform);
        popupPanel.AddComponent<Image>().color = new Color(0.067f, 0.094f, 0.153f);
        var ppRT = popupPanel.GetComponent<RectTransform>();
        ppRT.anchorMin = ppRT.anchorMax = new Vector2(0.5f, 0.5f);
        ppRT.sizeDelta = new Vector2(420, 340);

        float py = -20;
        var popTitleGO = MakeText("PopupTitle", popupPanel.transform, "", 22, TextAlignmentOptions.TopLeft);
        popTitleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        PosText(popTitleGO, ref py, 36);

        var popBiomeGO = MakeText("PopupBiome", popupPanel.transform, "", 12, TextAlignmentOptions.TopLeft);
        PosText(popBiomeGO, ref py, 22);

        var popDescGO = MakeText("PopupDesc", popupPanel.transform, "", 13, TextAlignmentOptions.TopLeft);
        popDescGO.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f);
        PosText(popDescGO, ref py, 80);

        var popElemGO = MakeText("PopupElements", popupPanel.transform, "", 14, TextAlignmentOptions.TopLeft);
        popElemGO.GetComponent<TextMeshProUGUI>().color = new Color(0.13f, 0.83f, 0.93f);
        PosText(popElemGO, ref py, 80);

        py -= 10;
        var closeBtn = MakeButton("CloseButton", popupPanel.transform, "Close", new Vector2(120, 38));
        var closeBtnRT = closeBtn.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = closeBtnRT.anchorMax = new Vector2(0.5f, 1);
        closeBtnRT.anchoredPosition = new Vector2(0, py - 20);

        // Load item assets
        var itemAssets = LoadItemAssets();

        // Wire CompendiumUI
        var compUI = canvasGO.AddComponent<Compendium>();
        var so = new SerializedObject(compUI);
        so.FindProperty("cardParent").objectReferenceValue = contentGO.transform;
        so.FindProperty("cardPrefab").objectReferenceValue = cardTemplate;
        so.FindProperty("popup").objectReferenceValue = popupOverlay;
        so.FindProperty("popupTitle").objectReferenceValue = popTitleGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("popupBiome").objectReferenceValue = popBiomeGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("popupDesc").objectReferenceValue = popDescGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("popupElements").objectReferenceValue = popElemGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("popupClose").objectReferenceValue = closeBtn.GetComponent<Button>();
        so.FindProperty("rpText").objectReferenceValue = rpText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();

        var itemsProp = so.FindProperty("allItems");
        itemsProp.arraySize = itemAssets.Count;
        for (int i = 0; i < itemAssets.Count; i++)
            itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = itemAssets[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Compendium.unity");

        // Build settings
        var scenes = new List<EditorBuildSettingsScene>();
        foreach (var p in new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/ResearchTree.unity",
            "Assets/Scenes/PeriodicTable.unity", "Assets/Scenes/Compendium.unity", "Assets/Scenes/Upgrades.unity" })
        {
            if (File.Exists(p)) scenes.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("Compendium scene built with discoverable items!");
    }

    struct ItemDef
    {
        public string name, biome, desc;
        public string[] elemSyms;
        public int[] elemAmts;
    }

static ItemDef[] itemDefs = new ItemDef[]
{
    // ===================== CAVERNS =====================

    new ItemDef
    {
        name = "Rock",
        biome = "Caverns",
        desc = "A dense mineral formation found deep in cave walls. Rich in metallic elements deposited over millions of years.",

        elemSyms = new[]{"Silicon","Calcium","Iron","Aluminum","Sulfur","Copper","Gold","Tin"},
        elemAmts = new[]{3,2,2,2,1,1,1,1}
    },

    new ItemDef
    {
        name = "Salt Crystal",
        biome = "Caverns",
        desc = "Crystalline salt deposits formed by evaporation of ancient underground water sources.",

        elemSyms = new[]{"Sodium","Chlorine","Magnesium","Calcium","Potassium"},
        elemAmts = new[]{4,3,2,2,1}
    },

    // ===================== FOREST =====================

    new ItemDef
    {
        name = "Tree",
        biome = "Forest",
        desc = "A living tree absorbing carbon dioxide and releasing oxygen. Rich in organic compounds.",

        elemSyms = new[]{"Carbon","Oxygen","Hydrogen","Nitrogen","Iron","Zinc","Magnesium","Copper"},
        elemAmts = new[]{4,3,2,1,1,1,1,1}
    },

    new ItemDef
    {
        name = "Burnt Tree",
        biome = "Forest",
        desc = "Charred remains of a tree after fire, leaving behind concentrated carbon.",

        elemSyms = new[]{"Carbon","Oxygen","Calcium","Potassium","Magnesium"},
        elemAmts = new[]{6,2,1,1,1}
    },

    new ItemDef
    {
        name = "Mushroom",
        biome = "Forest",
        desc = "A decomposer fungus rich in nitrogen and organic compounds.",

        elemSyms = new[]{"Carbon","Oxygen","Nitrogen","Phosphorus","Potassium","Hydrogen"},
        elemAmts = new[]{4,3,2,1,1,2}
    },

    new ItemDef
    {
        name = "Water",
        biome = "Forest",
        desc = "A building block of life, Consisting of Hydrogen and Oxygen.",

        elemSyms = new[]{"Hydrogen","Oxygen"},
        elemAmts = new[]{1,2}
    },
    };

    static void CreateItemAssets()
    {
        string dir = "Assets/ScriptableObjects/Items";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        foreach (var def in itemDefs)
        {
            string safeName = def.name.Replace(" ", "");
            string path = dir + "/" + safeName + ".asset";
            if (AssetDatabase.LoadAssetAtPath<DiscoverableItem>(path) != null) continue;

            var item = ScriptableObject.CreateInstance<DiscoverableItem>();
            item.itemName = def.name;
            item.biome = def.biome;
            item.description = def.desc;
            item.elementYields = new ElementYield[def.elemSyms.Length];
            for (int i = 0; i < def.elemSyms.Length; i++)
            {
                item.elementYields[i] = new ElementYield
                {
                    elementSymbol = def.elemSyms[i],
                    amount = def.elemAmts[i]
                };
            }
            AssetDatabase.CreateAsset(item, path);
        }
        AssetDatabase.Refresh();
        Debug.Log("Created " + itemDefs.Length + " discoverable item assets");
    }

    static List<DiscoverableItem> LoadItemAssets()
    {
        var list = new List<DiscoverableItem>();
        foreach (var def in itemDefs)
        {
            string safeName = def.name.Replace(" ", "");
            string path = "Assets/ScriptableObjects/Items/" + safeName + ".asset";
            var item = AssetDatabase.LoadAssetAtPath<DiscoverableItem>(path);
            if (item != null) list.Add(item);
        }
        return list;
    }

    static GameObject CreateCardTemplate()
    {
        var card = new GameObject("CardTemplate", typeof(RectTransform));
        card.AddComponent<Image>().color = new Color(0.067f, 0.094f, 0.153f);
        card.AddComponent<Button>();

        // Item name (top)
        var nameGO = new GameObject("ItemName", typeof(RectTransform));
        nameGO.transform.SetParent(card.transform, false);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.fontSize = 16; nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.alignment = TextAlignmentOptions.TopLeft;
        nameTMP.color = new Color(0.91f, 0.93f, 0.96f);
        var nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.6f); nameRT.anchorMax = new Vector2(1, 1);
        nameRT.offsetMin = new Vector2(16, 0); nameRT.offsetMax = new Vector2(-16, -10);

        // Element list (middle)
        var elemGO = new GameObject("Elements", typeof(RectTransform));
        elemGO.transform.SetParent(card.transform, false);
        var elemTMP = elemGO.AddComponent<TextMeshProUGUI>();
        elemTMP.fontSize = 12; elemTMP.alignment = TextAlignmentOptions.Left;
        elemTMP.color = new Color(0.13f, 0.83f, 0.93f);
        var elemRT = elemGO.GetComponent<RectTransform>();
        elemRT.anchorMin = new Vector2(0, 0.35f); elemRT.anchorMax = new Vector2(1, 0.62f);
        elemRT.offsetMin = new Vector2(16, 0); elemRT.offsetMax = new Vector2(-16, 0);

        // Description (bottom)
        var descGO = new GameObject("Desc", typeof(RectTransform));
        descGO.transform.SetParent(card.transform, false);
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.fontSize = 11; descTMP.alignment = TextAlignmentOptions.TopLeft;
        descTMP.color = new Color(0.39f, 0.45f, 0.53f);
        descTMP.enableWordWrapping = true;
        var descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0); descRT.anchorMax = new Vector2(1, 0.37f);
        descRT.offsetMin = new Vector2(16, 8); descRT.offsetMax = new Vector2(-16, 0);

        return card;
    }

    static void PosText(GameObject go, ref float y, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(20, 0); rt.offsetMax = new Vector2(-20, 0);
        rt.anchoredPosition = new Vector2(0, y); rt.sizeDelta = new Vector2(rt.sizeDelta.x, h);
        y -= h + 4;
    }

    static GameObject MakeUI(string n, Transform p)
    { var g = new GameObject(n, typeof(RectTransform)); g.transform.SetParent(p, false); return g; }
    static void Stretch(GameObject g)
    { var r = g.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    static GameObject MakeText(string n, Transform p, string t, int s, TextAlignmentOptions a)
    { var g = new GameObject(n, typeof(RectTransform)); g.transform.SetParent(p, false);
      var tmp = g.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = s; tmp.alignment = a;
      tmp.color = new Color(0.91f, 0.93f, 0.96f); tmp.enableWordWrapping = true; return g; }
    static GameObject MakeButton(string n, Transform p, string l, Vector2 sz)
    { var g = new GameObject(n, typeof(RectTransform)); g.transform.SetParent(p, false);
      g.AddComponent<Image>().color = new Color(0.14f, 0.19f, 0.29f); g.AddComponent<Button>();
      g.GetComponent<RectTransform>().sizeDelta = sz;
      var t = MakeText("Text", g.transform, l, 13, TextAlignmentOptions.Center); Stretch(t);
      t.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f); return g; }
}
#endif