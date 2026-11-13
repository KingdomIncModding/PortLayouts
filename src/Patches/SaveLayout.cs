using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class SaveLayout
    {
        internal static void Postfix(WorkspaceObject __instance, ref JsonValue __result)
        {
            int layout = PortGeometry.Layout(__instance);
            if (layout != 0)
                __result.AsJsonObject[PortGeometry.JsonKey] = layout;
        }
    }
}
