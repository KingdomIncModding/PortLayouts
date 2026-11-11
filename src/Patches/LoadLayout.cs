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
        [HarmonyPatch(typeof(FlowElement), nameof(FlowElement.FromJson))]
    static partial class LoadLayout
    {
        static void Postfix(JsonValue data, FlowElement __result)
        {
            if (__result == null || !data.IsJsonObject) return;
            var value = data[PortGeometry.JsonKey];
            PortGeometry.SetLayout(__result, value.IsInteger ? value.AsInteger : 0);
        }
    }
}
