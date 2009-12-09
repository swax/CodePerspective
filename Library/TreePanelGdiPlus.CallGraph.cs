using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

/*Based on call graph in test panel
  
The dot algorithm produces a ranked layout of a graph honoring edge directions. 
It is particularly appropriate for displaying hierarchies or directed acyclic 
graphs. The basic layout scheme is attributed to Sugiyama et al. The specific 
algorithm used by dot follows the steps described by Gansner et al.

dot draws a graph in four main phases. Knowing this helps you to understand what 
kind of layouts dot makes and how you can control them. The layout procedure used 
by dot relies on the graph being acyclic. Thus, the first step is to break any 
cycles which occur in the input graph by reversing the internal direction of 
certain cyclic edges. The next step assigns nodes to discrete ranks or levels. 
In a top-to-bottom drawing, ranks determine Y coordinates. Edges that span more 
than one rank are broken into chains of virtual nodes and unit-length edges. The 
third step orders nodes within ranks to avoid crossings. The fourth step sets X 
coordnates of nodes to keep edges short, and the final step routes edge splines.

In dot, higher edge weights have the effect of causing edges to be shorter and straighter. 
 */

namespace XLibrary
{
    partial class TreePanelGdiPlus
    {
        List<Graph> Graphs = new List<Graph>();

        private void DrawCallGraph(Graphics buffer)
        {
            if (DoRevalue || XRay.CallChange || XRay.CoverChange)
            {
                RecalcCover(InternalRoot);
                RecalcCover(ExternalRoot);

                Graphs.Clear();
                PositionMap.Clear();
                CenterMap.Clear();

                // iternate nodes at this zoom level
                AddCalledNodes(CurrentRoot, true);

                if (ShowOutside)
                    AddCalledNodes(InternalRoot, false);

                if (ShowExternal)
                    AddCalledNodes(ExternalRoot, false);

                // todo need way to identify ext/outside nodes graphically

                if (PositionMap.Count > 0)
                {
                    BuildGraphs();

                    if (Graphs.Count > 0)
                        SizeGraphs();
                }

                XRay.CallChange = false;
                DoRevalue = false;
                DoResize = true;
            }

            // graph created in relative coords so it doesnt need to be re-computed each resize, only on recalc

            if (DoResize)
            {
                float fullSize = (float)Math.Min(Width, Height) / 2;

                foreach (var node in Graphs.SelectMany(g => g.Nodes()))
                {
                    float size = fullSize * node.ScaledSize;

                    float halfSize = size / 2;

                    if (size < 3)
                        size = 3;

                    node.SetArea(new RectangleD(
                        Width * node.ScaledLocation.X - halfSize,
                        Height * node.ScaledLocation.Y - halfSize,
                        size, size));

                    if (node.ID == 0) // dont draw intermediate nodes
                        continue;

                    // check if enough room in box for label
                    node.RoomForLabel = false;
                    RectangleF label = new RectangleF(node.AreaF.Location, buffer.MeasureString(node.Name, TextFont));

                    if (label.Height < node.AreaF.Height + LabelPadding * 2 &&
                        label.Width < node.AreaF.Width + LabelPadding * 2)
                    {
                        label.X += LabelPadding;
                        label.Y += LabelPadding;

                        node.RoomForLabel = true;
                        node.LabelRect = label;
                    }
                }

                DoResize = false;
            }

            foreach (var node in Graphs.SelectMany(g => g.Nodes()))
            {
                if (node.ID == 0) // dont draw intermediate nodes
                    continue;

                DrawNode(buffer, node, 0);
            }
        }

