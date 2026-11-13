using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class LoadLayout
    {
        internal static void Postfix(JsonValue data, WorkspaceObject __result)
        {
            if (__result == null || !data.IsJsonObject)
                return;
            var value = data[PortGeometry.JsonKey];
            PortGeometry.SetLayout(__result, value.IsInteger ? value.AsInteger : 0);
        }
    }
}
