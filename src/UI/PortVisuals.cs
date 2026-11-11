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
    internal static partial class PortVisuals
    {
        internal static Vector2 SpriteOffset(WorldFlowIO prefab, PortSide side)
        {
            var image = (Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(prefab);
            var offset = image.rectTransform.anchoredPosition;
            // The port root supplies the shift so artwork, hitbox and anchor agree.
            return offset;
        }
    }
}
