# Port Layouts

Choose which sides of a Kingdom Inc node hold its inputs and outputs. The node body and icon stay upright; labels move to an edge without ports.

## Controls

- **Shift + Alt + left-click** the node body or label to select the next layout.
- **Alt + left-click** reverses the inputs and outputs of the current layout. Click again to restore them.

The six layouts, with **inputs listed first**, cycle in this order:

| Layout | Inputs | Outputs |
| --- | --- | --- |
| 1 | Left | Right |
| 2 | Top | Bottom |
| 3 | Left | Top |
| 4 | Left | Bottom |
| 5 | Right | Top |
| 6 | Right | Bottom |

The next cycle returns to Left/Right. Selecting the next layout resets reversal to the listed direction. Alt-click continues to reverse the selected pair.

Buildings with expandable port slots cannot rotate: **Collector, Distributor, Manifold and Ledger** always use their native Left/Right ports. Shift+Alt+click does nothing on these nodes; plain Alt+click still flips their ports normally. Older custom layouts on these buildings return to native sides when loaded, copied or placed from a blueprint. Expansion counts, settings, native mirroring and pipe connections are retained.

Labels remain visible normally and sit on an edge without I/O ports. During dragging, labels hide and stop intercepting the mouse; hovering over a node or its ports reveals that node's label. Moving a node keeps its own label visible, and labels return when dragging ends or is cancelled. A clear top edge is preferred, followed by bottom, then right or left. Top and bottom labels read normally; left and right labels run sideways with the bottoms of the letters facing inward toward the node. Titles stay centred along their selected edge, and the normal label drag handle still works. Native Left/Right layouts and expandable buildings keep their normal top labels. Copy previews use the same placement and orientation.

Existing connections stay attached to the same inputs/outputs. Pipe endpoints and pipe hitboxes follow their new sides; manual pipe bends stay in place. Node movement keeps the game's native 64-unit snapping and Ctrl free-placement behavior. Horizontal port rows use the same 64-unit spacing. Layout changes that would overlap another node are rejected.

Version 1.0.10 moves the whole top/bottom port two more world units left, for a total seven-unit shift (seven screen pixels at normal 1:1 zoom). Its connection snap point, hitbox, drag preview and pipe endpoint move together with the artwork. Existing manual pipe bends and node snapping are retained, including Snappy's finer grid when installed. Copy previews match the port geometry. Labels still hide during dragging with hover-to-reveal on the node body or ports. Invisible labels do not block the mouse, and native label dragging remains available. Free-edge placement, readable bottom text, inward-facing side text, and expandable-building restrictions are retained.

Layout and reversal are saved per node. Native copying/picker placement and Sharable Blueprints retain them; the copy preview shows the relocated ports. Recipients need Port Layouts to display custom layouts.

## Requirements and installation

Requires the Mono version of Kingdom Inc and BepInEx 5.x. Tested with Kingdom Inc **Release v1.0.12**, Unity **6000.3.27f2**, and BepInEx **5.4.23.6**.

With the game closed, extract the player ZIP into the game directory. The plugin belongs at:

```text
BepInEx/plugins/PortLayouts/PortLayouts.dll
```

For a local source build:

```powershell
dotnet build _Mods/PortLayouts/PortLayouts.csproj -c Release
& _Mods/PortLayouts/install.ps1
```

The installer backs up an existing Port Layouts DLL under this project's `_Backup` folder. It does not replace other plugins. Start the game normally after installation. To stage an update while the game is running, use `install.ps1 -StageForNextLaunch`; the running session keeps its original mapped DLL, and the update loads after restarting the game.

To uninstall, close the game and remove only `BepInEx/plugins/PortLayouts/PortLayouts.dll`. Nodes return to native Left/Right placement, with their native mirrored flag retained. Native saves ignore the optional layout field; saving without this plugin drops custom layout information. Keep a save backup if you intend to reinstall and restore those layouts.

## Verification

The isolated runtime creates a disposable kingdom, blocks normal saves/autosaves/loading and achievements, and runs the actual game renderers, controls and physics. It does not load a normal kingdom. Diagnostic code runs only with `-portlayouts-smoke`.

```powershell
& _Mods/PortLayouts/verify-isolated.ps1
& _Mods/PortLayouts/verify-isolated.ps1 -WithExistingPlugins
python _Mods/PortLayouts/Tools/package.py
```

Checks cover all six layouts and their reversals on a fixed-slot Filter, native click modifiers, upright node geometry, label visibility before/after dragging and mouse hover, hidden-label raycast passthrough, preserved native label drag handles, free-edge label placement, text orientation, port-hitbox clearance, copy-preview titles, pipe endpoints/hitboxes/bends, collision rejection, dragging/snapping/cancellation, saves, copying/placement, and companion nodes. Arrow alignment checks read actual sprite alpha pixels for every connection, hover and error state. A connected sword Blacksmith reproduces two top inputs and one bottom output for a detailed screenshot. Restriction regressions load old layouts, reject cycling, and use the actual native + controls to expand and collapse Collectors, Distributors and Manifolds while checking native sizes and collision boxes. Native Alt flipping and the Ledger restriction are also checked. The companion run includes Snappy and verifies Sharable Blueprints capture and compact KIB2 sharing. Reports, logs and native-rendered screenshots are kept locally under `Verification/`.

The project builds against locally installed game and BepInEx assemblies. They are not redistributed in the package. Mod-manager deployment has not been tested; use manual installation.

Source repository: https://github.com/urakkaamyx/KingdomInc_PortLayouts
