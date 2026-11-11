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
        internal static Vector2 Size(FlowElement element)
        {
            var size = CanonicalSize(element);
            if (Layout(element) == 0) return size;
            int across = Math.Max(Horizontal(Side(element, true)) ? element.Inputs.Count : 0,
                Horizontal(Side(element, false)) ? element.Outputs.Count : 0);
            size.x = Mathf.Max(size.x, across * Grid);
            return size;
        }
    }
}
