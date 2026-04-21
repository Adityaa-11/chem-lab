#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public class AutoSetup
{
    static AutoSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool("ChemGame_SetupDone_v5", false))
            {
                if (EditorUtility.DisplayDialog("Elemental Explorer Setup",
                    "Set up Research Tree, Periodic Table, element data, and GameManager?\n\nThis will create all necessary assets and scenes.",
                    "Set Up Now", "Skip"))
                {
                    RunSetup();
                }
                EditorPrefs.SetBool("ChemGame_SetupDone_v5", true);
            }
        };
    }

    [MenuItem("ChemGame/Run Full Setup")]
    public static void RunSetup()
    {
        CreateElementAssets();
        CreateGameManagerPrefab();
        BuildResearchTreeScene();
        BuildPeriodicTableScene();
        BuildChemistPrefab.Build();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== Elemental Explorer setup complete! ===");
    }

    [MenuItem("ChemGame/Reset Setup Flag")]
    public static void ResetFlag()
    {
        EditorPrefs.SetBool("ChemGame_SetupDone_v5", false);
        Debug.Log("Setup flag reset. Will prompt on next editor reload.");
    }

    // ======================== ELEMENT DATA ========================

    struct ElemDef
    {
        public string sym, name, desc;
        public int z, shells, ve;
        public Color color;
        public string[] biomes, prereqs;
    }

    static ElemDef[] elementDefs = new ElemDef[]
    {
        new ElemDef { sym="H", name="Hydrogen", z=1, shells=1, ve=1,
            color=new Color(0.9f,0.63f,0.7f),
            desc="Hydrogen, the first element, is the single most abundant element in the universe. At room temperature it is a colorless, odorless, and highly combustible gas. With 1 shell and 1 valence electron, it is highly reactive.",
            biomes=new[]{"Forest","Caverns"}, prereqs=new string[0] },
        new ElemDef { sym="C", name="Carbon", z=6, shells=2, ve=4,
            color=new Color(0.63f,0.47f,0.16f),
            desc="Carbon is the backbone of organic chemistry. With 4 valence electrons, it can form four covalent bonds, enabling the incredible diversity of organic molecules.",
            biomes=new[]{"Forest"}, prereqs=new[]{"H"} },
        new ElemDef { sym="N", name="Nitrogen", z=7, shells=2, ve=5,
            color=new Color(0.29f,0.54f,0.88f),
            desc="Nitrogen makes up about 78% of Earth's atmosphere. It has 5 valence electrons and commonly forms triple bonds, making molecular nitrogen very stable.",
            biomes=new[]{"Forest","Wasteland"}, prereqs=new[]{"H"} },
        new ElemDef { sym="O", name="Oxygen", z=8, shells=2, ve=6,
            color=new Color(0.88f,0.25f,0.25f),
            desc="Oxygen is essential for respiration and combustion. With 6 valence electrons, it needs 2 more to fill its outer shell, making it highly reactive.",
            biomes=new[]{"Forest","Caverns"}, prereqs=new[]{"H"} },
        new ElemDef { sym="Fe", name="Iron", z=26, shells=4, ve=2,
            color=new Color(0.75f,0.31f,0.13f),
            desc="Iron is the most commonly used metal on Earth. With 2 valence electrons, it readily forms ions. A key component of steel, and its abundance in Earth's core generates our planet's magnetic field.",
            biomes=new[]{"Caverns","Wasteland"}, prereqs=new string[0] },
        new ElemDef { sym="Cu", name="Copper", z=29, shells=4, ve=1,
            color=new Color(0.8f,0.5f,0.2f),
            desc="Copper is a soft, malleable, ductile metal with exceptional electrical and thermal conductivity. One of the few metals found in directly usable form in nature.",
            biomes=new[]{"Caverns"}, prereqs=new[]{"Fe"} },
        new ElemDef { sym="Sn", name="Tin", z=50, shells=5, ve=4,
            color=new Color(0.66f,0.66f,0.69f),
            desc="Tin is a soft, malleable metal with a relatively low melting point, commonly used in alloys. Resistant to oxidation, historically used to coat other metals to prevent rusting.",
            biomes=new[]{"Caverns","Wasteland"}, prereqs=new[]{"Fe"} },
        new ElemDef { sym="Al", name="Aluminum", z=13, shells=3, ve=3,
            color=new Color(0.44f,0.46f,0.47f),
            desc="Aluminum is the most abundant metal in Earth's crust. Lightweight yet strong, with excellent corrosion resistance due to a thin oxide layer.",
            biomes=new[]{"Wasteland"}, prereqs=new[]{"Fe","Sn"} },
        new ElemDef { sym="Si", name="Silicon", z=14, shells=3, ve=4,
            color=new Color(0.55f,0.55f,0.58f),
            desc="Silicon is the second most abundant element in Earth's crust, the backbone of rocks, sand, and quartz. With 4 valence electrons it forms the silicate minerals that make up most of the planet's surface — and the silicon chips that power every computer.",
            biomes=new[]{"Caverns"}, prereqs=new string[0] },
        new ElemDef { sym="Ca", name="Calcium", z=20, shells=4, ve=2,
            color=new Color(0.92f,0.92f,0.85f),
            desc="Calcium is the mineral of bones, shells, and limestone. Dripping water deposits it over millennia to form the stalactites and stalagmites of caves. A reactive alkaline-earth metal with 2 valence electrons.",
            biomes=new[]{"Caverns"}, prereqs=new string[0] },
        new ElemDef { sym="S", name="Sulfur", z=16, shells=3, ve=6,
            color=new Color(0.98f,0.86f,0.18f),
            desc="Sulfur is the bright yellow mineral that crystallizes near volcanic vents and deep cave deposits. With 6 valence electrons it readily forms sulfides, sulfates, and the smell of struck matches.",
            biomes=new[]{"Caverns"}, prereqs=new string[0] },
        new ElemDef { sym="Au", name="Gold", z=79, shells=6, ve=1,
            color=new Color(1f,0.84f,0f),
            desc="Gold is the unreactive noble metal that resists tarnish and rust, prized since antiquity for its luster and conductivity. Rare veins form deep in quartz-rich cave systems — a lucky find for any prospector.",
            biomes=new[]{"Caverns"}, prereqs=new[]{"Cu"} },
    };

    static void CreateElementAssets()
    {
        string dir = "Assets/ScriptableObjects/Elements";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        foreach (var def in elementDefs)
        {
            string path = $"{dir}/{def.sym}_{def.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<ElementData>(path) != null) continue;

            var el = ScriptableObject.CreateInstance<ElementData>();
            el.symbol = def.sym;
            el.elementName = def.name;
            el.atomicNumber = def.z;
            el.shellCount = def.shells;
            el.valenceElectrons = def.ve;
            el.description = def.desc;
            el.elementColor = def.color;
            el.foundInBiomes = def.biomes;
            el.prerequisiteSymbols = def.prereqs;
            AssetDatabase.CreateAsset(el, path);
        }
        Debug.Log($"Created {elementDefs.Length} element assets");
    }

    // ======================== GAME MANAGER PREFAB ========================

    static void CreateGameManagerPrefab()
    {
        string path = "Assets/Prefabs/GameManager.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("GameManager");
        var gs = go.AddComponent<GameState>();

        // Load all element assets into the array
        var elems = new List<ElementData>();
        foreach (var def in elementDefs)
        {
            var el = AssetDatabase.LoadAssetAtPath<ElementData>($"Assets/ScriptableObjects/Elements/{def.sym}_{def.name}.asset");
            if (el != null) elems.Add(el);
        }

        var so = new SerializedObject(gs);
        var allElemProp = so.FindProperty("allElements");
        allElemProp.arraySize = elems.Count;
        for (int i = 0; i < elems.Count; i++)
            allElemProp.GetArrayElementAtIndex(i).objectReferenceValue = elems[i];

        var starterProp = so.FindProperty("starterSymbols");
        starterProp.arraySize = 2;
        starterProp.GetArrayElementAtIndex(0).stringValue = "H";
        starterProp.GetArrayElementAtIndex(1).stringValue = "Fe";

        so.ApplyModifiedPropertiesWithoutUndo();

        if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("Created GameManager prefab");
    }

    // ======================== RESEARCH TREE SCENE ========================

    static void BuildResearchTreeScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // -- GameManager --
        var gmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameManager.prefab");
        if (gmPrefab != null) PrefabUtility.InstantiatePrefab(gmPrefab);

        // -- Canvas --
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // -- EventSystem --
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // -- Background --
        var bg = CreateUIElement("Background", canvasGO.transform);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.055f, 0.09f, 1f);
        StretchFill(bg);

        // -- Top Bar --
        var topBar = CreateUIElement("TopBar", canvasGO.transform);
        var topImg = topBar.AddComponent<Image>();
        topImg.color = new Color(0.067f, 0.094f, 0.153f, 1f);
        var topRT = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 1);
        topRT.anchorMax = new Vector2(1, 1);
        topRT.pivot = new Vector2(0.5f, 1);
        topRT.sizeDelta = new Vector2(0, 60);
        topRT.anchoredPosition = Vector2.zero;

        // Back button
        var backBtnGO = CreateButton("BackButton", topBar.transform, "Back", new Vector2(70, 36));
        var backRT = backBtnGO.GetComponent<RectTransform>();
        backRT.anchorMin = backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.anchoredPosition = new Vector2(60, 0);

        // Title
        var titleGO = CreateText("Title", topBar.transform, "Research Tree", 18, TextAlignmentOptions.Center);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.3f, 0);
        titleRT.anchorMax = new Vector2(0.7f, 1);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // RP display
        var rpGO = CreateText("RPText", topBar.transform, "5 RP", 14, TextAlignmentOptions.Right);
        var rpRT = rpGO.GetComponent<RectTransform>();
        rpRT.anchorMin = rpRT.anchorMax = new Vector2(1, 0.5f);
        rpRT.sizeDelta = new Vector2(120, 36);
        rpRT.anchoredPosition = new Vector2(-80, 0);
        rpGO.GetComponent<TextMeshProUGUI>().color = new Color(0.96f, 0.62f, 0.04f);

        // -- Tree Area (left side) --
        var treeArea = CreateUIElement("TreeArea", canvasGO.transform);
        var treeRT = treeArea.GetComponent<RectTransform>();
        treeRT.anchorMin = new Vector2(0, 0);
        treeRT.anchorMax = new Vector2(0.65f, 1);
        treeRT.offsetMin = new Vector2(0, 0);
        treeRT.offsetMax = new Vector2(0, -60);

        // Cluster labels
        var orgLabel = CreateText("OrganicLabel", treeArea.transform, "ORGANIC", 12, TextAlignmentOptions.Center);
        var orgLabelRT = orgLabel.GetComponent<RectTransform>();
        orgLabelRT.anchorMin = orgLabelRT.anchorMax = new Vector2(0.3f, 0.85f);
        orgLabelRT.sizeDelta = new Vector2(120, 30);
        orgLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);

        var metLabel = CreateText("MetalsLabel", treeArea.transform, "METALS", 12, TextAlignmentOptions.Center);
        var metLabelRT = metLabel.GetComponent<RectTransform>();
        metLabelRT.anchorMin = metLabelRT.anchorMax = new Vector2(0.7f, 0.45f);
        metLabelRT.sizeDelta = new Vector2(120, 30);
        metLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);

        // -- Create Hex Nodes --
        // Positions as anchor percentages within the tree area
        var nodePositions = new Dictionary<string, Vector2>
        {
            {"H",  new Vector2(0.30f, 0.70f)},
            {"C",  new Vector2(0.15f, 0.70f)},
            {"O",  new Vector2(0.38f, 0.80f)},
            {"N",  new Vector2(0.45f, 0.70f)},
            {"Fe", new Vector2(0.65f, 0.35f)},
            {"Cu", new Vector2(0.82f, 0.35f)},
            {"Sn", new Vector2(0.57f, 0.22f)},
            {"Al", new Vector2(0.65f, 0.12f)},
        };

        foreach (var def in elementDefs)
        {
            if (!nodePositions.ContainsKey(def.sym)) continue;
            var pos = nodePositions[def.sym];
            CreateHexNode(def.sym, def.color, pos, treeArea.transform);
        }

        // -- Connection Lines --
        string[][] connections = new string[][] {
            new[]{"H","C"}, new[]{"H","N"}, new[]{"H","O"},
            new[]{"Fe","Cu"}, new[]{"Fe","Sn"}, new[]{"Sn","Al"}, new[]{"Fe","Al"}
        };
        foreach (var conn in connections)
        {
            if (nodePositions.ContainsKey(conn[0]) && nodePositions.ContainsKey(conn[1]))
                CreateLine(conn[0] + "_" + conn[1], nodePositions[conn[0]], nodePositions[conn[1]], treeArea.transform);
        }

        // Cross-cluster dashed line indicator
        var crossLine = CreateUIElement("CrossClusterLine", treeArea.transform);
        var crossImg = crossLine.AddComponent<Image>();
        crossImg.color = new Color(1, 1, 1, 0.04f);
        var crossRT = crossLine.GetComponent<RectTransform>();
        crossRT.anchorMin = crossRT.anchorMax = new Vector2(0.47f, 0.52f);
        crossRT.sizeDelta = new Vector2(3, 180);
        crossRT.localRotation = Quaternion.Euler(0, 0, 30);

        // -- Info Panel (right side) --
        var infoPanel = CreateUIElement("InfoPanel", canvasGO.transform);
        var infoPanelImg = infoPanel.AddComponent<Image>();
        infoPanelImg.color = new Color(0.067f, 0.094f, 0.153f, 1f);
        var infoPanelRT = infoPanel.GetComponent<RectTransform>();
        infoPanelRT.anchorMin = new Vector2(0.65f, 0);
        infoPanelRT.anchorMax = new Vector2(1, 1);
        infoPanelRT.offsetMin = new Vector2(0, 0);
        infoPanelRT.offsetMax = new Vector2(0, -60);

        // Info panel contents
        float yPos = -20;
        var infoTitleGO = CreateText("InfoTitle", infoPanel.transform, "Select an element", 22, TextAlignmentOptions.TopLeft);
        SetInfoTextPos(infoTitleGO, ref yPos, 40);

        var infoDescGO = CreateText("InfoDesc", infoPanel.transform, "", 13, TextAlignmentOptions.TopLeft);
        infoDescGO.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f);
        SetInfoTextPos(infoDescGO, ref yPos, 140);

        var infoShellsGO = CreateText("InfoShells", infoPanel.transform, "", 12, TextAlignmentOptions.TopLeft);
        infoShellsGO.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        SetInfoTextPos(infoShellsGO, ref yPos, 24);

        var infoValGO = CreateText("InfoValence", infoPanel.transform, "", 12, TextAlignmentOptions.TopLeft);
        infoValGO.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        SetInfoTextPos(infoValGO, ref yPos, 24);

        var infoStatusGO = CreateText("InfoStatus", infoPanel.transform, "", 13, TextAlignmentOptions.TopLeft);
        SetInfoTextPos(infoStatusGO, ref yPos, 30);

        // Research button
        yPos -= 10;
        var resBtnGO = CreateButton("ResearchButton", infoPanel.transform, "Research", new Vector2(260, 42));
        var resBtnRT = resBtnGO.GetComponent<RectTransform>();
        resBtnRT.anchorMin = resBtnRT.anchorMax = new Vector2(0.5f, 1);
        resBtnRT.anchoredPosition = new Vector2(0, yPos - 20);
        var resBtnImg = resBtnGO.GetComponent<Image>();
        resBtnImg.color = new Color(0.13f, 0.83f, 0.93f, 1f);
        var resBtnTxt = resBtnGO.GetComponentInChildren<TextMeshProUGUI>();
        resBtnTxt.color = new Color(0.04f, 0.055f, 0.09f);
        resBtnTxt.fontStyle = FontStyles.Bold;

        // -- Wire up ResearchTreeManager --
        var manager = canvasGO.AddComponent<ResearchTreeManager>();
        var mso = new SerializedObject(manager);
        mso.FindProperty("infoPanel").objectReferenceValue = infoPanel;
        mso.FindProperty("infoTitle").objectReferenceValue = infoTitleGO.GetComponent<TextMeshProUGUI>();
        mso.FindProperty("infoDescription").objectReferenceValue = infoDescGO.GetComponent<TextMeshProUGUI>();
        mso.FindProperty("infoShells").objectReferenceValue = infoShellsGO.GetComponent<TextMeshProUGUI>();
        mso.FindProperty("infoValence").objectReferenceValue = infoValGO.GetComponent<TextMeshProUGUI>();
        mso.FindProperty("infoStatus").objectReferenceValue = infoStatusGO.GetComponent<TextMeshProUGUI>();
        mso.FindProperty("researchButton").objectReferenceValue = resBtnGO.GetComponent<Button>();
        mso.FindProperty("researchButtonText").objectReferenceValue = resBtnTxt;
        mso.FindProperty("rpText").objectReferenceValue = rpGO.GetComponent<TextMeshProUGUI>();
        mso.FindProperty("backButton").objectReferenceValue = backBtnGO.GetComponent<Button>();
        mso.ApplyModifiedPropertiesWithoutUndo();

        // Save scene
        string scenePath = "Assets/Scenes/ResearchTree.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Built ResearchTree scene");
    }

    // ======================== HELPERS ========================

    static GameObject CreateUIElement(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchFill(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static GameObject CreateText(string name, Transform parent, string text, int size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = new Color(0.91f, 0.93f, 0.96f);
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go;
    }

    static void SetInfoTextPos(GameObject go, ref float yPos, float height)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(20, 0);
        rt.offsetMax = new Vector2(-20, 0);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        yPos -= height + 6;
    }

    static GameObject CreateButton(string name, Transform parent, string label, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.14f, 0.19f, 0.29f, 1f);
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var txtGO = CreateText("Text", go.transform, label, 13, TextAlignmentOptions.Center);
        StretchFill(txtGO);
        txtGO.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f);

        return go;
    }

    static void CreateHexNode(string sym, Color color, Vector2 anchorPos, Transform parent)
    {
        var go = new GameObject($"Node_{sym}", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.17f, 0.15f, 0.22f);

        var btn = go.AddComponent<Button>();
        var node = go.AddComponent<ResearchTreeNode>();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchorPos;
        rt.sizeDelta = new Vector2(72, 72);
        rt.anchoredPosition = Vector2.zero;

        var txtGO = CreateText("Label", go.transform, "?", 20, TextAlignmentOptions.Center);
        StretchFill(txtGO);
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.fontStyle = FontStyles.Bold;

        // Setup node via SerializedObject
        var so = new SerializedObject(node);
        so.FindProperty("elementSymbol").stringValue = sym;
        so.FindProperty("background").objectReferenceValue = img;
        so.FindProperty("label").objectReferenceValue = tmp;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Call Setup at runtime, but set the color as default
        // The node's Refresh() will handle colors at runtime based on GameState
    }

    static void CreateLine(string name, Vector2 from, Vector2 to, Transform parent)
    {
        var go = new GameObject($"Line_{name}", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.transform.SetAsFirstSibling(); // behind nodes

        var img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.06f);

        var rt = go.GetComponent<RectTransform>();
        Vector2 mid = (from + to) / 2f;
        rt.anchorMin = rt.anchorMax = mid;

        // Calculate distance and angle in normalized space (approximate)
        float dx = (to.x - from.x) * 1200; // approximate pixel space
        float dy = (to.y - from.y) * 700;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(dist, 2);
        rt.localRotation = Quaternion.Euler(0, 0, angle);
    }

    // ======================== PERIODIC TABLE SCENE ========================

    static readonly int[,] ptLayout = {
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2},
        {3,4,0,0,0,0,0,0,0,0,0,0,5,6,7,8,9,10},
        {11,12,0,0,0,0,0,0,0,0,0,0,13,14,15,16,17,18},
        {19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36},
        {37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54},
        {55,56,0,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86},
        {87,88,0,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118},
    };

    static readonly string[] ptSymbols = {
        "","H","He","Li","Be","B","C","N","O","F","Ne",
        "Na","Mg","Al","Si","P","S","Cl","Ar",
        "K","Ca","Sc","Ti","V","Cr","Mn","Fe","Co","Ni","Cu","Zn","Ga","Ge","As","Se","Br","Kr",
        "Rb","Sr","Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn","Sb","Te","I","Xe",
        "Cs","Ba","","","","","","","","","","","","","","","","",
        "","","","Hf","Ta","W","Re","Os","Ir","Pt","Au","Hg","Tl","Pb","Bi","Po","At","Rn",
        "Fr","Ra","","","","","","","","","","","","","","","","",
        "","","","Rf","Db","Sg","Bh","Hs","Mt","Ds","Rg","Cn","Nh","Fl","Mc","Lv","Ts","Og"
    };

    static void BuildPeriodicTableScene()
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
        var bg = CreateUIElement("Background", canvasGO.transform);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.055f, 0.09f, 1f);
        StretchFill(bg);

        // Top Bar
        var topBar = CreateUIElement("TopBar", canvasGO.transform);
        var topImg = topBar.AddComponent<Image>();
        topImg.color = new Color(0.067f, 0.094f, 0.153f, 1f);
        var topRT = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 1);
        topRT.anchorMax = new Vector2(1, 1);
        topRT.pivot = new Vector2(0.5f, 1);
        topRT.sizeDelta = new Vector2(0, 60);
        topRT.anchoredPosition = Vector2.zero;

        var backBtnGO = CreateButton("BackButton", topBar.transform, "Back", new Vector2(70, 36));
        var backRT = backBtnGO.GetComponent<RectTransform>();
        backRT.anchorMin = backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.anchoredPosition = new Vector2(60, 0);

        var titleGO = CreateText("Title", topBar.transform, "Periodic Table", 18, TextAlignmentOptions.Center);
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.3f, 0);
        titleRT.anchorMax = new Vector2(0.7f, 1);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var rpGO = CreateText("RPText", topBar.transform, "5 RP", 14, TextAlignmentOptions.Right);
        var rpRT = rpGO.GetComponent<RectTransform>();
        rpRT.anchorMin = rpRT.anchorMax = new Vector2(1, 0.5f);
        rpRT.sizeDelta = new Vector2(120, 36);
        rpRT.anchoredPosition = new Vector2(-80, 0);
        rpGO.GetComponent<TextMeshProUGUI>().color = new Color(0.96f, 0.62f, 0.04f);

        // Hint text
        var hintGO = CreateText("Hint", canvasGO.transform, "Click on researched elements for details", 12, TextAlignmentOptions.Center);
        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.2f, 1);
        hintRT.anchorMax = new Vector2(0.8f, 1);
        hintRT.pivot = new Vector2(0.5f, 1);
        hintRT.anchoredPosition = new Vector2(0, -70);
        hintRT.sizeDelta = new Vector2(0, 24);
        hintGO.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);

        // -- Cell Prefab (create in-scene, not saved as asset) --
        var cellPrefabGO = CreatePTCellTemplate();
        cellPrefabGO.SetActive(false);
        cellPrefabGO.transform.SetParent(canvasGO.transform, false);

        // -- Grid Container --
        var gridGO = CreateUIElement("Grid", canvasGO.transform);
        var gridRT = gridGO.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.02f, 0.02f);
        gridRT.anchorMax = new Vector2(0.98f, 0.92f);
        gridRT.offsetMin = gridRT.offsetMax = Vector2.zero;

        var gridLayout = gridGO.AddComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 18;
        gridLayout.cellSize = new Vector2(52, 52);
        gridLayout.spacing = new Vector2(2, 2);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.padding = new RectOffset(10, 10, 10, 10);

        // Populate grid cells
        int rows = ptLayout.GetLength(0);
        int cols = ptLayout.GetLength(1);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int z = ptLayout[r, c];
                var cell = Object.Instantiate(cellPrefabGO, gridGO.transform);
                cell.SetActive(true);
                cell.name = z > 0 ? $"Cell_{(z < ptSymbols.Length ? ptSymbols[z] : z.ToString())}" : "Cell_Empty";

                var cellImg = cell.GetComponent<Image>();
                var texts = cell.GetComponentsInChildren<TextMeshProUGUI>(true);

                if (z == 0)
                {
                    cellImg.color = Color.clear;
                    foreach (var t in texts) t.text = "";
                    var cellBtn = cell.GetComponent<Button>();
                    if (cellBtn != null) cellBtn.interactable = false;
                }
                else
                {
                    string sym = z < ptSymbols.Length ? ptSymbols[z] : "";
                    if (texts.Length > 0) texts[0].text = z.ToString();
                    if (texts.Length > 1) texts[1].text = sym;
                    cellImg.color = new Color(0.1f, 0.13f, 0.21f, 1f);
                }
            }
        }

        // -- Popup Overlay --
        var popupOverlay = CreateUIElement("PopupOverlay", canvasGO.transform);
        var popupOvImg = popupOverlay.AddComponent<Image>();
        popupOvImg.color = new Color(0, 0, 0, 0.85f);
        StretchFill(popupOverlay);

        var popupPanel = CreateUIElement("PopupPanel", popupOverlay.transform);
        var popupPanelImg = popupPanel.AddComponent<Image>();
        popupPanelImg.color = new Color(0.067f, 0.094f, 0.153f, 1f);
        var popupPanelRT = popupPanel.GetComponent<RectTransform>();
        popupPanelRT.anchorMin = popupPanelRT.anchorMax = new Vector2(0.5f, 0.5f);
        popupPanelRT.sizeDelta = new Vector2(460, 380);

        float py = -20;
        var popTitleGO = CreateText("PopupTitle", popupPanel.transform, "", 22, TextAlignmentOptions.TopLeft);
        SetInfoTextPos(popTitleGO, ref py, 40);

        var popDescGO = CreateText("PopupDesc", popupPanel.transform, "", 13, TextAlignmentOptions.TopLeft);
        popDescGO.GetComponent<TextMeshProUGUI>().color = new Color(0.58f, 0.64f, 0.72f);
        SetInfoTextPos(popDescGO, ref py, 140);

        var popShellsGO = CreateText("PopupShells", popupPanel.transform, "", 12, TextAlignmentOptions.TopLeft);
        popShellsGO.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        SetInfoTextPos(popShellsGO, ref py, 24);

        var popValGO = CreateText("PopupValence", popupPanel.transform, "", 12, TextAlignmentOptions.TopLeft);
        popValGO.GetComponent<TextMeshProUGUI>().color = new Color(0.39f, 0.45f, 0.53f);
        SetInfoTextPos(popValGO, ref py, 24);

        py -= 10;
        var closeBtnGO = CreateButton("CloseButton", popupPanel.transform, "Close", new Vector2(120, 38));
        var closeBtnRT = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = closeBtnRT.anchorMax = new Vector2(0.5f, 1);
        closeBtnRT.anchoredPosition = new Vector2(0, py - 20);

        // -- Wire PeriodicTableUI --
        var ptUI = canvasGO.AddComponent<PeriodicTableUI>();
        var ptSO = new SerializedObject(ptUI);
        ptSO.FindProperty("gridParent").objectReferenceValue = gridGO.transform;
        ptSO.FindProperty("cellPrefab").objectReferenceValue = cellPrefabGO;
        ptSO.FindProperty("popup").objectReferenceValue = popupOverlay;
        ptSO.FindProperty("popupTitle").objectReferenceValue = popTitleGO.GetComponent<TextMeshProUGUI>();
        ptSO.FindProperty("popupDesc").objectReferenceValue = popDescGO.GetComponent<TextMeshProUGUI>();
        ptSO.FindProperty("popupShells").objectReferenceValue = popShellsGO.GetComponent<TextMeshProUGUI>();
        ptSO.FindProperty("popupValence").objectReferenceValue = popValGO.GetComponent<TextMeshProUGUI>();
        ptSO.FindProperty("popupClose").objectReferenceValue = closeBtnGO.GetComponent<Button>();
        ptSO.FindProperty("rpText").objectReferenceValue = rpGO.GetComponent<TextMeshProUGUI>();
        ptSO.FindProperty("backButton").objectReferenceValue = backBtnGO.GetComponent<Button>();
        ptSO.ApplyModifiedPropertiesWithoutUndo();

        // Save
        string scenePath = "Assets/Scenes/PeriodicTable.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Built PeriodicTable scene");
    }

    static GameObject CreatePTCellTemplate()
    {
        var cell = new GameObject("CellTemplate", typeof(RectTransform));
        var img = cell.AddComponent<Image>();
        img.color = new Color(0.1f, 0.13f, 0.21f, 1f);
        cell.AddComponent<Button>();

        // Atomic number (top-left, small)
        var zGO = new GameObject("Z", typeof(RectTransform));
        zGO.transform.SetParent(cell.transform, false);
        var zTMP = zGO.AddComponent<TextMeshProUGUI>();
        zTMP.fontSize = 8;
        zTMP.alignment = TextAlignmentOptions.TopLeft;
        zTMP.color = new Color(0.39f, 0.45f, 0.53f);
        var zRT = zGO.GetComponent<RectTransform>();
        zRT.anchorMin = Vector2.zero;
        zRT.anchorMax = Vector2.one;
        zRT.offsetMin = new Vector2(4, 0);
        zRT.offsetMax = new Vector2(0, -2);

        // Symbol (center, bold)
        var symGO = new GameObject("Sym", typeof(RectTransform));
        symGO.transform.SetParent(cell.transform, false);
        var symTMP = symGO.AddComponent<TextMeshProUGUI>();
        symTMP.fontSize = 15;
        symTMP.fontStyle = FontStyles.Bold;
        symTMP.alignment = TextAlignmentOptions.Center;
        symTMP.color = new Color(0.91f, 0.93f, 0.96f);
        var symRT = symGO.GetComponent<RectTransform>();
        symRT.anchorMin = Vector2.zero;
        symRT.anchorMax = Vector2.one;
        symRT.offsetMin = new Vector2(0, -4);
        symRT.offsetMax = Vector2.zero;

        return cell;
    }

    // ======================== BUILD SETTINGS ========================

    static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        string[] scenePaths = {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/ResearchTree.unity",
            "Assets/Scenes/PeriodicTable.unity",
            "Assets/Scenes/CaveWorld.unity"
        };
        foreach (var p in scenePaths)
        {
            if (File.Exists(p))
                scenes.Add(new EditorBuildSettingsScene(p, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Updated build settings with scenes");
    }
}
#endif
