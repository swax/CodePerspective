﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace XLibrary
{
    public class GraphSet
    {
        ViewModel Model;
        CallGraphMode GraphMode;

        // a single graph area may be divided for independent graphs
        public List<Graph> Graphs = new List<Graph>();

        public Dictionary<int, NodeModel> PositionMap = new Dictionary<int, NodeModel>();
        public HashSet<int> CenterMap = new HashSet<int>(); // used to filter calls into and out of center

        // sub sets are entire graphs within a subnode in the current graph 
        public Dictionary<int, GraphSet> Subsets = new Dictionary<int, GraphSet>();

        public NodeModel GraphContainer;

        float GraphFillSpace = 0.80f;
        float GraphMinNodeProportion = 0.30f;


        public GraphSet(ViewModel model, NodeModel root, NodeModel container=null, int depth = -1)
        {
            Model = model;
            GraphMode = Model.GraphMode;
            GraphContainer = container;

            // iternate nodes at this zoom level
            if (GraphMode == CallGraphMode.Intermediates)
            {
                AddDependencyNodes();
            }

            else if (GraphMode == CallGraphMode.Layers)
            {
                foreach (var child in root.Nodes)
                    AddCalledNodes(child, true, 0);
            }

            else
            {
                AddCalledNodes(root, true, depth);

                if (GraphContainer == null)
                {
                    if (Model.ShowOutside || CenterMap.Count == 1) // prevent blank screen
                        AddCalledNodes(Model.InternalRoot, false);

                    if (Model.ShowExternal)
                        AddCalledNodes(Model.ExternalRoot, false);
                }
            }

            // process subsets before building graphs so we can prune empty subset graphs
            if (GraphMode == CallGraphMode.Layers)
            {
                foreach (var child in root.Nodes)
                    Subsets[child.ID] = new GraphSet(Model, child, child);
            }

            else if (GraphMode == CallGraphMode.Class &&
                     Model.ShowMethods &&
                     GraphContainer == null)
            {
                foreach (var classNode in PositionMap.Values)
                    Subsets[classNode.ID] = new GraphSet(Model, classNode, classNode, 1);
            }


            if (PositionMap.Count > 0)
            {
                BuildGraphs();

                if (Graphs.Count > 0)
                    LayoutGraphs();
            }

        }

        public void AddCalledNodes(NodeModel node, bool center, int depth = -1)
        {
            if (!node.Show)
                return;

            node.Intermediates = null;

            var xNode = node.XNode;

            if (node.XNode.IsAnon && !Model.ShowAnon)
            {
                // ignore node
            }

            else if (GraphMode == CallGraphMode.Dependencies)
            {
                if((xNode.Independencies != null && xNode.Independencies.Length > 0) ||
                   (xNode.Dependencies != null && xNode.Dependencies.Length > 0))
                    AddEdges(node, center, xNode.Independencies, xNode.Dependencies);
            }
            else if (GraphMode == CallGraphMode.Layers)
            {
                AddEdges(node, center, xNode.LayerIn, xNode.LayerOut);
            }
            else if (GraphMode == CallGraphMode.Init) 
            {
                if (xNode.InitsBy != null || xNode.InitsOf != null) // dont combine with above because we want to ignore nodes that arent classes
                {
                    if ((xNode.InitsBy != null && xNode.InitsBy.Count > 0) ||
                        (xNode.InitsOf != null && xNode.InitsOf.Count > 0))
                        AddEdges(node, center, xNode.InitsBy, xNode.InitsOf);
                }
            }
            else if ((GraphMode == CallGraphMode.Class && node.ObjType == XObjType.Class && GraphContainer == null) ||
                     (GraphMode == CallGraphMode.Class && node.ObjType != XObjType.Class && GraphContainer != null) ||
                     (GraphMode == CallGraphMode.Method && (node.ObjType == XObjType.Method || node.ObjType == XObjType.Field)))
            {
                IEnumerable<int> callsIn = null;
                IEnumerable<int> callsOut = null;

                if(xNode.CalledIn != null && xNode.CalledIn.Length > 0)
                    callsIn = xNode.CalledIn.Select(c => c.Source);

                if(xNode.CallsOut != null && xNode.CallsOut.Length > 0)
                    callsOut = xNode.CallsOut.Select(c => c.Destination);

                AddEdges(node, center, callsIn, callsOut);
            }

            if (depth == 0)
                return;

            depth--;

            foreach (var sub in node.Nodes)
                if (sub != Model.InternalRoot) // when traversing outside root, dont interate back into center root
                    AddCalledNodes(sub, center, depth);
        }

        private void AddEdges(NodeModel node, bool center, IEnumerable<int> callsIn, IEnumerable<int> callsOut)
        {
            if (center ||
                ((callsIn != null && callsIn.Any(source => CenterMap.Contains(source))) ||
                 (callsOut != null && callsOut.Any(dest => CenterMap.Contains(dest)))))
            {
                PositionMap[node.ID] = node;

                if(center)
                    CenterMap.Add(node.ID);

                node.EdgesIn = (callsIn != null) ? callsIn.ToArray() : null;
                node.EdgesOut = (callsOut != null) ? callsOut.ToArray() : null;
            }
        }

        private void LayoutGraphs()
        {
            Graphs = Graphs.OrderByDescending(g => g.Weight).ToList();

            float totalWeight = Graphs.Sum(g => g.Weight);
            float graphOffset = 0;

            // foreach graph
            for(int i = 0; i < Graphs.Count; i++)
            {
                var graph = Graphs[i];
                
                // Add reference to position map for the layout algorithm
                graph.NodeMap = PositionMap;

                // size graph height in proportion to weight
                graph.ScaledHeight = graph.Weight / totalWeight;
                graph.ScaledOffset = graphOffset;
                graphOffset += graph.ScaledHeight;
                
                // iterate all nodes in graph for min/max value maybe sort...
                float min = float.MaxValue;
                float max = float.MinValue;
                foreach (var node in graph.Nodes())
                {
                    if (node.Value < min)
                        min = node.Value;
                    if (node.Value > max)
                        max = node.Value;
                    
                    // Add reference to position map for the ideal position calculation
                    node.PositionMap = PositionMap;
                }

                // find column with the most nodes
                float maxNodeCount = graph.Ranks.Max(r => r.Column.Count);
                if (graph.Ranks.Length > maxNodeCount)
                    maxNodeCount = graph.Ranks.Length;

                // base node size on the biggest node we can create for the graph
                float fillSpace = graph.ScaledHeight * GraphFillSpace;
                float maxNodeSize = fillSpace / maxNodeCount;

                // assign nodes a size 
                float sizeRange = max - min;

                foreach (var node in graph.Nodes())
                {
                    node.ScaledSize = maxNodeSize;
            
                    if (sizeRange > 0)
                    {
                        // get 0 - 1 value for node value
                        float value = ((float)node.Value - min) / sizeRange;

                        // give the node a proportional size with 33% as the smallest
                        float proportion = GraphMinNodeProportion + ((1 - GraphMinNodeProportion) * value);
                        node.ScaledSize = maxNodeSize * proportion;
                    }
                }

                // position columns
                float spacePerColumn = 1.0f / (float)graph.Ranks.Length;
                float xOffset = spacePerColumn / 2.0f;

                for (int x = 0; x < graph.Ranks.Length; x++)
                {
                    var rank = graph.Ranks[x];
                    PositionRank(graph, rank, xOffset);
                    xOffset += spacePerColumn;
                }

                // Apply improved layout algorithm
                if (Model.SequenceOrder)
                {
                    // For sequence ordering, use mostly vertical spacing
                    for (int x = 0; x < 3; x++)
                        MinDistance(graph);
                }
                else
                {
                    // Apply the Sugiyama layout algorithm for better results
                    GraphLayout.ApplySugiyamaLayout(graph);
                }
            }
        }

        private void BuildGraphs()
        {
            foreach (var node in PositionMap.Values)
                node.Rank = null;

            do
            {
                // group nodes into connected graphs
                var graph = new Dictionary<int, NodeModel>();

                // add first unranked node to a graph
                var unrankedNode = PositionMap.Values.First(n => n.Rank == null);

                LayoutGraph(graph, unrankedNode, 0, new List<int>());

                // while group contains unranked nodes
                while (graph.Values.Any(n => n.Rank == null && n.EdgesOut != null))
                {
                    // head node to start traversal
                    unrankedNode = graph.Values.First(n => n.Rank == null && n.EdgesOut != null);

                    // only way node could be in group is if child added it, so there is a minrank
                    // min rank is 1 back from the lowest ranked child of the node
                    int? minRank = unrankedNode.EdgesOut.Min(dest =>
                    {
                        if (PositionMap.ContainsKey(dest))
                        {
                            var destNode = PositionMap[dest];
                            if (destNode.Rank.HasValue)
                                return destNode.Rank.Value;
                        }

                        return int.MaxValue;
                    });

                    LayoutGraph(graph, unrankedNode, minRank.Value - 1, new List<int>());
                }

                // remove graphs with 1 element
                if (graph.Count == 1)
                {
                    bool remove = false;
                    var onlyNode = graph.First().Value;
                    
                    if (GraphMode == CallGraphMode.Dependencies || GraphMode == CallGraphMode.Method)
                        remove = true;

                    if(GraphMode == CallGraphMode.Class || GraphMode == CallGraphMode.Layers)
                    {
                        // dont remove method/field if alone as a graph because it may be still connect to other classes
                        if (onlyNode.ObjType == XObjType.Method || onlyNode.ObjType == XObjType.Field)
                        {
                            // in class mode edges between nodes set dynamically, in layers mode edges are based on calls
                            if ((GraphMode == CallGraphMode.Layers && onlyNode.XNode.CalledIn == null && onlyNode.XNode.CallsOut == null) ||
                                (GraphMode == CallGraphMode.Class && onlyNode.EdgesIn == null && onlyNode.EdgesOut == null))
                                remove = true;
                        }
                        // remove empty lonesome classes
                        else if (onlyNode.ObjType == XObjType.Class && !Model.ShowMethods && !Model.ShowFields)
                            remove = true;

                        // if node is by lonesome and has no sub-graphs inside of it
                        else if (Subsets.ContainsKey(onlyNode.ID) && Subsets[onlyNode.ID].Graphs.Count == 0)
                            remove = true;
                    }

                    if(remove)
                    {
                        PositionMap.Remove(onlyNode.ID);
                        CenterMap.Remove(onlyNode.ID);
                        continue;
                    }
                }
            

                // normalize ranks so sequential without any missing between
                int nextSequentialRank = -1;
                int currentRank = int.MinValue;
                foreach (var n in graph.Values.OrderBy(v => v.Rank))
                {
                    if (n.Rank != currentRank)
                    {
                        currentRank = n.Rank.Value;
                        nextSequentialRank++;
                    }

                    n.Rank = nextSequentialRank;
                }

                // put all nodes into a rank based multi-map
                Rank[] ranks = new Rank[nextSequentialRank + 1];
                for (int i = 0; i < ranks.Length; i++)
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

                        var destination = graph[destId];

                        // ranks are equal if nodes are outside zoom
                        if (source.ID == destination.ID || destination.Rank == source.Rank)
                            continue;

                        if (source.Intermediates != null)
                            source.Intermediates.Remove(destId);

                        // if destination is not 1 forward/1 back then create intermediate nodes
                        if (source.Rank != destination.Rank + 1 &&
                            source.Rank != destination.Rank - 1)
                        {
                            if (source.Intermediates == null)
                                source.Intermediates = new Dictionary<int, List<NodeModel>>();

                            source.Intermediates[destId] = new List<NodeModel>();

                            bool increase = destination.Rank > source.Rank;
                            int nextRank = increase ? source.Rank.Value + 1 : source.Rank.Value - 1;
                            var lastNode = source;

                            while (nextRank != destination.Rank)
                            {

                                // create new node
                                var intermediate = new NodeModel(Model);
                                intermediate.Rank = nextRank;
                                intermediate.Value = 10; // todo make smarter - 
                                intermediate.Adjacents = new List<NodeModel>();

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

        // Keep the original methods for compatibility
        private void Uncross(Graph graph)
        {
            // This method is kept for backward compatibility
            // The new GraphLayout.ApplySugiyamaLayout provides better results
            
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

        private float AvgPos(NodeModel node)
        {
            // This method is kept for backward compatibility
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
                        if (sourceNode.Intermediates != null && sourceNode.Intermediates.ContainsKey(node.ID))
                            sum += sourceNode.Intermediates[node.ID].Last().ScaledLocation.Y;
                        else
                            sum += PositionMap[source].ScaledLocation.Y;

                        count++;
                    }

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

        private void Spring(Graph graph)
        {
            float total_kinetic_energy = 40; // running sum of total kinetic energy over all particles

            float spring_const = -1;
            float damping = 0.8f; // between 0 and 1
            float timestep = 0.1f;

            do
            {
                // for each node
                foreach (var rank in graph.Ranks)
                {
                    float minY = 0;
                    float maxY = 1;

                    foreach (var node in rank.Column)
                    {
                        float forceY = 0; // running sum of total force on this particular node


                        // for each other node apply Coulomb repulsion 1/distance
                        foreach (var other_node in rank.Column.Where(n => n != node))
                        {
                            float distance = GetDistanceY(other_node, node);
                            if (distance > 0)
                                forceY += 1.0f / distance;
                            else
                                forceY += .1f;
                        }

                        // for each spring connected to this node net-force + Hooke_attraction( this_node, spring )

                        if (node.EdgesOut != null)
                            foreach (var destId in node.EdgesOut)
                                if (PositionMap.ContainsKey(destId))
                                {
                                    if (node.Intermediates != null && node.Intermediates.ContainsKey(destId))
                                        forceY += -spring_const * GetDistanceY(node.Intermediates[destId][0], node);
                                    else
                                        forceY += -spring_const * GetDistanceY(PositionMap[destId], node);
                                }

                        if (node.EdgesIn != null)
                            foreach (var source in node.EdgesIn)
                                if (PositionMap.ContainsKey(source))
                                {
                                    var sourceNode = PositionMap[source];
                                    if (sourceNode.Intermediates != null && sourceNode.Intermediates.ContainsKey(node.ID))
                                        forceY += -spring_const * GetDistanceY(sourceNode.Intermediates[node.ID].Last(), node);
                                    else
                                        forceY += -spring_const * GetDistanceY(PositionMap[source], node);
                                }

                        // should only be attached to intermediate nodes
                        if (node.Adjacents != null)
                            foreach (var adj in node.Adjacents)
                                forceY += -spring_const * GetDistanceY(adj, node);

                        // without damping, it moves forever
                        node.VelocityY = (node.VelocityY + timestep * forceY) * damping;
                        node.ScaledLocation.Y += timestep * node.VelocityY;

                        if (node.ScaledLocation.Y < minY)
                            minY = node.ScaledLocation.Y;
                        if (node.ScaledLocation.Y > maxY)
                            maxY = node.ScaledLocation.Y;

                        total_kinetic_energy--; // += node.Value * (float)Math.Pow(node.VelocityY, 2); // node.value is mass
                    }


                    // scale positions of nodes back
                    float range = maxY - minY;

                    if (range > 1)
                        foreach (var node in rank.Column)
                            node.ScaledLocation.Y /= range;
                }

            } while (total_kinetic_energy > 0); // the simulation has stopped moving

        }

        private float GetDistanceY(NodeModel other_node, NodeModel node)
        {
            return Math.Abs(other_node.ScaledLocation.Y - node.ScaledLocation.Y);
        }

        private void MinDistance(Graph graph)
        {
            // moves nodes with-in rank closer to their adjacent nodes without changing order in rank
            try
            {
                foreach (Rank rank in graph.Ranks)
                {
                    var nodes = rank.Column;

                    // divide the min space allotted for column by the number of nodes in it, *2 for top/bottom of node
                    float minHeightSpace = graph.ScaledHeight * (1.0f - GraphFillSpace) / ((float) nodes.Count * 2);
               
                    // foreach node average y pos form all connected edges
                    for (int x = 0; x < nodes.Count; x++)
                    {
                        var node = nodes[x];

       
                        float lowerbound = graph.ScaledOffset;
                        if (x > 0)
                        {
                            var prevNode = nodes[x - 1];
                            lowerbound = prevNode.ScaledLocation.Y + (prevNode.ScaledSize / 2);
                        }
                        lowerbound += minHeightSpace;

                        float upperbound = graph.ScaledOffset + graph.ScaledHeight;
                        if (x < nodes.Count - 1)
                        {
                            var nextNode = nodes[x + 1];
                            upperbound = nextNode.ScaledLocation.Y - (nextNode.ScaledSize / 2);
                        }
                        upperbound -= minHeightSpace;

                        //Debug.Assert(lowerbound <= upperbound);
                        if (lowerbound >= upperbound)
                        {
                            // usually if this happens they're very close
                            XRay.LogError("lower bound greater than upper in layout. pos: {0}, nodeID: {1}, lower: {2}, upper: {3}, minheight: {4}", x, node.ID, lowerbound, upperbound, minHeightSpace);
                            //continue;
                        }


                        float optimalY = AvgPos(node);
                        float halfSize = node.ScaledSize / 2;

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

            float spacePerRow = graph.ScaledHeight / (float)nodes.Count;
            float yOffset = spacePerRow / 2.0f;

            foreach (var node in nodes)
            {
                node.ScaledLocation.X = xOffset;
                node.ScaledLocation.Y = graph.ScaledOffset + yOffset;

                yOffset += spacePerRow;
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
        private void LayoutGraph(Dictionary<int, NodeModel> graph, NodeModel node, int minRank, List<int> parents)
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
                    {
                        var target = PositionMap[destId];

                        LayoutGraph(graph, target, node.Rank.Value + 1, parents.ToList());//, debugLog);
                    }
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

        private void AddDependencyNodes()
        {
            foreach (var n in Model.NodeModels)
            {
                n.Intermediates = null;
                n.DependencyChainIn = null;
                n.DependencyChainOut = null;
            }

            foreach (var n in Model.InterDependencies.Values)
            {
                CenterMap.Add(n.ID);
                PositionMap[n.ID] = n;

                var find = Model.InterDependencies.Keys.ToList();
                find.Remove(n.ID);

                FindChainTo(n, find);
                //FindIntermediatesFrom(n); // creates too many interdependent links, gets confusing
            }

            foreach (var n in Model.NodeModels)
            {
                if (n.DependencyChainIn != null)
                    n.EdgesIn = n.DependencyChainIn.Keys.ToArray();

                if (n.DependencyChainOut != null)
                    n.EdgesOut = n.DependencyChainOut.Keys.ToArray();
            }
        }

        public bool FindChainTo(NodeModel n, List<int> find, HashSet<int> traversed = null)
        {
            if (traversed == null)
                traversed = new HashSet<int>();

            if (traversed.Contains(n.ID))
                return false;

            traversed.Add(n.ID);

            bool pathFound = false;

            if (n.XNode.Dependencies == null)
                return false;

            foreach (var d in n.XNode.Dependencies)
            {
                var sub = Model.NodeModels[d];
                bool addLink = false;

                if (find.Contains(d))
                {
                    addLink = true;
                    find.Remove(d);
                }

                if (find.Count > 0 && FindChainTo(sub, find, traversed))
                    addLink = true;

                if (addLink)
                {
                    PositionMap[sub.ID] = sub;

                    n.AddIntermediateDependency(sub);
                    pathFound = true;
                }
            }

            return pathFound;
        }

        public bool FindChainFrom(NodeModel n, List<int> find, HashSet<int> traversed = null)
        {
            if (traversed == null)
                traversed = new HashSet<int>();

            if (traversed.Contains(n.ID))
                return false;

            traversed.Add(n.ID);

            bool pathFound = false;

            if (n.XNode.Independencies == null)
                return false;

            foreach (var d in n.XNode.Independencies)
            {
                var parent = Model.NodeModels[d];
                bool addLink = false;

                if (find.Contains(d))
                {
                    addLink = true;
                    find.Remove(d);
                }

                if (find.Count > 0 && FindChainFrom(parent, find, traversed))
                    addLink = true;

                if (addLink)
                {
                    PositionMap[parent.ID] = parent;

                    parent.AddIntermediateDependency(n);
                    pathFound = true;
                }
            }

            return pathFound;
        }
    }

    public class Graph
    {
        public Rank[] Ranks;
        public long Weight;

        public float ScaledHeight;
        public float ScaledOffset;
        
        // Added to support the new layout algorithm
        public Dictionary<int, NodeModel> NodeMap;

        public IEnumerable<NodeModel> Nodes()
        {
            foreach (var r in Ranks)
                foreach (var n in r.Column)
                    yield return n;
        }
    }

    public class Rank
    {
        internal List<NodeModel> Column = new List<NodeModel>();
    }
}
