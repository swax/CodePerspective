using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace XLibrary
{
	partial class ViewModel
    {
        const int MinCallNodeSize = 5;

        public GraphSet TopGraph;


        public void DrawCallGraph()
        {
            if (DoRevalue ||
                XRay.CallChange ||
                (ShowLayout != ShowNodes.All && XRay.CoverChange) ||
                (ShowLayout == ShowNodes.Instances && XRay.InstanceChange))
            {
                RecalcCover(InternalRoot);
                RecalcCover(ExternalRoot);

                PositionMap.Clear();
                CenterMap.Clear();

                TopGraph = new GraphSet(this, CurrentRoot);

                // combine position and center maps for graph tree
                Utilities.RecurseTree(
                    TopGraph,
                    s =>
                    {
                        foreach (var kvp in s.PositionMap)
                            PositionMap[kvp.Key] = kvp.Value;

                        foreach (var id in s.CenterMap)
                            CenterMap.Add(id);
                    },
                    s => s.Subsets
                );

                XRay.CallChange = false;
                XRay.CoverChange = false;
                XRay.InstanceChange = false;

                DoRevalue = false;
                RevalueCount++;

                DoResize = true;
            }

            // graph created in relative coords so it doesnt need to be re-computed each resize, only on recalc

            if (DoResize)
            {
                Utilities.RecurseTree(
                    TopGraph,
                    s =>
                    {
                        foreach(var graph in s.Graphs)
                        {
                            if (s.GraphContainer == null)
                                ScaleGraph(graph, new RectangleF(ScreenOffset, ScreenSize));

                            else if (s.GraphContainer.XNode.External)
                            {
                                // this is assuming the external node is a triangle
                                var area = s.GraphContainer.AreaF;
                                var inside = new RectangleF(area.X + area.Width / 4f, area.Y + area.Height / 2f, area.Width / 2f, area.Height / 2f);
                                ScaleGraph(graph, inside);
                            }
                            else
                                ScaleGraph(graph, s.GraphContainer.AreaF);
                        }
                    },
                    s => s.Subsets
                );

                DoResize = false;
                ResizeCount++;
            }
        }

        private void ScaleGraph(Graph graph, RectangleF area)
        {
            float fullSize = (float)Math.Min(area.Width, area.Height) / 2;

            for (int i = 0; i < graph.Ranks.Length; i++)
            {
                var rank = graph.Ranks[i];

                if (DrawCallGraphVertically)
                {
                    for (int x = 0; x < rank.Column.Count; x++)
                    {
                        var node = rank.Column[x];

                        var temp = node.ScaledLocation.X;
                        node.ScaledLocation.X = node.ScaledLocation.Y;
                        node.ScaledLocation.Y = temp;
                    }
                }

                float right = area.X + area.Width;
                if (ShowLabels)
                {
                    if (i < graph.Ranks.Length - 1)
                        right = area.X + area.Width * graph.Ranks[i + 1].Column[0].ScaledLocation.X -
                                (graph.Ranks[i + 1].Column.Max(c => fullSize * c.ScaledSize) / 2);
                }

                // set node area
                for (int x = 0; x < rank.Column.Count; x++)
                {
                    var node = rank.Column[x];

                    float size = fullSize * node.ScaledSize;

                    float halfSize = size / 2;

                    if (size < MinCallNodeSize)
                        size = MinCallNodeSize;

                    node.SetArea(new RectangleF(
                        area.X + area.Width * node.ScaledLocation.X - halfSize,
                        area.Y + area.Height * node.ScaledLocation.Y - halfSize,
                        size, size));
                }

                if (ShowLabels)
                    SetLabelArea(graph, area, rank, right);
            }
        }

        private void SetLabelArea(Graph graph, RectangleF area, Rank rank, float right)
        {
            var rankNodes = rank.Column.Where(n => n.ID != 0).ToArray();

            for (int x = 0; x < rankNodes.Length; x++)
            {
                var node = rankNodes[x];

                // check if enough room in box for label
                node.RoomForLabel = false;
                node.LabelClipped = false;

                SizeF textSize = Renderer.MeasureString(node.Name, TextFont);

                // first see if label fits above node, we prefer this because it looks better when zoomed in
                float left = node.AreaF.Left;
                float top = area.Y + area.Height * graph.ScaledOffset;
                bool topFit = true;

                if (x > 0)
                    top = rankNodes[x - 1].AreaF.Bottom;

                float bottom = node.AreaF.Top;

                node.LabelRect = new RectangleF(left, top, right - left, bottom - top);

                // if label doesnt fit above, put it to the right of the node
                if (textSize.Height > node.LabelRect.Height)
                {
                    //area from middle of node to edges of midpoint between adjacent nodes, and length to next rank - max node's width /2
                    topFit = false;

                    left = node.AreaF.Right;
                    top = graph.ScaledOffset;

                    var thisY = rankNodes[x].ScaledLocation.Y;

                    if (x > 0)
                    {
                        float aboveY = rankNodes[x - 1].ScaledLocation.Y;
                        float distance = thisY - aboveY;
                        top = aboveY + (distance / 2f);
                    }

                    bottom = graph.ScaledOffset + graph.ScaledHeight;
                    if (x < rankNodes.Length - 1)
                    {
                        float belowY = rankNodes[x + 1].ScaledLocation.Y;
                        float distance = belowY - thisY;
                        bottom = thisY + (distance / 2f);
                    }

                    float distanceFromCenter = Math.Min(node.ScaledLocation.Y - top, bottom - node.ScaledLocation.Y);
                    top = area.Y + (node.ScaledLocation.Y - distanceFromCenter) * area.Height;
                    bottom = area.Y + (node.ScaledLocation.Y + distanceFromCenter) * area.Height;

                    node.LabelRect = new RectangleF(left, top, right - left, bottom - top);
                }

                if (textSize.Height < node.LabelRect.Height)// && textSize.Width < node.LabelRect.Width)
                {
                    node.RoomForLabel = true;

                    if (node.LabelRect.Width < textSize.Width)
                        node.LabelClipped = true;

                    if (topFit)
                        node.LabelRect.Y = node.LabelRect.Bottom - textSize.Height;
                    else
                        node.LabelRect.Y = (node.LabelRect.Y + node.LabelRect.Height / 2f) - (textSize.Height / 2f);

                    node.LabelRect.Height = textSize.Height;
                }
            }
        } 
    }
}
