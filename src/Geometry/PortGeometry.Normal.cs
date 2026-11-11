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
        internal static Vector2 Normal(PortSide side)
        {
            switch (side) {
                case PortSide.Right: return Vector2.right;
                case PortSide.Top: return Vector2.up;
                case PortSide.Bottom: return Vector2.down;
                default: return Vector2.left;
            }
        }
    }
}
