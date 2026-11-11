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
}
