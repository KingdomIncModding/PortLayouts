using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    public sealed class PortGhostView : MonoBehaviour
    {
        RectTransform label;
        Vector2 originalLabelPosition, originalAnchorMin, originalAnchorMax, originalPivot;
        Quaternion originalRotation;
        float originalLabelInset;
        internal void Initialize(WorkspaceObject element)
        {
            ((RectTransform)transform).sizeDelta = PortGeometry.Size(element);
            if (!label)
            {
                label = FoundationApi.WorkspaceVisuals.GhostLabel(this);
                originalLabelPosition = label.anchoredPosition;
                originalAnchorMin = label.anchorMin;
                originalAnchorMax = label.anchorMax;
                originalPivot = label.pivot;
                originalRotation = label.localRotation;
                originalLabelInset = PortLabels.NativeInset(label);
            }

            label.anchorMin = originalAnchorMin;
            label.anchorMax = originalAnchorMax;
            label.pivot = originalPivot;
            label.anchoredPosition = originalLabelPosition;
            label.localRotation = originalRotation;
            label.gameObject.SetActive(true);
            var ports = transform.Find("PortLayouts.Preview");
            if (ports)
            {
                ports.gameObject.SetActive(false);
                Destroy(ports.gameObject);
            }

            // Native default placement has no preview ports; preserve it for old layouts.
            if (PortGeometry.Layout(element) == 0)
                return;
            var root = new GameObject("PortLayouts.Preview", typeof(RectTransform));
            root.layer = gameObject.layer;
            root.transform.SetParent(transform, false);
            var parent = (RectTransform)root.transform;
            parent.anchorMin = parent.anchorMax = parent.pivot = new Vector2(0, 1);
            parent.anchoredPosition = Vector2.zero;
            var prefab = FoundationApi.WorkspaceVisuals.PortTemplate(element);
            var spriteTemplate = (Image)FoundationApi.WorkspaceVisuals.PortGraphic(prefab, GamePortGraphic.Sprite);
            foreach (var io in element.IO)
            {
                var side = PortGeometry.Side(element, io.IsInput);
                var obj = new GameObject("Port", typeof(RectTransform), typeof(Image));
                obj.layer = gameObject.layer;
                obj.transform.SetParent(parent, false);
                var rect = (RectTransform)obj.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                var rotation = Quaternion.Euler(0, 0, PortGeometry.Angle(side));
                rect.anchoredPosition = PortGeometry.Point(io) + (Vector2)(rotation * PortVisuals.SpriteOffset(prefab, side));
                rect.sizeDelta = spriteTemplate.rectTransform.rect.size;
                rect.pivot = spriteTemplate.rectTransform.pivot;
                rect.localRotation = Quaternion.Euler(0, 0, PortGeometry.Angle(side));
                var image = obj.GetComponent<Image>();
                image.raycastTarget = false;
                image.sprite = FoundationApi.WorkspaceVisuals.PortSprite(prefab, io.IsInput, 2);
            }

            PortLabels.Place(label, PortLabels.Side(element), originalLabelInset);
        }
    }
}
