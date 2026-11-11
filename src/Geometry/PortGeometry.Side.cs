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
        internal static PortSide Side(FlowElement element, bool input)
            => sides[Layout(element), (input ^ element.Mirrored) ? 0 : 1];
    }
}
