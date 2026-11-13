using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class FinishPorts
    {
        internal static void Prefix(UnityEngine.Behaviour __instance)
        {
            // Native UpdateIO destroys old views but leaves them in this list.
            // Keep native drag/mirror loops restricted to the current port views.
            FoundationApi.WorkspaceVisuals.ClearPortViewIndex(__instance);
        }

        internal static void Postfix(UnityEngine.Behaviour __instance) => PortNodeView.Apply(__instance);
    }
}
