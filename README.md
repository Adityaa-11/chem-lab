# chem-lab — Setup Guide

Unity 6 URP chemistry game. Explore biomes, collect elements, research the periodic table, construct compounds.

## Prerequisites

- **Unity 6000.0.46f1** — install via [Unity Hub](https://unity.com/download) → Installs → Install Editor → Archive → pick `6000.0.46f1`
- Open this repo in Unity Hub (`Add project from disk`)
- Wait for the first import to finish (a few minutes — Unity compiles shaders + generates metas)

## First-time setup — click these 3 menu items in order

All three live under the **`ChemGame`** menu in Unity's top menu bar.

### 1. `ChemGame → Fix URP Configuration`

Assigns the Universal Render Pipeline asset to `GraphicsSettings` and every quality tier.
Creates a URP asset at `Assets/Settings/URP-ChemLab.asset` if one doesn't exist yet.

**Watch the Console for:**

- ✅ `✓ URP asset passes health check` → good, proceed to step 2
- ❌ `URP asset has ZERO renderers` → stop.
  In the Project panel, **right-click → Create → Rendering → URP Asset (with Universal Renderer)**, save the asset, then re-run `Fix URP Configuration`.

### 2. `ChemGame → Run Full Setup`

Regenerates:

- All 12 element ScriptableObjects (H, C, N, O, Al, Fe, Cu, Sn, Si, Ca, S, Au)
- `GameManager.prefab` with every element wired in
- `ResearchTree.unity` + `PeriodicTable.unity` scenes
- `Chemist.prefab`
- Build settings scene list

**Watch for:** `=== Elemental Explorer setup complete! ===`

### 3. `ChemGame → Build Cave World`

Generates the entire CaveWorld scene procedurally:

- Bowl-shaped terrain (procedural Perlin heightmap)
- Gray rocky dome ceiling + 28 stalactites
- 4 rock prefabs → scattered at runtime by the RockSpawner
- Dim lighting + exponential fog for cave atmosphere
- FPS player with headlamp + GameManager + InventorySystem + NPC spawner

**Watch for:** zero red `[DIAG-FIX]` errors, then `[ChemGame] Built CaveWorld at Assets/Scenes/CaveWorld.unity`

## Playing the game

1. In the Project panel, open **`Assets/Scenes/CaveWorld.unity`** (or `ForestWorld.unity`)
2. Press **▶ Play** (top-center of the Editor)
3. Use **WASD + mouse** to move/look
4. **Left-click** on:
   - Trees (Forest) → zap for carbon/oxygen/hydrogen (battery cost: 60)
   - Rocks (Cave) → zap for silicon/calcium/iron/copper/sulfur/gold (battery cost: 60)
   - The blue capsule **Chemist** NPC → talk, get a random element lesson + 1 RP

Rocks should be gray, terrain gray-brown, Chemist blue. **If anything renders pink**, jump to Troubleshooting below.

## Optional — Chemist in the forest

`ChemGame → Add Chemist to Forest` opens `ForestWorld.unity`, adds an `NPCSpawner` configured for the forest's layer masks, and saves. The Chemist will spawn within 100m of the player each time the scene loads.

## Troubleshooting

### Everything is pink (missing shader)

99% of the time this is URP not being the active render pipeline. Run `ChemGame → Fix URP Configuration` again. If the Console shows a `[DIAG-FIX]` error, follow the `→ FIX` instructions it prints verbatim.

If URP is active but still pink: close Unity, delete `Library/PackageCache/com.unity.render-pipelines.universal@*` in the project root, reopen Unity. URP re-downloads fresh.

### Terrain is pink but rocks aren't

Terrain needs at least one `TerrainLayer`. `Build Cave World` adds one automatically. If you see the `TERRAIN HAS ZERO LAYERS` diagnostic, re-run `Build Cave World`.

### `ChemGame` menu doesn't appear in the menu bar

Compile errors in the Editor scripts. Open **`Window → General → Console`** and look for red errors. Fix those, Unity will recompile, menu appears.

### Rocks don't spawn when I press Play

Check the `RockSpawner` GameObject in the scene hierarchy — its `rockPrefabs` array should have 4 entries. If empty, re-run `Build Cave World`.

### The Chemist isn't anywhere in the forest/cave

`NPCSpawner` spawns within `areaSize` of the player's starting position. If you walked far from spawn quickly, walk back. If still missing, check the `NPCSpawner` component's `groundMask` — forest uses `9`, cave uses `1`.

## Project structure

```
Assets/
├── Scenes/           7 scenes: MainMenu, WorldSelect, Forest, Cave, ResearchTree, PeriodicTable, Construct, Compendium
├── Scripts/
│   ├── Core/         GameState singleton (progression, RP, inventory)
│   ├── Data/         ElementData + CompoundData ScriptableObjects
│   ├── Battery/      BatterySystem + BatteryUI
│   ├── Mechanics/    InventorySystem + PlayerInteract
│   ├── Interactables/ Tree, Rock, Chemist
│   ├── WorldGen/     TreeSpawner, RockSpawner, NPCSpawner
│   ├── UI/           MainMenu, WorldSelect, PeriodicTableUI, ResearchTree UI, DialogUI, Billboard, etc.
│   └── Editor/       AutoSetup, BuildMainMenu, BuildCaveWorld, BuildChemistPrefab, FixURPConfiguration, ShaderLookupHelpers
├── ScriptableObjects/Elements/   12 element .asset files (H, C, N, O, Al, Fe, Cu, Sn, Si, Ca, S, Au)
├── Prefabs/          GameManager, Chemist, Rocks/
├── Materials/        CaveRock, CaveTerrain, ChemistCoat, ChemistSkin, WaterMaterial
├── Terrains/         CaveTerrain.asset + CaveTerrainLayer + procedural diffuse PNG
├── Meshes/           CaveCeiling + Stalactite (generated)
├── Settings/         URP-ChemLab.asset (render pipeline)
├── NatureStarterKit2/  free Asset Store pack — tree/bush prefabs for forest
├── LowlyPoly/          free pack — grass textures
└── StarterAssets/      Unity's official FirstPersonController
```

All "generated" folders above (Terrains, Meshes, Prefabs/Rocks, Settings/URP-ChemLab, and all element SOs) come from running the 3 menu items. Don't commit manual edits to those — regenerate via the menus.

## Branches

- `main` — integration branch
- `cave-world-+-rocks` — cave environment + rock elements (merged)
- `chemist-npc` — shared NPC that teaches biome-appropriate elements (merged)
- feature branches use the `topic-+-subtopic` naming pattern
