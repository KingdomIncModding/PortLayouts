# Port Layouts

**Current release: 1.0.14 — for Kingdom Inc game version 1.0.14.**

[GitHub releases](https://github.com/KingdomIncModding/PortLayouts/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/15) · Version 1.0.14

Choose which sides of your Kingdom Inc buildings hold their inputs and outputs. The building body and icon stay upright, with ports, pipe connections, and snapping following the selected layout.

## Features

- **Six Distinct Port Layouts:** Cycle between Left/Right, Top/Bottom, Left/Top, Left/Bottom, Right/Top, and Right/Bottom configurations.
- **Direction Reversal:** Instantly swap input and output sides with a simple hotkey modifier.
- **Upright Machine Icons:** Only the ports rotate; building bodies, icons, and status indicators remain cleanly upright.
- **Smart Adaptive Labels:** Labels position along clear borders without ports, reading legibly at all angles.
- **Automatic Pipe Routing:** Existing and new pipe connections smoothly adapt to relocated ports and bends.
- **Persistent Layouts:** Custom port orientations survive saving, reloading, and vanilla blueprint copying.
- **Safe Expandable Nodes:** Expandable structures (Collectors, Distributors, Manifolds) retain their native multi-port geometry.

## Unlock and Controls

Open **Tech Tree > Mods** and research **Port Rotation** for free.

- **Shift + Alt + Left-Click:** Click a building or its label to cycle through the 6 layouts (Left/Right, Top/Bottom, Left/Top, Left/Bottom, Right/Top, Right/Bottom).
- **Alt + Left-Click:** Reverses the current input/output pair.

Cycling returns to Left/Right after the sixth layout and resets reversal to the default direction.

## Installation

1. Install [BepInEx 5 (Windows x64 Mono)](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6).
2. Install [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.14.4 or newer).
3. Extract the release ZIP into your Kingdom Inc game folder.
4. Confirm the file is located at `BepInEx/plugins/PortLayouts/PortLayouts.dll`.
5. Launch the game and unlock Port Rotation in Tech Tree > Mods!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| FlowElement.CreateCopy | Foundation hook (postfix) | Preserves port rotation and side layouts when copying or duplicating buildings. |
| WorldFlowElement.UpdateIO | Foundation hooks (prefix / postfix) | Refreshes and updates building I/O port node views when layout changes. |
| WorldFlowElement.Size | Foundation hook (prefix) | Dynamically calculates building bounds based on active rotated port positions. |
| FlowElement.FromJson | Foundation hook (postfix) | Restores saved custom port rotations and layouts from kingdom save data. |
| WorldFlowPipe.UpdateLinePositions | Foundation hook (postfix) | Adjusts connecting pipe endpoints and routes to snap to rotated ports. |
| WorldFlowElement.OnPointerClick | Foundation hook (prefix) | Intercepts click events with Shift+Alt / Alt modifiers to cycle port layouts or reverse directions. |
| WorldFlowIO.SetIO | Foundation hook (postfix) | Positions port visuals, connection points, and snapping colliders around the building perimeter. |
| WorldFlowGhost.SetElement | Foundation hook (postfix) | Aligns placement preview ghosts with the selected port rotation layout. |
| FlowElement.ToJson | Foundation hook (postfix) | Serializes custom building port layout and orientation data into kingdom saves. |

## Removal

Close the game and delete `BepInEx/plugins/PortLayouts/`. Buildings return to native Left/Right ports with their mirrored setting retained.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/KingdomIncModding/PortLayouts). Game artwork belongs to its respective owners.