# CaveWorld — setup instructions

Click these 3 menu items **in this exact order** to build the cave scene. All three live under the **`ChemGame`** menu in Unity's top menu bar.

## 1. `ChemGame → Fix URP Configuration`

Sets up the render pipeline (finds or creates a URP asset and assigns it to Graphics + every quality tier).

**Watch the Console for:**

- ✅ `✓ URP asset passes health check` → good, proceed to #2
- ❌ `URP asset has ZERO renderers` → **stop**. Right-click in Project panel → `Create → Rendering → URP Asset (with Universal Renderer)`. Then run this menu again.

## 2. `ChemGame → Run Full Setup`

Regenerates element SOs, GameManager prefab, ResearchTree + PeriodicTable scenes, and the Chemist prefab — all with the now-active URP pipeline so materials get proper URP Lit shaders.

**Watch for:** `=== Elemental Explorer setup complete! ===`

## 3. `ChemGame → Build Cave World`

Generates the cave scene + terrain + rocks + ceiling + stalactites + NPC spawner.

**Watch for:** zero red `[DIAG-FIX]` errors, then `[ChemGame] Built CaveWorld at Assets/Scenes/CaveWorld.unity`

## Then:

- Open `Assets/Scenes/CaveWorld.unity` (double-click in Project panel)
- Press **▶** (top-center Play button)

Rocks should be gray, terrain gray-brown, Chemist blue. **No pink.**

## If you see pink

Whichever step logs a red `[DIAG-FIX]` error, follow the `→ FIX` steps printed alongside it verbatim, then retry from that step.
