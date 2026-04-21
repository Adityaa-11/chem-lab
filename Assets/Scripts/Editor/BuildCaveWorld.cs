#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.IO;

public class BuildCaveWorld
{
    const string ScenePath = "Assets/Scenes/CaveWorld.unity";
    const string TerrainDataPath = "Assets/Terrains/CaveTerrain.asset";
    const string RocksFolder = "Assets/Prefabs/Rocks";
    const string RockMaterialPath = "Assets/Materials/CaveRock.mat";
    const string TerrainMaterialPath = "Assets/Materials/CaveTerrain.mat";
    const string CeilingMeshPath = "Assets/Meshes/CaveCeiling.mesh";
    const string StalactiteMeshPath = "Assets/Meshes/Stalactite.mesh";

    const int HeightmapRes = 257;
    const float WorldSize = 200f;
    const float WorldHeight = 45f;
    const float FloorY = -5f;

    const float CeilingRadius = 60f;   // half-diameter of the inverted sphere dome
    const float CeilingCenterY = 8f;   // world-Y of dome center; top of dome ~ CeilingCenterY + CeilingRadius
    const int StalactiteCount = 28;

    [MenuItem("ChemGame/Build Cave World")]
    public static void Build()
    {
        DumpRenderPipelineEnvironment();

        EnsureFolder("Assets/Terrains");
        EnsureFolder(RocksFolder);
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Meshes");
        EnsureFolder("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        BuildTerrain();
        SetCaveAtmosphere();
        DimDirectionalLight();
        DumpAtmosphereState();

        var rockMat = EnsureRockMaterial();
        var rockPrefabs = MakeRockPrefabs(rockMat);
        PlaceRockSpawner(rockPrefabs);

        BuildCaveCeiling(rockMat);
        ScatterStalactites(rockMat);
        DumpCeilingAndStalactites();

        PlaceGameManager();
        PlaceInventorySystem();
        PlacePlayer();
        PlaceHeadlamp();
        PlaceNPCSpawner();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        DumpFinalSceneState();
        Debug.Log($"[ChemGame] Built CaveWorld at {ScenePath}");
    }

    // ========================= DIAGNOSTICS =========================

    static void DumpRenderPipelineEnvironment()
    {
        Debug.Log("[DIAG] ===== RENDER PIPELINE ENVIRONMENT =====");
        Debug.Log($"[DIAG] Unity version: {Application.unityVersion}");
        var current = GraphicsSettings.currentRenderPipeline;
        var defaultRp = GraphicsSettings.defaultRenderPipeline;
        var qualityRp = QualitySettings.renderPipeline;
        Debug.Log($"[DIAG] GraphicsSettings.currentRenderPipeline = '{current?.GetType().Name ?? "NULL"}'");
        Debug.Log($"[DIAG] GraphicsSettings.defaultRenderPipeline = '{defaultRp?.GetType().Name ?? "NULL"}' (name='{defaultRp?.name ?? "NULL"}')");
        Debug.Log($"[DIAG] QualitySettings.activeQualityLevel = {QualitySettings.GetQualityLevel()}");
        Debug.Log($"[DIAG] QualitySettings.renderPipeline = '{qualityRp?.GetType().Name ?? "NULL"}' (name='{qualityRp?.name ?? "NULL"}')");

        if (defaultRp != null)
        {
            Debug.Log($"[DIAG] defaultRP.defaultMaterial = '{defaultRp.defaultMaterial?.name ?? "NULL"}', shader='{defaultRp.defaultMaterial?.shader?.name ?? "NULL"}'");
            Debug.Log($"[DIAG] defaultRP.defaultTerrainMaterial = '{defaultRp.defaultTerrainMaterial?.name ?? "NULL"}', shader='{defaultRp.defaultTerrainMaterial?.shader?.name ?? "NULL"}'");
        }

        // ----- Actionable diagnoses -----
        if (current == null && defaultRp == null && qualityRp == null)
        {
            Debug.LogError(
                "[DIAG-FIX] NO RENDER PIPELINE IS ACTIVE. This is likely why materials render pink.\n" +
                "  → FIX: Edit → Project Settings → Graphics → 'Scriptable Render Pipeline Settings' field: assign the URP asset from Assets/.\n" +
                "  → ALSO: Edit → Project Settings → Quality → every quality level's 'Render Pipeline Asset' field: assign URP asset.\n" +
                "  → If no URP asset exists: Assets → Create → Rendering → URP Asset (with Universal Renderer).\n" +
                "  → Then re-run ChemGame → Build Cave World.");
        }
        else if (current == null && (defaultRp != null || qualityRp != null))
        {
            Debug.LogWarning(
                "[DIAG-FIX] currentRenderPipeline is null but a pipeline IS configured. Unity may not have applied it yet.\n" +
                "  → FIX: restart Unity, then re-run Build Cave World.");
        }
        Debug.Log("[DIAG] =========================================");
    }

    static void DumpShader(string label, Shader s)
    {
        if (s == null)
        {
            Debug.LogError($"[DIAG] {label}: shader is NULL");
            Debug.LogError(
                "[DIAG-FIX] Shader could not be located.\n" +
                "  → FIX: the URP package may be missing or corrupted.\n" +
                "  → Close Unity, delete 'Library/PackageCache/com.unity.render-pipelines.universal@*' from the project, reopen.\n" +
                "  → Or: Window → Package Manager → Universal RP → Remove → re-add version 17.0.4.");
            return;
        }
        var path = AssetDatabase.GetAssetPath(s);
        Debug.Log($"[DIAG] {label}: name='{s.name}', isSupported={s.isSupported}, renderQueue={s.renderQueue}, path='{path}'");

        if (!s.isSupported)
        {
            Debug.LogError(
                $"[DIAG-FIX] Shader '{s.name}' is NOT SUPPORTED in the current render pipeline. This will render pink.\n" +
                "  → PROBABLE CAUSE: the URP render pipeline isn't the active one at build time (see RENDER PIPELINE ENVIRONMENT above).\n" +
                "  → PRIMARY FIX: Edit → Project Settings → Graphics → assign URP asset to 'Scriptable Render Pipeline Settings'.\n" +
                "  → SECONDARY FIX: Edit → Project Settings → Quality → each tier's 'Render Pipeline Asset' → assign URP asset.\n" +
                "  → IF PIPELINE IS SET: the URP package may be corrupted — delete Library/PackageCache/com.unity.render-pipelines.universal@* and reopen Unity.");
        }
    }

    static void DumpMaterial(string label, Material m)
    {
        if (m == null) { Debug.Log($"[DIAG] {label}: material is NULL"); return; }
        DumpShader(label + ".shader", m.shader);
        Debug.Log($"[DIAG] {label}: name='{m.name}', color={m.color}, renderQueue={m.renderQueue}");
        Debug.Log($"[DIAG] {label}: hasProp _BaseColor={m.HasProperty("_BaseColor")}, _Color={m.HasProperty("_Color")}, _MainTex={m.HasProperty("_MainTex")}, _BaseMap={m.HasProperty("_BaseMap")}");
        Debug.Log($"[DIAG] {label}: shaderKeywords=[{string.Join(", ", m.shaderKeywords)}], passCount={m.passCount}");
        Debug.Log($"[DIAG] {label}: assetPath='{AssetDatabase.GetAssetPath(m)}'");
    }

    static void DumpAtmosphereState()
    {
        Debug.Log("[DIAG] ===== ATMOSPHERE + LIGHTING =====");
        Debug.Log($"[DIAG] RenderSettings.fog={RenderSettings.fog}, mode={RenderSettings.fogMode}, density={RenderSettings.fogDensity}, color={RenderSettings.fogColor}");
        Debug.Log($"[DIAG] RenderSettings.ambientMode={RenderSettings.ambientMode}, ambientLight={RenderSettings.ambientLight}, ambientIntensity={RenderSettings.ambientIntensity}");
        Debug.Log($"[DIAG] RenderSettings.skybox='{RenderSettings.skybox?.name ?? "NULL"}' shader='{RenderSettings.skybox?.shader?.name ?? "NULL"}'");
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            Debug.Log($"[DIAG] Light '{l.name}': type={l.type}, intensity={l.intensity}, color={l.color}");
        var cam = Camera.main;
        if (cam != null)
            Debug.Log($"[DIAG] Main Camera: clearFlags={cam.clearFlags}, backgroundColor={cam.backgroundColor}, nearClip={cam.nearClipPlane}, farClip={cam.farClipPlane}");
    }

    static void DumpCeilingAndStalactites()
    {
        Debug.Log("[DIAG] ===== CEILING + STALACTITES =====");
        var ceiling = GameObject.Find("CaveCeiling");
        if (ceiling != null)
        {
            var mr = ceiling.GetComponent<MeshRenderer>();
            var mf = ceiling.GetComponent<MeshFilter>();
            Debug.Log($"[DIAG] CaveCeiling found at {ceiling.transform.position}, scale={ceiling.transform.localScale}");
            DumpMaterial("  ceiling.MR.sharedMaterial", mr?.sharedMaterial);
            Debug.Log($"[DIAG]   mesh='{mf?.sharedMesh?.name}', vertCount={mf?.sharedMesh?.vertexCount ?? 0}, triCount={(mf?.sharedMesh?.triangles?.Length ?? 0) / 3}");
        }
        else Debug.Log("[DIAG] CaveCeiling NOT FOUND in scene");

        var stalRoot = GameObject.Find("Stalactites");
        if (stalRoot != null)
        {
            Debug.Log($"[DIAG] Stalactites root has {stalRoot.transform.childCount} children");
            if (stalRoot.transform.childCount > 0)
            {
                var first = stalRoot.transform.GetChild(0);
                DumpMaterial("  stalactite[0].MR.sharedMaterial", first.GetComponent<MeshRenderer>()?.sharedMaterial);
            }
        }
    }

    static void DumpFinalSceneState()
    {
        Debug.Log("[DIAG] ===== FINAL SCENE STATE =====");
        var terrain = Object.FindFirstObjectByType<Terrain>();
        if (terrain != null)
        {
            Debug.Log($"[DIAG] Terrain.materialTemplate = '{terrain.materialTemplate?.name ?? "NULL"}'");
            DumpMaterial("  terrain.materialTemplate", terrain.materialTemplate);

            int layerCount = terrain.terrainData.terrainLayers?.Length ?? 0;
            Debug.Log($"[DIAG] Terrain layers count = {layerCount}");
            if (terrain.terrainData.terrainLayers != null)
                for (int i = 0; i < terrain.terrainData.terrainLayers.Length; i++)
                {
                    var tl = terrain.terrainData.terrainLayers[i];
                    Debug.Log($"[DIAG]   layer[{i}]: name='{tl?.name}', diffuse='{tl?.diffuseTexture?.name}'");
                }

            // Actionable diagnosis
            if (layerCount == 0)
            {
                Debug.LogError(
                    "[DIAG-FIX] TERRAIN HAS ZERO LAYERS. URP Terrain/Lit renders pink without at least one layer.\n" +
                    "  → FIX (code): in BuildCaveWorld.BuildTerrain, after SetHeights, add a TerrainLayer asset with a diffuse texture and assign it to terrainData.terrainLayers.\n" +
                    "  → FIX (manual): select the CaveTerrain GameObject → Terrain component → Paint Texture tool → Edit Terrain Layers → Add Layer → pick any diffuse texture (e.g. from NatureStarterKit2/Textures/).\n" +
                    "  → After either fix the terrain will render the assigned layer's color/texture instead of magenta.");
            }
        }

        // Verify rock prefabs on disk have correct material refs
        int pinkRocks = 0;
        for (int i = 1; i <= 4; i++)
        {
            var path = $"{RocksFolder}/Rock_{i:00}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var mr = prefab.GetComponent<MeshRenderer>();
                var sh = mr?.sharedMaterial?.shader;
                bool supported = sh != null && sh.isSupported;
                Debug.Log($"[DIAG] {path}: MR.sharedMaterial='{mr?.sharedMaterial?.name ?? "NULL"}' shader='{sh?.name ?? "NULL"}' isSupported={supported}");
                if (!supported) pinkRocks++;
            }
        }
        if (pinkRocks > 0)
        {
            Debug.LogError(
                $"[DIAG-FIX] {pinkRocks} of 4 rock prefab(s) have an unsupported shader → they will render pink.\n" +
                "  → See earlier [DIAG-FIX] output on the rock material shader. The fix is whatever the shader-level diagnosis recommended (usually: set the URP asset in Project Settings → Graphics + Quality).");
        }

        // Chemist materials
        var coat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ChemistCoat.mat");
        DumpMaterial("ChemistCoat.mat", coat);
        var skin = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ChemistSkin.mat");
        DumpMaterial("ChemistSkin.mat", skin);

        Debug.Log("[DIAG] ================================");
        Debug.Log("[DIAG] If you see [DIAG-FIX] errors above, follow the → FIX steps in order.");
        Debug.Log("[DIAG] If no [DIAG-FIX] errors appear, all shaders/materials validated OK — any remaining visual issue is not a shader/material problem (check lighting, fog, camera position).");
    }

