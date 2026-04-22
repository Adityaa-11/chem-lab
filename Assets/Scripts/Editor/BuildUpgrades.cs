#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class BuildUpgrades
{
    [MenuItem("ChemGame/Build Upgrades")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // GameManager
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

        // EventSystem
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

        var title = MakeText("Title", topBar.transform, "Upgrades", 18, TextAlignmentOptions.Center);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.3f, 0); titleRT.anchorMax = new Vector2(0.7f, 1);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var rpText = MakeText("RPText", topBar.transform, "5 RP", 14, TextAlignmentOptions.Right);
        var rpRT = rpText.GetComponent<RectTransform>();
        rpRT.anchorMin = rpRT.anchorMax = new Vector2(1, 0.5f);
        rpRT.sizeDelta = new Vector2(120, 36); rpRT.anchoredPosition = new Vector2(-80, 0);
        rpText.GetComponent<TextMeshProUGUI>().color = new Color(0.96f, 0.62f, 0.04f);

        // Main upgrade card
        var card = MakeUI("BatteryCard", canvasGO.transform);
        card.AddComponent<Image>().color = new Color(0.067f, 0.094f, 0.153f);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = cardRT.anchorMax = new Vector2(0.5f, 0.55f);
        cardRT.sizeDelta = new Vector2(480, 380);

        // Card title
        float y = -24;
        var cardTitle = MakeText("CardTitle", card.transform, "Scanner Battery", 24, TextAlignmentOptions.TopLeft);
        cardTitle.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        PosText(cardTitle, ref y, 40, 24);

        var cardDesc = MakeText("CardDesc", card.transform, "Upgrade your scanner battery to extend exploration time in biomes.", 13, TextAlignmentOptions.TopLeft);
        cardDesc.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f);
        PosText(cardDesc, ref y, 36, 24);

        // Battery level
        y -= 12;
        var levelLabel = MakeText("LevelLabel", card.transform, "Battery Level", 11, TextAlignmentOptions.TopLeft);
        levelLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        PosText(levelLabel, ref y, 20, 24);

        var levelText = MakeText("BatteryLevel", card.transform, "Level 0", 20, TextAlignmentOptions.TopLeft);
        levelText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        PosText(levelText, ref y, 32, 24);

        var timerText = MakeText("TimerText", card.transform, "15s explore time", 14, TextAlignmentOptions.TopLeft);
        timerText.GetComponent<TextMeshProUGUI>().color = new Color(0.13f, 0.83f, 0.93f);
        PosText(timerText, ref y, 28, 24);

        // Divider
        y -= 12;
        var divider = MakeUI("Divider", card.transform);
        divider.AddComponent<Image>().color = new Color(1, 1, 1, 0.04f);
        var divRT = divider.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0, 1); divRT.anchorMax = new Vector2(1, 1);
        divRT.pivot = new Vector2(0.5f, 1);
        divRT.anchoredPosition = new Vector2(0, y);
        divRT.sizeDelta = new Vector2(-48, 1);
        y -= 16;

        // Materials header
        var matLabel = MakeText("MatLabel", card.transform, "Required Materials", 11, TextAlignmentOptions.TopLeft);
        matLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        PosText(matLabel, ref y, 20, 24);

        // Iron row
        y -= 4;
        var ironRow = MakeUI("IronRow", card.transform);
        var ironRowRT = ironRow.GetComponent<RectTransform>();
        ironRowRT.anchorMin = new Vector2(0, 1); ironRowRT.anchorMax = new Vector2(1, 1);
        ironRowRT.pivot = new Vector2(0.5f, 1);
        ironRowRT.anchoredPosition = new Vector2(0, y);
        ironRowRT.sizeDelta = new Vector2(-48, 28);
        y -= 32;

        var ironLabel = MakeText("IronLabel", ironRow.transform, "Iron (Fe)", 14, TextAlignmentOptions.Left);
        ironLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.75f, 0.31f, 0.13f);
        Stretch(ironLabel);

        var ironCount = MakeText("IronCount", ironRow.transform, "0", 14, TextAlignmentOptions.Right);
        ironCount.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        var ironCountRT = ironCount.GetComponent<RectTransform>();
        ironCountRT.anchorMin = new Vector2(0.6f, 0); ironCountRT.anchorMax = new Vector2(0.75f, 1);
        ironCountRT.offsetMin = ironCountRT.offsetMax = Vector2.zero;

        var ironReq = MakeText("IronReq", ironRow.transform, "/ 5 Fe", 14, TextAlignmentOptions.Left);
        var ironReqRT = ironReq.GetComponent<RectTransform>();
        ironReqRT.anchorMin = new Vector2(0.76f, 0); ironReqRT.anchorMax = new Vector2(1, 1);
        ironReqRT.offsetMin = ironReqRT.offsetMax = Vector2.zero;

        // Copper row
        var copperRow = MakeUI("CopperRow", card.transform);
        var copperRowRT = copperRow.GetComponent<RectTransform>();
        copperRowRT.anchorMin = new Vector2(0, 1); copperRowRT.anchorMax = new Vector2(1, 1);
        copperRowRT.pivot = new Vector2(0.5f, 1);
        copperRowRT.anchoredPosition = new Vector2(0, y);
        copperRowRT.sizeDelta = new Vector2(-48, 28);
        y -= 32;

        var copperLabel = MakeText("CopperLabel", copperRow.transform, "Copper (Cu)", 14, TextAlignmentOptions.Left);
        copperLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.5f, 0.2f);
        Stretch(copperLabel);

        var copperCount = MakeText("CopperCount", copperRow.transform, "0", 14, TextAlignmentOptions.Right);
        copperCount.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        var copperCountRT = copperCount.GetComponent<RectTransform>();
        copperCountRT.anchorMin = new Vector2(0.6f, 0); copperCountRT.anchorMax = new Vector2(0.75f, 1);
        copperCountRT.offsetMin = copperCountRT.offsetMax = Vector2.zero;

        var copperReq = MakeText("CopperReq", copperRow.transform, "/ 10 Cu", 14, TextAlignmentOptions.Left);
        var copperReqRT = copperReq.GetComponent<RectTransform>();
        copperReqRT.anchorMin = new Vector2(0.76f, 0); copperReqRT.anchorMax = new Vector2(1, 1);
        copperReqRT.offsetMin = copperReqRT.offsetMax = Vector2.zero;

        // Upgrade button
        y -= 16;
        var upgBtn = MakeButton("UpgradeButton", card.transform, "Upgrade Battery", new Vector2(280, 44));
        var upgBtnRT = upgBtn.GetComponent<RectTransform>();
        upgBtnRT.anchorMin = upgBtnRT.anchorMax = new Vector2(0.5f, 1);
        upgBtnRT.anchoredPosition = new Vector2(0, y - 22);
        upgBtn.GetComponent<Image>().color = new Color(0.13f, 0.83f, 0.93f);
        var upgBtnTxt = upgBtn.GetComponentInChildren<TextMeshProUGUI>();
        upgBtnTxt.color = new Color(0.04f, 0.055f, 0.09f);
        upgBtnTxt.fontStyle = FontStyles.Bold;
        y -= 56;

        // Status text
        var statusText = MakeText("StatusText", card.transform, "Upgrade your scanner battery to explore longer.", 12, TextAlignmentOptions.Center);
        statusText.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        var statusRT = statusText.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0, 1); statusRT.anchorMax = new Vector2(1, 1);
        statusRT.pivot = new Vector2(0.5f, 1);
        statusRT.anchoredPosition = new Vector2(0, y);
        statusRT.sizeDelta = new Vector2(-48, 30);

        // Wire UpgradesUI
        var ui = canvasGO.AddComponent<UpgradesUI>();
        var so = new SerializedObject(ui);
        so.FindProperty("batteryLevelText").objectReferenceValue = levelText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("batteryTimerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("ironCountText").objectReferenceValue = ironCount.GetComponent<TextMeshProUGUI>();
        so.FindProperty("copperCountText").objectReferenceValue = copperCount.GetComponent<TextMeshProUGUI>();
        so.FindProperty("ironReqText").objectReferenceValue = ironReq.GetComponent<TextMeshProUGUI>();
        so.FindProperty("copperReqText").objectReferenceValue = copperReq.GetComponent<TextMeshProUGUI>();
        so.FindProperty("upgradeButton").objectReferenceValue = upgBtn.GetComponent<Button>();
        so.FindProperty("upgradeButtonText").objectReferenceValue = upgBtnTxt;
        so.FindProperty("upgradeStatusText").objectReferenceValue = statusText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("rpText").objectReferenceValue = rpText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Upgrades.unity");

        // Update build settings
        var scenes = new List<EditorBuildSettingsScene>();
        foreach (var p in new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/ResearchTree.unity",
            "Assets/Scenes/PeriodicTable.unity", "Assets/Scenes/Compendium.unity", "Assets/Scenes/Upgrades.unity" })
        {
            if (File.Exists(p)) scenes.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
        Debug.Log("Upgrades scene built and saved!");
    }

    static void PosText(GameObject go, ref float y, float height, float margin)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(margin, 0); rt.offsetMax = new Vector2(-margin, 0);
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        y -= height;
    }

    static GameObject MakeUI(string name, Transform parent)
    { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return go; }

    static void Stretch(GameObject go)
    { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }

    static GameObject MakeText(string name, Transform parent, string text, int size, TextAlignmentOptions align)
    { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
      var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = text; tmp.fontSize = size; tmp.alignment = align;
      tmp.color = new Color(0.91f, 0.93f, 0.96f); tmp.enableWordWrapping = true; return go; }

    static GameObject MakeButton(string name, Transform parent, string label, Vector2 size)
    { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
      go.AddComponent<Image>().color = new Color(0.14f, 0.19f, 0.29f); go.AddComponent<Button>();
      go.GetComponent<RectTransform>().sizeDelta = size;
      var txt = MakeText("Text", go.transform, label, 13, TextAlignmentOptions.Center); Stretch(txt);
      txt.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f); return go; }
}
#endif