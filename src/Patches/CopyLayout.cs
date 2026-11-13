using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class CopyLayout
    {
        internal static void Postfix(WorkspaceObject __instance, WorkspaceObject __result)
        {
            if (__result != null)
                PortGeometry.SetLayout(__result, PortGeometry.Layout(__instance));
        }
    }
}
