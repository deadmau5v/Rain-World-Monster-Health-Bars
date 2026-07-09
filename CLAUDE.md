# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Rain World mod** that displays health bars above creatures using BepInEx and MonoMod IL hooks. The mod uses Rain World's FSprite rendering system and the Remix configuration interface for user settings.

**Version:** 1.5.0
**Target Game:** Rain World v1.11.8
**Framework:** BepInEx plugin system with .NET Framework 4.8

## Build System

**IMPORTANT:** Do not attempt to build/compile automatically. The user manages builds separately.

### Project Structure
- `Monster Health Bars.csproj` - .NET 4.8 SDK project
- `lib/` - Contains Rain World and BepInEx DLLs (HOOKS-Assembly-CSharp, PUBLIC-Assembly-CSharp, UnityEngine, BepInEx)
- References are marked `<Private>False</Private>` to prevent copying DLLs to output

### Dependencies Location
All game and framework DLLs are referenced from `../lib/` relative to the .csproj file:
- BepInEx.dll
- HOOKS-Assembly-CSharp.dll (MonoMod IL hooks)
- PUBLIC-Assembly-CSharp.dll (Rain World publicized assembly)
- UnityEngine assemblies (CoreModule, IMGUIModule, InputLegacyModule)

## Architecture

### Core Components

**HealthBarMod.cs** - BepInEx plugin entry point
- Manages hook lifecycle with safety flags (`_isInit`, `_hooksRegistered`)
- Registers hooks in `RainWorld_OnModsInit` to avoid duplicate registration
- Key hooks:
  - `On.RainWorld.OnModsInit` - Registers configuration and drawing hooks
  - `On.HUD.HUD.Draw` - Main rendering loop, called every frame
  - `On.RainWorldGame.ShutDownProcess` - Cleanup when game closes
- **Hook registration pattern:** Only subscribe to `OnModsInit` in `OnEnable()`, then register all other hooks once inside `RainWorld_OnModsInit` to prevent duplicate subscriptions

**HealthBarConfig.cs** - Remix configuration interface
- Extends `OptionInterface` from Rain World's Remix mod system
- Multi-language support via `InGameTranslator.LanguageID` dictionary (English + Chinese currently implemented)
- Configuration options:
  - `EnableHealthBars` - Master toggle (default: true)
  - `ShowPlayerHealthBar` - Show player's health bar (default: false)
  - `HideWhenFullHealth` - Hide when creature at full health (default: true)
  - `BarWidth` - Bar width in pixels (range: 20-100, default: 40)
  - `BarHeight` - Bar height in pixels (range: 2-10, default: 4)
  - `MaxDistance` - Fade distance (range: 400-1600, default: 800)
- **Note:** The `Translate()` method uses `new` keyword to intentionally hide inherited `OptionInterface.Translate()` for custom multi-language implementation

**HealthBarManager.cs** - Core rendering and creature tracking
- Static manager with Dictionary<Creature, HealthBarData> for per-creature tracking
- `DrawHealthBars()` - Main update loop, handles cleanup and visibility
- `IsCreatureHidden()` - Checks if creature is in shortcut/pipe (`creature.inShortcut`)
- Cleanup logic:
  - Immediate removal: Creature leaves room or `slatedForDeletetion`
  - Death animation: 1-second fade-out before removal
  - Pipe hiding: Temporarily hides sprites when `creature.inShortcut` is true

**HealthBarData class** - Individual health bar state and rendering
- FSprite rendering with 3 sprite types:
  - `_backgroundSprite` - Black background
  - `_healthSprite` - Colored health indicator (green → yellow → red)
  - `_cornerSprites[4]` - Rounded corners (separate sprites for visual effect)
- **Position smoothing:** Uses `Vector2.Lerp()` with `SmoothFactor = 0.3f` to reduce jitter
- **Death animation:** When creature dies, triggers 1-second fade-out:
  - `_isDying` flag enables death state
  - `_deathTimer` tracks elapsed time
  - `_deathAlphaMultiplier` lerps from 1→0 over `DeathFadeTime`
  - Health bar stays red (0 health) and follows corpse during fade
- **Alpha calculation:** Base alpha from distance, multiplied by `_deathAlphaMultiplier` during death fade
- **Coordinate system:** Converts creature world position to screen space via `camera.pos`

### Rain World Modding Specifics

**MonoMod IL Hooks Pattern:**
```csharp
On.RainWorld.OnModsInit += RainWorld_OnModsInit;

private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
{
    orig(self); // Always call original method first
    // Your code here
}
```

