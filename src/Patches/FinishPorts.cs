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
        [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.UpdateIO))]
    static partial class FinishPorts
    {
        static void Prefix(WorldFlowElement __instance)
        {
            // Native UpdateIO destroys old views but leaves them in this list.
            // Keep native drag/mirror loops restricted to the current port views.
            ((System.Collections.Generic.List<WorldFlowIO>)AccessTools.Field(typeof(WorldFlowElement), "_io").GetValue(__instance)).Clear();
        }
    }
}
