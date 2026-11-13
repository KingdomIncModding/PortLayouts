using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine.EventSystems;

namespace KingdomPortLayouts
{
    [BepInPlugin(Id, "Port Layouts", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.14.0")]
    public sealed class PortLayoutsPlugin : BaseUnityPlugin
    {
        internal static bool Blocked(UnityEngine.Behaviour node)
        {
            var item = FoundationApi.WorkspaceAccess.FromView(node);
            foreach (var other in FoundationApi.WorkspaceAccess.Overlap(item.Position, Vector2Int.RoundToInt(FoundationApi.WorkspaceVisuals.Size(node))))
                if (other.Kind == WorkspaceObjectKind.Building && other.World && other.World != node)
                    return true;
            return false;
        }

        internal bool Change(UnityEngine.Behaviour node, bool cycle)
        {
            if (cycle && !PortGeometry.CanRotate(FoundationApi.WorkspaceAccess.FromView(node)))
                return false;
            if (cycle && !RotationUnlocked)
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.CraftStep);
                FoundationApi.UI.ShowWarning(FoundationApi.View.MouseUi, "Research Port Rotation for free in Tech Tree > Mods.");
                return false;
            }

            int previous = PortGeometry.Layout(FoundationApi.WorkspaceAccess.FromView(node));
            bool flipped = FoundationApi.WorkspaceAccess.FromView(node).Mirrored;
            if (cycle)
            {
                PortGeometry.SetLayout(FoundationApi.WorkspaceAccess.FromView(node), (previous + 1) % PortGeometry.LayoutCount);
                FoundationApi.WorkspaceAccess.FromView(node).Mirrored = false;
            }
            else
                FoundationApi.WorkspaceAccess.FromView(node).Mirrored = !flipped;
            FoundationApi.WorkspaceVisuals.RefreshPorts(node);
            Physics2D.SyncTransforms();
            if (Blocked(node))
            {
                PortGeometry.SetLayout(FoundationApi.WorkspaceAccess.FromView(node), previous);
                FoundationApi.WorkspaceAccess.FromView(node).Mirrored = flipped;
                FoundationApi.WorkspaceVisuals.RefreshPorts(node);
                Physics2D.SyncTransforms();
                FoundationApi.UiAccess.PlaySound(GameUiSound.CraftStep);
                FoundationApi.UI.ShowWarning(FoundationApi.View.MouseUi, "@NoSpace");
                return false;
            }

            if (FoundationApi.CameraAccess.ActiveView)
                FoundationApi.View.RecalculateCameraBounds();
            FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
            FoundationApi.WorkspaceAccess.ShowMessage(node, PortGeometry.Description(FoundationApi.WorkspaceAccess.FromView(node)), Color.white);
            return true;
        }

        public const string Id = "urakka.kingdominc.portlayouts", Version = "1.0.13";
        internal const string ResearchId = Id + ".rotation";
        FoundationModContext foundation;
        internal static bool RotationUnlocked => FoundationApi.World.IsInKingdom && FoundationApi.Progression.HasTech(ResearchId);

        readonly System.Collections.Generic.List<IDisposable> hooks = new System.Collections.Generic.List<IDisposable>();
        internal static PortLayoutsPlugin Instance;
        void Awake()
        {
            Instance = this;
            try
            {
                foundation = FoundationApi.ForMod(Id);
                foundation.Register(new TechNodeDefinition(ResearchId, "Port Rotation", Id, "Port Layouts", new Vector2(640, 0), "Unlock six input/output side layouts. Shift + Alt + left-click cycles layouts; Alt + left-click reverses the selected pair. Expandable-slot buildings keep their native ports.", research: TechNodeOptions.Create(fixedCost: new GameMoney())));
                RegisterHooks();
            }
            catch
            {
                foundation?.Dispose();
                DisposeHooks();
                if (Instance == this)
                    Instance = null;
                throw;
            }

            Logger.LogInfo("Port Layouts ready: Shift+Alt+click cycles six port layouts; Alt+click reverses inputs and outputs.");
        }

        private void RegisterHooks()
        {
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingUpdatePorts, before: call => FinishPorts.Prefix(call.View as UnityEngine.Behaviour), after: call => FinishPorts.Postfix(call.View as UnityEngine.Behaviour)));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingSize, before: call =>
            {
                Vector2 result = default;
                if (!LayoutSize.Prefix(call.View as UnityEngine.Behaviour, ref result))
                {
                    call.SetResult(result);
                    call.Cancel();
                }
            }));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingCopy, after: call => CopyLayout.Postfix((WorkspaceObject)call.Subject, call.Result<WorkspaceObject>())));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingDeserialize, after: call => LoadLayout.Postfix(call.Argument<JsonValue>(0), call.Result<WorkspaceObject>())));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingSerialize, after: call =>
            {
                var result = call.Result<JsonValue>();
                SaveLayout.Postfix((WorkspaceObject)call.Subject, ref result);
                call.SetResult(result);
            }));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.PipeUpdateGeometry, after: call => PipeEndpoints.Postfix(call.View)));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.PortBound, after: call => PositionPorts.Postfix(call.View, call.Argument<WorkspacePort>(0))));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.GhostBound, after: call => PreviewLayout.Postfix(call.View, call.Argument<WorkspaceObject>(0))));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingClick, before: call =>
            {
                if (!call.IsCancelled && !PortClick.Prefix(call.View as UnityEngine.Behaviour, call.Argument<PointerEventData>(0)))
                    call.Cancel();
            }));
        }

        private void DisposeHooks()
        {
            foreach (var hook in hooks)
                hook.Dispose();
            hooks.Clear();
        }

        void OnDestroy()
        {
            foundation?.Dispose();
            DisposeHooks();
            if (Instance == this)
                Instance = null;
        }
    }
}
