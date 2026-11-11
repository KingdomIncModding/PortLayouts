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
        void OnDestroy() { foundation?.Dispose(); harmony?.UnpatchSelf(); if (Instance == this) Instance = null; }
    }
}