        private void SizeGraphs()
        {
            Graphs = Graphs.OrderByDescending(g => g.Weight).ToList();


            double totalWeight = Graphs.Sum(g => g.Weight);
            double totalArea = 1; // unit scalable area

            double weightToPix = totalArea / totalWeight * 0.5; // reduction factor

            Graphs.ForEach(g =>
            {
                foreach (var n in g.Nodes())
                    n.ScaledSize = (float)Math.Sqrt(n.Value * weightToPix);
            });


            // sum the heights of the biggest ranks of each graph
            float[] maxRankHeights = Graphs.Select(g => g.Ranks.Max(rank => rank.Column.Sum(n => n.ScaledSize))).ToArray();
            float stackHeight = maxRankHeights.Sum();

            float heightReduce = 1 / stackHeight;

            // check x axis reduce, and if less use that one
            // do for all graphs at once so proportions stay the same

            // find graph with max width, buy summing max node in each rank
            float[] maxRankWidths = Graphs.Select(g => g.Ranks.Sum(rank => rank.Column.Max(n => n.ScaledSize))).ToArray();

            float widthReduce = 1 / maxRankWidths.Max(); ;

            float reduce = (float)(Math.Min(heightReduce, widthReduce) * 0.75);

            Graphs.ForEach(g =>
            {
                foreach (var n in g.Nodes())
                    n.ScaledSize *= reduce;
            });


            // give each group a height proportional to their max rank height
            float groupOffset = 0;

            for (int i = 0; i < Graphs.Count; i++)
            {
                var graph = Graphs[i];

                graph.ScaledHeight = maxRankHeights[i] / stackHeight;
                graph.ScaledOffset = groupOffset;

                float freespace = 1 - maxRankWidths[i] * reduce;

                float spaceX = freespace / (graph.Ranks.Length + 1);
                float xOffset = spaceX;

                foreach (var rank in graph.Ranks)
                {
                    float maxSize = rank.Column.Max(n => n.ScaledSize);

                    PositionRank(graph, rank, xOffset + maxSize / 2);

                    xOffset += maxSize + spaceX;
                }

                groupOffset += graph.ScaledHeight;


                for (int x = 0; x < 3; x++)
                    Uncross(graph);

                for (int x = 0; x < 3; x++)
                    MinDistance(graph);

                for (int x = 0; x < 3; x++)
                    Uncross(graph);

                for (int x = 0; x < 3; x++)
                    MinDistance(graph);
            }
        }

        private void BuildGraphs()
        {
            // group nodes in position map into graphs
            foreach (XNodeIn node in PositionMap.Values)
                node.Rank = null;

            do
            {
                // group nodes into connected graphs
                Dictionary<int, XNodeIn> graph = new Dictionary<int, XNodeIn>();

                // add first unranked node to a graph
                XNodeIn unrankedNode = PositionMap.Values.First(n => n.Rank == null);

                LayoutGraph(graph, unrankedNode, 0, new List<int>());

                // while group contains unranked nodes
                while (graph.Values.Any(n => n.Rank == null))
                {
                    // head node to start traversal
                    unrankedNode = graph.Values.First(n => n.Rank == null);

                    // only way node could be in group is if child added it, so there is a minrank
                    // min rank is 1 back from the lowest ranked child of the node
                    int? minRank = unrankedNode.CallsOut.Min(e =>
                    {
                        if (PositionMap.ContainsKey(e.Destination))
                        {
                            XNodeIn dest = PositionMap[e.Destination];
                            if (dest.Rank.HasValue)
                                return dest.Rank.Value;
                        }

                        return int.MaxValue;
                    });

                    LayoutGraph(graph, unrankedNode, minRank.Value - 1, new List<int>());//, new List<string>());
                }

                if (graph.Count == 1)
                {
                    var remove = graph.Values.First();
                    PositionMap.Remove(remove.ID);
                    CenterMap.Remove(remove.ID);

                    continue;
                }

                // normalize ranks so sequential without any missing between
                int i = -1;
                int currentRank = int.MinValue;
                foreach (var n in graph.Values.OrderBy(v => v.Rank))
                {
                    if (n.Rank != currentRank)
                    {
                        currentRank = n.Rank.Value;
                        i++;
                    }

                    n.Rank = i;
                }


                // put all nodes into a rank based multi-map
                Rank[] ranks = new Rank[i + 1];
                for (i = 0; i < ranks.Length; i++)
                    ranks[i] = new Rank();

                long graphWeight = 0;

                foreach (var source in graph.Values)
                {
                    graphWeight += source.Value;


                    ranks[source.Rank.Value].Column.Add(source);

                    if (source.CallsOut == null)
                        continue;

                    foreach (FunctionCall edge in source.CallsOut)
                    {
                        if (!graph.ContainsKey(edge.Destination))
                            continue;

                        if (edge.Source == edge.Destination)
                            continue;

                        XNodeIn destination = graph[edge.Destination];

                        if (edge.Intermediates != null)
                            edge.Intermediates.Clear();

                        // if destination is not 1 forward/1 back then create intermediate nodes
                        if (source.Rank != destination.Rank + 1 &&
                            source.Rank != destination.Rank - 1)
                        {
                            if (edge.Intermediates == null)
                                edge.Intermediates = new List<XNodeIn>();


                            bool increase = destination.Rank > source.Rank;
                            int nextRank = increase ? source.Rank.Value + 1 : source.Rank.Value - 1;
                            XNodeIn lastNode = source;

                            while (nextRank != destination.Rank)
                            {

                                // create new node
                                XNodeIn intermediate = new XNodeIn();
                                intermediate.Rank = nextRank;
                                intermediate.Value = 10; // todo make smarter - 
                                intermediate.Adjacents = new List<XNodeIn>();

                                // add forward node to prev
                                if (lastNode != source)
                                    lastNode.Adjacents.Add(intermediate);

                                // add back node to curr
                                intermediate.Adjacents.Add(lastNode);

                                // add to temp path, rank map
                                edge.Intermediates.Add(intermediate);
                                ranks[nextRank].Column.Add(intermediate);
                                //PositionMap not needed because we dont need any mouse over events? just follow along and draw from list, not id

                                lastNode = intermediate;
                                nextRank = increase ? nextRank + 1 : nextRank - 1;

                            }


                            try
                            {

                                lastNode.Adjacents.Add(destination);
                                edge.Intermediates.Add(destination);
                            }
                            catch
                            {
                                System.IO.File.WriteAllText("debugX.txt", string.Format("{0}\r\n{1}\r\n", source.Rank, destination.Rank));

                                throw new Exception("wtf");
                            }
                        }
                    }
                }

                Graphs.Add(new Graph() { Ranks = ranks, Weight = graphWeight });

            } while (PositionMap.Values.Any(n => n.Rank == null));
        }

