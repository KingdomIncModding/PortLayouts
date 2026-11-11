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
        [HarmonyPatch(typeof(WorldFlowIO), nameof(WorldFlowIO.SetIO))]
    static partial class PositionPorts
    {
        static void Postfix(WorldFlowIO __instance, FlowIO io) => PortVisuals.Apply(__instance, io);
    }
}
