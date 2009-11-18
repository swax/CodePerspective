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
        List<Dictionary<int, List<Node>>> Groups = new List<Dictionary<int, List<Node>>>(); // list of groups, each group is a rank map

        SolidBrush NodeBrush = new SolidBrush(Color.Black);
        SolidBrush TempBrush = new SolidBrush(Color.Green);
        Pen SourcePen = new Pen(Color.Red);
        Pen DestPen = new Pen(Color.Blue);
        Pen TempPen = new Pen(Color.Green);

        Random RndGen = new Random();
        int NextID = 1;


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
            Groups.Clear();
            Nodes.ForEach(n => n.Rank = null);

            do
            {
                Dictionary<int, Node> group = new Dictionary<int, Node>();

                Node node = Nodes.First(n => n.Rank == null);

                // first figure out ranks
                node.Phase1(group, 0, new List<Node>());

                // get the min rank form the above step, use to offset ranks properly
                int lowestRank = group.Values.Min(n => n.Rank).Value;
                int offset = Math.Abs(lowestRank);
                foreach (var n in group.Values)
                    n.Rank += offset;

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
                                    Destination = new Node()// CREATE ID??
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
                Groups.Add(rankMap);

                foreach (var n in group.Values)
                {
                    if (!rankMap.ContainsKey(n.Rank.Value))
                        rankMap[n.Rank.Value] = new List<Node>();

                    rankMap[n.Rank.Value].Add(n);
                }

          
            } while (Nodes.Any(n => n.Rank == null));


            int groupOffset = 0;
            int total = Groups.Sum(rm => rm.Values.Sum(l => l.Count));

            // give each group a height proportional to their contents

            foreach (var rankMap in Groups)
            {
                int groupHeight = Height * rankMap.Values.Sum(l => l.Count) / total;

                int spaceX = Width / (rankMap.Count + 1);
                int xOffset = spaceX;

                foreach (int rank in rankMap.Keys.OrderBy(k => k))
                {
                    int spaceY = groupHeight / (rankMap[rank].Count + 1);
                    int yOffset = spaceY;


                    foreach (var node in rankMap[rank])
                    {
                        node.Location.X = xOffset;
                        node.Location.Y = groupOffset + yOffset;

                        yOffset += spaceY;
                    }


                    xOffset += spaceX;
                }

                groupOffset += groupHeight;
            }

            DoRedraw = true;
            Invalidate();
        }


        internal void Uncross()
        {
            /*1. minimize cross over
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
             */

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
        }

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
                                 where e.Source == this
                                 select e)
            {
                if (parents.Contains(edge.Destination))
                {
                    edge.Source = edge.Destination;
                    edge.Destination = this;
                    edge.Reversed = true;
                    continue;
                }

                // pass copy of parents list so that sub can add elemenets without affecting next iteration
                edge.Destination.Phase1(group, rank + 1, parents.ToList());
            }

            foreach (Node parent in from e in Edges
                                    where e.Destination == this
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

        internal bool Reversed;
    }

}