    // ========================= TERRAIN =========================

    static void BuildTerrain()
    {
        var td = new TerrainData
        {
            heightmapResolution = HeightmapRes,
            size = new Vector3(WorldSize, WorldHeight, WorldSize)
        };

        float[,] heights = new float[HeightmapRes, HeightmapRes];
        float center = (HeightmapRes - 1) * 0.5f;
        float maxRadius = center;

        for (int z = 0; z < HeightmapRes; z++)
        {
            for (int x = 0; x < HeightmapRes; x++)
            {
                float dx = x - center;
                float dz = z - center;
                float r = Mathf.Sqrt(dx * dx + dz * dz) / maxRadius;

                float bowl = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 0.97f, r));
                float coarse = (Mathf.PerlinNoise(x * 0.04f, z * 0.04f) - 0.5f) * 0.22f;
                float fine = (Mathf.PerlinNoise(x * 0.18f, z * 0.18f) - 0.5f) * 0.06f;

                float h = bowl * 0.85f + coarse * bowl + fine * 0.5f;
                heights[z, x] = Mathf.Clamp01(h);
            }
        }
        td.SetHeights(0, 0, heights);

        Debug.Log($"[DIAG] TerrainData created. size={td.size}, heightmapResolution={td.heightmapResolution}, terrainLayers.Length={td.terrainLayers?.Length ?? 0}");

        if (AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath) != null)
            AssetDatabase.DeleteAsset(TerrainDataPath);
        AssetDatabase.CreateAsset(td, TerrainDataPath);

        var terrainGO = Terrain.CreateTerrainGameObject(td);
        terrainGO.name = "CaveTerrain";
        terrainGO.transform.position = new Vector3(-WorldSize * 0.5f, FloorY, -WorldSize * 0.5f);
        terrainGO.layer = 0;

        var terrain = terrainGO.GetComponent<Terrain>();
        Debug.Log($"[DIAG] After CreateTerrainGameObject: terrain.materialTemplate='{terrain.materialTemplate?.name ?? "NULL"}' shader='{terrain.materialTemplate?.shader?.name ?? "NULL"}'");

        var terrainMat = EnsureTerrainMaterial();
        if (terrainMat != null) terrain.materialTemplate = terrainMat;

        Debug.Log($"[DIAG] After materialTemplate assignment: terrain.materialTemplate='{terrain.materialTemplate?.name ?? "NULL"}' shader='{terrain.materialTemplate?.shader?.name ?? "NULL"}'");
        Debug.Log($"[DIAG] Terrain final state: terrainData.terrainLayers.Length={terrain.terrainData.terrainLayers?.Length ?? 0}, alphamapLayers={terrain.terrainData.alphamapLayers}");
    }

    // ========================= ATMOSPHERE =========================

    static void SetCaveAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.055f;
        RenderSettings.fogColor = new Color(0.03f, 0.03f, 0.05f);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.08f, 0.1f);
        RenderSettings.ambientIntensity = 0.4f;
    }

    static void DimDirectionalLight()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = 0.1f;
                l.color = new Color(0.55f, 0.6f, 0.8f);
            }
        }
    }

    // ========================= MATERIALS =========================

    static Material EnsureRockMaterial()
    {
        Debug.Log("[DIAG] ===== ROCK MATERIAL =====");
        if (AssetDatabase.LoadAssetAtPath<Material>(RockMaterialPath) != null)
            AssetDatabase.DeleteAsset(RockMaterialPath);

        var mat = CreateURPLitMaterial("CaveRock", new Color(0.42f, 0.42f, 0.44f));
        DumpMaterial("rockMat (fresh, pre-save)", mat);

        AssetDatabase.CreateAsset(mat, RockMaterialPath);
        AssetDatabase.SaveAssets();

        var loaded = AssetDatabase.LoadAssetAtPath<Material>(RockMaterialPath);
        DumpMaterial("rockMat (reloaded from disk)", loaded);

        return mat;
    }

    static Material EnsureTerrainMaterial()
    {
        Debug.Log("[DIAG] ===== TERRAIN MATERIAL =====");
        if (AssetDatabase.LoadAssetAtPath<Material>(TerrainMaterialPath) != null)
            AssetDatabase.DeleteAsset(TerrainMaterialPath);

        var shader = ShaderLookupHelpers.FindURPTerrainLit();
        DumpShader("FindURPTerrainLit result", shader);

        if (shader == null)
        {
            Debug.Log("[DIAG] URP Terrain/Lit shader unavailable → returning null → Unity will auto-assign default terrain material.");
            return null;
        }

        var mat = new Material(shader) { name = "CaveTerrain" };
        SetColor(mat, new Color(0.3f, 0.3f, 0.33f));
        DumpMaterial("terrainMat (fresh, pre-save)", mat);

        AssetDatabase.CreateAsset(mat, TerrainMaterialPath);
        AssetDatabase.SaveAssets();

        var loaded = AssetDatabase.LoadAssetAtPath<Material>(TerrainMaterialPath);
        DumpMaterial("terrainMat (reloaded from disk)", loaded);

        return mat;
    }

    // Find URP Lit reliably. Tries five strategies because individual ones
    // have all been observed to fail on one project configuration or another.
    // See ShaderLookupHelpers below.
    static Material CreateURPLitMaterial(string name, Color color)
    {
        var shader = ShaderLookupHelpers.FindURPLit();
        if (shader == null)
        {
            Debug.LogError("[ChemGame] Could not locate URP Lit by any method. Using Standard — materials will render pink in URP. Check URP package install.");
            shader = Shader.Find("Standard");
        }
        var mat = new Material(shader) { name = name };
        SetColor(mat, color);
        return mat;
    }

    static void SetColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        mat.color = color;
    }

    // ========================= ROCK PREFABS =========================

    static GameObject[] MakeRockPrefabs(Material rockMat)
    {
        Debug.Log($"[DIAG] MakeRockPrefabs begin. rockMat arg: shader='{rockMat?.shader?.name ?? "NULL"}', name='{rockMat?.name}'");

        var prefabs = new GameObject[4];
        Vector3[] scales = {
            new Vector3(1.3f, 0.7f, 1.1f),
            new Vector3(0.9f, 1.2f, 1.4f),
            new Vector3(1.5f, 0.9f, 0.8f),
            new Vector3(1.1f, 0.6f, 1.3f)
        };
        PrimitiveType[] types = {
            PrimitiveType.Sphere, PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cube
        };

        for (int i = 0; i < 4; i++)
        {
            var go = GameObject.CreatePrimitive(types[i]);
            go.name = $"Rock_{i + 1:00}";
            go.transform.localScale = scales[i];

            var mr = go.GetComponent<MeshRenderer>();
            Debug.Log($"[DIAG] Rock_{i + 1:00}: primitive created. Default MR material shader='{mr.sharedMaterial?.shader?.name ?? "NULL"}'");

            mr.sharedMaterial = rockMat;
            Debug.Log($"[DIAG]   after assign: MR.sharedMaterial.shader='{mr.sharedMaterial?.shader?.name ?? "NULL"}', same instance as rockMat? {ReferenceEquals(mr.sharedMaterial, rockMat)}");

            go.AddComponent<Rock>();

            string path = $"{RocksFolder}/Rock_{i + 1:00}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                AssetDatabase.DeleteAsset(path);
            prefabs[i] = PrefabUtility.SaveAsPrefabAsset(go, path);

            // Reload the saved prefab and inspect what actually got written.
            var reloaded = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var reloadedMR = reloaded?.GetComponent<MeshRenderer>();
            Debug.Log($"[DIAG]   saved prefab reloaded: MR.sharedMaterial='{reloadedMR?.sharedMaterial?.name ?? "NULL"}' shader='{reloadedMR?.sharedMaterial?.shader?.name ?? "NULL"}'");

            Object.DestroyImmediate(go);
        }
        Debug.Log("[DIAG] MakeRockPrefabs end");
        return prefabs;
    }

    // ========================= SCENE OBJECTS =========================

    static void PlaceRockSpawner(GameObject[] rockPrefabs)
    {
        var go = new GameObject("RockSpawner");
        var spawner = go.AddComponent<RockSpawner>();
        var so = new SerializedObject(spawner);

        var arr = so.FindProperty("rockPrefabs");
        arr.arraySize = rockPrefabs.Length;
        for (int i = 0; i < rockPrefabs.Length; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = rockPrefabs[i];

        so.FindProperty("rockCount").intValue = 150;
        so.FindProperty("areaSize").vector2Value = new Vector2(45f, 45f);
        so.FindProperty("groundMask").intValue = 1;   // Default layer
        so.FindProperty("waterMask").intValue = 0;    // no water in cave
        so.FindProperty("scaleRange").vector2Value = new Vector2(0.6f, 1.8f);

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void PlaceGameManager()
    {
        var gmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameManager.prefab");
        if (gmPrefab != null) PrefabUtility.InstantiatePrefab(gmPrefab);
    }

    static void PlaceInventorySystem()
    {
        var go = new GameObject("InventorySystem");
        go.AddComponent<InventorySystem>();
    }

    static void PlacePlayer()
    {
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/StarterAssets/FirstPersonController/Prefabs/PlayerCapsule.prefab");
        if (playerPrefab == null)
        {
            Debug.LogWarning("[ChemGame] PlayerCapsule prefab not found — cave scene will have no player.");
            return;
        }
        var p = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        p.transform.position = new Vector3(0f, 8f, 0f);
        p.tag = "Player";
        if (p.GetComponent<BatterySystem>() == null)
            p.AddComponent<BatterySystem>();
        if (p.GetComponent<PlayerInteract>() == null)
            p.AddComponent<PlayerInteract>();

        var camRig = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/StarterAssets/FirstPersonController/Prefabs/PlayerFollowCamera.prefab");
        if (camRig != null) PrefabUtility.InstantiatePrefab(camRig);
    }

    static void PlaceNPCSpawner()
    {
        var chemist = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chemist.prefab");
        if (chemist == null)
        {
            chemist = BuildChemistPrefab.Build();
            if (chemist == null)
            {
                Debug.LogWarning("[ChemGame] Failed to build Chemist prefab — skipping NPCSpawner.");
                return;
            }
        }

        var go = new GameObject("NPCSpawner");
        var spawner = go.AddComponent<NPCSpawner>();
        var so = new SerializedObject(spawner);
        so.FindProperty("npcPrefab").objectReferenceValue = chemist;
        so.FindProperty("areaSize").vector2Value = new Vector2(38f, 38f);
        so.FindProperty("groundMask").intValue = 1;
        so.FindProperty("waterMask").intValue = 0;
        so.FindProperty("maxAttempts").intValue = 40;
        so.FindProperty("minDistanceFromPlayer").floatValue = 12f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void PlaceHeadlamp()
    {
        var go = new GameObject("Headlamp");
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 22f;
        light.intensity = 2.8f;
        light.color = new Color(1f, 0.9f, 0.7f);

        // Parent to the player so the light moves with them. If the player
        // prefab was missing (logged as a warning earlier), fall back to a
        // world-space position so the scene still has some illumination.
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            go.transform.SetParent(player.transform, false);
            go.transform.localPosition = new Vector3(0f, 1.6f, 0.2f); // head height, slight forward offset
        }
        else
        {
            go.transform.position = new Vector3(0f, 9f, 0f);
        }
    }

    // ========================= CEILING (inverted dome) =========================

    static void BuildCaveCeiling(Material rockMat)
    {
        // Start from the built-in sphere; instantiate its mesh so the shared
        // asset is never modified.
        var tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var source = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        var mesh = Object.Instantiate(source);
        Object.DestroyImmediate(tempSphere);
        mesh.name = "CaveCeiling";

        // Perlin vertex displacement for craggy ceiling (applied in local
        // coords of the unit sphere, scaled later by the transform).
        var verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];
            float n1 = Mathf.PerlinNoise(v.x * 2.5f + 17f, v.z * 2.5f + 91f) - 0.5f;
            float n2 = Mathf.PerlinNoise(v.y * 4.0f + 33f, v.x * 4.0f + 47f) - 0.5f;
            verts[i] = v + v.normalized * (n1 * 0.05f + n2 * 0.02f);
        }
        mesh.vertices = verts;

        // Reverse winding so the mesh renders on its INSIDE surface.
        var tris = mesh.triangles;
        for (int i = 0; i < tris.Length; i += 3)
        {
            int tmp = tris[i];
            tris[i] = tris[i + 1];
            tris[i + 1] = tmp;
        }
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (AssetDatabase.LoadAssetAtPath<Mesh>(CeilingMeshPath) != null)
            AssetDatabase.DeleteAsset(CeilingMeshPath);
        AssetDatabase.CreateAsset(mesh, CeilingMeshPath);

        var go = new GameObject("CaveCeiling");
        go.transform.position = new Vector3(0f, CeilingCenterY, 0f);
        go.transform.localScale = Vector3.one * (CeilingRadius * 2f); // unit sphere has radius 0.5

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = rockMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // ========================= STALACTITES =========================

    static void ScatterStalactites(Material rockMat)
    {
        var coneMesh = MakeStalactiteMesh();
        if (AssetDatabase.LoadAssetAtPath<Mesh>(StalactiteMeshPath) != null)
            AssetDatabase.DeleteAsset(StalactiteMeshPath);
        AssetDatabase.CreateAsset(coneMesh, StalactiteMeshPath);

        var root = new GameObject("Stalactites");
        for (int i = 0; i < StalactiteCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(4f, 40f);
            float x = Mathf.Cos(angle) * r;
            float z = Mathf.Sin(angle) * r;

            // Hang from roughly where the dome surface is at this (x, z)
            float domeLocal = CeilingRadius * CeilingRadius - x * x - z * z;
            if (domeLocal <= 0f) continue;
            float y = CeilingCenterY + Mathf.Sqrt(domeLocal) - Random.Range(0.5f, 3f);

            var go = new GameObject($"Stalactite_{i:00}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(0f, 360f), Random.Range(-4f, 4f));
            float scale = Random.Range(0.6f, 2.4f);
            go.transform.localScale = new Vector3(scale, scale * Random.Range(0.9f, 1.6f), scale);

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = coneMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = rockMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    static Mesh MakeStalactiteMesh()
    {
        const int sides = 8;
        const float height = 3f;
        const float radius = 0.6f;

        var mesh = new Mesh { name = "Stalactite" };
        Vector3[] verts = new Vector3[sides + 1];
        verts[0] = new Vector3(0f, -height, 0f); // apex points down
        for (int i = 0; i < sides; i++)
        {
            float a = (float)i / sides * Mathf.PI * 2f;
            verts[1 + i] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
        }

        int[] tris = new int[sides * 3];
        for (int i = 0; i < sides; i++)
        {
            tris[i * 3 + 0] = 0;
            tris[i * 3 + 1] = 1 + i;
            tris[i * 3 + 2] = 1 + (i + 1) % sides;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    // ========================= HELPERS =========================

    static void EnsureFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
#endif
