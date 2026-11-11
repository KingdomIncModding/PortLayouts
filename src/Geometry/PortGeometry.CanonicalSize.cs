using System;
using System.Runtime.CompilerServices;
using Assets.Behaviour.Flow;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using HarmonyLib;
using UnityEngine;

namespace KingdomPortLayouts
{
    internal static partial class PortGeometry
    {
        internal static Vector2 CanonicalSize(FlowElement element)
        {
            var fallback = ((RectTransform)FlowPrefabSet.Get(element).Prefab.transform).sizeDelta;
            return new Vector2(element.Size.x > 0 ? element.Size.x : fallback.x, element.Size.y > 0 ? element.Size.y : fallback.y);
        }
    }
}
