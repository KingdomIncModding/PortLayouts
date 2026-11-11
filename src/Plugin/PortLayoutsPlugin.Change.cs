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
        internal bool Change(WorldFlowElement node, bool cycle)
        {
            if (cycle && !PortGeometry.CanRotate(node.Element)) return false;
            if (cycle && !RotationUnlocked)
            {
                UISounds.CraftStep();
                GameUI.Instance.ShowWarning(Assets.Behaviour.Input.PlayerInput.MouseUI, "Research Port Rotation for free in Tech Tree > Mods.");
                return false;
            }
            int previous = PortGeometry.Layout(node.Element); bool flipped = node.Element.Mirrored;
            if (cycle) { PortGeometry.SetLayout(node.Element, (previous + 1) % PortGeometry.LayoutCount); node.Element.Mirrored = false; }
            else node.Element.Mirrored = !flipped;
            node.UpdateIO(); Physics2D.SyncTransforms();
            if (Blocked(node))
            {
                PortGeometry.SetLayout(node.Element, previous); node.Element.Mirrored = flipped;
                node.UpdateIO(); Physics2D.SyncTransforms(); UISounds.CraftStep();
                GameUI.Instance.ShowWarning(Assets.Behaviour.Input.PlayerInput.MouseUI, "@NoSpace");
                return false;
            }
            if (FoundationApi.View.NativeCamera) FoundationApi.View.RecalculateCameraBounds();
            UISounds.TurnPage(); node.ShowMessage(PortGeometry.Description(node.Element), Color.white);
            return true;
        }
    }
}
