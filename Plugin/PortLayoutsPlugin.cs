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
    [BepInPlugin(Id, "Port Layouts", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.5.0")]
    public sealed partial class PortLayoutsPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.portlayouts", Version = "1.0.12";
        internal const string ResearchId = Id + ".rotation";
        ITechNode research;
        internal static bool RotationUnlocked => GamePlayer.Current != null && GamePlayer.Current.HasTech(ResearchId);
        internal static bool Diagnostic => Environment.GetCommandLineArgs().Contains("-portlayouts-smoke");
        Harmony harmony;
        internal static PortLayoutsPlugin Instance;
        void Awake()
        {
            Instance = this;
            try
            {
                research = FoundationApi.CreateTechNode(new TechNodeDefinition(ResearchId, "Port Rotation", new Vector2(640, 0),
                    "Unlock six input/output side layouts. Shift + Alt + left-click cycles layouts; Alt + left-click reverses the selected pair. Expandable-slot buildings keep their native ports.",
                    research: new TechNodeOptions(fixedCost: new CurrencyCollection())));
                harmony = new Harmony(Id); harmony.PatchAll(typeof(PortLayoutsPlugin).Assembly);
            }
            catch { research?.Dispose(); harmony?.UnpatchSelf(); if (Instance == this) Instance = null; throw; }
            if (Diagnostic) { Application.runInBackground = true; StartCoroutine(RuntimeCheck()); }
            Logger.LogInfo("Port Layouts ready: Shift+Alt+click cycles six port layouts; Alt+click reverses inputs and outputs.");
        }
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
            if (WorldFlowCamera.Instance) WorldFlowCamera.Instance.RecalculateBounds();
            UISounds.TurnPage(); node.ShowMessage(PortGeometry.Description(node.Element), Color.white);
            return true;
        }
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
        void OnDestroy() { research?.Dispose(); harmony?.UnpatchSelf(); if (Instance == this) Instance = null; }
    }
}
