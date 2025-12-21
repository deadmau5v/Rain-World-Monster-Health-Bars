# Monster Health Bars

[![Version](https://img.shields.io/badge/version-1.4.0-green.svg)](https://github.com/deadmau5v/Rain-World-Monster-Health-Bars)

A Rain World mod that displays smooth, configurable health bars above creatures with rounded corners, distance fading, and death animations.

## Features

- **Visual Health Bars**: Smooth green→yellow→red bars with rounded corners and background
- **Smart Visibility**: Hides bars at full health, in pipes/shortcuts, or beyond max distance
- **Death Animation**: Red bars fade out over 1 second as creatures die
- **Player Toggle**: Optional player health bar (disabled by default)
- **Performance Optimized**: Position smoothing, efficient creature tracking
- **Configurable**: Width, height, distance, toggles via Remix config interface
- **Multi-Language**: English, Chinese + 8 more (community translations welcome)

**Health Calculation**: Uses `stun` counter + `damageResistance`/`stunResistance` for approximation (Rain World doesn't expose direct HP).

## Installation

1. Install [BepInEx 5.x](https://bepinex.github.io/) for Rain World v1.11.5
2. Drop `Monster Health Bars.dll` into `BepInEx/plugins/`
3. Launch game → Config appears in Remix Mod Manager

**Compatible with:** Base game + Downpour/Watcher DLCs

## Configuration

Access via Remix Mod Manager → Monster Health Bars:

| Option                  | Default    | Range/Values |
|-------------------------|------------|--------------|
| Enable Health Bars      | ✅ True    | Toggle       |
| Show Player Health Bar  | ❌ False   | Toggle       |
| Hide When Full Health   | ✅ True    | Toggle       |
| Bar Width               | 40px       | 20-100       |
| Bar Height              | 4px        | 2-10         |
| Max Distance            | 800px      | 400-1600     |

## Screenshots

*Coming soon*

## Translation

**In-Game Config UI:** Supports 10 languages (EN, ZH, JA, KO, FR, DE, ES, IT, PT, RU) via embedded dictionaries in `HealthBarConfig.cs`.

**Mod List:** English-only (future: external translation files).

To contribute, edit the `Translate()` method dictionary.

## Linux/Proton Setup

Add to Steam launch options:
```
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

Or run setup scripts:
```bash
chmod +x setup-protontricks.sh configure-bepinex-wine.sh
./setup-protontricks.sh
./configure-bepinex-wine.sh
```

## Development

- .NET Framework 4.8 + BepInEx 5.x
- References from repo root `lib/` (`../lib/` rel. to .csproj)
- Hooks: `On.RainWorld.OnModsInit` → `On.HUD.HUD.Draw`
- Build manually (no auto-builds)

## Limitations

- Health **approximated** via `creature.stun` counter, `baseDamageResistance`, `baseStunResistance` ratios
- Varies by creature type (no exact HP exposed)
- 40 FPS `timeStacker` aware

**Source**: [GitHub](https://github.com/deadmau5v/Rain-World-Monster-Health-Bars)
