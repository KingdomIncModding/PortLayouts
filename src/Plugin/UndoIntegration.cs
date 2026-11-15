using System;
using System.Runtime.CompilerServices;
using KingdomFoundation.Api;
#if HAS_UNDO
using KingdomIncUndo;
#endif

namespace KingdomPortLayouts
{
    internal static class UndoIntegration
    {
        internal const string UndoPluginId = "urakka.kingdominc.undo";

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static IDisposable TryRegister()
        {
#if HAS_UNDO
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(UndoPluginId))
                return null;
            return UndoApi.RegisterObjectExtension(
                PortLayoutsPlugin.Id,
                element => element != null && element.Kind == WorkspaceObjectKind.Building ? PortGeometry.Layout(element) : 0,
                (element, value) =>
                {
                    if (element == null || element.Kind != WorkspaceObjectKind.Building)
                        return;
                    int layout = value is int i ? i : 0;
                    PortGeometry.SetLayout(element, layout);
                    if (element.World)
                    {
                        FoundationApi.WorkspaceVisuals.RefreshPorts(element.World);
                        PortNodeView.Apply(element.World);
                    }
                    else
                    {
                        PortGeometry.RefreshPipes(element);
                    }
                },
                null,
                affectsConnectedPipeEndpoints: true);
#else
            return null;
#endif
        }
    }
}

