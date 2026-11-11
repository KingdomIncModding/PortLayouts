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
        [BepInDependency("urakka.kingdominc.foundation", "0.11.0")]
    public sealed partial class PortLayoutsPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.portlayouts", Version = "1.0.12";
        internal const string ResearchId = Id + ".rotation";
        FoundationModContext foundation;
        internal static bool RotationUnlocked => FoundationApi.World.Player != null && FoundationApi.World.Player.HasTech(ResearchId);

        Harmony harmony;
        internal static PortLayoutsPlugin Instance;
        void Awake()
        {
            Instance = this;
            try
            {
                foundation = FoundationApi.ForMod(Id);
                foundation.Register(new TechNodeDefinition(ResearchId, "Port Rotation", Id, "Port Layouts", new Vector2(640, 0),
                    "Unlock six input/output side layouts. Shift + Alt + left-click cycles layouts; Alt + left-click reverses the selected pair. Expandable-slot buildings keep their native ports.",
                    research: new TechNodeOptions(fixedCost: new CurrencyCollection())));
                harmony = new Harmony(Id); harmony.PatchAll(typeof(PortLayoutsPlugin).Assembly);
            }
            catch { foundation?.Dispose(); harmony?.UnpatchSelf(); if (Instance == this) Instance = null; throw; }
            Logger.LogInfo("Port Layouts ready: Shift+Alt+click cycles six port layouts; Alt+click reverses inputs and outputs.");
        }
    }
}