**FSprite Rendering:**
- Sprites must be added to `camera.hud.fContainers[1]` for HUD layer
- Use FContainer for grouping related sprites
- Sprite lifecycle: Create → Add to container → Update position/alpha → Remove from container

**Creature State Tracking:**
- `creature.dead` - Boolean death flag
- `creature.State.alive` - Alive state (more reliable than `dead`)
- `creature.inShortcut` - True when in pipes/shortcuts
- `creature.stun` - Stun counter (used for health approximation)
- `creature.slatedForDeletetion` - Marked for removal from room

**Health Calculation Strategy:**
Rain World doesn't expose creature health directly. This mod uses:
- `creature.Template.baseDamageResistance` as max health base
- `creature.Template.baseStunResistance` as multiplier
- `creature.stun` counter to approximate damage (higher stun = lower displayed health)
- Death state (dead/!alive) sets health to 0

### Multi-Language Support

**Translation System:**
- Uses Rain World's standard translation file system
- Translation files located in `translation/` folder
- File naming format: `Monster Health Bars-{language}.txt`
- Supported languages: English, Chinese, Japanese, Korean, French, German, Spanish, Italian, Portuguese, Russian

**Implementation:**
1. **Mod List Translation (modinfo.json):**
   - `name` and `description` use translation keys (e.g., `d5v.healthbar_modname`)
   - Rain World automatically loads and translates from `translation/*.txt` files
   - Translation keys format: `d5v.healthbar_modname : Translated Name`

2. **Configuration UI Translation (HealthBarConfig.cs):**
   - In-game options menu uses custom `Translate()` method
   - Dictionary structure: `Dictionary<LanguageID, Dictionary<string, string>>`
   - Detects language via `Custom.rainWorld.inGameTranslator.currentLanguage`

**Adding New Languages:**
1. Create `translation/Monster Health Bars-{language}.txt`
2. Add translation keys:
   ```
   d5v.healthbar_modname : Your Translated Name
   d5v.healthbar_moddesc : Your Translated Description
   ```
3. Add to `HealthBarConfig.Translate()` dictionary for UI strings

## Linux/Proton Setup

This mod is developed on Linux using Proton/Wine. BepInEx requires winhttp.dll configuration:

**Setup scripts:**
- `setup-protontricks.sh` - Installs protontricks (Flatpak)
- `configure-bepinex-wine.sh` - Configures DLL override via protontricks

**Alternative method (recommended):**
Add to Rain World's Steam launch options:
```
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

This ensures BepInEx's winhttp.dll loads before Wine's built-in version.

## Common Patterns

### Adding New Configuration Options
1. Add `Configurable<T>` field to `HealthBarConfig`
2. Initialize in constructor with `config.Bind()`
3. Add UI element in `Initialize()` method (OpCheckBox, OpSlider, OpLabel)
4. Access via static field (e.g., `HealthBarConfig.EnableHealthBars.Value`)
5. Add translations to both English and Chinese dictionaries in `Translate()` method

### Modifying Health Bar Visuals
- Sprite properties: `scaleX`, `scaleY`, `color`, `alpha`, `isVisible`
- Position: Use `SetPosition(Vector2)` after converting to screen space
- Always apply smoothing for position (`Vector2.Lerp` with low factor)
- Apply distance-based alpha in `DrawHealthBar()` method
- Death animation alpha is multiplicative: `baseAlpha * _deathAlphaMultiplier`

### Hook Safety
- Never register hooks multiple times (use boolean flags)
- Always call `orig()` first in hook methods
- Wrap hook logic in try-catch with Logger.LogError
- Unsubscribe hooks in `OnDisable()` to prevent memory leaks

### Creature Iteration
```csharp
foreach (var abstractCreature in room.abstractRoom.creatures)
{
    if (abstractCreature?.realizedCreature != null && abstractCreature.realizedCreature.State.alive)
    {
        Creature creature = abstractCreature.realizedCreature;
        // Process creature
    }
}
```

## Version Management

Always keep these synchronized:
- `HealthBarMod.cs`: `PluginVersion` constant (line 16)
- `modinfo.json`: `version` field (line 4)

## Known Constraints

1. **No direct health values** - Rain World doesn't expose creature HP, only approximations via stun/damage resistance
2. **No ModManager translation hooks** - Can't translate mod list at runtime
3. **40 FPS timeStacker** - Rain World runs at 40 FPS, use `timeStacker / 40f` for time calculations
4. **Camera coordinate system** - Must convert world pos to screen pos: `worldPos - camera.pos`
5. **Sprite removal timing** - Must remove sprites before Dictionary cleanup to avoid null references
