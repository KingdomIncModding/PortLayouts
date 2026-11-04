using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using HarmonyLib;
using LightJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.OnPointerClick))]
    static class PortClick
    {
        static bool Prefix(WorldFlowElement __instance, PointerEventData eventData)
        {
            if (Keyboard.current == null || eventData.button != PointerEventData.InputButton.Left
                || !Keyboard.current.altKey.isPressed || Keyboard.current.ctrlKey.isPressed) return true;
            // Consume the rotation chord on expandable nodes; plain Alt stays native.
            if (!PortGeometry.CanRotate(__instance.Element)) return !Keyboard.current.shiftKey.isPressed;
            if (Assets.Behaviour.Input.PlayerInput.Instance.TraverseDistance <= 50
                && !(DragDropManager.Instance && DragDropManager.Instance.Busy))
                PortLayoutsPlugin.Instance.Change(__instance, Keyboard.current.shiftKey.isPressed);
            return false;
        }
    }
    [HarmonyPatch(typeof(FlowElement), nameof(FlowElement.ToJson))]
    static class SaveLayout
    {
        static void Postfix(FlowElement __instance, ref JsonValue __result)
        {
            int layout = PortGeometry.Layout(__instance);
            if (layout != 0) __result.AsJsonObject[PortGeometry.JsonKey] = layout;
        }
    }
    [HarmonyPatch(typeof(FlowElement), nameof(FlowElement.FromJson))]
    static class LoadLayout
    {
        static void Postfix(JsonValue data, FlowElement __result)
        {
            if (__result == null || !data.IsJsonObject) return;
            var value = data[PortGeometry.JsonKey];
            PortGeometry.SetLayout(__result, value.IsInteger ? value.AsInteger : 0);
        }
    }
    [HarmonyPatch(typeof(FlowElement), nameof(FlowElement.CreateCopy))]
    static class CopyLayout
    {
        static void Postfix(FlowElement __instance, FlowElement __result)
        { if (__result != null) PortGeometry.SetLayout(__result, PortGeometry.Layout(__instance)); }
    }
    [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.Size), MethodType.Getter)]
    static class LayoutSize
    {
        static bool Prefix(WorldFlowElement __instance, ref Vector2 __result)
        {
            if (__instance.Element == null || !PortGeometry.CanRotate(__instance.Element)) return true;
            __result = PortGeometry.Size(__instance.Element); return false;
        }
    }
    [HarmonyPatch(typeof(WorldFlowIO), nameof(WorldFlowIO.SetIO))]
    static class PositionPorts
    { static void Postfix(WorldFlowIO __instance, FlowIO io) => PortVisuals.Apply(__instance, io); }
    [HarmonyPatch(typeof(WorldFlowElement), nameof(WorldFlowElement.UpdateIO))]
    static class FinishPorts
    {
        static void Prefix(WorldFlowElement __instance)
        {
            // Native UpdateIO destroys old views but leaves them in this list.
            // Keep native drag/mirror loops restricted to the current port views.
            ((System.Collections.Generic.List<WorldFlowIO>)AccessTools.Field(typeof(WorldFlowElement), "_io").GetValue(__instance)).Clear();
        }
        static void Postfix(WorldFlowElement __instance) => PortNodeView.Apply(__instance);
    }
    [HarmonyPatch(typeof(WorldFlowPipe), nameof(WorldFlowPipe.UpdateLinePositions))]
    static class PipeEndpoints
    {
        static void Postfix(WorldFlowPipe __instance)
        {
            var pipe = __instance.Pipe;
            if (pipe == null) return;
            bool output = pipe.ConnectedOutput?.World && PortGeometry.Layout(pipe.ConnectedOutput.Parent) != 0;
            bool input = pipe.ConnectedInput?.World && PortGeometry.Layout(pipe.ConnectedInput.Parent) != 0;
            if (!output && !input) return;
            var line = (LineRenderer)AccessTools.Field(typeof(WorldFlowPipe), "_line").GetValue(__instance);
            if (output) line.SetPosition(0, PortGeometry.Endpoint(pipe.ConnectedOutput));
            if (input) line.SetPosition(line.positionCount - 1, PortGeometry.Endpoint(pipe.ConnectedInput));
            var points = new Vector2[line.positionCount];
            for (int i = 0; i < points.Length; i++) points[i] = line.GetPosition(i);
            ((EdgeCollider2D)AccessTools.Field(typeof(WorldFlowPipe), "_collider").GetValue(__instance)).points = points;
        }
    }
    [HarmonyPatch(typeof(WorldFlowGhost), nameof(WorldFlowGhost.SetElement))]
    static class PreviewLayout
    {
        static void Postfix(WorldFlowGhost __instance, FlowElement el)
            => (__instance.GetComponent<PortGhostView>() ?? __instance.gameObject.AddComponent<PortGhostView>()).Initialize(el);
    }
}
