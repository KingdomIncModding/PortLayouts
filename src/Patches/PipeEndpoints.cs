using KingdomFoundation.Api;
using KingdomFoundation.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomPortLayouts
{
    static class PipeEndpoints
    {
        internal static void Postfix(UnityEngine.Component __instance)
        {
            var pipe = FoundationApi.WorkspaceVisuals.PipeFromView(__instance);
            if (pipe == null)
                return;
            bool output = pipe.ConnectedOutput?.World && PortGeometry.Layout(pipe.ConnectedOutput.Parent) != 0;
            bool input = pipe.ConnectedInput?.World && PortGeometry.Layout(pipe.ConnectedInput.Parent) != 0;
            if (!output && !input)
                return;
            var line = FoundationApi.WorkspaceVisuals.PipeLine(__instance);
            if (output)
                line.SetPosition(0, PortGeometry.Endpoint(pipe.ConnectedOutput));
            if (input)
                line.SetPosition(line.positionCount - 1, PortGeometry.Endpoint(pipe.ConnectedInput));
            var points = new Vector2[line.positionCount];
            for (int i = 0; i < points.Length; i++)
                points[i] = line.GetPosition(i);
            FoundationApi.WorkspaceVisuals.PipeCollider(__instance).points = points;
        }
    }
}
