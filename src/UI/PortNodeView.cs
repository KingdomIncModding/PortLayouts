using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    public sealed class PortNodeView : MonoBehaviour
    {
        internal static void Apply(UnityEngine.Behaviour node)
        {
            var view = node.GetComponent<PortNodeView>();
            if (!view)
            {
                view = node.gameObject.AddComponent<PortNodeView>();
                view.node = node;
                view.canvas = node.GetComponentInParent<Canvas>();
                view.label = FoundationApi.WorkspaceVisuals.BuildingLabel(node);
                view.labelVisibility = view.label.GetComponent<CanvasGroup>();
                if (!view.labelVisibility)
                    view.labelVisibility = view.label.gameObject.AddComponent<CanvasGroup>();
                view.originalLabelPosition = view.label.anchoredPosition;
                view.originalAnchorMin = view.label.anchorMin;
                view.originalAnchorMax = view.label.anchorMax;
                view.originalPivot = view.label.pivot;
                view.originalRotation = view.label.localRotation;
                view.originalLabelInset = PortLabels.NativeInset(view.label);
            }

            if (PortGeometry.Layout(FoundationApi.WorkspaceAccess.FromView(node)) == 0)
            {
                view.label.anchorMin = view.originalAnchorMin;
                view.label.anchorMax = view.originalAnchorMax;
                view.label.pivot = view.originalPivot;
                view.label.anchoredPosition = view.originalLabelPosition;
                view.label.localRotation = view.originalRotation;
            }
            else
            {
                PortLabels.Place(view.label, PortLabels.Side(FoundationApi.WorkspaceAccess.FromView(node)), view.originalLabelInset);
            }

            view.label.gameObject.SetActive(true);
            view.RefreshVisibility();
            PortGeometry.RefreshPipes(FoundationApi.WorkspaceAccess.FromView(node));
        }

        RectTransform label;
        CanvasGroup labelVisibility;
        UnityEngine.Behaviour node;
        Canvas canvas;
        Vector2 originalLabelPosition, originalAnchorMin, originalAnchorMax, originalPivot;
        Quaternion originalRotation;
        float originalLabelInset;
        internal bool LabelVisible => label.gameObject.activeSelf && labelVisibility.alpha > 0;

        void LateUpdate() => RefreshVisibility();
        bool HoveringNode()
        {
            if (Mouse.current == null)
                return false;
            var mouse = Mouse.current.position.ReadValue();
            var camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)node.transform, mouse, camera))
                return true;
            foreach (var io in FoundationApi.WorkspaceAccess.FromView(node).IO)
                if (io.World && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)io.World.transform, mouse, camera))
                    return true;
            return false;
        }

        internal void RefreshVisibility()
        {
            if (!label || !node)
                return;
            bool hasTarget = FoundationApi.WorkspaceVisuals.HasNativeDragTarget || FoundationApi.UiAccess.IsDeconstructionVisible;
            // Keep the native move handle alive even while its artwork is hidden.
            // Screen-space geometry also works when a placement ghost intercepts pointer events.
            bool visible = !hasTarget || FoundationApi.WorkspaceVisuals.IsNativeMoveHandleFor(node) || HoveringNode();
            labelVisibility.alpha = visible ? 1 : 0;
            labelVisibility.blocksRaycasts = visible;
            labelVisibility.interactable = visible;
        }

        internal static void RestoreAndRemoveAll()
        {
            var views = Resources.FindObjectsOfTypeAll<PortNodeView>();
            foreach (var view in views)
            {
                if (!view || !view.gameObject.scene.IsValid())
                    continue;
                if (view.label)
                {
                    view.label.anchorMin = view.originalAnchorMin;
                    view.label.anchorMax = view.originalAnchorMax;
                    view.label.pivot = view.originalPivot;
                    view.label.anchoredPosition = view.originalLabelPosition;
                    view.label.localRotation = view.originalRotation;
                }
                if (view.labelVisibility)
                {
                    view.labelVisibility.alpha = 1f;
                    view.labelVisibility.blocksRaycasts = true;
                    view.labelVisibility.interactable = true;
                }
                if (view.node)
                {
                    var item = FoundationApi.WorkspaceAccess.FromView(view.node);
                    if (item != null && view.node.transform is RectTransform rect)
                        rect.sizeDelta = PortGeometry.CanonicalSize(item);
                    FoundationApi.WorkspaceVisuals.ClearPortViewIndex(view.node);
                    FoundationApi.WorkspaceVisuals.RefreshPorts(view.node);
                    if (item != null)
                        PortGeometry.RefreshPipes(item);
                }
                Destroy(view);
            }
        }
    }
}
