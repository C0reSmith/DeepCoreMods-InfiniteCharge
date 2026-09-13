# DeepCore Mods – Infinite Charge

**Engineering Better Gameplay**

Infinite Charge is a quality-of-life mod for **Stationeers** that keeps supported batteries fully charged while the mod is enabled.

The mod integrates with the normal **Stationeers Controls** menu, so the toggle key can be changed in-game.

## Features

- Keeps Station Batteries at full charge.
- Keeps Battery Cells at full charge.
- Works with different Battery Cell capacities automatically.
- Tested with:
  - Station Battery
  - Standard Battery Cell
  - Large Battery Cell
  - Nuclear Battery
- Toggle Infinite Charge ON/OFF at any time.
- Default toggle key: **F7**
- Toggle key can be changed in **Settings → Controls**.
- No separate BepInEx configuration file is required.
- Lightweight background worker with fast key polling and 250 ms battery refresh.

## Requirements

- Stationeers
- BepInEx 5
- Windows

> This mod currently uses the Windows `user32.dll` keyboard API for toggle-key detection, so it is intended for Windows systems.

## Installation

1. Install **BepInEx 5** for Stationeers.
2. Create this folder if it does not already exist:

   `Stationeers\BepInEx\plugins\DeepCoreMods.InfiniteCharge\`

3. Copy:

   `DeepCoreMods.InfiniteCharge.dll`

   into that folder.

4. Start Stationeers.
5. Open:

   **Settings → Controls**

6. Find:

   **DeepCore Mods - Infinite Charge**

7. The default toggle key is **F7**. Change it there if desired.

## Usage

- Press the configured toggle key to turn Infinite Charge **OFF**.
- Press it again to turn Infinite Charge **ON**.
- When enabled, supported batteries are restored to their own maximum charge value.
- When disabled, batteries return to normal Stationeers behaviour and discharge normally.

## Notes

- The mod uses each battery's own maximum capacity instead of a hard-coded charge value.
- This allows different Battery Cell types and capacities to be handled automatically.
- The Stationeers Controls binding is the active keybinding system. There is no separate mod `.cfg` keybinding.

## Uninstallation

Delete:

`BepInEx\plugins\DeepCoreMods.InfiniteCharge\DeepCoreMods.InfiniteCharge.dll`

The mod does not require a persistent BepInEx configuration file.

## Author

**DeepCore Mods**  
**C0reSmith**

*Engineering Better Gameplay*
