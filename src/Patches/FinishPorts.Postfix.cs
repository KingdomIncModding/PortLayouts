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
    static partial class FinishPorts
    {
        static void Postfix(WorldFlowElement __instance) => PortNodeView.Apply(__instance);
    }
}
