using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    public sealed partial class PortNodeView : MonoBehaviour
    {
        bool HoveringNode()
        {
            if (Mouse.current == null) return false;
            var mouse = Mouse.current.position.ReadValue();
            var camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)node.transform, mouse, camera)) return true;
            foreach (var io in node.Element.IO)
                if (io.World && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)io.World.transform, mouse, camera)) return true;
            return false;
        }
    }
}
