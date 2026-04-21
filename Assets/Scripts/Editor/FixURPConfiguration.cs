#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// One-click fix for "no render pipeline active" — the most common root cause
// of pink materials in this project. Finds (or creates) a URP render pipeline
// asset and assigns it to GraphicsSettings.defaultRenderPipeline and every
// QualitySettings quality level.
public static class FixURPConfiguration
{
    const string URPAssetSavePath = "Assets/Settings/URP-ChemLab.asset";

    [MenuItem("ChemGame/Fix URP Configuration")]
    public static void Fix()
    {
        var urpAsset = FindExistingURPAsset();
        if (urpAsset == null)
        {
            urpAsset = TryCreateURPAsset();
        }

        if (urpAsset == null)
        {
            Debug.LogError(
                "[ChemGame] Could not find or create a URP RenderPipelineAsset.\n" +
                "  → Manual fix: in the Project window right-click → Create → Rendering → URP Asset (with Universal Renderer).\n" +
                "  → Save it anywhere (e.g. Assets/Settings/URP-ChemLab.asset).\n" +
                "  → Then re-run ChemGame → Fix URP Configuration.");
            return;
        }

        Debug.Log($"[ChemGame] Using URP asset: '{AssetDatabase.GetAssetPath(urpAsset)}'");
        DumpURPAssetHealth(urpAsset);

        GraphicsSettings.defaultRenderPipeline = urpAsset;

        int originalLevel = QualitySettings.GetQualityLevel();
        int count = QualitySettings.names.Length;
        for (int i = 0; i < count; i++)
        {
            QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
            QualitySettings.renderPipeline = urpAsset;
        }
        QualitySettings.SetQualityLevel(originalLevel, applyExpensiveChanges: false);

        AssetDatabase.SaveAssets();

        Debug.Log($"[ChemGame] ✓ URP assigned to GraphicsSettings.defaultRenderPipeline.");
        Debug.Log($"[ChemGame] ✓ URP assigned to all {count} quality levels.");
        Debug.Log("[ChemGame] Now re-run: ChemGame → Build Cave World. Pink materials should be resolved.");
    }

    static RenderPipelineAsset FindExistingURPAsset()
    {
        // Look for any RenderPipelineAsset that is specifically UniversalRP.
        var guids = AssetDatabase.FindAssets("t:RenderPipelineAsset");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            if (asset != null && asset.GetType().Name == "UniversalRenderPipelineAsset")
            {
                return asset;
            }
        }
        return null;
    }

    // Prints the four fields that determine whether a URP asset can actually
    // produce frames. If rendererCount is 0, the asset is broken and nothing
    // it drives will render — everything will be pink regardless of what's
    // assigned to GraphicsSettings.
    static void DumpURPAssetHealth(RenderPipelineAsset urpAsset)
    {
        Debug.Log("[ChemGame] URP asset health check:");
        Debug.Log($"  defaultMaterial         = '{urpAsset.defaultMaterial?.name ?? "NULL"}'");
        Debug.Log($"  defaultShader           = '{urpAsset.defaultShader?.name ?? "NULL"}'");
        Debug.Log($"  renderPipelineShaderTag = '{urpAsset.renderPipelineShaderTag ?? "NULL"}'");

        var t = urpAsset.GetType();
        var rcProp = t.GetProperty("rendererCount");
        int count = -1;
        if (rcProp != null)
        {
            try { count = (int)rcProp.GetValue(urpAsset); }
            catch { /* ignore */ }
        }
        Debug.Log($"  rendererCount           = {(count >= 0 ? count.ToString() : "UNKNOWN")}");

        if (count == 0)
        {
            Debug.LogError(
                "[ChemGame] URP asset has ZERO renderers — it cannot produce frames and everything will render pink.\n" +
                "  → FIX: delete the current URP asset, then in the Project panel:\n" +
                "     right-click → Create → Rendering → URP Asset (with Universal Renderer).\n" +
                "     That menu creates a valid Renderer Data alongside the asset.\n" +
                "  → Then re-run ChemGame → Fix URP Configuration.");
        }
        else if (urpAsset.defaultMaterial == null || urpAsset.defaultShader == null)
        {
            Debug.LogWarning(
                "[ChemGame] URP asset is missing defaultMaterial or defaultShader — renderers exist but may not issue valid draw calls.\n" +
                "  → If rocks still render pink after Build Cave World, recreate the URP asset via the Project panel menu as above.");
        }
        else
        {
            Debug.Log("[ChemGame] ✓ URP asset passes health check (has renderers and defaults).");
        }
    }

    static RenderPipelineAsset TryCreateURPAsset()
    {
        // Create via reflection so this script doesn't need to reference URP
        // assemblies directly. URP's type lives in Unity.RenderPipelines.Universal.Runtime.
        var urpType = System.Type.GetType(
            "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");

        if (urpType == null)
        {
            Debug.LogError("[ChemGame] URP type not found in loaded assemblies. Is the URP package actually installed?");
            return null;
        }

        Directory.CreateDirectory("Assets/Settings");
        var instance = ScriptableObject.CreateInstance(urpType) as RenderPipelineAsset;
        if (instance == null)
        {
            Debug.LogError("[ChemGame] Created URP ScriptableObject but it is not a RenderPipelineAsset. Something is deeply wrong.");
            return null;
        }

        AssetDatabase.CreateAsset(instance, URPAssetSavePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ChemGame] Created new URP asset at {URPAssetSavePath}. Note: it has no Universal Renderer assigned — Unity will auto-create a default one on first use. If rendering looks wrong, open the asset and check the Renderer List.");
        return instance;
    }
}
#endif
