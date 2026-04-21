#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Five-strategy URP shader resolver.
//
// Single-strategy lookups are unreliable in URP projects at Editor time:
// - Shader.Find can return null if the shader cache hasn't populated yet.
// - GraphicsSettings.defaultRenderPipeline is null in projects that only
//   set the pipeline per-quality-tier.
// - Package paths changed between URP 10 → 12 → 17.
// Combining all five makes the lookup work regardless of which one happens
// to be broken in a given project configuration.
public static class ShaderLookupHelpers
{
    public static Shader FindURPLit()
    {
        return Find(
            shaderName: "Universal Render Pipeline/Lit",
            packagePaths: new[] {
                "Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader"
            },
            defaultMaterialGrabber: rp => rp?.defaultMaterial?.shader
        );
    }

    public static Shader FindURPTerrainLit()
    {
        return Find(
            shaderName: "Universal Render Pipeline/Terrain/Lit",
            packagePaths: new[] {
                "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLit.shader"
            },
            defaultMaterialGrabber: rp => rp?.defaultTerrainMaterial?.shader
        );
    }

    // ------------------------------------------------------------------

    static Shader Find(
        string shaderName,
        string[] packagePaths,
        System.Func<RenderPipelineAsset, Shader> defaultMaterialGrabber)
    {
        Shader s;

        // (1) Standard name-based lookup.
        s = Shader.Find(shaderName);
        if (s != null) { Log(shaderName, 1, s); return s; }

        // (2) Load the shader file directly from URP's package path. This
        // works at Editor time even when Shader.Find's cache is cold.
        foreach (var p in packagePaths)
        {
            s = AssetDatabase.LoadAssetAtPath<Shader>(p);
            if (s != null) { Log(shaderName, 2, s); return s; }
        }

        // (3) Grab it off the project-wide default render pipeline asset.
        s = defaultMaterialGrabber(GraphicsSettings.defaultRenderPipeline);
        if (s != null) { Log(shaderName, 3, s); return s; }

        // (4) Try the currently-active quality tier's render pipeline.
        s = defaultMaterialGrabber(QualitySettings.renderPipeline);
        if (s != null) { Log(shaderName, 4, s); return s; }

        // (5) Last resort: scan every Material asset in the project for any
        // already using the target shader, and reuse its reference.
        var guids = AssetDatabase.FindAssets("t:Material");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null && m.shader != null && m.shader.name == shaderName)
            {
                Log(shaderName, 5, m.shader);
                return m.shader;
            }
        }

        return null;
    }

    static void Log(string target, int strategy, Shader s)
    {
        Debug.Log($"[ChemGame] Resolved '{target}' via strategy {strategy} → actual shader name: '{s.name}'");
    }
}
#endif
