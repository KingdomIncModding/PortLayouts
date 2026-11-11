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
        internal static float NativeInset(RectTransform label)
            => (label.pivot.y - .5f) * label.rect.height - label.anchoredPosition.y;
    }
}
