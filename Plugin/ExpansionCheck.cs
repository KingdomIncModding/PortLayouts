using System;
using System.Collections.Generic;
using System.IO;
using Assets.Behaviour.Flow;
using Assets.Behaviour.Flow.Elements;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Player;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomPortLayouts
{
    public sealed partial class PortLayoutsPlugin
    {
        readonly List<FlowElement> expansionFixtures = new List<FlowElement>();
        void CreateExpansionFixtures(GamePlayer player)
        {
            expansionFixtures.AddRange(new FlowElement[] { new Collector(), new Distributor(), new Manifold() });
            for (int i = 0; i < expansionFixtures.Count; i++)
                expansionFixtures[i].Position = new Vector2Int(2048 + i * 512, 2048);
            player.Flows[0].Elements.AddRange(expansionFixtures);
        }
        static void Collapse(FlowElement node)
        {
            if (node is Collector collector) collector.Collapse();
            else if (node is Distributor distributor) distributor.Collapse();
            else ((Manifold)node).Collapse();
        }
        static void ClickExpansionButton(FlowElement node)
        {
            if (node is Collector) node.World.GetComponent<WorldFlowCollector>().AddInputSlot();
            else if (node is Distributor) node.World.GetComponent<WorldFlowDistributor>().AddOutputSlot();
            else node.World.GetComponent<WorldFlowManifold>().AddOutputSlot();
        }
        void VerifyExpansionSizing(string directory)
        {
            foreach (var node in expansionFixtures)
            {
                Check(node.World, "Native expansion fixture loaded: " + node.ElementType);
                Check(!PortGeometry.CanRotate(node), "Expandable utility is excluded from rotation: " + node.ElementType);
                var nativeLabel = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(node.World)).rectTransform;
                var originalPosition = nativeLabel.anchoredPosition;
                var originalPivot = nativeLabel.pivot;
                for (int layout = 0; layout < 6; layout++)
                for (int reverse = 0; reverse < 2; reverse++)
                {
                    Collapse(node);
                    var legacy = node.ToJson();
                    legacy.AsJsonObject[PortGeometry.JsonKey] = layout;
                    legacy.AsJsonObject["Mirrored"] = reverse == 1;
                    var loaded = FlowElement.FromJson(legacy, GamePlayer.Current.ActiveFlow);
                    Check(PortGeometry.Layout(loaded) == 0 && loaded.Mirrored == (reverse == 1), "Legacy expandable layout falls back to native sides without losing flip");
                    Check(!loaded.ToJson()[PortGeometry.JsonKey].IsInteger, "Saving expandable node drops unsupported layout metadata");
                    PortGeometry.SetLayout(node, layout); node.Mirrored = reverse == 1; node.World.UpdateVisuals();
                    Check(!Change(node.World, true) && PortGeometry.Layout(node) == 0 && node.Mirrored == (reverse == 1), "Cycling an expandable node is ignored without flipping ports");
                    // Invoke the actual native + control: 2 -> 3 -> 4 -> collapse to 2.
                    for (int step = 0; step < 3; step++)
                    {
                        ClickExpansionButton(node); Canvas.ForceUpdateCanvases();
                        int count = step < 2 ? 3 + step : 2;
                        int inputs = node is Distributor ? 1 : count;
                        int outputs = node is Collector ? 1 : count;
                        Check(node.Inputs.Count == inputs && node.Outputs.Count == outputs, "Native + / collapse changes port count: " + node.ElementType);
                        var expected = new Vector2(128, count * 64);
                        Near(node.World.Size, expected, "Expandable node keeps native width and height: " + node.ElementType + "/" + count);
                        Near(((RectTransform)node.World.transform).sizeDelta, expected, "Native rendered expansion dimensions retained");
                        Check(node.Size.y == count * 64, "Native saved expansion data remains unchanged");
                        var collider = (BoxCollider2D)AccessTools.Field(typeof(FlowCollider), "_collider").GetValue(node.World.Collider);
                        Near(collider.size, expected - Vector2.one * 2, "Expanded collision box follows corrected footprint");
                        foreach (var io in node.IO)
                        {
                            var local = (Vector2)node.World.transform.InverseTransformPoint(io.World.transform.position);
                            Check(local.x >= -.01f && local.x <= expected.x + .01f && local.y <= .01f && local.y >= -expected.y - .01f,
                                "Expanded port remains on node footprint");
                        }
                        Check(nativeLabel.gameObject.activeSelf, "Expandable node keeps its native visible title");
                        Check(PortGeometry.Layout(node.CreateCopy()) == 0, "Copying an expandable node retains native sides");
                    }
                }
                PortGeometry.SetLayout(node, 0); node.Mirrored = false; node.World.UpdateVisuals();
                Near(nativeLabel.anchoredPosition, originalPosition, "Left/Right restores native label offset");
                Near(nativeLabel.pivot, originalPivot, "Left/Right restores native label pivot");
                PortGeometry.SetLayout(node, 1); node.World.UpdateVisuals(); ClickExpansionButton(node);
                Near(node.World.Size, new Vector2(128, 192), "Three-port utility expands natively and cannot turn sideways");
                node.World.OnPointerEnter(null);
            }
        }
        void CaptureExpansionScreenshot(string directory)
        {
            var camera = Camera.main;
            var previousPosition = camera.transform.position;
            int previousZoom = WorldFlowCamera.Instance.ZoomLevel;
            WorldFlowCamera.Instance.SetZoomLevel(3);
            camera.transform.position = new Vector3(2640, 1984, previousPosition.z);
            Capture(Path.Combine(directory, "expandable-nodes-native-layout.png"));
            camera.transform.position = previousPosition;
            WorldFlowCamera.Instance.SetZoomLevel(previousZoom);
        }
        void VerifyLabelAlignment(FlowElement node)
        {
            if (PortGeometry.Layout(node) == 0) return;
            Canvas.ForceUpdateCanvases();
            var label = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(node.World)).rectTransform;
            var side = PortLabels.Side(node);
            Check(!PortLabels.Occupied(node, side), "Permanent label occupies a port-free edge");
            VerifyTitleGeometry(label, (RectTransform)node.World.transform, side);
            foreach (var io in node.IO)
            {
                var titleBounds = BoundsIn(label, node.World.transform);
                var portBounds = BoundsIn((RectTransform)io.World.transform, node.World.transform);
                Check(!titleBounds.Overlaps(portBounds), "Permanent label does not overlap a port hitbox");
            }
            node.World.OnPointerExit(null);
            Check(label.gameObject.activeSelf, "Port-free label remains visible on pointer exit");
        }
        static Rect BoundsIn(RectTransform rect, Transform owner)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var corner in corners)
            {
                var local = (Vector2)owner.InverseTransformPoint(corner);
                min = Vector2.Min(min, local); max = Vector2.Max(max, local);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        void VerifyTitleGeometry(RectTransform title, RectTransform owner, PortSide side)
        {
            var bounds = BoundsIn(title, owner);
            var size = owner.sizeDelta;
            if (side == PortSide.Top || side == PortSide.Bottom)
            {
                Check(Mathf.Abs(bounds.center.x - size.x / 2) < .01f, "Top/bottom label is centred on node width");
                Check(Mathf.Abs((side == PortSide.Top ? bounds.yMin : -size.y - bounds.yMax) - 4) < .01f, "Top/bottom label has a four-unit gap from node edge");
                Check(Quaternion.Angle(title.localRotation, Quaternion.identity) < .01f, "Top/bottom text reads normally without inversion");
            }
            else
            {
                Check(Mathf.Abs(bounds.center.y + size.y / 2) < .01f, "Side label is centred on node height");
                Check(Mathf.Abs((side == PortSide.Right ? bounds.xMin - size.x : -bounds.xMax) - 4) < .01f, "Side label has a four-unit gap from node edge");
                var down = owner.InverseTransformDirection(title.TransformDirection(Vector3.down));
                Near(down, -PortGeometry.Normal(side), "Bottoms of sideways letters face inward toward the node");
            }
        }
    }
}
