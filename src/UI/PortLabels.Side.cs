using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    internal static partial class PortLabels
    {
        internal static PortSide Side(FlowElement element)
        {
            if (!Occupied(element, PortSide.Top)) return PortSide.Top;
            if (!Occupied(element, PortSide.Bottom)) return PortSide.Bottom;
            if (!Occupied(element, PortSide.Right)) return PortSide.Right;
            return PortSide.Left;
        }
    }
}
