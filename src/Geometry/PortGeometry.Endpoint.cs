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
        internal static Vector3 Endpoint(FlowIO io)
        {
            if (Layout(io.Parent) == 0)
                return io.World.transform.position + new Vector3((io is FlowOutput ? 16 : -16) * (io.Parent.Mirrored ? -1 : 1), 0, 0);
            return io.World.transform.position + io.Parent.World.transform.TransformVector(Normal(Side(io.Parent, io is FlowInput)) * 16);
        }
    }
}
