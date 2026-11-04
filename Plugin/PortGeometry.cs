using System;
using System.Runtime.CompilerServices;
using Assets.Behaviour.Flow;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using HarmonyLib;
using UnityEngine;

namespace KingdomPortLayouts
{
    internal enum PortSide { Left, Right, Top, Bottom }
    internal static class PortGeometry
    {
        internal const string JsonKey = "UrakkaPortLayout";
        internal const int LayoutCount = 6, Grid = 64, Inset = 40, HorizontalShift = -7;
        static readonly PortSide[,] sides = {
            {PortSide.Left, PortSide.Right}, {PortSide.Top, PortSide.Bottom},
            {PortSide.Left, PortSide.Top}, {PortSide.Left, PortSide.Bottom},
            {PortSide.Right, PortSide.Top}, {PortSide.Right, PortSide.Bottom}
        };
        sealed class State { internal int Layout; }
        static readonly ConditionalWeakTable<FlowElement, State> states = new ConditionalWeakTable<FlowElement, State>();
        internal static bool CanRotate(FlowElement element) => !(element is Collector || element is Distributor || element is Manifold)
            && element.ElementType != "Ledger";
        internal static int Layout(FlowElement element) => CanRotate(element) && states.TryGetValue(element, out var state) ? state.Layout : 0;
        internal static void SetLayout(FlowElement element, int layout)
            => states.GetOrCreateValue(element).Layout = CanRotate(element) && layout >= 0 && layout < LayoutCount ? layout : 0;
        internal static PortSide Side(FlowElement element, bool input)
            => sides[Layout(element), (input ^ element.Mirrored) ? 0 : 1];
        internal static bool Horizontal(PortSide side) => side == PortSide.Top || side == PortSide.Bottom;
        internal static string Description(FlowElement element) => "Inputs: " + Side(element, true) + " / Outputs: " + Side(element, false);
        internal static Vector2 CanonicalSize(FlowElement element)
        {
            var fallback = ((RectTransform)FlowPrefabSet.Get(element).Prefab.transform).sizeDelta;
            return new Vector2(element.Size.x > 0 ? element.Size.x : fallback.x, element.Size.y > 0 ? element.Size.y : fallback.y);
        }
        internal static Vector2 Size(FlowElement element)
        {
            var size = CanonicalSize(element);
            if (Layout(element) == 0) return size;
            int across = Math.Max(Horizontal(Side(element, true)) ? element.Inputs.Count : 0,
                Horizontal(Side(element, false)) ? element.Outputs.Count : 0);
            size.x = Mathf.Max(size.x, across * Grid);
            return size;
        }
        internal static Vector2 Normal(PortSide side)
        {
            switch (side) {
                case PortSide.Right: return Vector2.right;
                case PortSide.Top: return Vector2.up;
                case PortSide.Bottom: return Vector2.down;
                default: return Vector2.left;
            }
        }
        internal static float Angle(PortSide side)
        {
            switch (side) {
                case PortSide.Right: return 180;
                case PortSide.Top: return -90;
                case PortSide.Bottom: return 90;
                default: return 0;
            }
        }
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
        internal static Vector3 Endpoint(FlowIO io)
        {
            if (Layout(io.Parent) == 0)
                return io.World.transform.position + new Vector3((io is FlowOutput ? 16 : -16) * (io.Parent.Mirrored ? -1 : 1), 0, 0);
            return io.World.transform.position + io.Parent.World.transform.TransformVector(Normal(Side(io.Parent, io is FlowInput)) * 16);
        }
        internal static void RefreshPipes(FlowElement element)
        {
            foreach (var io in element.IO)
                if (io.ConnectedPipe?.World) io.ConnectedPipe.World.UpdateLinePositions();
        }
    }
}
