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
        SolidBrush TempBrush = new SolidBrush(Color.LimeGreen);
        Pen SourcePen = new Pen(Color.Red);
        Pen DestPen = new Pen(Color.Blue);
        Pen TempPen = new Pen(Color.Green);

        Random RndGen = new Random();
        int NextID = 1;

        const int MinInternodeDistance = 25;


        public TestPanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

        }

        internal void ResetText(int nodeCount, int edgeCount)
        {
            Nodes.Clear();
            Edges.Clear();
            
            NextID = 1;

            for (int i = 0; i < nodeCount; i++)
                Nodes.Add(new Node() { ID = NextID++, Location = new PointF(RndGen.Next(Width), RndGen.Next(Height)) });

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

            DoRedraw = true;
            Invalidate();
        }

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

 

            Edges.ForEach(e2 =>
            {
                PointF start = e2.Source.Location;
                PointF end = e2.Destination.Location;
                PointF mid = new PointF(start.X + (end.X - start.X) / 2, start.Y + (end.Y - start.Y) / 2);

                buffer.DrawLine(e2.Reversed ? TempPen : SourcePen, start, mid);
                buffer.DrawLine(e2.Reversed ? TempPen : DestPen, mid, end);
            });


            Nodes.ForEach(n =>
            {
                buffer.FillEllipse(n.Filler ? TempBrush : NodeBrush, n.Location.X - 5, n.Location.Y - 5, 10, 10);
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
            Graphs.Clear();

            Nodes = Nodes.Where(n => !n.Filler).ToList();
            Nodes.ForEach(n => n.Rank = null);

            Edges = Edges.Where(e => !e.Filler).ToList();
            Edges.ForEach(e => e.Reversed = false);

            do
            {
                
                Dictionary<int, Node> group = new Dictionary<int, Node>();

                Node node = Nodes.First(n => n.Rank == null);

                // first figure out ranks
                node.Phase1(group, 0, new List<Node>());

                /* get the min rank form the above step, use to offset ranks properly
                int lowestRank = group.Values.Min(n => n.Rank).Value;
                int offset = Math.Abs(lowestRank);
                foreach (var n in group.Values)
                    n.Rank += offset;*/

                // normalize ranks (for missing ranks, decrease following node ranks)
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
                        // create intermediate nodes
                        if (edge.Destination.Rank.Value > n.Rank.Value + 1)
                        {
                            // change edge destination to temp node, until rank equals true destination

                            Edge tempEdge = edge;
                            int nextRank = n.Rank.Value + 1;
                            Node target = edge.Destination ;
                            edge.Destination = new Node();// CREATE ID??

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
                                    Destination = new Node(),// CREATE ID??
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

                filler.ForEach(n => group[n.ID] = n);
              
                // also at this stage put all nodes into a rank based multi-map
                Dictionary<int, List<Node>> rankMap = new Dictionary<int, List<Node>>();
                Graphs.Add(new Graph() { RankMap = rankMap });

                foreach (var n in group.Values)
                {
                    if (!rankMap.ContainsKey(n.Rank.Value))
                        rankMap[n.Rank.Value] = new List<Node>();

                    rankMap[n.Rank.Value].Add(n);
                }

          
            } while (Nodes.Any(n => n.Rank == null));


            int groupOffset = 0;
            int total = Graphs.Sum(g => g.RankMap.Values.Sum(l => l.Count));

            // give each group a height proportional to their contents

            foreach (var graph in Graphs)
            {
                var rankMap = graph.RankMap;

                graph.Height = Height * rankMap.Values.Sum(l => l.Count) / total;
                graph.Offset = groupOffset;

                int spaceX = Width / (rankMap.Count + 1);
                int xOffset = spaceX;

                foreach (int rank in rankMap.Keys.OrderBy(k => k))
                {
                    PositionRank(graph,rankMap[rank],  xOffset);

                    xOffset += spaceX;
                }

                groupOffset += graph.Height;
            }

            DoRedraw = true;
            Invalidate();
        }

        private static void PositionRank(Graph graph, List<Node> nodes, float xOffset)
        {
            int spaceY = graph.Height / (nodes.Count + 1);
            int yOffset = spaceY;


            foreach (var node in nodes)
            {
                node.Location.X = xOffset;
                node.Location.Y = graph.Offset + yOffset;

                yOffset += spaceY;
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

        /*internal void Uncross2()
        {
            1. minimize cross over
	            foreach rank
		            foreach node
			            foreach edge of node
				            if edge intersections > 0
					            count intersections of all target nodes edges
						            try inserting lower rankd node to different positions (random, closest to source, use intersection information to form boundaries)
							            count again intersections of target node
								            keep lowest
            					
            find intersections for edge source y1 to dest y2

            foreach node in source rank that is not connected to the edge
	            for each edge of that source node
		            if y of source of source node is < y of eval node AND
		            y of dest node > y of eval dest node
			            increase cross over
			            set boundary that changed node must be greater than for no cross over
            			
            evaluate change	
             

            foreach (var rankMap in Groups)
            {
                foreach (int rank in rankMap.Keys.OrderBy(k => k))
                {
                    foreach (var node in rankMap[rank])
                    {
                        foreach (var edge in node.Edges.Where(e => e.Source == node))
                        {
                            int count = GetIntersections(edge, rankMap[rank]);
                        }
                    }
                }
            }
        }*/

        // evaluate all parent edges of nodes and get total intersections
            // determine where to place to minimize intersections
                // maybe do interactively while laying out rank

        private int GetIntersections(Edge testEdge, List<Node> sourcePeers)
        {
            int intersections = 0;
            float lowerbound = 0;
            float upperbound = float.MaxValue;


            // for each peer to the source node we're testing (source's own edges dont cross)
            foreach(var peer in sourcePeers.Where(sp => sp != testEdge.Source))
                // test if peer edge crosses test edge
                foreach (var peerEdge in peer.Edges.Where(e => e.Source == peer))
                {
                    if (peer.Location.Y < testEdge.Source.Location.Y &&
                        peerEdge.Destination.Location.Y > testEdge.Destination.Location.Y)
                    {
                        intersections++;

                        if (peerEdge.Destination.Location.Y > lowerbound)
                            lowerbound = peerEdge.Destination.Location.Y;
                    }

                    else if (peer.Location.Y > testEdge.Source.Location.Y &&
                             peerEdge.Destination.Location.Y < testEdge.Destination.Location.Y)
                    {
                        intersections++;

                        if(peerEdge.Destination.Location.Y < upperbound)
                            upperbound = peerEdge.Destination.Location.Y;
                    }
                }

            if (lowerbound < upperbound)
            {
                // use rank instead of y values
                // there is a clear position node can be be moved between
            }


           return intersections;
        }


    }

    [DebuggerDisplay("ID = {ID}")]
    class Node
    {
        internal int ID;
        internal List<Edge> Edges = new List<Edge>();
        internal PointF Location;
        internal int? Rank;
        internal bool Filler;

        // rank nodes and reverse cycles, ideally start at parent node
        internal void Phase1(Dictionary<int, Node> group, int rank, List<Node> parents)
        {

            // node already ranked correctly, no need to re-rank subordinates
            if (Rank != null && Rank.Value == rank)
                return;
            
            // only increase rank
            if (Rank == null || rank > Rank.Value)
                Rank = rank;

            

            parents.Add(this);
            group[ID] = this;

            foreach (Edge edge in from e in Edges
                                 where e.Source == this && e.Destination != this
                                 select e)
            {
                if (parents.Contains(edge.Destination))
                {
                    edge.Source = edge.Destination;
                    edge.Destination = this;
                    edge.Reversed = !edge.Reversed;
                    continue;
                }

                // pass copy of parents list so that sub can add elemenets without affecting next iteration
                edge.Destination.Phase1(group, rank + 1, parents.ToList());
            }

            foreach (Node parent in from e in Edges
                                    where e.Destination == this && e.Source != this
                                    select e.Source)
            {
                if (parent.Rank == null) // dont re-rank higher nodes
                {

                    // pass copy of parents list so that sub can add elemenets without affecting next iteration
                    parent.Phase1(group, rank - 1, new List<Node>());
                }
            }


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
