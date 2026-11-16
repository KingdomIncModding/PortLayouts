using KingdomFoundation.Api;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KingdomPortLayouts
{
    internal static class PortGeometry
    {
        internal static float Angle(PortSide side)
        {
            switch (side)
            {
                case PortSide.Right:
                    return 180;
                case PortSide.Top:
                    return -90;
                case PortSide.Bottom:
                    return 90;
                default:
                    return 0;
            }
        }

        internal static Vector2 CanonicalSize(WorkspaceObject element)
        {
            var template = FoundationApi.WorkspaceVisuals.BuildingTemplate(element);
            var fallback = template ? template.sizeDelta : Vector2.zero;
            return new Vector2(element.Size.x > 0 ? element.Size.x : fallback.x, element.Size.y > 0 ? element.Size.y : fallback.y);
        }

        internal const string JsonKey = "UrakkaPortLayout";
        internal const int LayoutCount = 6, Grid = 64, Inset = 40, HorizontalShift = -7;
        static readonly PortSide[, ] sides =
        {
            {
                PortSide.Left,
                PortSide.Right
            },
            {
                PortSide.Top,
                PortSide.Bottom
            },
            {
                PortSide.Left,
                PortSide.Top
            },
            {
                PortSide.Left,
                PortSide.Bottom
            },
            {
                PortSide.Right,
                PortSide.Top
            },
            {
                PortSide.Right,
                PortSide.Bottom
            }
        };
        static readonly ConditionalWeakTable<WorkspaceObject, global::KingdomPortLayouts.State> states = new ConditionalWeakTable<WorkspaceObject, global::KingdomPortLayouts.State>();
        internal static bool CanRotate(WorkspaceObject element) => element != null && element.Kind == WorkspaceObjectKind.Building && !element.IsJunction && element.TypeId != "Ledger" && PortLayoutsApi.AllowsRotation(element);
        internal static void ResetAllStates()
        {
            if (FoundationApi.Kingdom == null)
                return;
            foreach (var obj in FoundationApi.WorkspaceAccess.AllObjects)
                if (obj.Kind == WorkspaceObjectKind.Building)
                    states.Remove(obj);
        }
        internal static string Description(WorkspaceObject element) => "Inputs: " + Side(element, true) + " / Outputs: " + Side(element, false);
        internal static Vector3 Endpoint(WorkspacePort io)
        {
            if (Layout(io.Parent) == 0)
                return io.World.transform.position + new Vector3((io.IsOutput ? 16 : -16) * (io.Parent.Mirrored ? -1 : 1), 0, 0);
            return io.World.transform.position + io.Parent.World.transform.TransformVector(Normal(Side(io.Parent, io.IsInput)) * 16);
        }

        internal static bool Horizontal(PortSide side) => side == PortSide.Top || side == PortSide.Bottom;
        internal static int Layout(WorkspaceObject element) => CanRotate(element) && states.TryGetValue(element, out global::KingdomPortLayouts.State state) ? state.Layout : 0;
        internal static Vector2 Normal(PortSide side)
        {
            switch (side)
            {
                case PortSide.Right:
                    return Vector2.right;
                case PortSide.Top:
                    return Vector2.up;
                case PortSide.Bottom:
                    return Vector2.down;
                default:
                    return Vector2.left;
            }
        }

        internal static Vector2 Point(WorkspacePort io)
        {
            var element = io.Parent;
            var side = Side(element, io.IsInput);
            var size = Size(element);
            // Move the entire port, including its hitbox and connection anchor.
            if (Horizontal(side))
                return new Vector2(Inset + HorizontalShift + Grid * io.ParentIndex, side == PortSide.Top ? 0 : -size.y);
            // Retain native/custom vertical spacing and the IO container's header inset.
            var parent = FoundationApi.WorkspaceVisuals.PortParentTemplate(element);
            float header = -parent.offsetMax.y;
            return new Vector2(side == PortSide.Left ? 0 : size.x, -header - Math.Abs((float)io.Position));
        }

        internal static void RefreshPipes(WorkspaceObject element)
        {
            foreach (var io in element.IO)
                if (io.ConnectedPipe?.World)
                    FoundationApi.WorkspaceVisuals.RefreshPipe(io.ConnectedPipe.World);
        }

        internal static void SetLayout(WorkspaceObject element, int layout) => states.GetOrCreateValue(element).Layout = CanRotate(element) && layout >= 0 && layout < LayoutCount ? layout : 0;
        internal static PortSide Side(WorkspaceObject element, bool input) => sides[Layout(element), (input ^ element.Mirrored) ? 0 : 1];
        internal static Vector2 Size(WorkspaceObject element)
        {
            var size = CanonicalSize(element);
            if (Layout(element) == 0)
                return size;
            int across = Math.Max(Horizontal(Side(element, true)) ? element.Inputs.Count : 0, Horizontal(Side(element, false)) ? element.Outputs.Count : 0);
            size.x = Mathf.Max(size.x, across * Grid);
            return size;
        }
    }
}
