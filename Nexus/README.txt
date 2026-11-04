Port Rotation 1.0.10
By Urakka (rockyamyx on Nexus Mods)

Choose which sides of a node hold its inputs and outputs. The building body and icon remain upright.

REQUIREMENTS
Kingdom Inc, Unity Mono edition, and BepInEx 5.x with its bundled Harmony.
Tested on Release v1.0.12, Unity 6000.3.27f2, and BepInEx 5.4.23.6.
No other plugin is required. Snappy is optional and its finer movement grid is supported.

INSTALLATION
Close the game. Extract this ZIP into the Kingdom Inc game directory.
The installed file must be: BepInEx/plugins/PortLayouts/PortLayouts.dll
Start the game normally. The plugin keeps its existing internal name, Port Layouts.
If updating an existing installation, replace this DLL rather than installing a second copy.

CONTROLS
Shift + Alt + left-click the node body or label to cycle these input/output pairs:
Left/Right, Top/Bottom, Left/Top, Left/Bottom, Right/Top, Right/Bottom.
Alt + left-click reverses the selected pair. Selecting the next layout resets reversal.
The node body itself does not rotate.

Expandable-slot buildings (Collector, Distributor, Manifold and Ledger) keep native Left/Right ports.
Shift+Alt does nothing on them; plain Alt still flips their ports normally.

LABELS AND CONNECTIONS
Labels use a side without ports. Top and bottom text reads normally; side text faces inward.
Labels hide while dragging and reappear over the node body or ports; the moved node keeps its own label.
Port artwork, hitboxes, snap points, pipe endpoints and copy previews share the same geometry.
Existing pipe connections and manual bends are retained. Layout and reversal save with the node.
Native copies and Sharable Blueprints retain layout data; recipients need this plugin to display it.

REMOVAL
Close the game and remove BepInEx/plugins/PortLayouts/PortLayouts.dll.
Nodes return to native Left/Right, retaining their native mirrored flag.
Saving without the plugin drops custom layout metadata; keep a save backup if you intend to restore it later.

VERIFICATION
Version 1.0.10 passed 2417 isolated runtime checks and 2497 companion checks, including Snappy,
Party Node, Ledger, Leyline Conduits and Sharable Blueprints. Actual pipe dragging and dropping,
raycasts, previews, hitboxes and endpoints were tested on top/bottom and reversed layouts.
Diagnostic runs use a disposable kingdom with normal saves, loading and achievements blocked.
Manual installation is supported; mod-manager deployment has not been tested.

SOURCE AND TROUBLESHOOTING
Source: https://github.com/urakkaamyx/KingdomInc_PortLayouts
For reports, include game/plugin versions, the selected port layout and a screenshot.
The game log is BepInEx/LogOutput.log; its plugin entry is Port Layouts 1.0.10.
