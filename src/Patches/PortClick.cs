using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class PortClick
    {
        internal static bool Prefix(UnityEngine.Behaviour __instance, PointerEventData eventData)
        {
            if (Keyboard.current == null || eventData.button != PointerEventData.InputButton.Left || !Keyboard.current.altKey.isPressed || Keyboard.current.ctrlKey.isPressed)
                return true;
            // Consume the rotation chord on expandable nodes; plain Alt stays native.
            if (!PortGeometry.CanRotate(FoundationApi.WorkspaceAccess.FromView(__instance)))
                return !Keyboard.current.shiftKey.isPressed;
            if (FoundationApi.UiAccess.PointerTraverseDistance <= 50 && !FoundationApi.UiAccess.IsDragDropBusy)
                PortLayoutsPlugin.Instance.Change(__instance, Keyboard.current.shiftKey.isPressed);
            return false;
        }
    }
}
