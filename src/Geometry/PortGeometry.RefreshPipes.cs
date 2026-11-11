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
        internal static void RefreshPipes(FlowElement element)
        {
            foreach (var io in element.IO)
                if (io.ConnectedPipe?.World) io.ConnectedPipe.World.UpdateLinePositions();
        }
    }
}
