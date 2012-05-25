using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace XLibrary
{
    public partial class ViewModel
    {
        public IRenderer Renderer;

        public IColorProfile XColors;


        PointF LastHoverPoint = new PointF();
        DateTime LastHoverTime = DateTime.Now;


        public void Render()
        {
            // clear and pre-process marked depencies
            RecalcDependencies();

            DoSearch();

            // draw layout
            ScreenSize.Width = Renderer.ViewWidth * ZoomFactor;
            ScreenSize.Height = Renderer.ViewHeight * ZoomFactor;
            ScreenOffset.X = PanOffset.X * ScreenSize.Width;// +(Width * CenterFactor.X - ModelSize.Width * CenterFactor.X);
            ScreenOffset.Y = PanOffset.Y * ScreenSize.Height;// +(Height * CenterFactor.Y - ModelSize.Height * CenterFactor.Y);

            if (ViewLayout == LayoutType.TreeMap)
            {
                DrawTreeMap(Renderer);

                if (ShowingOutside)
                {
                    Renderer.DrawTextBackground(XColors.BorderColor, InternalRoot.AreaF.Width, 0, PanelBorderWidth, InternalRoot.AreaF.Height);
                    DrawNode(InternalRoot, 0, true);
                }

                if (ShowingExternal)
                {
                    Renderer.DrawTextBackground(XColors.BorderColor, ExternalRoot.AreaF.X - PanelBorderWidth, 0, PanelBorderWidth, ExternalRoot.AreaF.Height);
                    DrawNode(ExternalRoot, 0, true);
                }

                DrawNode(CurrentRoot, 0, true);
            }

            else if (ViewLayout == LayoutType.CallGraph)
            {
                DrawCallGraph();

                // id 0 nodes are intermediates
                foreach (var node in Graphs.SelectMany(g => g.Nodes()).Where(n => n.ID != 0))
                    DrawNode(node, 0, false);
            }

            else if (ViewLayout == LayoutType.Timeline)
            {
                DrawTheadline();

                foreach (var node in ThreadlineNodes)
                    DrawNode(node.Node, node.Area, node.LabelArea, 0, false, node.ShowHit);
            }


            // draw ignored over nodes ignored may contain
            foreach (var ignored in IgnoredNodes.Values)
                if (PositionMap.ContainsKey(ignored.ID))
                {
                    Renderer.DrawLine(XColors.IgnoredColor, 1, ignored.AreaF.UpperLeftCorner(), ignored.AreaF.LowerRightCorner(), false);
                    Renderer.DrawLine(XColors.IgnoredColor, 1, ignored.AreaF.UpperRightCorner(), ignored.AreaF.LowerLeftCorner(), false);
                }

            // draw dividers for call graph
            /*if (ViewLayout == LayoutType.CallGraph)
            {
                if (ShowRightOutsideArea)
                    buffer.DrawLine(CallDividerPen, RightDivider, 0, RightDivider, Height);

                if (ShowLeftOutsideArea)
                    buffer.DrawLine(CallDividerPen, LeftDivider, 0, LeftDivider, Height);
            }*/


            // draw dependency graph
            if (ViewLayout == LayoutType.CallGraph &&
                (GraphMode == CallGraphMode.Dependencies ||
                 GraphMode == CallGraphMode.Init ||
                 GraphMode == CallGraphMode.Intermediates))
            {
                foreach (var source in PositionMap.Values)
                {
                    if (source.EdgesOut == null)
                        continue;

                    foreach (var to in source.EdgesOut)
                    {
                        if (!PositionMap.ContainsKey(to))
                            continue;

                        var destination = PositionMap[to];

                        int penWidth = (source.Focused || destination.Focused) ? 2 : 1;

                        if ((!DrawCallGraphVertically && source.AreaF.X < destination.AreaF.X) ||
                            (DrawCallGraphVertically && source.AreaF.Y < destination.AreaF.Y))
                            DrawGraphEdge(penWidth, XColors.CallOutColor, source, destination);
                        else
                            DrawGraphEdge(penWidth, XColors.CallInColor, source, destination);
                    }
                }
            }


            // draw call graph
            if (XRay.FlowTracking && ViewLayout != LayoutType.Timeline)
            {
                foreach (var source in PositionMap.Values)
                {
                    if (source.XNode.CallsOut == null)
                        continue;

                    if (ViewLayout == LayoutType.TreeMap && source.ObjType == XObjType.Class)
                        continue;

                    foreach (var call in source.XNode.CallsOut)
                    {
                        if (!PositionMap.ContainsKey(call.Destination))
                            continue;

                        var destination = PositionMap[call.Destination];

                        // if there are items we're filtering on then only show calls to those nodes
                        if (FilteredNodes.Count > 0 && !IsNodeFiltered(true, source) && !IsNodeFiltered(true, destination))
                            continue;

                        // do after select filter so we can have ignored nodes inside selected, but not the otherway around
                        if (IgnoredNodes.Count > 0 && IsNodeFiltered(false, source) || IsNodeFiltered(false, destination))
                            continue;

                        int lineWidth = (source.Focused || destination.Focused) ? 2 : 1;
   
                        if (call.StillInside > 0 && ShowCalls)
                        {
                            if (ViewLayout == LayoutType.TreeMap)
                                Renderer.DrawLine(XColors.HoldingCallColor, lineWidth, source.CenterF, destination.CenterF, false);
                            else if (ViewLayout == LayoutType.CallGraph)
                                DrawGraphEdge(lineWidth, XColors.HoldingCallColor, source, destination);
                        }

                        else if (XRay.ShowAllCalls &&
                                 GraphMode != CallGraphMode.Intermediates &&
                                 GraphMode != CallGraphMode.Init)
                        {
                            if (ViewLayout == LayoutType.TreeMap)
                            {
                                PointF start = PositionMap[call.Source].CenterF;
                                PointF end = PositionMap[call.Destination].CenterF;
                                PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                                Renderer.DrawLine(XColors.CallOutColor, lineWidth, start, mid, false);
                                Renderer.DrawLine(XColors.CallInColor, lineWidth, mid, end, false);
                            }
                            else if (ViewLayout == LayoutType.CallGraph)
                            {
                                if ((!DrawCallGraphVertically && source.AreaF.X < destination.AreaF.X) ||
                                    (DrawCallGraphVertically && source.AreaF.Y < destination.AreaF.Y))
                                    DrawGraphEdge(lineWidth, XColors.CallOutColor, source, destination);
                                else
                                    DrawGraphEdge(lineWidth, XColors.CallInColor, source, destination);
                            }
                        }

                        if (call.Hit > 0 && ShowCalls)
                        {
                            var color = XColors.CallPenColors[call.Hit];

                            if (ViewLayout == LayoutType.TreeMap)
                                Renderer.DrawLine(color, lineWidth, source.CenterF, destination.CenterF, true);

                            else if (ViewLayout == LayoutType.CallGraph)
                                DrawGraphEdge(lineWidth, color, source, destination, true);
                        }
                    }
                }
            }

            // draw mouse over label
            DrawFooterLabel();
            DrawToolTip();
        }

        private bool IsNodeFiltered(bool select, NodeModel node)
        {
            var map = select ? FilteredNodes : IgnoredNodes;

            foreach (var parent in node.GetParents())
                if (map.ContainsKey(parent.ID))
                    return true;

            return false;
        }

        private void DrawGraphEdge(int penWidth, Color pen, NodeModel source, NodeModel destination, bool dashed=false)
        {
            if (source.Intermediates == null || !source.Intermediates.ContainsKey(destination.ID))
                Renderer.DrawLine(pen, penWidth, source.CenterF, destination.CenterF, dashed);
            else
            {
                var intermediates = source.Intermediates[destination.ID];

                //var endCap = LineCap.NoAnchor;

                var prev = source;
                var last = intermediates.Last();

                foreach (var next in intermediates)
                {
                    //if (next == last)
                    //    endCap = LineCap.ArrowAnchor;

                    Renderer.DrawLine(pen, penWidth, prev.CenterF, next.CenterF, dashed);
                    prev = next;
                }
            }
        }

        private void DrawNode(NodeModel node, int depth, bool drawChildren)
        {
            DrawNode(node, node.AreaF, node.LabelRect, depth, drawChildren, true);
        }

        private void DrawNode(NodeModel node, RectangleF area, RectangleF labelArea, int depth, bool drawChildren, bool showHit)
        {
            if (!node.Show)
                return;

            var xNode = node.XNode;

            bool pointBorder = area.Width < 3.0f || area.Height < 3.0f;

            // use a circle for external/outside nodes in the call map
            bool outside = ViewLayout == LayoutType.CallGraph && node.XNode.External;
            bool needBorder = true;

            Action<Color> draw = (c) =>
            {
                Renderer.DrawNode(c, area, outside, node, depth);
                needBorder = false;
            };

            // blue selection area
            if (node.Hovered)
            {
                if (depth > XColors.OverColors.Length - 1)
                    depth = XColors.OverColors.Length - 1;

                draw(XColors.OverColors[depth]);
            }
            else if (ViewLayout == LayoutType.TreeMap ||
                     ViewLayout == LayoutType.Timeline ||
                     CenterMap.ContainsKey(node.ID))
                draw(XColors.EmptyColor);
            else
                draw(XColors.OutsideColor);

            if (showHit)
            {
                // check if function is an entry point or holding
                if (XRay.FlowTracking && xNode.StillInside > 0)
                {
                    if (xNode.EntryPoint > 0)
                    {
                        if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                            draw(XColors.MultiEntryColor);
                        else
                            draw(XColors.EntryColor);
                    }
                    else
                    {
                        if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                            draw(XColors.MultiHoldingColor);
                        else
                            draw(XColors.HoldingColor);
                    }
                }

                // not an else if, draw over holding or entry
                if (xNode.ExceptionHit > 0)
                    draw(XColors.ExceptionColors[xNode.ExceptionHit]);

                else if (xNode.FunctionHit > 0)
                {
                    if (XRay.ThreadTracking && xNode.ConflictHit > 0)
                        draw(XColors.MultiHitColors[xNode.FunctionHit]);

                    else if (node.ObjType == XObjType.Field)
                    {
                        if (xNode.LastFieldOp == FieldOp.Set)
                            draw(XColors.FieldSetColors[xNode.FunctionHit]);
                        else
                            draw(XColors.FieldGetColors[xNode.FunctionHit]);
                    }
                    else
                        draw(XColors.HitColors[xNode.FunctionHit]);
                }

                else if (xNode.ConstructedHit > 0)
                {
                    draw(XColors.ConstructedColors[xNode.ConstructedHit]);
                }

                else if (xNode.DisposeHit > 0)
                {
                    draw(XColors.DisposedColors[xNode.DisposeHit]);
                }
            }

            if (FocusedNodes.Count > 0 && node.ObjType == XObjType.Class)
            {
                bool dependent = DependentClasses.Contains(node.ID);
                bool independent = IndependentClasses.Contains(node.ID);

                if (dependent && independent)
                    draw(XColors.InterdependentColor);

                else if (dependent)
                    draw(XColors.DependentColor);

                else if (independent)
                    draw(XColors.IndependentColor);
            }

            if (node.SearchMatch && !SearchStrobe)
                draw(XColors.SearchMatchColor);

            // if just a point, drawing a border messes up pixels
            if (pointBorder)
            {
                if (FilteredNodes.ContainsKey(node.ID))
                    draw(XColors.FilteredColor);
                else if (IgnoredNodes.ContainsKey(node.ID))
                    draw(XColors.IgnoredColor);

                else if (needBorder) // dont draw the point if already lit up
                    draw(XColors.ObjColors[(int)node.ObjType]);
            }
            else
            {
                Color pen = XColors.ObjColors[(int)node.ObjType];

                if (FilteredNodes.ContainsKey(node.ID))
                    pen = XColors.FilteredColor;
                else if (IgnoredNodes.ContainsKey(node.ID))
                    pen = XColors.IgnoredColor;

                int penWidth = 1;
                if (FocusedNodes.Contains(node))
                    penWidth = 2;

                try
                {
                    Renderer.DrawNodeOutline(pen, penWidth, area, outside, node, depth);
                }
                catch (Exception ex)
                {
                    //File.WriteAllText("debug.txt", string.Format("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n", ex, area.X, area.Y, area.Width, area.Height));
                }
            }

            // draw label
            //buffer.FillRectangle(SearchMatchBrush, node.DebugRect);
            if (ShowLabels && node.RoomForLabel)
            {
                Renderer.DrawTextBackground(XColors.LabelBgColor, labelArea.X, labelArea.Y, labelArea.Width, labelArea.Height);
                Renderer.DrawString(node.Name, TextFont, XColors.ObjColors[(int)node.ObjType], labelArea);
            }


            if (MapMode == TreeMapMode.Dependencies && node.ObjType == XObjType.Class)
                drawChildren = false;

            if (drawChildren && area.Width > 1 && area.Height > 1)
                foreach (var sub in node.Nodes)
                    DrawNode(sub, depth + 1, drawChildren);


            // after drawing children, draw instance tracking on top of it all
            /*if (XRay.InstanceTracking && node.ObjType == XObjType.Class)
            {
               if (XRay.InstanceCount[node.ID] > 0)
                {
                    string count = XRay.InstanceCount[node.ID].ToString();
                    Rectangle x = new Rectangle(node.Area.Location, buffer.MeasureString(count, InstanceFont).ToSize());

                    if (node.Area.Contains(x))
                    {
                        buffer.FillRectangle(NothingBrush, x);
                        buffer.DrawString(count, InstanceFont, InstanceBrush, node.Area.Location.X + 2, node.Area.Location.Y + 2);
                    }
                }
            }*/
        }

        public void DoSearch()
        {
            // figure out if we need to do a search
            if (SearchString == LastSearch)
                return;

            LastSearch = SearchString;
            bool empty = string.IsNullOrEmpty(SearchString);

            if (SearchHighlightSubs)
            {
                // reset all
                Utilities.RecurseTree<NodeModel>(
                    tree: TopRoot.Nodes,
                    evaluate: n => n.SearchMatch = false,
                    recurse: n => n.Nodes
                );

                // look for match
                Utilities.RecurseTree<NodeModel>(
                    tree: TopRoot.Nodes,
                    evaluate: n =>
                    {
                        // set all nodes under match to also matched
                        n.SearchMatch = !empty && n.Name.ToLowerInvariant().IndexOf(SearchString) != -1;
                        if (n.SearchMatch)
                            Utilities.RecurseTree<NodeModel>(
                                tree: n.Nodes,
                                evaluate: n2 => n2.SearchMatch = true,
                                recurse: n2 => n2.Nodes
                            );
                    },
                    recurse: n => n.Nodes.Where(n2 => !n2.SearchMatch)
                );
            }
            // only highlight specific node matches
            else
            {
                Utilities.RecurseTree<NodeModel>(
                    tree: TopRoot.Nodes,
                    evaluate: n => n.SearchMatch = !empty && n.Name.ToLowerInvariant().IndexOf(SearchString) != -1,
                    recurse: n => n.Nodes
                );
            }

        }

        public void DrawFooterLabel()
        {
            if (NodesHovered.Length == 0)
                return;

            PointF pos = Renderer.GetCursorPosition();
            //if (!ClientRectangle.Contains((int)pos.X, (int)pos.Y))
            //    return;

            float x = 0;
            var lastNode = NodesHovered.Last();

            foreach (var node in NodesHovered)
            {
                DrawFooterPart(node.Name, XColors.ObjColors[(int)node.ObjType], ref x);

                if (node != lastNode)
                    DrawFooterPart( ".", XColors.TextColor, ref x);
            }

            DrawFooterPart(" (" + GetMetricForNode(lastNode) + ")", XColors.TextColor, ref x);
        }

        public void DrawFooterPart(string label, Color color, ref float x)
        {
            SizeF size = Renderer.MeasureString(label, TextFont);
            Renderer.DrawTextBackground(XColors.FooterBgColor, x, Renderer.ViewHeight - size.Height, size.Width, size.Height);
            Renderer.DrawString(label, TextFont, color, x, Renderer.ViewHeight - size.Height);
            x += size.Width;
        }


        public string GetMetricForNode(NodeModel node)
        {
            switch (SizeLayout)
            {
                case SizeLayouts.Constant:
                    return node.Value.ToString() + " elements";

                case SizeLayouts.MethodSize:
                    return node.XNode.Lines.ToString() + " lines";

                case SizeLayouts.TimeInMethod:
                    return Utilities.TicksToString(node.Value);

                case SizeLayouts.Hits:
                    return node.Value.ToString() + " calls";

                case SizeLayouts.TimePerHit:
                    return Utilities.TicksToString(node.Value);
            }

            return "";
        }

        public void DrawToolTip()
        {
            if (NodesHovered.Length == 0)
                return;

            PointF pos = Renderer.GetCursorPosition();
            //if (!ClientRectangle.Contains((int)pos.X, (int)pos.Y))
            //    return;

            if (!LastHoverPoint.Equals(pos))
            {
                LastHoverPoint = pos;
                LastHoverTime = DateTime.Now;
                return;
            }
            else if (DateTime.Now.Subtract(LastHoverTime).TotalSeconds < .5)
            {
                return;
            }

            // for each node selected, get size, figure out bg size and indents, then pass again and draw

            float bgWidth = 0;
            float bgHeight = 0;
            float lineHeight = 0;
            float indentAmount = 0;

            var labels = new List<Tuple<string, Color>>();
            string indentStr = "";

            // show all labels if showing just graph nodes, or show labels isnt on, or the label isnt displayed cause there's not enough room
            //foreach(var node in NodesHovered)//.Where(n => !ShowLabels || !n.RoomForLabel || n.LabelClipped))
            //{    
            var n = NodesHovered.Last();
            Color color = GuiHovered.Contains(n) ? XColors.ObjColors[(int)n.ObjType] : XColors.ObjDitheredColors[(int)n.ObjType];
            labels.Add(new Tuple<string, Color>(indentStr + n.Name, color));
            indentStr += " ";
            //}

            if (labels.Count == 0)
                return;

            var lastNode = NodesHovered.Last();

            if (lastNode.ObjType == XObjType.Class)
            {
                int staticCount = 0;
                int instCount = 0;
                if (lastNode != null && lastNode.XNode.Record != null && lastNode.XNode.Record.Active.Count > 0)
                {
                    var record = lastNode.XNode.Record;

                    lock (record.Active)
                    {
                        for (int i = 0; i < record.Active.Count; i++)
                        {
                            var instance = record.Active[i];

                            if (instance.IsStatic)
                                staticCount++;
                            else
                                instCount++;
                        }
                    }
                }

                string classInfo = "0 instances";

                if (instCount > 0)
                    classInfo = instCount.ToString() + " instances";
                else if (staticCount > 0)
                    classInfo = "static instance";

                labels.Add(new Tuple<string, Color>(classInfo, XColors.TextColor));
            }

            else if (lastNode.ObjType == XObjType.Method)
            {
                labels.Add(new Tuple<string, Color>(lastNode.XNode.GetMethodName(false), XColors.TextColor));
            }

            else if (lastNode.ObjType == XObjType.Namespace)
            {
                labels.Clear();
                labels.Add(new Tuple<string, Color>(lastNode.XNode.FullName(true), XColors.ObjColors[(int)lastNode.ObjType]));
            }

            else if (lastNode.ObjType == XObjType.Field)
            {
                var classNode = lastNode.GetParentClass(false);

                if (classNode != null && classNode.XNode.Record != null && classNode.XNode.Record.Active.Count > 0)
                {
                    var record = classNode.XNode.Record;

                    lock (record.Active)
                    {
                        FieldInfo field = null;

                        for (int i = 0; i < record.Active.Count; i++)
                        {
                            var instance = record.Active[i];

                            field = instance.GetField(lastNode.XNode.UnformattedName);

                            object target = null;
                            if (instance != null && instance.Ref != null)
                                target = instance.Ref.Target;

                            // dont query the static class instance of the class for non-static fields
                            if (field == null || !field.IsStatic && target == null)
                                continue;

                            string text = "";
                            try
                            {
                                if (target == null)
                                    text += "(static) ";
                                else
                                    text += "#" + instance.Number + ": ";

                                object val = field.GetValue(target);

                                text += (val != null) ? val.ToString() : "<null>";
                            }
                            catch (Exception ex)
                            {
                                text = ex.Message;
                                //continue; 
                            }

                            labels.Add(new Tuple<string, Color>(text, XColors.TextColor));

                            if (field.IsStatic)
                                break;
                        }
                    }
                }
            }

            // find the size of the background box
            foreach (var node in labels)
            {
                string label = node.Item1;
                //if (node == lastNode)
                //     label += " (" + GetMetricForNode(node) + ")";

                SizeF size = Renderer.MeasureString(label, TextFont);

                if (size.Width + indentAmount > bgWidth)
                    bgWidth = size.Width + indentAmount;

                bgHeight += size.Height;
                lineHeight = size.Height;
                //indentAmount += indent;
            }

            // put box lower right corner at cursor
            if (pos.X + bgWidth > Renderer.ViewWidth)
                pos.X = Renderer.ViewWidth - bgWidth;

            pos.Y -= bgHeight;

            // ensure it doesnt go off screen
            if (pos.X < 0) pos.X = 0;
            if (pos.Y < 0) pos.Y = 0;


            // draw background
            Renderer.DrawTextBackground(XColors.TextBgColor, pos.X, pos.Y, bgWidth, bgHeight);

            //GraphDebuggingLabel(buffer, pos);

            foreach (var node in labels)
            {
                Renderer.DrawString(node.Item1, TextFont, node.Item2, pos.X, pos.Y);

                pos.Y += lineHeight;
                // pos.X += indent;
            }
        }
    }
}