        private void Uncross(Graph graph)
        {
            foreach (Rank rank in graph.Ranks)
            {
                // foreach node average y pos form all connected edges
                foreach (var node in rank.Column)
                    node.ScaledLocation.Y = AvgPos(node);

                // set rank list to new node list
                rank.Column = rank.Column.OrderBy(n => n.ScaledLocation.Y).ToList();

                PositionRank(graph, rank, rank.Column[0].ScaledLocation.X);
            }
        }

        private float AvgPos(XNodeIn node)
        {
            float sum = 0;
            float count = 0;

            if (node.CallsOut != null)
                foreach (var call in node.CallsOut)
                    if (PositionMap.ContainsKey(call.Destination))
                    {
                        if(call.Intermediates != null && call.Intermediates.Count > 0)
                            sum += call.Intermediates[0].ScaledLocation.Y;
                        else
                            sum += PositionMap[call.Destination].ScaledLocation.Y;

                        count++;
                    }

            if (node.CalledIn != null)
                foreach (var call in node.CalledIn)
                    if (PositionMap.ContainsKey(call.Source))
                    {
                         if(call.Intermediates != null && call.Intermediates.Count > 0)
                            sum += call.Intermediates.Last().ScaledLocation.Y;
                        else
                            sum += PositionMap[call.Source].ScaledLocation.Y;
                       
                        count++;
                    }

            // should only be attached to intermediate nodes
            if (node.Adjacents != null)
            {
                Debug.Assert(node.ID == 0); // adjacents should only be on temp nodes

                foreach (var adj in node.Adjacents)
                {
                    sum += adj.ScaledLocation.Y;
                    count++;
                }
            }

            if (count == 0)
                return node.ScaledLocation.Y;

            return sum / count;
        }

        private void MinDistance(Graph graph)
        {

            // foreach rank list
            foreach (Rank rank in graph.Ranks)
            {
                var nodes = rank.Column;

                // foreach node average y pos form all connected edges
                for (int x = 0; x < nodes.Count; x++)
                {
                    var node = nodes[x];

                    float halfSize = node.ScaledSize / 2;

                    float lowerbound = (x > 0) ? nodes[x - 1].ScaledLocation.Y + (nodes[x - 1].ScaledSize / 2) : 0;
                    lowerbound += rank.MinNodeSpace;

                    float upperbound = (x < nodes.Count - 1) ? nodes[x + 1].ScaledLocation.Y - (nodes[x + 1].ScaledSize / 2) : float.MaxValue;
                    upperbound -= rank.MinNodeSpace;

                    Debug.Assert(lowerbound <= upperbound);
                    if (lowerbound >= upperbound)
                        continue;

                    float optimalY = AvgPos(node);

                    if (optimalY - halfSize < lowerbound)
                        optimalY = lowerbound + halfSize;

                    else if (optimalY + halfSize > upperbound)
                        optimalY = upperbound - halfSize;

                    node.ScaledLocation.Y = optimalY;
                }
            }
        }

        private void PositionRank(Graph graph, Rank rank, float xOffset)
        {
            var nodes = rank.Column;

            float totalHeight = nodes.Sum(n => n.ScaledSize);

            float freespace = 1 * graph.ScaledHeight - totalHeight;

            float ySpace = freespace / (nodes.Count + 1); // space between each block
            rank.MinNodeSpace = (float) Math.Min(ySpace, 4.0 / (double) Height);
            float yOffset = ySpace;


            foreach (var node in nodes)
            {
                node.ScaledLocation.X = xOffset;
                node.ScaledLocation.Y = 1 * graph.ScaledOffset + yOffset + node.ScaledSize / 2;

                yOffset += node.ScaledSize + ySpace;
            }
        }

