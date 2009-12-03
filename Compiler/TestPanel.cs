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
                    ScaledLocation = new PointF((float)RndGen.NextDouble(), (float)RndGen.NextDouble())
                });

            for (int i = 0; i < edgeCount; i++)
            {
                Edge edge = new Edge(Nodes[RndGen.Next(nodeCount)], Nodes[RndGen.Next(nodeCount)]);

                if (Edges.Any(e => e.Source == edge.Source && e.Destination == edge.Destination))
                    continue;
                
                Edges.Add(edge);

                edge.Source.Edges.Add(edge);
                edge.Destination.Edges.Add(edge);
            }





            Graphs.Clear();

            Nodes = Nodes.Where(n => !n.Filler).ToList();
            Nodes.ForEach(n => n.Rank = null);

            Edges = Edges.Where(e => !e.Filler).ToList();
            Edges.ForEach(e => e.Reset());

            // do while still more graph groups to process
            do
            {
                // group nodes into connected graphs
                Dictionary<int, Node> graph = new Dictionary<int, Node>();

                // add first unranked node to a graph
                Node node = Nodes.First(n => n.Rank == null);
                node.Layout(graph, 0, new List<Node>(), new List<string>());

                // while group contains unranked nodes
                while (graph.Values.Any(n => n.Rank == null))
                {
                    // head node to start traversal
                    node = graph.Values.First(n => n.Rank == null);

                    // only way node could be in group is if child added it, so there is a minrank
                    // min rank is 1 back from the lowest ranked child of the node
                    int? minRank = node.OutboundEdges.Min(e => e.Destination.Rank.HasValue ? e.Destination.Rank : int.MaxValue);

                    node.Layout(graph, minRank.Value - 1, new List<Node>(), new List<string>());
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
                Dictionary<int, Rank> rankMap = new Dictionary<int, Rank>();


                foreach (var n in graph.Values)
                {
                    int index = n.Rank.Value;

                    if (!rankMap.ContainsKey(index))
                        rankMap[index] = new Rank() { Order = index };

                    rankMap[index].Column.Add(n);
                }


                // once all nodes ranked, run back through and create any intermediate nodes
                foreach (var n in graph.Values)
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
                            Node target = edge.Destination;
                            edge.Destination = new Node(); // used as init for tempNode below

                            do
                            {
                                Node tempNode = tempEdge.Destination;
                                tempNode.ID = NextID++;
                                tempNode.Filler = true;
                                tempNode.Rank = nextRank;
                                tempNode.Weight = 3;

                                tempEdge.Destination = tempNode;
                                tempNode.Edges.Add(tempEdge);

                                rankMap[nextRank].Column.Add(tempNode);
                                graph[tempNode.ID] = tempNode;
                          
                                Nodes.Add(tempNode);



                                tempEdge = new Edge(tempNode, new Node()) // initd above as tempNode 
                                {
                                    Reversed = edge.Reversed,
                                    Filler = true
                                };

                                Edges.Add(tempEdge);

                                nextRank++;
                                if (nextRank == target.Rank.Value)
                                    tempEdge.Destination = target;

                            } while (tempEdge.Destination != target);

                            // TODO do need to add edge to assist in uncross?
                            // dont need to add inbound edge to real node because inbound edges
                            // only traversed in layout which is called before this
                            //target.Edges.Add(tempEdge);
                        }
                    }
                }
        

                Graphs.Add(new Graph() { Ranks = rankMap.Values.OrderBy(r => r.Order).ToList() });

            } while (Nodes.Any(n => n.Rank == null));


            Graphs = Graphs.OrderByDescending(g => g.Ranks.Sum(r => r.Column.Sum(n => n.Weight))).ToList();



            double totalWeight = Nodes.Sum(n => n.Weight);
            double totalArea = 1; // Width* Height;

            double weightToPix = totalArea / totalWeight * Reduce;

            Nodes.ForEach(n => n.ScaledSize = (float)Math.Sqrt(n.Weight * weightToPix));



            DoRedraw = true;
            Invalidate();
        }

        double Reduce = 0.5;


        private void TestPanel_Paint(object sender, PaintEventArgs e)
        {
            if (DisplayBuffer == null)
                DisplayBuffer = new Bitmap(Width, Height);

            if (!DoRedraw )
            {
                e.Graphics.DrawImage(DisplayBuffer, 0, 0);
                return;
            }

            // background
            Graphics buffer = Graphics.FromImage(DisplayBuffer);
            buffer.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality; // todo option to turn this off

            buffer.Clear(Color.White);

            float fullSize = (float) Math.Min(Width, Height) / 2;

            Nodes.ForEach(n =>
            {
                n.Location.X = Width * n.ScaledLocation.X;
                n.Location.Y = Height * n.ScaledLocation.Y;

                float size = fullSize * n.ScaledSize;
                float halfSize = size / 2;

                if(!n.Filler)
                    buffer.DrawRectangle(NodePen, n.Location.X - halfSize, n.Location.Y - halfSize, size, size);

                if(!CallGraphOn)
                    buffer.FillEllipse(n.Filler ? TempBrush : NodeBrush, n.Location.X - 5, n.Location.Y - 5, 10, 10);
            });

            Edges.ForEach(e2 =>
            {
                if (!CallGraphOn)
                {
                    PointF start = e2.Source.ScaledLocation;
                    PointF end = e2.Destination.ScaledLocation;
                    PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                    buffer.DrawLine(e2.Reversed ? TempPen : SourcePen, start, mid);
                    buffer.DrawLine(e2.Reversed ? TempPen : DestPen, mid, end);
                }
                else
                {
                    buffer.DrawLine(e2.Reversed ? DestPen : SourcePen, e2.Source.Location, e2.Destination.Location);
                }

            });


    



            // Copy buffer to display
            e.Graphics.DrawImage(DisplayBuffer, 0, 0);

            DoRedraw = false;
        }

        private void TestPanel_Resize(object sender, EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                DisplayBuffer = new Bitmap(Width, Height);

                DoRedraw = true;

                Invalidate();
            }
        }

        internal void LayoutGraph()
        {
            Relayout();

            CallGraphOn = true;

            Invalidate();
        }

        internal void Relayout()
        {
            double totalWeight = Nodes.Sum(n => n.Weight);
            double totalArea = 1; // Width* Height;
            double weightToPix = totalArea / totalWeight;
            Nodes.ForEach(n => n.ScaledSize = (float)Math.Sqrt(n.Weight * weightToPix));

            // sum the heights of the biggest ranks of each graph
            float[] maxRankHeights = Graphs.Select(g => g.Ranks.Max(rank => rank.Column.Sum(n => n.ScaledSize))).ToArray();
            float stackHeight = maxRankHeights.Sum();

            float heightReduce = 1 / stackHeight;

            // check x axis reduce, and if less use that one
            // do for all graphs at once so proportions stay the same

            // find graph with max width, buy summing max node in each rank
            float[] maxRankWidths = Graphs.Select(g => g.Ranks.Sum(rank => rank.Column.Max(n => n.ScaledSize))).ToArray();

            float widthReduce = 1 / maxRankWidths.Max(); ;

            float reduce = (float) ( Math.Min(heightReduce, widthReduce) * 0.75);

            Nodes.ForEach(n => n.ScaledSize *= reduce);


            // give each group a height proportional to their max rank height
            float groupOffset = 0;
            
            for(int i = 0; i < Graphs.Count; i++)
            {
                var graph = Graphs[i];

                graph.ScaledHeight = maxRankHeights[i] / stackHeight;
                graph.ScaledOffset = groupOffset;

                float freespace = 1 - maxRankWidths[i] * reduce;

                float spaceX = freespace / (graph.Ranks.Count + 1);
                float xOffset = spaceX;

                foreach (var rank in graph.Ranks)
                {
                    float maxSize = rank.Column.Max(n => n.ScaledSize);

                    PositionRank(graph, rank, xOffset + maxSize / 2);

                    xOffset += maxSize + spaceX;
                }

                groupOffset += graph.ScaledHeight;
            }

            DoRedraw = true;
            Invalidate();
        }

        private void PositionRank(Graph graph, Rank rank, float xOffset)
        {
            var nodes = rank.Column;

            float totalHeight = nodes.Sum(n => n.ScaledSize);

            float freespace = 1 * graph.ScaledHeight - totalHeight;

            float ySpace = freespace / (nodes.Count + 1); // space between each block
            rank.MinNodeSpace = Math.Min(ySpace, 16);
            float yOffset = ySpace;


            foreach (var node in nodes)
            {
                node.ScaledLocation.X = xOffset;
                node.ScaledLocation.Y = 1 * graph.ScaledOffset + yOffset + node.ScaledSize / 2;

                yOffset += node.ScaledSize + ySpace;
            }
        }

        
        internal void Uncross()
        {
            foreach (var graph in Graphs)
                foreach(Rank rank in graph.Ranks)
                {
                    // foreach node average y pos form all connected edges
                    foreach (var node in rank.Column.Where(n => n.Edges.Count > 0))
                        node.ScaledLocation.Y = node.Edges.Average(e => (e.Source == node) ? e.Destination.ScaledLocation.Y : e.Source.ScaledLocation.Y);

                    // set rank list to new node list
                    rank.Column = rank.Column.OrderBy(n => n.ScaledLocation.Y).ToList();

                    PositionRank(graph, rank, rank.Column[0].ScaledLocation.X);
                }

            DoRedraw = true;
            Invalidate();
        }

        internal void MinDistance()
        {
            foreach (var graph in Graphs)
            {
                // foreach rank list
                foreach (Rank rank in graph.Ranks)
                {
                    var nodes = rank.Column;

                    // foreach node average y pos form all connected edges
                    for (int x = 0; x < nodes.Count; x++)
                    {
                        var node = nodes[x];

                        if (node.Edges.Count == 0)
                            continue;

                        float halfSize = node.ScaledSize / 2;

                        float lowerbound = (x > 0) ? nodes[x - 1].ScaledLocation.Y + (nodes[x - 1].ScaledSize / 2) : 0;
                        lowerbound += rank.MinNodeSpace;

                        float upperbound = (x < nodes.Count - 1) ? nodes[x + 1].ScaledLocation.Y - (nodes[x + 1].ScaledSize / 2) : float.MaxValue;
                        upperbound -= rank.MinNodeSpace;

                        Debug.Assert(lowerbound <= upperbound);
                        if (lowerbound >= upperbound)
                            continue;

                        float optimalY = node.Edges.Average(e => (e.Source == node) ? e.Destination.ScaledLocation.Y : e.Source.ScaledLocation.Y);

                        if (optimalY - halfSize < lowerbound)
                            optimalY = lowerbound + halfSize;

                        else if (optimalY + halfSize > upperbound)
                            optimalY = upperbound - halfSize;

                        node.ScaledLocation.Y = optimalY;
                    }
                }
                // order list of y pos to node
            }
            // reset y positions of nodes in rank

            DoRedraw = true;
            Invalidate();
        }
    }

    [DebuggerDisplay("ID = {ID}, Rank = {Rank}")]
    class Node
    {
        internal int ID;
        internal double Weight;
        internal List<Edge> Edges = new List<Edge>();

        internal PointF Location;
        internal PointF ScaledLocation;
        internal float ScaledSize; // width and height
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

            // record so later group can be traversed for null ranked members (parents) so layout can be run on them
            foreach (Edge edge in InboundEdges)
                group[edge.Source.ID] = edge.Source;

            // check if same edges down go back up and create intermediates in that case?

            debugLog.Add(string.Format("Exited Node ID {0} rank {1}", ID, Rank));
        }
        
    }

    [DebuggerDisplay("{Source.ID} -> {Destination.ID}")]
    class Edge
    {
        internal Node Source;
        internal Node Destination;

        Node RealSource;
        Node RealDestination;

        internal bool Filler;
        internal bool Reversed;


        internal Edge(Node source, Node destination)
        {
            Source = RealSource = source;
            Destination = RealDestination = destination;
        }

        internal void Reset()
        {
            Source = RealSource;
            Destination = RealDestination;

            Reversed = false;
        }
    }

    class Graph
    {
        internal List<Rank> Ranks = new List<Rank>();

        internal float ScaledHeight;
        internal float ScaledOffset;
    }

    class Rank
    {
        internal int Order;

        internal List<Node> Column = new List<Node>();

        internal float MinNodeSpace;
    }

}
