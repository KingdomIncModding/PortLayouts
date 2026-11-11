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
        internal static void Apply(WorldFlowIO world, FlowIO io)
        {
            if (PortGeometry.Layout(io.Parent) == 0) return;
            var side = PortGeometry.Side(io.Parent, io is FlowInput);
            var rect = (RectTransform)world.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.Euler(0, 0, PortGeometry.Angle(side));
            rect.position = io.Parent.World.transform.TransformPoint(PortGeometry.Point(io));
            var prefab = (WorldFlowIO)AccessTools.Field(typeof(WorldFlowElement), "_ioPrefab").GetValue(FlowPrefabSet.Get(io.Parent).Prefab);
            var image = (Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(world);
            image.rectTransform.anchoredPosition = SpriteOffset(prefab, side);
            var hint = (Image)AccessTools.Field(typeof(WorldFlowIO), "_itemHint").GetValue(world);
            var count = (TMP_Text)AccessTools.Field(typeof(WorldFlowIO), "_itemCount").GetValue(world);
            hint.transform.localScale = Vector3.one; count.transform.localScale = Vector3.one;
            count.rectTransform.anchoredPosition = new Vector2(2, -8);
            var tooltip = (RectTransform)AccessTools.Field(typeof(WorldFlowIO), "_miniTooltip").GetValue(world);
            tooltip.localRotation = Quaternion.Euler(0, 0, -PortGeometry.Angle(side));
        }
    }
}
