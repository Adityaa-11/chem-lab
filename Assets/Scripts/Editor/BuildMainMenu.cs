#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BuildMainMenu
{
    [MenuItem("ChemGame/Build Main Menu")]
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
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.055f, 0.09f);
        Stretch(bg);

        // Subtle radial glow
        var glow = MakeUI("Glow", canvasGO.transform);
        var glowImg = glow.AddComponent<Image>();
        glowImg.color = new Color(0.13f, 0.83f, 0.93f, 0.015f);
        var glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = glowRT.anchorMax = new Vector2(0.5f, 0.6f);
        glowRT.sizeDelta = new Vector2(600, 600);

        // Title
        var title = MakeText("Title", canvasGO.transform, "Elemental Explorer", 44, TextAlignmentOptions.Center);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = titleRT.anchorMax = new Vector2(0.5f, 0.82f);
        titleRT.sizeDelta = new Vector2(600, 60);
        var titleTMP = title.GetComponent<TextMeshProUGUI>();
        titleTMP.fontStyle = FontStyles.Bold;

        // Subtitle
        var sub = MakeText("Subtitle", canvasGO.transform, "CHEMISTRY LEARNING GAME", 13, TextAlignmentOptions.Center);
        var subRT = sub.GetComponent<RectTransform>();
        subRT.anchorMin = subRT.anchorMax = new Vector2(0.5f, 0.76f);
        subRT.sizeDelta = new Vector2(400, 30);
        sub.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);

        // Button grid container
        var grid = MakeUI("ButtonGrid", canvasGO.transform);
        var gridRT = grid.GetComponent<RectTransform>();
        gridRT.anchorMin = gridRT.anchorMax = new Vector2(0.5f, 0.45f);
        gridRT.sizeDelta = new Vector2(500, 300);
        var gridLayout = grid.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(230, 110);
        gridLayout.spacing = new Vector2(16, 16);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 2;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;

        // Buttons
        var researchBtn = MakeMenuButton("ResearchButton", grid.transform, "R", "Research",
            "Discover elements on the hex tree", new Color(0.13f, 0.83f, 0.93f));
        var ptableBtn = MakeMenuButton("PeriodicTableButton", grid.transform, "Pt", "Periodic Table",
            "Browse researched elements", new Color(0.79f, 0.66f, 0.3f));
        var exploreBtn = MakeMenuButton("ExploreButton", grid.transform, "Ex", "Explore",
            "Collect atoms from biomes", new Color(0.06f, 0.73f, 0.51f));
        var constructBtn = MakeMenuButton("ConstructButton", grid.transform, "Cn", "Construct",
            "Build compounds from atoms", new Color(0.98f, 0.45f, 0.14f));

        // Wire MainMenuController
        var controller = canvasGO.AddComponent<MainMenuController>();
        var so = new SerializedObject(controller);
        so.FindProperty("researchButton").objectReferenceValue = researchBtn.GetComponent<Button>();
        so.FindProperty("periodicTableButton").objectReferenceValue = ptableBtn.GetComponent<Button>();
        so.FindProperty("exploreButton").objectReferenceValue = exploreBtn.GetComponent<Button>();
        so.FindProperty("constructButton").objectReferenceValue = constructBtn.GetComponent<Button>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("Main Menu scene built and saved!");
    }

    static GameObject MakeMenuButton(string name, Transform parent, string icon, string title, string desc, Color accent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.067f, 0.094f, 0.153f);
        go.AddComponent<Button>();

        // Icon badge
        var badge = MakeUI("Icon", go.transform);
        var badgeImg = badge.AddComponent<Image>();
        badgeImg.color = new Color(accent.r, accent.g, accent.b, 0.12f);
        var badgeRT = badge.GetComponent<RectTransform>();
        badgeRT.anchorMin = badgeRT.anchorMax = new Vector2(0, 1);
        badgeRT.pivot = new Vector2(0, 1);
        badgeRT.anchoredPosition = new Vector2(16, -16);
        badgeRT.sizeDelta = new Vector2(32, 32);

        var badgeTxt = MakeText("IconText", badge.transform, icon, 13, TextAlignmentOptions.Center);
        Stretch(badgeTxt);
        badgeTxt.GetComponent<TextMeshProUGUI>().color = accent;
        badgeTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Title
        var titleGO = MakeText("Title", go.transform, title, 16, TextAlignmentOptions.TopLeft);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.offsetMin = new Vector2(16, 16);
        titleRT.offsetMax = new Vector2(-16, -52);
        titleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Description
        var descGO = MakeText("Desc", go.transform, desc, 12, TextAlignmentOptions.BottomLeft);
        var descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0);
        descRT.anchorMax = new Vector2(1, 0.45f);
        descRT.offsetMin = new Vector2(16, 12);
        descRT.offsetMax = new Vector2(-16, 0);
        descGO.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);

        return go;
    }

    static GameObject MakeUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeText(string name, Transform parent, string text, int size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = new Color(0.91f, 0.93f, 0.96f);
        tmp.enableWordWrapping = true;
        return go;
    }
}
#endif
