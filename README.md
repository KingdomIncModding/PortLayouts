# Port Rotation

Choose which sides of a Kingdom Inc building hold its inputs and outputs. The building and its icon stay upright, while ports, pipe connections and snapping follow the selected layout.

[Nexus Mods page](https://www.nexusmods.com/kingdominc/mods/15)

## Unlock and controls

Open **Tech Tree → Mods** and research **Port Rotation** for free. This unlocks layout cycling for that kingdom.

- **Shift + Alt + left-click** the building or its label to cycle layouts.
- **Alt + left-click** reverses the inputs and outputs. Ordinary flipping remains available before research.

Inputs are listed first:

| Layout | Inputs | Outputs |
| --- | --- | --- |
| 1 | Left | Right |
| 2 | Top | Bottom |
| 3 | Left | Top |
| 4 | Left | Bottom |
| 5 | Right | Top |
| 6 | Right | Bottom |

Cycling returns to Left/Right after the sixth layout. Selecting the next layout resets reversal to the direction shown above.

Buildings with expandable port slots keep their native Left/Right ports. Shift+Alt does nothing on them; Alt still flips them normally.

## Labels and connections

Labels sit across a border without ports, matching the normal upright label placement. Top and bottom labels read normally. Side labels run sideways with the bottoms of the letters facing the building.

Labels hide while dragging something across the workspace. Hovering over a building or its ports reveals its label; a building being moved keeps its own label visible.

Existing pipe connections and manual bends stay intact. Port artwork, connection points and copy previews remain aligned. Buildings keep normal movement and snapping, and layout changes that overlap another building are rejected.

Layouts, reversal and the research unlock are saved per kingdom. Existing rotated buildings keep their layout when updating, even before the new research is purchased. Native copying retains the selected layout.

## Installation and updating

Use the Mono edition of Kingdom Inc with BepInEx 5. Install the requirements listed on the download page, then close the game and extract the player ZIP into the game directory.

The plugin must be at:

```text
BepInEx/plugins/PortLayouts/PortLayouts.dll
```

Replace the existing DLL when updating, then restart the game. The internal plugin name remains **Port Layouts**.

## Removal

Close the game and remove `BepInEx/plugins/PortLayouts/PortLayouts.dll`. Buildings return to native Left/Right ports and retain their normal mirrored setting. Saving without the plugin drops custom layout information; keep a save backup if you want to restore it later.

Created by Urakka (rockyamyx).

[Source repository](https://github.com/urakkaamyx/KingdomInc_PortLayouts)
