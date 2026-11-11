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
        [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.Size), MethodType.Getter)]
    static partial class LayoutSize
    {
        static bool Prefix(WorldFlowElement __instance, ref Vector2 __result)
        {
            if (__instance.Element == null || !PortGeometry.CanRotate(__instance.Element)) return true;
            __result = PortGeometry.Size(__instance.Element); return false;
        }
    }
}
