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
        RectTransform label;
        CanvasGroup labelVisibility;
        WorldFlowElement node;
        Canvas canvas;
        Vector2 originalLabelPosition, originalAnchorMin, originalAnchorMax, originalPivot;
        Quaternion originalRotation;
        float originalLabelInset;
        internal bool LabelVisible => label.gameObject.activeSelf && labelVisibility.alpha > 0;
        void LateUpdate() => RefreshVisibility();
    }
}
