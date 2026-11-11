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
    internal static partial class PortLabels
    {
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
    }
}
