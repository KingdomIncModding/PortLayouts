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
        internal const string JsonKey = "UrakkaPortLayout";
        internal const int LayoutCount = 6, Grid = 64, Inset = 40, HorizontalShift = -7;
        static readonly PortSide[,] sides = {
            {PortSide.Left, PortSide.Right}, {PortSide.Top, PortSide.Bottom},
            {PortSide.Left, PortSide.Top}, {PortSide.Left, PortSide.Bottom},
            {PortSide.Right, PortSide.Top}, {PortSide.Right, PortSide.Bottom}
        };

        static readonly ConditionalWeakTable<FlowElement, State> states = new ConditionalWeakTable<FlowElement, State>();
        internal static bool CanRotate(FlowElement element) => !(element is Collector || element is Distributor || element is Manifold)
            && element.ElementType != "Ledger";
    }
}
