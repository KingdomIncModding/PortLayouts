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
        internal static void SetLayout(FlowElement element, int layout)
            => states.GetOrCreateValue(element).Layout = CanRotate(element) && layout >= 0 && layout < LayoutCount ? layout : 0;
    }
}
