using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using HarmonyLib;
using LightJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
        [HarmonyPatch(typeof(FlowElement), nameof(FlowElement.ToJson))]
    static partial class SaveLayout
    {
        static void Postfix(FlowElement __instance, ref JsonValue __result)
        {
            int layout = PortGeometry.Layout(__instance);
            if (layout != 0) __result.AsJsonObject[PortGeometry.JsonKey] = layout;
        }
    }
}
