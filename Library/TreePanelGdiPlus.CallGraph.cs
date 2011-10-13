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
    public enum CallGraphMode { Method, Class, Dependencies }

    partial class TreePanelGdiPlus
    {
        List<Graph> Graphs = new List<Graph>();

        const int MinCallNodeSize = 5;

        public bool SequenceOrder = false;
        public CallGraphMode GraphMode = CallGraphMode.Method;

        private void DrawCallGraph(Graphics buffer)
        {
            if (DoRevalue || 
                XRay.CallChange || 
                (ShowLayout != ShowNodes.All && XRay.CoverChange) ||  
                (ShowLayout == ShowNodes.Instances && XRay.InstanceChange) )
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
                XRay.CoverChange = false;
                XRay.InstanceChange = false;

                DoRevalue = false;
                DoResize = true;
            }

            // graph created in relative coords so it doesnt need to be re-computed each resize, only on recalc

            if (DoResize)
            {
                float fullSize = (float)Math.Min(Width, Height) / 2;

                foreach (var graph in Graphs)
                {
                    for (int i = 0; i < graph.Ranks.Length; i++)
                    {
                        var rank = graph.Ranks[i];

                        float right = Width;
                        if (ShowLabels)
                        {
                            if (i < graph.Ranks.Length - 1)
                                right = Width * graph.Ranks[i + 1].Column[0].ScaledLocation.X -
                                        (graph.Ranks[i + 1].Column.Max(c => fullSize * c.ScaledSize) / 2);
                        }

                        for (int x = 0; x < rank.Column.Count; x++)
                        {
                            var node = rank.Column[x];

                            float size = fullSize * node.ScaledSize;

                            float halfSize = size / 2;

                            if (size < MinCallNodeSize)
                                size = MinCallNodeSize;

                            node.SetArea(new RectangleD(
                                Width * node.ScaledLocation.X - halfSize,
                                Height * node.ScaledLocation.Y - halfSize,
                                size, size));

                            if (node.ID == 0) // dont draw intermediate nodes
                                continue;

                            // check if enough room in box for label
                            node.RoomForLabel = false;

                            if (ShowLabels)
                            {
                                SizeF textSize = buffer.MeasureString(node.Name, TextFont);

                                //area from middle of node to edges of midpoint between adjacent nodes, and length to next rank - max node's width /2
                                float left = node.AreaF.Right;
                                float top = graph.ScaledOffset;
                                if (x != 0)
                                {
                                    float distance = rank.Column[x].ScaledLocation.Y - rank.Column[x - 1].ScaledLocation.Y;
                                    top = rank.Column[x - 1].ScaledLocation.Y + (distance / 2);
                                }

                                float bottom = graph.ScaledOffset + graph.ScaledHeight;
                                if (x < rank.Column.Count - 1)
                                {
                                    float distance = rank.Column[x + 1].ScaledLocation.Y - rank.Column[x].ScaledLocation.Y;
                                    bottom = rank.Column[x].ScaledLocation.Y + (distance / 2);
                                }
 
                                float distanceFromCenter = Math.Min(node.ScaledLocation.Y - top, bottom - node.ScaledLocation.Y);
                                top = (node.ScaledLocation.Y - distanceFromCenter) * Height;
                                bottom = (node.ScaledLocation.Y + distanceFromCenter) * Height;

                                
                                node.LabelRect = new RectangleF(left, top, right - left, bottom - top);

                                if (textSize.Height < node.LabelRect.Height && textSize.Width < node.LabelRect.Width)
                                {
                                    node.RoomForLabel = true;

                                    node.LabelRect.Y = (node.LabelRect.Y + node.LabelRect.Height / 2) - (textSize.Height / 2);
                                    node.LabelRect.Height = textSize.Height;
                                }

                                /*if (label.Height < node.AreaF.Height + LabelPadding * 2 &&
                                    label.Width < node.AreaF.Width + LabelPadding * 2)
                                {
                                    label.X += LabelPadding;
                                    label.Y += LabelPadding;

                                    node.RoomForLabel = true;
                                    node.LabelRect = label;
                                }*/
                            }
                        }
                    }
                }

                DoResize = false;
            }

            foreach (var node in Graphs.SelectMany(g => g.Nodes()))
            {
                if (node.ID == 0) // dont draw intermediate nodes
                    continue;

                DrawNode(buffer, node, 0, false);
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

                    // a good first guess at how nodes should be ordered to min crossing
                    rank.Column = rank.Column.OrderBy(n => n.HitSequence).ToList();

                    PositionRank(graph, rank, xOffset + maxSize / 2);

                    xOffset += maxSize + spaceX;
                }

                groupOffset += graph.ScaledHeight;

                if (SequenceOrder)
                {
                    for (int x = 0; x < 6; x++)
                        MinDistance(graph);
                }
                else
                {
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
                    int? minRank = unrankedNode.EdgesOut.Min(dest =>
                    {
                        if (PositionMap.ContainsKey(dest))
                        {
                            XNodeIn destNode = PositionMap[dest];
                            if (destNode.Rank.HasValue)
                                return destNode.Rank.Value;
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

                    if (source.EdgesOut == null)
                        continue;

                    foreach (var destId in source.EdgesOut)
                    {
                        if (!graph.ContainsKey(destId))
                            continue;

                        if (source.ID == destId)
                            continue;

                        XNodeIn destination = graph[destId];

                        if (source.Intermediates != null)
                            source.Intermediates.Remove(destId);
   
                        // if destination is not 1 forward/1 back then create intermediate nodes
                        if (source.Rank != destination.Rank + 1 &&
                            source.Rank != destination.Rank - 1)
                        {
                            if (source.Intermediates == null)
                                source.Intermediates = new Dictionary<int, List<XNodeIn>>();

                            source.Intermediates[destId] = new List<XNodeIn>();

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
                                source.Intermediates[destId].Add(intermediate);
                                ranks[nextRank].Column.Add(intermediate);
                                //PositionMap not needed because we dont need any mouse over events? just follow along and draw from list, not id

                                lastNode = intermediate;
                                nextRank = increase ? nextRank + 1 : nextRank - 1;
                            }

                            try
                            {
                                lastNode.Adjacents.Add(destination);
                                source.Intermediates[destId].Add(destination);
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
            // moves nodes closer to attached nodes in adjacent ranks

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

            if (node.EdgesOut != null)
                foreach (var destId in node.EdgesOut)
                    if (PositionMap.ContainsKey(destId))
                    {
                        if (node.Intermediates != null && node.Intermediates.ContainsKey(destId))
                            sum += node.Intermediates[destId][0].ScaledLocation.Y;
                        else
                            sum += PositionMap[destId].ScaledLocation.Y;

                        count++;
                    }

            if (node.EdgesIn != null)
                foreach (var source in node.EdgesIn)
                    if (PositionMap.ContainsKey(source))
                    {
                        var sourceNode = PositionMap[source];
                        if(sourceNode.Intermediates != null && sourceNode.Intermediates.ContainsKey(node.ID))
                             sum += sourceNode.Intermediates[node.ID].Last().ScaledLocation.Y;
                        else
                            sum += PositionMap[source].ScaledLocation.Y;
                       
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
            // moves nodes with-in rank closer to their adjacent nodes without changing order in rank
            try
            {
                foreach (Rank rank in graph.Ranks)
                {
                    var nodes = rank.Column;

                    // foreach node average y pos form all connected edges
                    for (int x = 0; x < nodes.Count; x++)
                    {
                        var node = nodes[x];

                        float halfSize = node.ScaledSize / 2;

                        float lowerbound = (x > 0) ? nodes[x - 1].ScaledLocation.Y + (nodes[x - 1].ScaledSize / 2) : 0;
                        lowerbound += rank.MinHeightSpace;

                        float upperbound = (x < nodes.Count - 1) ? nodes[x + 1].ScaledLocation.Y - (nodes[x + 1].ScaledSize / 2) : float.MaxValue;
                        upperbound -= rank.MinHeightSpace;

                        //Debug.Assert(lowerbound <= upperbound);
                        if (lowerbound >= upperbound)
                        {
                            // usually if this happens they're very close
                            XRay.LogError("lower bound greater than upper in layout. pos: {0}, nodeID: {1}, lower: {4}, upper: {5}, minheight: {6}", x, node.ID, lowerbound, upperbound, rank.MinHeightSpace);
                            //continue;
                        }


                        float optimalY = AvgPos(node);

                        if (optimalY - halfSize < lowerbound)
                            optimalY = lowerbound + halfSize;

                        else if (optimalY + halfSize > upperbound)
                            optimalY = upperbound - halfSize;

                        node.ScaledLocation.Y = optimalY;
                    }
                }
            }
            catch (Exception ex)
            {
                XRay.LogError(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void PositionRank(Graph graph, Rank rank, float xOffset)
        {
            // spreads nodes in rank across y-axis at even intervals

            var nodes = rank.Column;

            float totalHeight = nodes.Sum(n => n.ScaledSize);

            float freespace = 1 * graph.ScaledHeight - totalHeight;

            float ySpace = freespace / (nodes.Count + 1); // space between each block
            rank.MinHeightSpace = (float) Math.Min(ySpace, 4.0 / (double) Height);
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

            if (node.EdgesOut != null)
                foreach (var destId in node.EdgesOut)
                {
                    if (parents.Contains(destId))
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

                    if (PositionMap.ContainsKey(destId))
                        LayoutGraph(graph, PositionMap[destId], node.Rank.Value + 1, parents.ToList());//, debugLog);

                    //debugLog.Add(string.Format("Return to node {0} rank {1}", ID, Rank));
                }

            // record so later group can be traversed for null ranked members (parents) so layout can be run on them
            if (node.EdgesIn != null)
                foreach (var source in node.EdgesIn)
                    if (PositionMap.ContainsKey(source))
                        graph[source] = PositionMap[source];

            // check if same edges down go back up and create intermediates in that case?

            //debugLog.Add(string.Format("Exited Node ID {0} rank {1}", ID, Rank));
        }

        private void AddCalledNodes(XNodeIn node, bool center)
        {
            if (!node.Show)
                return;

            if (GraphMode == CallGraphMode.Dependencies)
            {
                if ((node.DependenciesFrom != null && node.DependenciesFrom.Length > 0) ||
                    (node.DependenciesTo != null && node.DependenciesTo.Length > 0))
                {
                    if (center)
                        CenterMap[node.ID] = node;

                    // if not center then only add if connected to center, center=false called on second pass so centerMap is totally initd
                    if (center ||
                         (node.DependenciesFrom != null && node.DependenciesFrom.Any(e => CenterMap.ContainsKey(e))) ||
                         (node.DependenciesTo != null && node.DependenciesTo.Any(e => CenterMap.ContainsKey(e))))
                    {
                        PositionMap[node.ID] = node;

                        node.EdgesIn = null;
                        node.EdgesOut = null;
                        node.Intermediates = null;

                        if (node.DependenciesFrom != null)
                            node.EdgesIn = node.DependenciesFrom;

                        if (node.DependenciesTo != null)
                            node.EdgesOut = node.DependenciesTo;
                    }
                }
            }
            else if (GraphMode == CallGraphMode.Method || GraphMode == CallGraphMode.Class)
            {
                if ( ((node.CalledIn != null && node.CalledIn.Length > 0) ||
                      (node.CallsOut != null && node.CallsOut.Length > 0)) 
                      &&
                     ((GraphMode == CallGraphMode.Method && node.ObjType != XObjType.Class) ||
                      (GraphMode == CallGraphMode.Class && node.ObjType == XObjType.Class)))
                {
                    if (center)
                        CenterMap[node.ID] = node;

                    // if not center then only add if connected to center, center=false called on second pass so centerMap is totally initd
                    if ( center ||
                         (node.CalledIn != null && node.CalledIn.Any(c => CenterMap.ContainsKey(c.Source))) ||
                         (node.CallsOut != null && node.CallsOut.Any(c => CenterMap.ContainsKey(c.Destination))))
                    {
                        PositionMap[node.ID] = node;

                        node.EdgesIn = null;
                        node.EdgesOut = null;
                        node.Intermediates = null;

                        if (node.CalledIn != null)
                            node.EdgesIn = node.CalledIn.Select(c => c.Source).ToArray();

                        if (node.CallsOut != null)
                            node.EdgesOut = node.CallsOut.Select(c => c.Destination).ToArray();
                    }
                }
            }

            foreach (XNodeIn sub in node.Nodes)
                if (sub != InternalRoot) // when traversing outside root, dont interate back into center root
                    AddCalledNodes(sub, center);
        }

        private void DrawGraphEdge(Graphics buffer, Pen pen, XNodeIn source, XNodeIn destination)
        {
            if (source.Intermediates == null || !source.Intermediates.ContainsKey(destination.ID))
                buffer.DrawLine(pen, source.CenterF, destination.CenterF);
            else
            {
                var intermediates = source.Intermediates[destination.ID];

                var originalCap = pen.EndCap;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;

                var prev = source;
                var last = intermediates.Last();

                foreach (var next in intermediates)
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
            internal List<XNodeIn> Column = new List<XNodeIn>();

            internal float MinHeightSpace;
        }
    }
}
