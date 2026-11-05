using System;
using System.IO;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.TechTree;
using Assets.Source.Flow;
using Assets.Source.Player;
using BepInEx.Bootstrap;
using HarmonyLib;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomPortLayouts
{
    public sealed partial class PortLayoutsPlugin
    {
        void VerifyFoundationResearch(FlowElement source, string directory)
        {
            Check(Chainloader.PluginInfos.TryGetValue("urakka.kingdominc.foundation", out var dependency)
                && dependency.Metadata.Version >= new Version(0, 4, 1), "Foundation loads before Port Rotation");
            Check(FoundationApi.TechNodes.TryGet(ResearchId, out var registered) && ReferenceEquals(registered, research)
                && registered is INativeTechNode, "Port Rotation owns one public Foundation research registration");
            var native = ((INativeTechNode)registered).NativeNode;
            Check(native.Name == "Port Rotation" && native.Root == null && native.Previous == null
                && !native.AllRequirements.Any() && native.GetCost().IsEmpty, "Standalone Port Rotation research is free with no prerequisites");
            var player = GamePlayer.Current;
            Check(!RotationUnlocked, "New kingdoms begin with rotation research locked");
            Check(!Change(source.World, true) && PortGeometry.Layout(source) == 0 && !source.Mirrored,
                "Unresearched Shift+Alt rotation leaves the node unchanged");
            Check(Change(source.World, false) && source.Mirrored && Change(source.World, false) && !source.Mirrored,
                "Native Alt reversal remains usable before research");
            var legacy = source.ToJson(); legacy.AsJsonObject[PortGeometry.JsonKey] = 1;
            Check(PortGeometry.Layout(FlowElement.FromJson(legacy, player.ActiveFlow)) == 1,
                "Existing rotated saves retain their layout before research");
            try
            {
                GameUI.Instance.ToggleTechMenu(true);
                var tree = TechTreeMenu.Instance;
                Check(!tree.Nodes.Any(n => n.Tech.Identifier == ResearchId), "Port Rotation does not appear on Base Game research");
                Resources.FindObjectsOfTypeAll<Button>().Single(b => b.name == "ModsTab" && b.gameObject.activeInHierarchy).onClick.Invoke();
                Resources.FindObjectsOfTypeAll<Button>().Single(b => b.name == "Mod:" + Id && b.gameObject.activeInHierarchy).onClick.Invoke();
                Check(tree.Nodes.Count(n => n.Tech.Identifier == ResearchId) == 1, "Mods tab displays exactly one Port Rotation card");
                tree.FocusNode(native);
                var card = tree.Nodes.Single(n => n.Tech.Identifier == ResearchId);
                card.UpdateNodeStatus();
                var prefab = (UITechNode)AccessTools.Field(typeof(TechTreeMenu), "_elementNodePrefab").GetValue(tree);
                Check(card.GetType() == prefab.GetType() && ((RectTransform)card.transform).sizeDelta == ((RectTransform)prefab.transform).sizeDelta
                    && card.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "Port Rotation"), "Research uses the native card and visible Port Rotation title");
                long gold = player.GetCurrency(Currency.Gold);
                card.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
                Check(RotationUnlocked && native.IsPurchased && player.GetCurrency(Currency.Gold) == gold,
                    "Real native research click unlocks rotation without charging gold");
                card.UpdateNodeStatus();
                Capture(Path.Combine(directory, "port-rotation-research.png"));
                Resources.FindObjectsOfTypeAll<Button>().Single(b => b.name == "Base GameTab" && b.gameObject.activeInHierarchy).onClick.Invoke();
                Check(!tree.Nodes.Any(n => n.Tech.Identifier == ResearchId) && tree.Nodes.Any(), "Base Game research remains separate after purchase");
            }
            finally { GameUI.Instance.ToggleTechMenu(false); WorldFlowCamera.Instance.SetZoomLevel(3); }
            try
            {
                var restored = GamePlayer.FromJson(player.ToJson());
                Check(restored.HasTech(ResearchId), "Free research unlock survives native kingdom JSON persistence");
                Check(!GamePlayer.CreateNewPlayer().HasTech(ResearchId), "Research progress does not leak into a new kingdom");
            }
            finally { GamePlayer.Current = player; }
            Check(RotationUnlocked && Change(source.World, true) && PortGeometry.Layout(source) == 1,
                "Researched rotation enables the normal layout cycle immediately");
            PortGeometry.SetLayout(source, 0); source.World.UpdateIO();
        }
    }
}
