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
        internal static Vector2 Point(FlowIO io)
        {
            var element = io.Parent; var side = Side(element, io is FlowInput); var size = Size(element);
            // Move the entire port, including its hitbox and connection anchor.
            if (Horizontal(side)) return new Vector2(Inset + HorizontalShift + Grid * io.ParentIndex, side == PortSide.Top ? 0 : -size.y);
            // Retain native/custom vertical spacing and the IO container's header inset.
            var parent = (RectTransform)AccessTools.Field(typeof(WorldFlowElement), "_ioParent").GetValue(FlowPrefabSet.Get(element).Prefab);
            float header = -parent.offsetMax.y;
            return new Vector2(side == PortSide.Left ? 0 : size.x, -header - Math.Abs((float)io.Position));
        }
    }
}
