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
        [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.OnPointerClick))]
    static partial class PortClick
    {
        static bool Prefix(WorldFlowElement __instance, PointerEventData eventData)
        {
            if (Keyboard.current == null || eventData.button != PointerEventData.InputButton.Left
                || !Keyboard.current.altKey.isPressed || Keyboard.current.ctrlKey.isPressed) return true;
            // Consume the rotation chord on expandable nodes; plain Alt stays native.
            if (!PortGeometry.CanRotate(__instance.Element)) return !Keyboard.current.shiftKey.isPressed;
            if (Assets.Behaviour.Input.PlayerInput.Instance.TraverseDistance <= 50
                && !(DragDropManager.Instance && DragDropManager.Instance.Busy))
                PortLayoutsPlugin.Instance.Change(__instance, Keyboard.current.shiftKey.isPressed);
            return false;
        }
    }
}
