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
        internal static int Layout(FlowElement element) => CanRotate(element) && states.TryGetValue(element, out var state) ? state.Layout : 0;
    }
}
