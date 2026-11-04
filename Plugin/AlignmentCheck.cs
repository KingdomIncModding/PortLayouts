using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Items;
using Assets.Source.Player;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace KingdomPortLayouts
{
    public sealed partial class PortLayoutsPlugin
    {
        readonly Dictionary<Texture2D, Color32[]> atlasPixels = new Dictionary<Texture2D, Color32[]>();
        FlowElement alignmentNode;
        readonly List<FlowPipe> alignmentPipes = new List<FlowPipe>();

        Color32[] Pixels(Texture2D atlas)
        {
            if (atlasPixels.TryGetValue(atlas, out var pixels)) return pixels;
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(atlas.width, atlas.height, 0, RenderTextureFormat.ARGB32);
            var copy = new Texture2D(atlas.width, atlas.height, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(atlas, target); RenderTexture.active = target;
                copy.ReadPixels(new Rect(0, 0, atlas.width, atlas.height), 0, 0); copy.Apply();
                pixels = copy.GetPixels32(); atlasPixels.Add(atlas, pixels); return pixels;
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); Destroy(copy); }
        }
        void VerifyPortArtwork(FlowIO io)
        {
            var image = (Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(io.World);
            var saved = image.sprite;
            try
            {
                // Check every native connection/hover/error artwork state, using its
                // actual alpha pixels rather than assuming the image centre is its tip.
                foreach (var field in new[] { "_inputSprites", "_outputSprites", "_errorSprites" })
                foreach (var sprite in (Sprite[])AccessTools.Field(typeof(WorldFlowIO), field).GetValue(io.World))
                {
                    image.sprite = sprite;
                    var rect = sprite.rect; var pixels = Pixels(sprite.texture);
                    int column = -1, minY = 0, maxY = 0;
                    for (int x = 0; x < (int)rect.width && column < 0; x++)
                    for (int y = 0; y < (int)rect.height; y++)
                    {
                        if (pixels[((int)rect.y + y) * sprite.texture.width + (int)rect.x + x].a < 128) continue;
                        if (column < 0) { column = x; minY = y; }
                        maxY = y;
                    }
                    Check(column >= 0, "Native IO artwork has visible tip pixels: " + sprite.name);
                    var drawing = image.rectTransform.rect;
                    var tip = image.rectTransform.TransformPoint(new Vector2(
                        drawing.xMin + (column + .5f) / rect.width * drawing.width,
                        drawing.yMin + (minY + maxY + 1f) / (2 * rect.height) * drawing.height));
                    var delta = (Vector2)io.Parent.World.transform.InverseTransformVector(tip - PortGeometry.Endpoint(io));
                    var normal = PortGeometry.Normal(PortGeometry.Side(io.Parent, io is FlowInput));
                    Check(Mathf.Abs(delta.x) < .02f,
                        "Shifted pipe anchor matches visible top/bottom arrow tip centreline: " + sprite.name);
                    Check(Vector2.Dot(delta, normal) >= 0 && Vector2.Dot(delta, normal) <= 4.1f,
                        "Left shift preserves the arrow tip's normal distance from the border: " + sprite.name);
                }
            }
            finally { image.sprite = saved; }
        }
        void BuildAlignmentFixture()
        {
            foreach (var node in GamePlayer.Current.ActiveFlow.Elements) if (node.World) node.World.gameObject.SetActive(false);
            foreach (var pipe in GamePlayer.Current.ActiveFlow.Pipes) if (pipe.World) pipe.World.gameObject.SetActive(false);
            var blacksmith = new Blacksmith { Position = new Vector2Int(-192, 160) };
            blacksmith.SetupRecipe(CraftingRecipe.Get("iron_sword"));
            alignmentNode = blacksmith;
            PortGeometry.SetLayout(alignmentNode, 1);
            WorldFlowManager.Instance.CreateNewElement(alignmentNode);
            var source = new Filter { Position = new Vector2Int(-576, 640) };
            var sink = new Filter { Position = new Vector2Int(-576, -512) };
            WorldFlowManager.Instance.CreateNewElement(source); WorldFlowManager.Instance.CreateNewElement(sink);
            for (int i = 0; i < alignmentNode.Inputs.Count; i++)
            {
                var pipe = new FlowPipe(); pipe.Connect(source.Outputs[i % source.Outputs.Count], alignmentNode.Inputs[i]);
                var endpoint = PortGeometry.Endpoint(alignmentNode.Inputs[i]);
                pipe.Nodes.Add(Vector2Int.RoundToInt((Vector2)endpoint + new Vector2(16, 96)));
                WorldFlowManager.Instance.CreateNewPipe(pipe); alignmentPipes.Add(pipe);
            }
            var output = new FlowPipe(); output.Connect(alignmentNode.Outputs[0], sink.Inputs[0]);
            output.Nodes.Add(Vector2Int.RoundToInt((Vector2)PortGeometry.Endpoint(alignmentNode.Outputs[0]) + new Vector2(-16, -96)));
            WorldFlowManager.Instance.CreateNewPipe(output); alignmentPipes.Add(output);
            WorldFlowCamera.Instance.SetZoomLevel(4);
            Camera.main.transform.position = new Vector3(alignmentNode.World.Center.x, alignmentNode.World.Center.y, Camera.main.transform.position.z);
            Canvas.ForceUpdateCanvases();
        }
        void VerifyAlignmentFixture()
        {
            Check(alignmentNode.Inputs.Count == 2 && alignmentNode.Outputs.Count == 1,
                "Sword blacksmith regression reproduces two top inputs and one bottom output");
            foreach (var io in alignmentNode.IO) VerifyPortArtwork(io);
            foreach (var pipe in alignmentPipes)
            {
                var line = pipe.World.GetComponentInChildren<LineRenderer>();
                Check(line.useWorldSpace, "Rendered pipe uses the world coordinates checked for artwork alignment");
                Near(line.GetPosition(0), PortGeometry.Endpoint(pipe.ConnectedOutput), "Blacksmith regression output line endpoint matches attachment");
                Near(line.GetPosition(line.positionCount - 1), PortGeometry.Endpoint(pipe.ConnectedInput), "Blacksmith regression input line endpoint matches attachment");
            }
        }
        IEnumerator VerifyPortDrop(Mouse mouse)
        {
            foreach (var node in GamePlayer.Current.ActiveFlow.Elements) if (node.World) node.World.gameObject.SetActive(false);
            for (int reverse = 0; reverse < 2; reverse++)
            {
                var from = new Filter { Position = new Vector2Int(-320, 128), Mirrored = reverse == 1 };
                var to = new Filter { Position = new Vector2Int(192, 128), Mirrored = reverse == 1 };
                PortGeometry.SetLayout(from, 1); PortGeometry.SetLayout(to, 1);
                WorldFlowManager.Instance.CreateNewElement(from); WorldFlowManager.Instance.CreateNewElement(to);
                WorldFlowCamera.Instance.SetZoomLevel(3);
                Camera.main.transform.position = new Vector3(0, 64, Camera.main.transform.position.z);
                Canvas.ForceUpdateCanvases();
                yield return new WaitForSecondsRealtime(.2f);
                var output = from.Outputs[0]; var input = to.Inputs[0];
                var start = output.World; var end = input.World;
                foreach (var io in new FlowIO[] { output, input })
                {
                    var hitbox = BoundsIn((RectTransform)io.World.transform, io.Parent.World.transform);
                    Check(Mathf.Abs(hitbox.center.x - 33) < .01f, "Top/bottom IO hitbox moves left with the artwork");
                    Check(Mathf.Abs(PortGeometry.Endpoint(io).x - io.Parent.Position.x - 33) < .01f,
                        "Pipe connection snap point uses the shifted IO position");
                }
                start.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
                Check(ReferenceEquals(DragDropManager.Instance.Target, start), "Native pipe drag starts at shifted IO port");
                var target = PortGeometry.Endpoint(input);
                var screen = RectTransformUtility.WorldToScreenPoint(end.GetComponentInParent<Canvas>().worldCamera, target);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
                yield return new WaitForSecondsRealtime(.2f);
                Assets.Behaviour.Input.PlayerInput.Instance.UpdateMouseWorld();
                var hovered = UIHelper.GetMouseOverUI();
                Check(hovered?.GetComponentInParent<WorldFlowIO>() == end,
                    "Native raycast recognizes shifted target: hovered=" + (hovered ? hovered.name : "none")
                    + " screen=" + screen + " mouse=" + Mouse.current.position.ReadValue() + " target=" + target
                    + " active=" + end.gameObject.activeInHierarchy);
                start.OnDragDropMove(Vector2Int.RoundToInt(target), Vector2Int.zero);
                var ghost = (WorldFlowPipe)AccessTools.Field(typeof(WorldFlowIO), "_pipeGhost").GetValue(start);
                var preview = ghost.GetComponentInChildren<LineRenderer>();
                Near(preview.GetPosition(0), PortGeometry.Endpoint(output), "Pipe drag preview starts at shifted source attachment");
                Near(preview.GetPosition(preview.positionCount - 1), target, "Pipe drag preview reaches shifted target attachment");
                DragDropManager.Instance.ClearDragDrop(); start.OnDragDropEnd();
                var pipe = output.ConnectedPipe;
                Check(pipe != null && pipe.ConnectedInput == input && input.ConnectedPipe == pipe,
                    "Dropping on shifted port commits native IO connection");
                var line = pipe.World.GetComponentInChildren<LineRenderer>();
                Near(line.GetPosition(0), PortGeometry.Endpoint(output), "Committed pipe stays on shifted source attachment");
                Near(line.GetPosition(line.positionCount - 1), target, "Committed pipe stays on shifted target attachment");
                from.World.gameObject.SetActive(false); to.World.gameObject.SetActive(false); pipe.World.gameObject.SetActive(false);
            }
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-200, -200) });
        }
    }
}
