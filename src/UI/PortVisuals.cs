using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    internal static class PortVisuals
    {
        internal static void Apply(UnityEngine.Component world, WorkspacePort io)
        {
            if (PortGeometry.Layout(io.Parent) == 0)
                return;
            var side = PortGeometry.Side(io.Parent, io.IsInput);
            var rect = (RectTransform)world.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.Euler(0, 0, PortGeometry.Angle(side));
            rect.position = io.Parent.World.transform.TransformPoint(PortGeometry.Point(io));
            var prefab = FoundationApi.WorkspaceVisuals.PortTemplate(io.Parent);
            var image = (Image)FoundationApi.WorkspaceVisuals.PortGraphic(world, GamePortGraphic.Sprite);
            image.rectTransform.anchoredPosition = SpriteOffset(prefab, side);
            var hint = (Image)FoundationApi.WorkspaceVisuals.PortGraphic(world, GamePortGraphic.ItemHint);
            var count = (TMP_Text)FoundationApi.WorkspaceVisuals.PortGraphic(world, GamePortGraphic.ItemCount);
            hint.transform.localScale = Vector3.one;
            count.transform.localScale = Vector3.one;
            count.rectTransform.anchoredPosition = new Vector2(2, -8);
            var tooltip = (RectTransform)FoundationApi.WorkspaceVisuals.PortGraphic(world, GamePortGraphic.MiniTooltip);
            tooltip.localRotation = Quaternion.Euler(0, 0, -PortGeometry.Angle(side));
        }

        internal static Vector2 SpriteOffset(UnityEngine.Component prefab, PortSide side)
        {
            var image = (Image)FoundationApi.WorkspaceVisuals.PortGraphic(prefab, GamePortGraphic.Sprite);
            var offset = image.rectTransform.anchoredPosition;
            // The port root supplies the shift so artwork, hitbox and anchor agree.
            return offset;
        }
    }
}
