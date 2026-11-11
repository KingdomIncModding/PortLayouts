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
        [HarmonyPatch(typeof(FlowElement), nameof(FlowElement.CreateCopy))]
    static partial class CopyLayout
    {
        static void Postfix(FlowElement __instance, FlowElement __result)
        { if (__result != null) PortGeometry.SetLayout(__result, PortGeometry.Layout(__instance)); }
    }
}
