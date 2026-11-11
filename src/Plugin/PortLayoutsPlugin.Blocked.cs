using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using KingdomFoundation.Api;
using Assets.Source.Player;

namespace KingdomPortLayouts
{
    public sealed partial class PortLayoutsPlugin : BaseUnityPlugin
    {
        internal static bool Blocked(WorldFlowElement node)
        {
            foreach (var hit in FlowColliderManager.Instance.GetOverlap(node.Position, Vector2Int.RoundToInt(node.Size)))
            {
                var collider = hit.GetComponent<FlowCollider>();
                if (!collider) continue;
                var other = collider.Element ? collider.Element : collider.GetComponentInParent<WorldFlowElement>();
                if (other && other != node) return true;
            }
            return false;
        }
    }
}