        /*
        flow through entire list of children first
	        keep track of node parents
		        if any unranked parents at end of child run
			        look at all of parents children, give rank of 1 - lowest ranked child
				        re-run alg on that parent
        				
	        this way we start with first (entry) node, run through linearly from that, and tack on alternate parents later
		        works great for application call graph, and for general graphs as well
         */
        private void LayoutGraph(Dictionary<int, XNodeIn> graph, XNodeIn node, int minRank, List<int> parents)
        {
            //debugLog.Add(string.Format("Entered Node ID {0} rank {1}", ID, Rank));

            // node already ranked correctly, no need to re-rank subordinates
            if (node.Rank != null && node.Rank.Value >= minRank)
                return;

            int? prevRank = node.Rank;

            // only increase rank
            Debug.Assert(node.Rank == null || minRank > node.Rank.Value);
            node.Rank = minRank;

            //debugLog.Add(string.Format("Node ID {0} rank set from {1} to {2}", ID, prevRank, Rank));

            parents.Add(node.ID);
            graph[node.ID] = node;

            if (node.CallsOut != null)
                foreach (FunctionCall edge in node.CallsOut)
                {
                    if (parents.Contains(edge.Destination))
                    {
                        // destination rank should be less than source
                        //Debug.Assert(edge.Destination.Rank < edge.Source.Rank);

                        //debugLog.Add(string.Format("Switching edge {0} -> {1}, rank {2} -> {3}", ID, edge.Destination.ID, Rank, edge.Destination.Rank));

                        //edge.Source = edge.Destination;
                        //edge.Destination = this;
                        //edge.Reversed = !edge.Reversed;

                        continue;
                    }

                    // pass copy of parents list so that sub can add elemenets without affecting next iteration
                    //debugLog.Add(string.Format("Traversing to child {0} -> {1}, rank {2} -> {3}", ID, edge.Destination.ID, Rank, edge.Destination.Rank));

                    if (PositionMap.ContainsKey(edge.Destination))
                        LayoutGraph(graph, PositionMap[edge.Destination], node.Rank.Value + 1, parents.ToList());//, debugLog);

                    //debugLog.Add(string.Format("Return to node {0} rank {1}", ID, Rank));
                }

            // record so later group can be traversed for null ranked members (parents) so layout can be run on them
            if (node.CalledIn != null)
                foreach (FunctionCall edge in node.CalledIn)
                    if (PositionMap.ContainsKey(edge.Source))
                        graph[edge.Source] = PositionMap[edge.Source];

            // check if same edges down go back up and create intermediates in that case?

            //debugLog.Add(string.Format("Exited Node ID {0} rank {1}", ID, Rank));
        }

        private void AddCalledNodes(XNodeIn root, bool center)
        {
            if (!root.Show)
                return;

            if ((root.CalledIn != null && root.CalledIn.Length > 0) ||
                (root.CallsOut != null && root.CallsOut.Length > 0))
            {
                if (center)
                {
                    PositionMap[root.ID] = root;
                    CenterMap[root.ID] = root;
                }

                // if not center then only add if connected to center
                else if ((root.CalledIn != null && root.CalledIn.Any(c => CenterMap.ContainsKey(c.Source))) ||
                         (root.CallsOut != null && root.CallsOut.Any(c => CenterMap.ContainsKey(c.Destination))))
                {
                    PositionMap[root.ID] = root;
                }
            }

            foreach (XNodeIn sub in root.Nodes)
                if (sub != InternalRoot) // when traversing outside root, dont interate back into center root
                    AddCalledNodes(sub, center);
        }

        private void DrawGraphEdge(Graphics buffer, Pen pen, FunctionCall call, XNodeIn source, XNodeIn destination)
        {
            if (call.Intermediates == null || call.Intermediates.Count == 0)
                buffer.DrawLine(pen, source.CenterF, destination.CenterF);
            else
            {
                var originalCap = pen.EndCap;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;

                var prev = source;
                var last = call.Intermediates.Last();

                foreach (var next in call.Intermediates)
                {
                    if(next == last)
                        pen.EndCap = originalCap;

                    buffer.DrawLine(pen, prev.CenterF, next.CenterF);
                    prev = next;
                }
            }
        }


        class Graph
        {
            internal Rank[] Ranks;
            internal long Weight;

            internal float ScaledHeight;
            internal float ScaledOffset;

            internal IEnumerable<XNodeIn> Nodes()
            {
                foreach (var r in Ranks)
                    foreach (var n in r.Column)
                        yield return n;
            }
        }

        class Rank
        {
            internal int Order;

            internal List<XNodeIn> Column = new List<XNodeIn>();

            internal float MinNodeSpace;
        }
    }
}
