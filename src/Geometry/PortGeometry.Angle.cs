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
        internal static float Angle(PortSide side)
        {
            switch (side) {
                case PortSide.Right: return 180;
                case PortSide.Top: return -90;
                case PortSide.Bottom: return 90;
                default: return 0;
            }
        }
    }
}
