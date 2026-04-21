#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class BuildChemistPrefab
{
    const string PrefabPath = "Assets/Prefabs/Chemist.prefab";
    const string CoatMaterialPath = "Assets/Materials/ChemistCoat.mat";
    const string SkinMaterialPath = "Assets/Materials/ChemistSkin.mat";

    [MenuItem("ChemGame/Build Chemist Prefab")]
    public static GameObject Build()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Materials");

        var coatMat = EnsureMaterial(CoatMaterialPath, "ChemistCoat", new Color(0.18f, 0.45f, 0.75f));
        var skinMat = EnsureMaterial(SkinMaterialPath, "ChemistSkin", new Color(0.95f, 0.77f, 0.62f));

        var root = new GameObject("Chemist");
        root.AddComponent<Chemist>();

        // Body capsule (capsule primitive is 2m tall, radius 0.5, centered at origin).
        // Scale y=0.8 -> 1.6m tall; place so feet sit at y=0.
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.55f, 0.8f, 0.55f);
        body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        body.GetComponent<MeshRenderer>().sharedMaterial = coatMat;

        // Head sphere
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localScale = Vector3.one * 0.36f;
        head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
        head.GetComponent<MeshRenderer>().sharedMaterial = skinMat;

        // Collider on the root so raycasts hit the whole NPC. Replace the
        // default sphere/capsule colliders on body/head with a single capsule.
        Object.DestroyImmediate(body.GetComponent<Collider>());
        Object.DestroyImmediate(head.GetComponent<Collider>());
        var rootCap = root.AddComponent<CapsuleCollider>();
        rootCap.height = 2.1f;
        rootCap.radius = 0.45f;
        rootCap.center = new Vector3(0f, 1.05f, 0f);

        // World-space name tag above head
        var tag = new GameObject("NameTag", typeof(RectTransform));
        tag.transform.SetParent(root.transform, false);
        tag.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        var tagRT = tag.GetComponent<RectTransform>();
        tagRT.sizeDelta = new Vector2(200, 50);
        tagRT.localScale = Vector3.one * 0.01f;

        var canvas = tag.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        tag.AddComponent<Billboard>();

        var txtGO = new GameObject("Text", typeof(RectTransform));
        txtGO.transform.SetParent(tag.transform, false);
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text = "Chemist";
        txt.fontSize = 24;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontStyle = FontStyles.Bold;
        txt.color = new Color(0.13f, 0.83f, 0.93f);
        var txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            AssetDatabase.DeleteAsset(PrefabPath);
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ChemGame] Built Chemist prefab at {PrefabPath}");
        return prefab;
    }

    [MenuItem("ChemGame/Add Chemist to Forest")]
    public static void AddToForest()
    {
        const string scenePath = "Assets/Scenes/ForestWorld.unity";
        if (!File.Exists(scenePath))
        {
            Debug.LogError("[ChemGame] ForestWorld.unity not found.");
            return;
        }

        var chemist = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (chemist == null)
        {
            chemist = Build();
            if (chemist == null) return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath);

        // Idempotent: remove any existing NPCSpawner before adding a fresh one.
        foreach (var existing in Object.FindObjectsByType<NPCSpawner>(FindObjectsSortMode.None))
            Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("NPCSpawner");
        var spawner = go.AddComponent<NPCSpawner>();
        var so = new SerializedObject(spawner);
        so.FindProperty("npcPrefab").objectReferenceValue = chemist;
        so.FindProperty("areaSize").vector2Value = new Vector2(100f, 100f);
        // ForestWorld's TreeSpawner uses groundMask=9 (layers 0+3), waterMask=16 (layer 4/Water).
        so.FindProperty("groundMask").intValue = 9;
        so.FindProperty("waterMask").intValue = 16;
        so.FindProperty("maxAttempts").intValue = 60;
        so.FindProperty("minDistanceFromPlayer").floatValue = 20f;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ChemGame] Added NPCSpawner to ForestWorld");
    }

    static Material EnsureMaterial(string path, string name, Color color)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var shader = ShaderLookupHelpers.FindURPLit();
        if (shader == null)
        {
            Debug.LogError("[ChemGame] Could not locate URP Lit by any method. Using Standard — materials will render pink in URP. Check URP package install.");
            shader = Shader.Find("Standard");
        }
        var mat = new Material(shader) { name = name };
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void EnsureFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
#endif
