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
        EnsureFolder("Assets/Terrains");
        EnsureFolder(RocksFolder);
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Meshes");
        EnsureFolder("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        BuildTerrain();
        SetCaveAtmosphere();
        DimDirectionalLight();

        var rockMat = EnsureRockMaterial();
        var rockPrefabs = MakeRockPrefabs(rockMat);
        PlaceRockSpawner(rockPrefabs);

        BuildCaveCeiling(rockMat);
        ScatterStalactites(rockMat);

        PlaceGameManager();
        PlaceInventorySystem();
        PlacePlayer();
        PlaceHeadlamp();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ChemGame] Built CaveWorld at {ScenePath}");
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

        if (AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath) != null)
            AssetDatabase.DeleteAsset(TerrainDataPath);
        AssetDatabase.CreateAsset(td, TerrainDataPath);

        var terrainGO = Terrain.CreateTerrainGameObject(td);
        terrainGO.name = "CaveTerrain";
        terrainGO.transform.position = new Vector3(-WorldSize * 0.5f, FloorY, -WorldSize * 0.5f);
        terrainGO.layer = 0;

        var terrain = terrainGO.GetComponent<Terrain>();
        var terrainMat = EnsureTerrainMaterial();
        if (terrainMat != null) terrain.materialTemplate = terrainMat;
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
        var existing = AssetDatabase.LoadAssetAtPath<Material>(RockMaterialPath);
        if (existing != null) return existing;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader) { name = "CaveRock" };
        SetColor(mat, new Color(0.42f, 0.42f, 0.44f));
        AssetDatabase.CreateAsset(mat, RockMaterialPath);
        return mat;
    }

    static Material EnsureTerrainMaterial()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(TerrainMaterialPath);
        if (existing != null) return existing;

        var shader = Shader.Find("Universal Render Pipeline/Terrain/Lit")
                     ?? Shader.Find("Nature/Terrain/Standard")
                     ?? Shader.Find("Standard");
        var mat = new Material(shader) { name = "CaveTerrain" };
        SetColor(mat, new Color(0.3f, 0.3f, 0.33f));
        AssetDatabase.CreateAsset(mat, TerrainMaterialPath);
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
            go.GetComponent<MeshRenderer>().sharedMaterial = rockMat;
            go.AddComponent<Rock>();

            string path = $"{RocksFolder}/Rock_{i + 1:00}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                AssetDatabase.DeleteAsset(path);
            prefabs[i] = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
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
