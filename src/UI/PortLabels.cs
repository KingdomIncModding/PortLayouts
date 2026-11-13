using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    internal static class PortLabels
    {
        internal static bool Occupied(WorkspaceObject element, PortSide side) => (element.Inputs.Count > 0 && PortGeometry.Side(element, true) == side) || (element.Outputs.Count > 0 && PortGeometry.Side(element, false) == side);
        internal static float NativeInset(RectTransform label) => (label.pivot.y - .5f) * label.rect.height - label.anchoredPosition.y;
        internal static void Place(RectTransform label, PortSide side, float inset)
        {
            // Native titles straddle the border; keep that treatment on every free edge.
            label.pivot = new Vector2(.5f, .5f);
            label.anchoredPosition = -PortGeometry.Normal(side) * inset;
            switch (side)
            {
                case PortSide.Bottom:
                    label.anchorMin = label.anchorMax = new Vector2(.5f, 0);
                    label.localRotation = Quaternion.identity;
                    break;
                case PortSide.Right:
                    label.anchorMin = label.anchorMax = new Vector2(1, .5f);
                    label.localRotation = Quaternion.Euler(0, 0, -90);
                    break;
                case PortSide.Left:
                    label.anchorMin = label.anchorMax = new Vector2(0, .5f);
                    label.localRotation = Quaternion.Euler(0, 0, 90);
                    break;
                default:
                    label.anchorMin = label.anchorMax = new Vector2(.5f, 1);
                    label.localRotation = Quaternion.identity;
                    break;
            }
        }

        internal static PortSide Side(WorkspaceObject element)
        {
            if (!Occupied(element, PortSide.Top))
                return PortSide.Top;
            if (!Occupied(element, PortSide.Bottom))
                return PortSide.Bottom;
            if (!Occupied(element, PortSide.Right))
                return PortSide.Right;
            return PortSide.Left;
        }
    }
}
