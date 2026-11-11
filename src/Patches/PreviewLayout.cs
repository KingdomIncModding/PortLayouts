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
        [HarmonyPatch(typeof(WorldFlowGhost), nameof(WorldFlowGhost.SetElement))]
    static partial class PreviewLayout
    {
        static void Postfix(WorldFlowGhost __instance, FlowElement el)
            => (__instance.GetComponent<PortGhostView>() ?? __instance.gameObject.AddComponent<PortGhostView>()).Initialize(el);
    }
}
