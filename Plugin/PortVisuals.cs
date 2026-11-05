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
    public sealed class PortNodeView : MonoBehaviour
    {
        RectTransform label;
        CanvasGroup labelVisibility;
        WorldFlowElement node;
        Canvas canvas;
        Vector2 originalLabelPosition, originalAnchorMin, originalAnchorMax, originalPivot;
        Quaternion originalRotation;
        float originalLabelInset;
        internal bool LabelVisible => label.gameObject.activeSelf && labelVisibility.alpha > 0;
        void LateUpdate() => RefreshVisibility();
        internal void RefreshVisibility()
        {
            if (!label || !node) return;
            var target = DragDropManager.Instance ? DragDropManager.Instance.Target : null;
            // Keep the native move handle alive even while its artwork is hidden.
            // Screen-space geometry also works when a placement ghost intercepts pointer events.
            bool visible = target == null
                || (target is WorldFlowLabel handle && handle.GetComponentInParent<WorldFlowElement>() == node)
                || HoveringNode();
            labelVisibility.alpha = visible ? 1 : 0;
            labelVisibility.blocksRaycasts = visible;
            labelVisibility.interactable = visible;
        }
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
        internal static void Apply(WorldFlowElement node)
        {
            var view = node.GetComponent<PortNodeView>();
            if (!view)
            {
                view = node.gameObject.AddComponent<PortNodeView>();
                view.node = node;
                view.canvas = node.GetComponentInParent<Canvas>();
                view.label = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(node)).rectTransform;
                view.labelVisibility = view.label.GetComponent<CanvasGroup>();
                if (!view.labelVisibility) view.labelVisibility = view.label.gameObject.AddComponent<CanvasGroup>();
                view.originalLabelPosition = view.label.anchoredPosition;
                view.originalAnchorMin = view.label.anchorMin; view.originalAnchorMax = view.label.anchorMax;
                view.originalPivot = view.label.pivot;
                view.originalRotation = view.label.localRotation;
                view.originalLabelInset = PortLabels.NativeInset(view.label);
            }
            if (PortGeometry.Layout(node.Element) == 0)
            {
                view.label.anchorMin = view.originalAnchorMin; view.label.anchorMax = view.originalAnchorMax;
                view.label.pivot = view.originalPivot; view.label.anchoredPosition = view.originalLabelPosition;
                view.label.localRotation = view.originalRotation;
            }
            else
            {
                PortLabels.Place(view.label, PortLabels.Side(node.Element), view.originalLabelInset);
            }
            view.label.gameObject.SetActive(true);
            view.RefreshVisibility();
            PortGeometry.RefreshPipes(node.Element);
        }
    }
    internal static class PortLabels
    {
        internal static bool Occupied(FlowElement element, PortSide side)
            => (element.Inputs.Count > 0 && PortGeometry.Side(element, true) == side)
                || (element.Outputs.Count > 0 && PortGeometry.Side(element, false) == side);
        internal static PortSide Side(FlowElement element)
        {
            if (!Occupied(element, PortSide.Top)) return PortSide.Top;
            if (!Occupied(element, PortSide.Bottom)) return PortSide.Bottom;
            if (!Occupied(element, PortSide.Right)) return PortSide.Right;
            return PortSide.Left;
        }
        internal static float NativeInset(RectTransform label)
            => (label.pivot.y - .5f) * label.rect.height - label.anchoredPosition.y;
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
    internal static class PortVisuals
    {
        internal static Vector2 SpriteOffset(WorldFlowIO prefab, PortSide side)
        {
            var image = (Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(prefab);
            var offset = image.rectTransform.anchoredPosition;
            // The port root supplies the shift so artwork, hitbox and anchor agree.
            return offset;
        }
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
    public sealed class PortGhostView : MonoBehaviour
    {
        RectTransform label;
        Vector2 originalLabelPosition, originalAnchorMin, originalAnchorMax, originalPivot;
        Quaternion originalRotation;
        float originalLabelInset;
        internal void Initialize(FlowElement element)
        {
            ((RectTransform)transform).sizeDelta = PortGeometry.Size(element);
            if (!label)
            {
                label = ((Image)AccessTools.Field(typeof(WorldFlowGhost), "_labelRect").GetValue(GetComponent<WorldFlowGhost>())).rectTransform;
                originalLabelPosition = label.anchoredPosition;
                originalAnchorMin = label.anchorMin; originalAnchorMax = label.anchorMax; originalPivot = label.pivot;
                originalRotation = label.localRotation;
                originalLabelInset = PortLabels.NativeInset(label);
            }
            label.anchorMin = originalAnchorMin; label.anchorMax = originalAnchorMax; label.pivot = originalPivot;
            label.anchoredPosition = originalLabelPosition;
            label.localRotation = originalRotation; label.gameObject.SetActive(true);
            var ports = transform.Find("PortLayouts.Preview");
            if (ports) { ports.gameObject.SetActive(false); Destroy(ports.gameObject); }
            // Native default placement has no preview ports; preserve it for old layouts.
            if (PortGeometry.Layout(element) == 0) return;
            var root = new GameObject("PortLayouts.Preview", typeof(RectTransform));
            root.layer = gameObject.layer; root.transform.SetParent(transform, false);
            var parent = (RectTransform)root.transform;
            parent.anchorMin = parent.anchorMax = parent.pivot = new Vector2(0, 1);
            parent.anchoredPosition = Vector2.zero;
            var prefab = (WorldFlowIO)AccessTools.Field(typeof(WorldFlowElement), "_ioPrefab").GetValue(FlowPrefabSet.Get(element).Prefab);
            var spriteTemplate = (Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(prefab);
            foreach (var io in element.IO)
            {
                var side = PortGeometry.Side(element, io is FlowInput);
                var obj = new GameObject("Port", typeof(RectTransform), typeof(Image));
                obj.layer = gameObject.layer; obj.transform.SetParent(parent, false);
                var rect = (RectTransform)obj.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                var rotation = Quaternion.Euler(0, 0, PortGeometry.Angle(side));
                rect.anchoredPosition = PortGeometry.Point(io) + (Vector2)(rotation * PortVisuals.SpriteOffset(prefab, side));
                rect.sizeDelta = spriteTemplate.rectTransform.rect.size;
                rect.pivot = spriteTemplate.rectTransform.pivot;
                rect.localRotation = Quaternion.Euler(0, 0, PortGeometry.Angle(side));
                var image = obj.GetComponent<Image>(); image.raycastTarget = false;
                image.sprite = ((Sprite[])AccessTools.Field(typeof(WorldFlowIO), io is FlowInput ? "_inputSprites" : "_outputSprites").GetValue(prefab))[2];
            }
            PortLabels.Place(label, PortLabels.Side(element), originalLabelInset);
        }
    }
}
