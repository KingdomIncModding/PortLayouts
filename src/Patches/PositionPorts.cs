using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class PositionPorts
    {
        internal static void Postfix(UnityEngine.Component __instance, WorkspacePort io) => PortVisuals.Apply(__instance, io);
    }
}
