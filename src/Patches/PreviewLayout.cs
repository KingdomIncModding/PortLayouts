using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class PreviewLayout
    {
        internal static void Postfix(UnityEngine.Component __instance, WorkspaceObject el) => (__instance.GetComponent<PortGhostView>() ?? __instance.gameObject.AddComponent<PortGhostView>()).Initialize(el);
    }
}
