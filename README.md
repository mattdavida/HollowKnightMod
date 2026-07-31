# Hollow Knight Cheats

A comprehensive cheat menu mod for Hollow Knight that enables and extends the game's built-in developer cheat system with a professional user interface.

## Features

### Toggle Features
- **Invincibility** - Immune to all damage
- **Infinite Air Jump** - Jump infinitely in the air
- **Auto Soul Refill** - Automatically refills soul every second
- **Insta Kill** - One-hit kill all enemies
- **All Charms Cost 1** - Reduces all charm notch costs to 1

### Action Amounts
- **Health** - Add health or set to maximum (10 masks)
- **Geo** - Add currency
- **Dream Orbs** - Add essence
- **Charm Slots** - Set notch slots up to maximum (12)

### Quick Actions
- **Refill Health** - Instantly restore all health
- **Refill Soul** - Instantly restore all soul
- **All Charms** - Unlock all 40 charms with 10 slots
- **All Key Items** - Unlock all progression items
- **All Nail Arts** - Unlock all nail techniques
- **All Map** - Reveal entire world map
- **All Stags** - Unlock all fast travel stations
- **All Powerups** - Unlock everything at once

### Abilities
Toggle individual movement abilities:
- Dash, Shadow Dash, Double Jump, Wall Jump
- Super Dash, Acid Armor, Dreamgate, Dream Nail

### Spells
Unlock and upgrade offensive spells with visual level indicators:
- Fireball (Vengeful Spirit / Shade Soul)
- Quake (Desolate Dive / Descending Dark)
- Scream (Howling Wraiths / Abyss Shriek)

### Charms
- Searchable dropdown of all 40 charms
- Visual status indicators showing unlocked state
- Individual charm toggle

## Technical Features

### Dual Framework Support
The mod is built to support both modding frameworks:
- **BepInEx** - Outputs to `HollowKnight.BepInEx.dll`
- **MelonLoader** - Outputs to `HollowKnight.MelonLoader.dll`

Single codebase using conditional compilation to target both frameworks.

### Built-in Cheat Manager Integration
Hollow Knight includes a developer cheat system (`CheatManager`) that is disabled in production builds. This mod:
1. Patches the game to enable the built-in `CheatManager`
2. Initializes it properly on mod load
3. Extends it with a professional GUI and additional features
4. Provides all functionality through a clean, organized interface

### Architecture
- **Service-based design** - Separate services for health, currency, invincibility, abilities, spells, and items
- **Framework abstraction** - Clean interfaces for logging, input, and timing that work across both frameworks
- **Configuration persistence** - Toggle states save automatically and restore on game restart
- **Toast notifications** - Non-intrusive feedback system
- **Confirmation modals** - Safety confirmations for destructive actions
- **State enforcement** - Maintains toggle states across deaths and scene changes

## Installation

### BepInEx
1. Download and install [BepInEx](https://github.com/BepInEx/BepInEx/releases) (x86 version)
2. Download `HollowKnight.BepInEx.dll` from releases
3. Place in `Hollow Knight/BepInEx/plugins/`
4. Launch the game

### MelonLoader
1. Download and install [MelonLoader](https://github.com/LavaGang/MelonLoader/releases)
2. Download `HollowKnight.MelonLoader.dll` from releases
3. Place in `Hollow Knight/Mods/`
4. Launch the game

## Usage

**Open Menu**: Press `Insert` or `~` (Tilde/Backtick)

For **international keyboards** where Insert/Tilde may not work well, you can configure a custom key in the config file.

The menu will appear on the right side of the screen. All features are organized into collapsible sections for easy navigation.

### Configuration

Toggle features automatically save their state when changed and restore on game restart:
- **BepInEx**: `BepInEx/config/com.hollowknight.cheats.cfg`
- **MelonLoader**: `UserData/MelonPreferences.cfg`

#### Custom GUI Toggle Key (International Keyboard Support)

Edit your config file and change the `GUI Toggle Key` setting:

**BepInEx** (`BepInEx/config/com.hollowknight.cheats.cfg`):
```ini
[Keybinds]
## Key to open/close the cheat GUI. Insert/Tilde always work. 
## Examples: F7, Home, G, Minus, None
## Full list: https://docs.unity3d.com/ScriptReference/KeyCode.html
GUI Toggle Key = F7
```

**MelonLoader** (`UserData/MelonPreferences.cfg`):
```ini
[HollowKnightKeybinds]
GuiToggleKey = "F7"
```

Common alternatives for non-US keyboards: `F7`, `Home`, `End`, `PageUp`, `PageDown`, `Minus`, `Equals`

**Note**: Insert and Tilde always work as fallbacks, even if you set a custom key.

## Building from Source

### Prerequisites
- .NET SDK 4.7.2 or higher
- Visual Studio 2019/2022 or Rider
- Hollow Knight installed with BepInEx and/or MelonLoader

### Build Configurations
- **Debug-BepInEx** / **Release-BepInEx** - Builds BepInEx version
- **Debug-MelonLoader** / **Release-MelonLoader** - Builds MelonLoader version

### Setup
1. Clone the repository
2. Update game path in `HollowKnight.csproj`:
   ```xml
   <HintPath>YOUR_PATH\Hollow Knight\hollow_knight_Data\Managed\Assembly-CSharp.dll</HintPath>
   ```
3. Select your desired build configuration
4. Build the solution

Compiled DLLs will be in `bin/BepInEx/Release/` or `bin/MelonLoader/Release/`

## Project Structure

```
HollowKnight/
├── Core/               # Core utilities and helpers
├── Framework/          # Framework abstraction layer
├── GUI/                # User interface components
├── Interfaces/         # Abstraction interfaces
├── Patches/            # Harmony patches for CheatManager
├── Services/           # Feature implementation services
├── ConfirmationSystem.cs
├── ToastSystem.cs
└── HollowKnightMod.cs  # Main entry point
```

## Dependencies

- **0Harmony** - Included with BepInEx/MelonLoader
- **UniverseLib.Mono** (v1.5.1) - UI enhancements and cursor management

## License

This is a modding tool for personal use. Hollow Knight and all game assets are © Team Cherry.

## Acknowledgments

- Team Cherry for Hollow Knight and the built-in developer tools
- BepInEx and MelonLoader communities
- UniverseLib for UI utilities

