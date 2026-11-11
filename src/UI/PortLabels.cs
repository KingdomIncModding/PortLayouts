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
        internal static bool Occupied(FlowElement element, PortSide side)
            => (element.Inputs.Count > 0 && PortGeometry.Side(element, true) == side)
                || (element.Outputs.Count > 0 && PortGeometry.Side(element, false) == side);
    }
}
