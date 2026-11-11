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
        [HarmonyPatch(typeof(WorldFlowPipe), nameof(WorldFlowPipe.UpdateLinePositions))]
    static partial class PipeEndpoints
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
}
