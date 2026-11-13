using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class LayoutSize
    {
        internal static bool Prefix(UnityEngine.Behaviour __instance, ref Vector2 __result)
        {
            if (FoundationApi.WorkspaceAccess.FromView(__instance) == null || !PortGeometry.CanRotate(FoundationApi.WorkspaceAccess.FromView(__instance)))
                return true;
            __result = PortGeometry.Size(FoundationApi.WorkspaceAccess.FromView(__instance));
            return false;
        }
    }
}
