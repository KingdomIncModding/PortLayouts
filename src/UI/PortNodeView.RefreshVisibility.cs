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
    }
}
