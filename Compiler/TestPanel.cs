using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XBuilder
{
    public partial class TestPanel : UserControl
    {
        bool DoRedraw = true;
        bool DoResize = true;
        Bitmap DisplayBuffer;

        List<Node> Nodes = new List<Node>();
        List<Edge> Edges = new List<Edge>();
        List<Graph> Graphs = new List<Graph>();

        SolidBrush NodeBrush = new SolidBrush(Color.Black);
        Pen NodePen = new Pen(Color.Black);

        SolidBrush TempBrush = new SolidBrush(Color.LimeGreen);
        Pen SourcePen = new Pen(Color.Red);
        Pen DestPen = new Pen(Color.Blue);
        Pen TempPen = new Pen(Color.Green);

        Random RndGen = new Random();
        int NextID = 1;

        bool CallGraphOn;


        const int MinInternodeDistance = 25;


        public TestPanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

        }

        internal void Reset(int nodeCount, int edgeCount, int weightMax)
        {
            CallGraphOn = false;

            Nodes.Clear();
            Edges.Clear();
            
            NextID = 1;

            for (int i = 0; i < nodeCount; i++)
                Nodes.Add(new Node()
                {
                    ID = NextID++,
                    Weight = RndGen.Next(weightMax),
                    Location = new PointF(RndGen.Next(Width), RndGen.Next(Height))
                });

            for (int i = 0; i < edgeCount; i++)
            {
                Edge edge = new Edge()
                {
                    Source = Nodes[RndGen.Next(nodeCount)],
                    Destination = Nodes[RndGen.Next(nodeCount)]
                };

                if (Edges.Any(e => e.Source == edge.Source && e.Destination == edge.Destination))
                    continue;
                
                Edges.Add(edge);

                edge.Source.Edges.Add(edge);
                edge.Destination.Edges.Add(edge);
            }

            DoResize = true;
            DoRedraw = true;
            Invalidate();
        }

        double Reduce = 0.5;


        private void TestPanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if (!DoRedraw && !DoResize)
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality; // todo option to turn this off

            buffer.Clear(Color.White);

            if (DoResize)
            {

                if (CallGraphOn)
                    Relayout();
                else
                {
                    double totalWeight = Nodes.Sum(n => n.Weight);
                    double totalArea = Width * Height;

                    double weightToPix = totalArea / totalWeight * Reduce;

                    Nodes.ForEach(n => n.Size = (int)Math.Sqrt(n.Weight * weightToPix));
                }

            }

            
            Nodes.ForEach(n =>
            {
                float half = n.Size / 2;
                buffer.DrawRectangle(NodePen, n.Location.X - half, n.Location.Y - half, n.Size, n.Size);

                buffer.FillEllipse(n.Filler ? TempBrush : NodeBrush, n.Location.X - 5, n.Location.Y - 5, 10, 10);
            });

            Edges.ForEach(e2 =>
            {
                PointF start = e2.Source.Location;
                PointF end = e2.Destination.Location;
                PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                buffer.DrawLine(e2.Reversed ? TempPen : SourcePen, start, mid);
                buffer.DrawLine(e2.Reversed ? TempPen : DestPen, mid, end);
            });


    



            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
            DoResize = false;
        }

        private void TestPanel_Resize(object sender, EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                DisplayBuffer = new Bitmap(Width, Height);

                DoResize = true;
                Invalidate();
            }
        }

        internal void LayoutGraph()
        {
            CallGraphOn = true;

            DoResize = true;

            Invalidate();
        }

        internal void Relayout()
        {
            Graphs.Clear();

            Nodes = Nodes.Where(n => !n.Filler).ToList();
            Nodes.ForEach(n => n.Rank = null);

            Edges = Edges.Where(e => !e.Filler).ToList();
            Edges.ForEach(e => e.Reversed = false);

            // do while still more graph groups to process
            do
            {
                // group nodes into connected graphs
                Dictionary<int, Node> group = new Dictionary<int, Node>();

                // add first unranked node to a graph
                Node node = Nodes.First(n => n.Rank == null);
                node.Layout(group, 0, new List<Node>(), new List<string>());

                // while group contains unranked nodes
                while(group.Values.Any(n => n.Rank == null))
                {
                    // head node to start traversal
                    node = group.Values.First(n => n.Rank == null);

                    // only way node could be in group is if child added it, so there is a minrank
                    // min rank is 1 back from the lowest ranked child of the node
                    int? minRank = node.OutboundEdges.Min(e => e.Destination.Rank.HasValue ? e.Destination.Rank : int.MaxValue);

                    node.Layout(group, minRank.Value - 1, new List<Node>(), new List<string>());
                }

                // normalize ranks so sequential without any missing between
                int i = -1;
                int currentRank = int.MinValue;
                foreach (var n in group.Values.OrderBy(v => v.Rank))
                {
                    if (n.Rank != currentRank)
                    {
                        currentRank = n.Rank.Value;
                        i++;
                    }

                    n.Rank = i;
                }

                // once all nodes ranked, run back through and create any intermediate nodes
                List<Node> filler = new List<Node>();

                foreach (var n in group.Values)
                {
                    foreach (Edge edge in from e in n.Edges
                                          where e.Source == n
                                          select e)
                    {
                        Debug.Assert(edge.Source == edge.Destination || edge.Source.Rank < edge.Destination.Rank);
                        
                        // create intermediate nodes
                        if (edge.Destination.Rank.Value > n.Rank.Value + 1)
                        {
                            // change edge destination to temp node, until rank equals true destination

                            Edge tempEdge = edge;
                            int nextRank = n.Rank.Value + 1;
                            Node target = edge.Destination ;
                            edge.Destination = new Node(); // used as init for tempNode below

                            do
                            {
                                Node tempNode = tempEdge.Destination;
                                tempNode.ID = NextID++;
                                tempNode.Filler = true;
                                tempNode.Rank = nextRank;

                                tempEdge.Destination = tempNode;
                                tempNode.Edges.Add(tempEdge);
                                Edges.Add(tempEdge);

                                filler.Add(tempNode);
                                Nodes.Add(tempNode);

                               

                                tempEdge = new Edge()
                                {
                                    Source = tempNode,
                                    Destination = new Node(), // initd above as tempNode 
                                    Reversed = edge.Reversed,
                                    Filler = true
                                };

                                nextRank++;
                                if (nextRank == target.Rank.Value)
                                    tempEdge.Destination = target;

                            } while (tempEdge.Destination != target);

                            target.Edges.Add(tempEdge);
                            Edges.Add(tempEdge);
                        }
                    }
                }

                // add filler nodes to the group
                filler.ForEach(n => group[n.ID] = n);
              


                // put all nodes into a rank based multi-map
                Dictionary<int, List<Node>> rankMap = new Dictionary<int, List<Node>>();
                Graphs.Add(new Graph() { RankMap = rankMap });

                foreach (var n in group.Values)
                {
                    if (!rankMap.ContainsKey(n.Rank.Value))
                        rankMap[n.Rank.Value] = new List<Node>();

                    rankMap[n.Rank.Value].Add(n);
                }

          
            } while (Nodes.Any(n => n.Rank == null));


            //
            double totalWeight = Nodes.Sum(n => n.Weight);
            double totalArea = Width * Height;

            int groupOffset = 0;

            double weightToPix = totalArea / totalWeight;
            Nodes.ForEach(n => n.Size = (float)Math.Sqrt(n.Weight * weightToPix));

            // sum the heights of the biggest ranks of each graph
            float[] maxRankHeights = Graphs.Select(g => g.RankMap.Values.Max(rank => rank.Sum(n => n.Size))).ToArray();
            float stackHeight = maxRankHeights.Sum();

            float heightReduce = Height / stackHeight;

            // check x axis reduce, and if less use that one
            // do for all graphs at once so proportions stay the same

            // find graph with max width, buy summing max node in each rank
            float[] maxRankWidths = Graphs.Select(g => g.RankMap.Values.Sum(rank => rank.Max(n => n.Size))).ToArray();

            float widthReduce = Width / maxRankWidths.Max(); ;

            float reduce = Math.Min(heightReduce, widthReduce) / 1;

            Nodes.ForEach(n => n.Size *= reduce);


            // give each group a height proportional to their max rank height

            for(int i = 0; i < Graphs.Count; i++)
            {
                var graph = Graphs[i];

                var rankMap = graph.RankMap;

                graph.Height = (int)((float)Height * maxRankHeights[i] / stackHeight);
                graph.Offset = groupOffset;

                float freespace = Width - maxRankWidths[i] * reduce;

                float spaceX = freespace / (rankMap.Count + 1);
                float xOffset = spaceX;

                foreach (int rank in rankMap.Keys.OrderBy(k => k))
                {
                    List<Node> nodes = rankMap[rank];
                    PositionRank(graph, nodes,  xOffset);

                    xOffset += nodes.Max(n => n.Size) + spaceX;
                }

                groupOffset += graph.Height;
            }

            DoRedraw = true;
            Invalidate();
        }

        private static void PositionRank(Graph graph, List<Node> nodes, float xOffset)
        {
            float totalHeight = nodes.Sum(n => n.Size);

            float freespace = graph.Height - totalHeight;

            float spaceY = freespace / (nodes.Count + 1); // space between each block
            float yOffset = spaceY;


            foreach (var node in nodes)
            {
                node.Location.X = xOffset;
                node.Location.Y = graph.Offset + yOffset;

                yOffset += node.Size + spaceY;
            }
        }

        
        internal void Uncross()
        {
            foreach (var graph in Graphs)
            {
                var rankMap = graph.RankMap;

                // foreach rank list
                foreach(int i in rankMap.Keys.OrderBy(k => k).ToArray())
                {
                    var nodes = rankMap[i];

                    // foreach node average y pos form all connected edges
                    foreach (var node in nodes.Where(n => n.Edges.Count > 0))
                        node.Location.Y = node.Edges.Average(e => (e.Source == node) ? e.Destination.Location.Y : e.Source.Location.Y);

                    // set rank list to new node list
                    rankMap[i] = nodes.OrderBy(n => n.Location.Y).ToList();

                    PositionRank(graph, rankMap[i], nodes[0].Location.X);
                }
                // order list of y pos to node
            } 
                    // reset y positions of nodes in rank

            DoRedraw = true;
            Invalidate();
        }

        internal void MinDistance()
        {
            foreach (var graph in Graphs)
            {
                var rankMap = graph.RankMap;

                // foreach rank list
                foreach (int i in rankMap.Keys.OrderBy(k => k).ToArray())
                {
                    var nodes = rankMap[i];

                    // foreach node average y pos form all connected edges
                    for (int x = 0; x < nodes.Count; x++)
                    {
                        var node = nodes[x];

                        if (node.Edges.Count == 0)
                            continue;

                        float lowerbound = (x > 0) ? nodes[x - 1].Location.Y : 0;
                        lowerbound += MinInternodeDistance;

                        float upperbound = (x < nodes.Count - 1) ? nodes[x + 1].Location.Y : float.MaxValue;
                        upperbound -= MinInternodeDistance;

                        if (lowerbound >= upperbound)
                            continue;

                        float optimalY = node.Edges.Average(e => (e.Source == node) ? e.Destination.Location.Y : e.Source.Location.Y);

                        if (optimalY < lowerbound)
                            optimalY = lowerbound;

                        else if (optimalY > upperbound)
                            optimalY = upperbound;

                        node.Location.Y = optimalY;
                    }
                }
                // order list of y pos to node
            }
            // reset y positions of nodes in rank

            DoRedraw = true;
            Invalidate();
        }

        internal void AddNodes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Node x = new Node() { ID = NextID++ };

                // connect to random
            }
        }
    }

    [DebuggerDisplay("ID = {ID}, Rank = {Rank}")]
    class Node
    {
        internal int ID;
        internal int Weight;
        internal List<Edge> Edges = new List<Edge>();

        internal PointF Location;
        internal float Size; // width and height
        internal int? Rank;
        internal bool Filler;

        internal IEnumerable<Edge> OutboundEdges;
        internal IEnumerable<Edge> InboundEdges;


        internal Node()
        {
            InboundEdges = Edges.Where(e => e.Destination == this && e.Source != this);
            OutboundEdges = Edges.Where(e => e.Source == this && e.Destination != this);
        }

        // rank nodes and reverse cycles, ideally start at parent node
        internal void Layout(Dictionary<int, Node> group, int minRank, List<Node> parents, List<string> debugLog)
        {
            debugLog.Add(string.Format("Entered Node ID {0} rank {1}", ID, Rank));

            // node already ranked correctly, no need to re-rank subordinates
            if (Rank != null && Rank.Value >= minRank)
                return;

            int? prevRank = Rank;

            // only increase rank
            Debug.Assert(Rank == null || minRank > Rank.Value);
            Rank = minRank;

            debugLog.Add(string.Format("Node ID {0} rank set from {1} to {2}", ID, prevRank, Rank));

            parents.Add(this);
            group[ID] = this;

            foreach (Edge edge in OutboundEdges)
            {
                if (parents.Contains(edge.Destination))
                {
                    // destination rank should be less than source
                    Debug.Assert(edge.Destination.Rank < edge.Source.Rank);

                    debugLog.Add(string.Format("Switching edge {0} -> {1}, rank {2} -> {3}", ID, edge.Destination.ID, Rank, edge.Destination.Rank));
                    
                    edge.Source = edge.Destination;
                    edge.Destination = this;
                    edge.Reversed = !edge.Reversed;

                    continue;
                }

                // pass copy of parents list so that sub can add elemenets without affecting next iteration
                debugLog.Add(string.Format("Traversing to child {0} -> {1}, rank {2} -> {3}", ID, edge.Destination.ID, Rank, edge.Destination.Rank));
                
                edge.Destination.Layout(group, Rank.Value + 1, parents.ToList(), debugLog);
                
                debugLog.Add(string.Format("Return to node {0} rank {1}", ID, Rank));
            }

            // record so later group can be traversed for null ranked members so layout can be run on them
            foreach (Edge edge in InboundEdges)
                group[edge.Source.ID] = edge.Source;

            /*foreach (Node parent in from e in Edges
                                    where e.Destination == this && e.Source != this
                                    select e.Source)
            {
                if (parent.Rank == null) // dont re-rank higher nodes
                {

                    int min = int.MaxValue;

                    foreach (int value in from e in parent.Edges
                                          where e.Source == parent &&
                                                e.Destination != parent &&
                                                e.Destination.Rank != null
                                          select e.Destination.Rank.Value)
                    {
                        if (value < min)
                            min = value;
                    }

                    if (min == int.MaxValue)
                    {
                        // should not happen cause caller (child) should have rank
                        Debug.Assert(false);
                    }

                    //parent.Rank = min - 1;

                    // pass copy of parents list so that sub can add elemenets without affecting next iteration
                    debugLog.Add(string.Format("Traversing to parent  {0} -> {1}, rank {2} -> {3}", ID, parent.ID, Rank, parent.Rank));
                    parent.Layout(group, min - 1, new List<Node>(), debugLog);
                    debugLog.Add(string.Format("Return to node {0} rank {1}", ID, Rank));
                }
            }*/

            debugLog.Add(string.Format("Exited Node ID {0} rank {1}", ID, Rank));
        }

  

        
    }

    [DebuggerDisplay("{Source.ID} -> {Destination.ID}")]
    class Edge
    {
        internal Node Source;
        internal Node Destination;

        internal bool Filler;
        internal bool Reversed;
    }

    class Graph
    {
        internal Dictionary<int, List<Node>> RankMap = new Dictionary<int, List<Node>>();

        internal int Height;
        internal int Offset;
    }

}
