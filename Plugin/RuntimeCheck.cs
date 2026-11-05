using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Player;
using Assets.Source.Util;
using BepInEx;
using HarmonyLib;
using LightJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KingdomPortLayouts
{
    public sealed partial class PortLayoutsPlugin
    {
        readonly List<string> checks = new List<string>();
        readonly List<FlowElement> companions = new List<FlowElement>();
        void Check(bool passed, string name)
        { if (!passed) throw new InvalidOperationException(name); checks.Add(name); }
        void Near(Vector2 actual, Vector2 expected, string name)
            => Check(Vector2.Distance(actual, expected) < .02f, name + " (" + actual + " / " + expected + ")");
        IEnumerator RuntimeCheck()
        {
            yield return new WaitForSecondsRealtime(5);
            string directory = Path.Combine(Paths.GameRootPath, "_Mods", "PortLayouts", "Verification");
            Directory.CreateDirectory(directory);
            string error = null;
            FlowElement source = null, sink = null; FlowPipe pipe = null;
            Keyboard keyboard = null; Mouse mouse = null;
            IEnumerator portDrop = null;
            Vector2 hover = default;
            try
            {
                Check(!GameUI.Instance && GamePlayer.Current == null, "No normal kingdom or save loaded");
                var player = GamePlayer.CreateNewPlayer();
                player.Flows[0].Elements.Clear(); player.Flows[0].Pipes.Clear();
                player.Flows[0].Label = "Port Layouts verification";
                player.AddTech("manifold"); player.AddTech("collector"); player.AddTech("distributor");
                source = new Filter { Position = new Vector2Int(-512, 128) };
                sink = new Filter { Position = new Vector2Int(512, 128) };
                player.Flows[0].Elements.AddRange(new FlowElement[] { source, sink });
                CreateExpansionFixtures(player);
                pipe = new FlowPipe(); pipe.Connect(source.Outputs[0], sink.Inputs[0]);
                pipe.Nodes.Add(new Vector2Int(64, 88)); pipe.Nodes.Add(new Vector2Int(384, 88));
                player.Flows[0].Pipes.Add(pipe);
                Check(PortGeometry.Layout(source) == 0, "Existing nodes default to Left/Right");
                SceneManager.LoadScene("Game");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(4);
            if (error == null) try
            {
                Check(source.World && sink.World && pipe.World, "Native nodes and pipes loaded in disposable kingdom");
                VerifyFoundationResearch(source, directory);
                Canvas.ForceUpdateCanvases();
                var nativeTitle = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(source.World)).rectTransform;
                VerifyTitleGeometry(nativeTitle, (RectTransform)source.World.transform, PortSide.Top);
                VerifyExpansionSizing(directory);
                var originalPosition = source.Position;
                var originalBends = pipe.Nodes.ToArray();
                var originalPorts = source.IO.Select(io => (Vector2)io.World.transform.position).ToArray();
                var line = pipe.World.GetComponentInChildren<LineRenderer>();
                Near(line.GetPosition(0), source.Outputs[0].World.transform.position + Vector3.right * 16, "Default native pipe endpoint preserved");
                Check(!source.ToJson()[PortGeometry.JsonKey].IsInteger, "Old/default saves need no layout metadata");
                var expectedInputs = new[] {PortSide.Left, PortSide.Top, PortSide.Left, PortSide.Left, PortSide.Right, PortSide.Right};
                var expectedOutputs = new[] {PortSide.Right, PortSide.Bottom, PortSide.Top, PortSide.Bottom, PortSide.Top, PortSide.Bottom};
                var expectedLabels = new[] {PortSide.Top, PortSide.Right, PortSide.Bottom, PortSide.Top, PortSide.Bottom, PortSide.Top};
                for (int layout = 0; layout < 6; layout++)
                for (int reverse = 0; reverse < 2; reverse++)
                {
                    PortGeometry.SetLayout(source, layout); source.Mirrored = reverse == 1;
                    PortGeometry.SetLayout(sink, layout); sink.Mirrored = reverse == 0;
                    source.World.UpdateIO(); sink.World.UpdateIO(); Canvas.ForceUpdateCanvases();
                    Check(PortGeometry.Side(source, true) == (reverse == 0 ? expectedInputs[layout] : expectedOutputs[layout]), "Input side matches cycle " + layout + "/" + reverse);
                    Check(PortGeometry.Side(source, false) == (reverse == 0 ? expectedOutputs[layout] : expectedInputs[layout]), "Output side matches cycle " + layout + "/" + reverse);
                    Check(source.Position == originalPosition, "Changing layout preserves snapped node position");
                    Check(source.World.transform.localRotation == Quaternion.identity, "Node body stays upright");
                    foreach (var io in source.IO)
                    {
                        var local = (Vector2)source.World.transform.InverseTransformPoint(io.World.transform.position);
                        var side = PortGeometry.Side(source, io is FlowInput);
                        var size = source.World.Size;
                        if (side == PortSide.Left) Check(Mathf.Abs(local.x) < .01f, "Port on left edge");
                        if (side == PortSide.Right) Check(Mathf.Abs(local.x - size.x) < .01f, "Port on right edge");
                        if (side == PortSide.Top) Check(Mathf.Abs(local.y) < .01f, "Port on top edge");
                        if (side == PortSide.Bottom) Check(Mathf.Abs(local.y + size.y) < .01f, "Port on bottom edge");
                        if (layout != 0 && PortGeometry.Horizontal(side))
                        {
                            Check(Mathf.Abs(local.x - (33 + io.ParentIndex * 64)) < .01f && local.x < size.x, "Horizontal port anchor and hitbox include the seven-unit left shift with native spacing");
                            var artwork = ((Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(io.World)).rectTransform;
                            var center = (Vector2)source.World.transform.InverseTransformPoint(artwork.TransformPoint(artwork.rect.center));
                            Check(Mathf.Abs(center.x - local.x) < .01f,
                                "Top and bottom artwork share their shifted attachment and hitbox centreline");
                            Check(Mathf.Abs(center.y - local.y - (side == PortSide.Top ? -2 : 2)) < .01f,
                                "Left shift retains native sprite inset at the border");
                            VerifyPortArtwork(io);
                        }
                    }
                    Near(line.GetPosition(0), PortGeometry.Endpoint(source.Outputs[0]), "Rendered output meets selected port edge");
                    Near(line.GetPosition(line.positionCount - 1), PortGeometry.Endpoint(sink.Inputs[0]), "Rendered input meets selected port edge");
                    Check(pipe.Nodes.SequenceEqual(originalBends), "Layout preserves existing pipe bends");
                    Check(pipe.ConnectedOutput == source.Outputs[0] && pipe.ConnectedInput == sink.Inputs[0], "Layout preserves actual pipe connections");
                    var edge = (EdgeCollider2D)AccessTools.Field(typeof(WorldFlowPipe), "_collider").GetValue(pipe.World);
                    Near(edge.points[0], line.GetPosition(0), "Pipe hitbox follows selected endpoint");
                    var collider = (BoxCollider2D)AccessTools.Field(typeof(FlowCollider), "_collider").GetValue(source.World.Collider);
                    Near(collider.size, source.World.Size - Vector2.one * 2, "Node collision footprint contains all ports");
                    var loaded = FlowElement.FromJson(source.ToJson(), GamePlayer.Current.ActiveFlow);
                    Check(PortGeometry.Layout(loaded) == layout && loaded.Mirrored == source.Mirrored, "Layout and reverse survive native JSON round trip");
                    var copy = source.CreateCopy();
                    Check(PortGeometry.Layout(copy) == layout && copy.Mirrored == source.Mirrored, "Native picker/copy retains layout and reverse");
                    var view = source.World.GetComponent<PortNodeView>();
                    source.World.OnPointerExit(null);
                    Check(view.LabelVisible, "Label stays visible without hovering in every layout");
                    Check(PortLabels.Side(source) == expectedLabels[layout], "Label selects an edge without ports");
                    source.World.OnPointerEnter(null);
                    Check(view.LabelVisible, "Hovering does not hide the permanent label");
                    VerifyLabelAlignment(source);
                    source.World.OnPointerExit(null);
                }
                PortGeometry.SetLayout(source, 0); source.Mirrored = false; source.World.UpdateIO();
                PortGeometry.SetLayout(sink, 0); sink.Mirrored = false; sink.World.UpdateIO();
                int portIndex = 0;
                foreach (var io in source.IO) Near(io.World.transform.position, originalPorts[portIndex++], "Returning to Left/Right restores exact native port coordinates");
                for (int layout = 1; layout <= 6; layout++)
                {
                    Check(Change(source.World, true), "Cycle succeeds in clear space");
                    Check(PortGeometry.Layout(source) == layout % 6 && !source.Mirrored, "Cycle follows requested order and wraps");
                }
                Check(Change(source.World, false) && source.Mirrored, "Alt reversal enables native mirrored state");
                Check(Change(source.World, false) && !source.Mirrored, "Second Alt reversal restores sides");
                PortGeometry.SetLayout(source, 1); source.World.UpdateIO();
                VerifyDrag(source);
                VerifyCollision(source);
                VerifyGhost(source);
                VerifyBlueprint(source, sink);
                CreateCompanions();
                PortGeometry.SetLayout(source, 0); source.Mirrored = false; source.World.UpdateIO();
                WorldFlowCamera.Instance.SetZoomLevel(2);
                Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
                Canvas.ForceUpdateCanvases();
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                mouse = InputSystem.AddDevice<Mouse>("PortLayoutsDiagnosticMouse");
                keyboard = InputSystem.AddDevice<Keyboard>("PortLayoutsDiagnosticKeyboard");
                hover = RectTransformUtility.WorldToScreenPoint(source.World.GetComponentInParent<Canvas>().worldCamera, source.World.Center);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = hover });
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.LeftAlt, Key.LeftShift));
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                CaptureExpansionScreenshot(directory);
                var click = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, position = hover };
                source.World.OnPointerClick(click);
                Check(PortGeometry.Layout(source) == 1 && !source.Mirrored, "Actual Shift+Alt+click selects Top/Bottom without native double-flip");
                expansionFixtures[0].World.OnPointerClick(click);
                Check(PortGeometry.Layout(expansionFixtures[0]) == 0 && !expansionFixtures[0].Mirrored, "Actual Shift+Alt+click on expandable utility does nothing");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.LeftAlt));
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                var click = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, position = hover };
                source.World.OnPointerClick(click);
                Check(PortGeometry.Layout(source) == 1 && source.Mirrored, "Actual Alt+click reverses current Top/Bottom pair");
                expansionFixtures[0].World.OnPointerClick(click);
                Check(PortGeometry.Layout(expansionFixtures[0]) == 0 && expansionFixtures[0].Mirrored, "Actual Alt+click retains native flip on expandable utility");
                var label = source.World.GetComponentInChildren<WorldFlowLabel>(true);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.LeftAlt, Key.LeftShift));
                Check(label != null, "Native label drag/click component retained");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                source.World.GetComponentInChildren<WorldFlowLabel>(true).OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
                Check(PortGeometry.Layout(source) == 2 && !source.Mirrored, "Shift+Alt+click on label advances to Left/Top in listed direction");
                VerifyCompanions();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-200, -200) });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                Check(source.World.GetComponent<PortNodeView>().LabelVisible, "Real mouse leaving node keeps its label visible");
                var port = source.Inputs[0].World;
                var screen = RectTransformUtility.WorldToScreenPoint(port.GetComponentInParent<Canvas>().worldCamera, port.transform.position);
                InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                Check(source.World.GetComponent<PortNodeView>().LabelVisible, "Port hovering keeps the permanent label visible");
                Capture(Path.Combine(directory, "port-label-free-edge.png"));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-200, -200) });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                Check(source.World.GetComponent<PortNodeView>().LabelVisible, "Label remains visible after leaving a port");
                var ghost = WorldFlowManager.Instance.ShowPurchaseBlueprint(sink, false);
                Check(ReferenceEquals(DragDropManager.Instance.Target, ghost), "Native placement drag begins for label visibility check");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, false, "Placement drag away from node");
                var title = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(source.World)).rectTransform;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = ScreenOutsideTitle(title, PortLabels.Side(source)) });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, false, "Dragging over label alone");
                var title = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(source.World)).rectTransform;
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = ScreenOutsideTitle(title, PortLabels.Side(source)) }, hits);
                Check(!hits.Any(hit => hit.gameObject.transform == title || hit.gameObject.transform.IsChildOf(title)), "Hidden title does not intercept UI raycasts");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = ScreenCenter((RectTransform)source.World.transform) });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, true, "Hovering node beneath placement ghost");
                Capture(Path.Combine(directory, "port-label-drag-hover.png"));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = ScreenCenter((RectTransform)source.Inputs[0].World.transform) });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, true, "Hovering port during placement drag");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(-200, -200) });
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, false, "Leaving node during placement drag");
                Capture(Path.Combine(directory, "port-label-drag-hidden.png"));
                Check(DragDropManager.Instance.CancelDragDrop(), "Placement drag cancels without placing a node");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, true, "Labels restored after placement cancellation");
                var label = source.World.GetComponentInChildren<WorldFlowLabel>(true);
                AccessTools.Method(typeof(WorldFlowLabel), "_startDragDrop").Invoke(label, new object[] { false });
                Check(ReferenceEquals(DragDropManager.Instance.Target, label), "Restored label still starts native node movement");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, true, "Moving node keeps its own label handle visible");
                VerifyLabelVisibility(sink, false, "Moving node hides other labels");
                Check(DragDropManager.Instance.CancelDragDrop(), "Node movement cancels with visibility behavior active");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                VerifyLabelVisibility(source, true, "Moved node label restored after cancellation");
                VerifyLabelVisibility(sink, true, "Other labels restored after movement cancellation");
                Check(source.Position == new Vector2Int(-512, 128), "Visibility behavior preserves native drag cancellation position");
                BuildAlignmentFixture();
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.5f);
            if (error == null) try
            {
                VerifyAlignmentFixture();
                Capture(Path.Combine(directory, "top-bottom-pipe-alignment.png"));
                portDrop = VerifyPortDrop(mouse);
            }
            catch (Exception ex) { error = ex.ToString(); }
            while (error == null && portDrop != null)
            {
                bool more = false; object step = null;
                try { more = portDrop.MoveNext(); if (more) step = portDrop.Current; }
                catch (Exception ex) { error = ex.ToString(); }
                if (!more) break;
                yield return step;
            }
            if (error == null) try { BuildGallery(); }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.5f);
            if (error == null) try { Capture(Path.Combine(directory, "port-layouts.png")); }
            catch (Exception ex) { error = ex.ToString(); }
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            var report = new JsonObject {
                {"passed", error == null}, {"version", Version}, {"gameVersion", Application.version},
                {"unityVersion", Application.unityVersion}, {"bepInExVersion", typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString()},
                {"dllSha256", HashFile(typeof(PortLayoutsPlugin).Assembly.Location)},
                {"foundationSha256", HashFile(typeof(KingdomFoundation.Api.FoundationApi).Assembly.Location)},
                {"count", checks.Count}, {"checks", new JsonArray(checks.Select(c => (JsonValue)c).ToArray())},
                {"companions", new JsonArray(companions.Select(c => (JsonValue)c.ElementType).ToArray())}, {"error", error ?? ""}
            };
            File.WriteAllText(Path.Combine(directory, "runtime-tests.json"), report.ToString());
            if (error != null) Logger.LogError(error); else Logger.LogInfo("Port Layouts diagnostic passed: " + checks.Count + " checks.");
            Application.Quit();
        }
        void VerifyDrag(FlowElement node)
        {
            var start = node.Position;
            var label = node.World.GetComponentInChildren<WorldFlowLabel>(true);
            AccessTools.Method(typeof(WorldFlowLabel), "_startDragDrop").Invoke(label, new object[] { false });
            Check(ReferenceEquals(DragDropManager.Instance.Target, label), "Native label drag begins with top/bottom ports");
            label.OnDragDropMove(start + new Vector2Int(77, -137), Vector2Int.zero);
            Check(node.Position == WorldFlowManager.ClampToGrid(start + new Vector2Int(77, -137)), "Dragging still snaps to native 64-unit grid");
            label.OnDragDropCancel();
            Check(node.Position == start && PortGeometry.Layout(node) == 1, "Cancel drag restores position and retains selected layout");
        }
        static Vector2 ScreenCenter(RectTransform rect)
        {
            var canvas = rect.GetComponentInParent<Canvas>();
            return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.TransformPoint(rect.rect.center));
        }
        static Vector2 ScreenOutsideTitle(RectTransform title, PortSide side)
        {
            // The title now overlaps the body. Only its outer half is label-only hover.
            var point = title.rect.center + Vector2.up * (title.rect.height * (side == PortSide.Bottom ? -.4f : .4f));
            return RectTransformUtility.WorldToScreenPoint(title.GetComponentInParent<Canvas>().worldCamera, title.TransformPoint(point));
        }
        void VerifyLabelVisibility(FlowElement node, bool visible, string scenario)
        {
            var title = ((Image)AccessTools.Field(typeof(WorldFlowElement), "_labelContainer").GetValue(node.World)).rectTransform;
            var group = title.GetComponent<CanvasGroup>();
            Check(node.World.GetComponent<PortNodeView>().LabelVisible == visible, scenario + ": expected artwork visibility");
            Check(group.blocksRaycasts == visible && group.interactable == visible, scenario + ": matching pointer interaction");
            Check(title.gameObject.activeInHierarchy && title.GetComponentInChildren<WorldFlowLabel>(true), scenario + ": native drag handle stays active");
        }
        void VerifyCollision(FlowElement source)
        {
            PortGeometry.SetLayout(source, 0); source.World.UpdateIO();
            var blocker = new Collector { Position = source.Position + new Vector2Int(64, 0) };
            WorldFlowManager.Instance.CreateNewElement(blocker); Physics2D.SyncTransforms();
            Check(!Change(source.World, true), "Layout change while overlapping a building is rejected");
            Check(PortGeometry.Layout(source) == 0 && !source.Mirrored, "Blocked layout restores old ports and flip state");
            blocker.World.gameObject.SetActive(false); WorldFlowManager.Instance.RemoveElement(blocker); Physics2D.SyncTransforms();
            Check(Change(source.World, true), "Layout change succeeds after space is cleared");
        }
        void VerifyGhost(FlowElement source)
        {
            var ghost = WorldFlowManager.Instance.ShowPurchaseBlueprint(source, false);
            Near(((RectTransform)ghost.transform).sizeDelta, source.World.Size, "Copy ghost footprint matches selected port layout");
            var title = ((Image)AccessTools.Field(typeof(WorldFlowGhost), "_labelRect").GetValue(ghost)).rectTransform;
            VerifyTitleGeometry(title, (RectTransform)ghost.transform, PortLabels.Side(source));
            Check(ghost.transform.Find("PortLayouts.Preview").childCount == source.Inputs.Count + source.Outputs.Count, "Copy preview includes all relocated ports");
            int previewIndex = 0;
            foreach (var io in source.IO)
            {
                var preview = (RectTransform)ghost.transform.Find("PortLayouts.Preview").GetChild(previewIndex++);
                var actual = ((Image)AccessTools.Field(typeof(WorldFlowIO), "_sprite").GetValue(io.World)).rectTransform;
                var previewCenter = ghost.transform.InverseTransformPoint(preview.TransformPoint(preview.rect.center));
                var actualCenter = source.World.transform.InverseTransformPoint(actual.TransformPoint(actual.rect.center));
                Near(previewCenter, actualCenter, "Placement preview artwork matches the corrected live port exactly");
            }
            ghost.OnDragDropMove(new Vector2Int(-650, -384), Vector2Int.zero);
            var target = WorldFlowManager.ClampToGrid(new Vector2Int(-650, -384));
            Near(((RectTransform)ghost.transform).anchoredPosition, target, "Copy preview snaps to native grid");
            var player = GamePlayer.Current;
            player.AddCurrency(Currency.Gold, 1000000000, false); player.AddCurrency(Currency.Medal, 1000000000, false);
            player.AddCurrency(Currency.Ring, 1000000000, false);
            int count = player.ActiveFlow.Elements.Count;
            DragDropManager.Instance.ClearDragDrop(); AccessTools.Method(typeof(WorldFlowGhost), "Update").Invoke(ghost, null);
            ghost.OnDragDropEnd();
            Check(player.ActiveFlow.Elements.Count == count + 1, "Selected layout commits through native copy placement");
            var placed = player.ActiveFlow.Elements.Last();
            Check(placed.Position == target && PortGeometry.Layout(placed) == 1, "Placed copy retains position and Top/Bottom layout");
            Near(placed.World.Size, source.World.Size, "Placed copy matches preview footprint");
        }
        void VerifyBlueprint(FlowElement source, FlowElement sink)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SharableBlueprints");
            if (assembly == null) return;
            var box = Activator.CreateInstance(assembly.GetType("SharableBlueprints.SelectionBox"), new object[] {-550, -200, 800, 180});
            var capture = assembly.GetType("SharableBlueprints.BlueprintCapture").GetMethod("Capture").Invoke(null, new[] {(object)GamePlayer.Current.ActiveFlow, box});
            var document = (JsonObject)capture.GetType().GetField("Document").GetValue(capture);
            var restored = FlowDiagram.FromJson(document["layout"]);
            Check(restored.Elements.Count == 2 && restored.Pipes.Count == 1, "Sharable Blueprints captures both nodes and internal pipe");
            Check(PortGeometry.Layout(restored.Elements[0]) == 1, "Sharable Blueprints preserves custom port layout");
            var codec = assembly.GetType("SharableBlueprints.BlueprintShareCode");
            string code = (string)codec.GetMethod("EncodeCompact").Invoke(null, new object[] {document.ToString()});
            string decoded = (string)codec.GetMethod("Decode").Invoke(null, new object[] {code});
            var shared = FlowDiagram.FromJson(JsonValue.Parse(decoded)["layout"]);
            Check(PortGeometry.Layout(shared.Elements[0]) == 1 && shared.Pipes.Count == 1, "Compact KIB2 share-code round trip preserves port layout and connections");
        }
        void CreateCompanions()
        {
            foreach (var name in new[] {"Party", "Ledger", "LeylineConduit"})
            {
                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Any(p => p.Metadata.Name == (name == "Party" ? "Party Node" : name == "Ledger" ? "Ledger" : "Leyline Conduits")))
                {
                    // Constructors are supplied by their respective loaded plugins.
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == (name == "Party" ? "PartyNode" : name == "Ledger" ? "LedgerMod" : "LeylineConduits"))) continue;
                }
                var node = FlowElement.Create(name);
                node.Position = new Vector2Int(1536, -companions.Count * 512);
                WorldFlowManager.Instance.CreateNewElement(node); companions.Add(node);
            }
        }
        void VerifyCompanions()
        {
            foreach (var node in companions)
            for (int layout = 0; layout < 6; layout++)
            {
                PortGeometry.SetLayout(node, layout); node.World.UpdateIO(); Canvas.ForceUpdateCanvases();
                Near(node.World.Size, PortGeometry.Size(node), "Companion node footprint: " + node.ElementType + "/" + layout);
                int expected = PortGeometry.CanRotate(node) ? layout : 0;
                Check(PortGeometry.Layout(node.CreateCopy()) == expected, "Companion node copied allowed layout: " + node.ElementType);
                if (!PortGeometry.CanRotate(node)) Check(!Change(node.World, true) && PortGeometry.Layout(node) == 0, "Expandable companion cannot cycle port layouts: " + node.ElementType);
                foreach (var io in node.IO)
                    if (expected != 0) Near(io.World.transform.position, (Vector2)node.Position + PortGeometry.Point(io), "Companion custom ports follow layout: " + node.ElementType);
            }
        }
        void BuildGallery()
        {
            foreach (var node in GamePlayer.Current.ActiveFlow.Elements) if (node.World) node.World.gameObject.SetActive(false);
            foreach (var pipe in GamePlayer.Current.ActiveFlow.Pipes) if (pipe.World) pipe.World.gameObject.SetActive(false);
            for (int i = 0; i < 12; i++)
            {
                var node = new Filter { Position = new Vector2Int(-720 + (i % 6) * 256, 240 - (i / 6) * 400), Mirrored = i >= 6 };
                PortGeometry.SetLayout(node, i % 6); WorldFlowManager.Instance.CreateNewElement(node);
            }
            var doodles = (RectTransform)AccessTools.Field(typeof(WorldFlowManager), "_doodleParent").GetValue(WorldFlowManager.Instance);
            foreach (Transform flow in doodles) foreach (Transform doodle in flow) doodle.gameObject.SetActive(false);
            WorldFlowCamera.Instance.SetZoomLevel(3);
            Camera.main.transform.position = new Vector3(0, 0, Camera.main.transform.position.z);
        }
        static string HashFile(string path)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        static void Capture(string path)
        {
            Canvas.ForceUpdateCanvases();
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            var previous = RenderTexture.active;
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                foreach (var camera in Camera.allCameras.OrderBy(c => c.depth))
                {
                    var old = camera.targetTexture;
                    try { camera.targetTexture = target; camera.Render(); }
                    finally { camera.targetTexture = old; }
                }
                RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); texture.Apply();
                File.WriteAllBytes(path, ImageConversion.EncodeToPNG(texture));
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); Destroy(texture); }
        }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.Store))] static class NoSave { static bool Prefix() => !Diagnostic; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.StoreAutosaveState))] static class NoAutosave { static bool Prefix() => !Diagnostic; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.LoadLatestSave))] static class NoLoad
        { static bool Prefix(ref bool __result) { if (!Diagnostic) return true; __result = false; return false; } }
        [HarmonyPatch(typeof(SteamAchievement), nameof(SteamAchievement.Trigger))] static class NoAchievement { static bool Prefix() => !Diagnostic; }
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Initialized), MethodType.Getter)] static class Offline
        { static bool Prefix(ref bool __result) { if (!Diagnostic) return true; __result = false; return false; } }
    }
}
