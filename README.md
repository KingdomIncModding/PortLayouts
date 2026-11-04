# Port Layouts 1.0.10

[Download the player release](https://github.com/urakkaamyx/KingdomInc_PortLayouts/releases/tag/v1.0.10)

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

Top/bottom ports use a seven-world-unit left offset, with their connection snap points, hitboxes, drag previews and pipe endpoints moving together. Copy previews use the same geometry. Node movement respects Snappy's finer grid when that plugin is installed.

Layout and reversal are saved per node. Native copying/picker placement and Sharable Blueprints retain them; the copy preview shows the relocated ports. Recipients need Port Layouts to display custom layouts.

## Requirements and installation

Requires the Mono version of Kingdom Inc and BepInEx 5.x. Tested with Kingdom Inc **Release v1.0.12**, Unity **6000.3.27f2**, and BepInEx **5.4.23.6**.

With the game closed, extract the player ZIP into the game directory. The plugin belongs at:

```text
BepInEx/plugins/PortLayouts/PortLayouts.dll
```

The installer backs up an existing Port Layouts DLL under this project's `_Backup` folder. It does not replace other plugins. Start the game normally after installation. To stage an update while the game is running, use `install.ps1 -StageForNextLaunch`; the running session keeps its original mapped DLL, and the update loads after restarting the game.

To uninstall, close the game and remove only `BepInEx/plugins/PortLayouts/PortLayouts.dll`. Nodes return to native Left/Right placement, with their native mirrored flag retained. Native saves ignore the optional layout field; saving without this plugin drops custom layout information. Keep a save backup if you intend to reinstall and restore those layouts.

## Building from source

Requires a .NET SDK with .NET Standard 2.1 support, PowerShell, and a local installation of the Mono game with BepInEx 5. Python 3 is needed for packaging. Game, Unity, BepInEx and Harmony assemblies are referenced from that installation and are not included in this repository.

Clone into `<KingdomInc>/_Mods/PortLayouts`; the build, installation and diagnostic scripts resolve the game directory from that location. Run these commands from the game directory for a fresh checkout:

```powershell
git clone https://github.com/urakkaamyx/KingdomInc_PortLayouts.git _Mods/PortLayouts
dotnet build _Mods/PortLayouts/PortLayouts.csproj -c Release
& ./_Mods/PortLayouts/install.ps1
```

The output is `_Mods/PortLayouts/bin/Release/netstandard2.1/PortLayouts.dll`. Close the game before installation, or use the staging option described above. The game and its extracted source are not needed in a GitHub checkout; only the installed assemblies are used for building. Hosted GitHub runners cannot build this project without those local game dependencies.

## Verification and packaging

The isolated runtime creates a disposable kingdom, blocks normal saves/autosaves/loading and achievements, and runs the actual game renderers, controls and physics. It does not load a normal kingdom. Diagnostic code runs only with `-portlayouts-smoke`.

```powershell
& ./_Mods/PortLayouts/verify-isolated.ps1
& ./_Mods/PortLayouts/verify-isolated.ps1 -WithExistingPlugins
python _Mods/PortLayouts/Tools/package.py
```

Checks cover all six layouts and their reversals on a fixed-slot Filter, native click modifiers, upright node geometry, label visibility before/after dragging and mouse hover, hidden-label raycast passthrough, preserved native label drag handles, free-edge label placement, text orientation, port-hitbox clearance, copy-preview titles, pipe endpoints/hitboxes/bends, collision rejection, dragging/snapping/cancellation, saves, copying/placement, and companion nodes. Arrow alignment checks read actual sprite alpha pixels for every connection, hover and error state. A connected sword Blacksmith reproduces two top inputs and one bottom output for a detailed screenshot. Restriction regressions load old layouts, reject cycling, and use the actual native + controls to expand and collapse Collectors, Distributors and Manifolds while checking native sizes and collision boxes. Native Alt flipping and the Ledger restriction are also checked. The companion run includes Snappy and verifies Sharable Blueprints capture and compact KIB2 sharing. Reports, logs and native-rendered screenshots are kept locally under `Verification/`.

The project builds against locally installed game and BepInEx assemblies. They are not redistributed in the package. Mod-manager deployment has not been tested; use manual installation.

Version 1.0.10 passed **2,417 isolated runtime checks** and **2,497 companion checks**. Native pipe dragging and dropping were checked on both shifted top/bottom ports and reversed layouts, including actual UI raycasts, hitboxes, previews and committed pipe endpoints.

The optional companion run requires these plugins to be installed locally: Container Colors, Nightmare Distiller Fix, Path Finder, Party Node, Sharable Blueprints, Kingdom Inc Factory API, Ledger, Leyline Conduits and Snappy. They are not required to use Port Layouts. Packaging requires successful reports for the current DLL from both runs and an installed DLL matching the tested build. The ZIP contains only the plugin and player instructions. Generated packages, private runtimes, reports, backups and local publication notes stay outside Git.

Author: Urakka. Plugin ID: `urakka.kingdominc.portlayouts`.

Source repository: https://github.com/urakkaamyx/KingdomInc_PortLayouts
