using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Threading;

namespace XLibrary
{
    public partial class ViewModel
    {
        int ThreadOrder = 0;
        public Dictionary<int, Threadline> Threadlines = new Dictionary<int, Threadline>();
        public List<ThreadlineNode> ThreadlineNodes = new List<ThreadlineNode>();
        public HashSet<int> AddedNodes = new HashSet<int>();

        public NodeModel CurrentThreadlineZoom;
        public bool Paused;


        public void DrawTheadline()
        {
            if (DoRevalue)
            {
                RecalcCover(InternalRoot);
                RecalcCover(ExternalRoot);

                DoRevalue = false;
                RevalueCount++;

                DoResize = true;
            }

            // set what nodes are allowed in the threadline based on the current root
            if (CurrentThreadlineZoom != CurrentRoot)
            {
                CenterMap.Clear();
                CenterMap.Add(CurrentRoot.ID);

                Utilities.RecurseTree<NodeModel>(
                    tree: CurrentRoot.Nodes,
                    evaluate: n => CenterMap.Add(n.ID),
                    recurse: n => n.Nodes);

                CurrentThreadlineZoom = CurrentRoot;
            }

            long currentTick = XRay.Watch.ElapsedTicks;

            if(!Paused)
                CalcThreadline(currentTick);

            LayoutThreadlines(currentTick);
        }

        List<int> RemoveLines = new List<int>();

        private void CalcThreadline(long currentTick)
        {
            // clear thread map so order of items is kept and we dont get threads switching around
            RemoveLines.Clear();
            foreach (var timeline in Threadlines.Values)
            {
                timeline.Sequence.Clear();
                timeline.DepthSet.Clear();

                if (!timeline.IsAlive)
                    RemoveLines.Add(timeline.ThreadID);
            }

            foreach(var id in RemoveLines)
                Threadlines.Remove(id);

            AddedNodes.Clear();

            foreach (var flow in XRay.FlowMap)
            {
                if (ShowThreads != null && !ShowThreads.Contains(flow.ThreadID))
                    continue;

                if (ShowThreads == null && !flow.IsAlive)
                    continue;

                long startTick = 0;
                int startDepth = int.MaxValue;

                foreach(var item in flow.EnumerateThreadline())
                {
                    if (startTick == 0)
                        startTick = item.StartTick;

                    // we should be reading into the past, if the next item is 
                    // in the future, then we are reading space that the app as logged 
                    // since this process was started (its nice cause we avoid locks on the timeline
                    if (item.StartTick > startTick)
                        break;

                    if(AddToTimeline(flow, item))
                        if (item.Depth < startDepth)
                            startDepth = item.Depth;
                }

                // add stack items that have been pushed off threadline (functions that haven't exited yet)
                for (int i = startDepth - 1; i >= 0; i--)
                {
                    var item = flow.Stack[i];

                    if (item != null)
                        AddToTimeline(flow, item);
                }
            }

            // remove empty threads
            var removeTimelines = Threadlines.Values.Where(t => t.Sequence.Count == 0).ToArray();
            foreach (var timeline in removeTimelines)
                Threadlines.Remove(timeline.ThreadID);
        }

        private bool AddToTimeline(ThreadFlow flow, StackItem item)
        {
            // do stuff with item
            Threadline timeline;
            if (!Threadlines.TryGetValue(flow.ThreadID, out timeline))
            {
                timeline = new Threadline(flow, ThreadOrder++);
                Threadlines[flow.ThreadID] = timeline;
            }

            timeline.IsAlive = flow.IsAlive; // update

            var node = NodeModels[item.NodeID];

            if (node.Show &&
                (CenterMap.Contains(node.ID) ||
                 (ShowOutside && !node.XNode.External) ||
                 (ShowExternal && node.XNode.External)))
            {
                timeline.Sequence.Add(item);

                if (item.Depth > timeline.Deepest)
                    timeline.Deepest = item.Depth;

                timeline.DepthSet.Add(item.Depth);

                return true;
            }
            else
                return false;
        }

        private void LayoutThreadlines(long currentTick)
        {
            PositionMap.Clear();
            
            ThreadlineNodes.Clear();

            // go through each thread object and position nodes
            float xOffset = 0;
            float nodeHeight = 18;
            float nodeWidth = 18;

            foreach (var timeline in Threadlines.Values.OrderBy(t => t.ThreadID))
            {
                // do depth correction so we dont have empty columns
                int fixedDepth = 0;
                timeline.FixedDepths = new int[timeline.Deepest + 1];
                foreach (var depth in timeline.DepthSet)
                {
                    timeline.FixedDepths[depth] = fixedDepth;
                    fixedDepth++;
                }

                timeline.NodeDepths = new ThreadlineNode[timeline.Deepest + 2]; // an extra level to prevent outside bounds exc when checking lower level

                float yPos = ScreenSize.Height - nodeHeight - 16;
          
                float colWidth = timeline.Deepest * nodeWidth + 100;

                string label = "#" + timeline.ThreadID.ToString() + ": " + timeline.Name; 
                float x = ScreenOffset.X + xOffset + 2;
                float y = ScreenOffset.Y + yPos + nodeHeight + 2;
                Renderer.DrawString(label, TextFont, timeline.IsAlive ? Color.Black : Color.Gray, x, y, colWidth, 18);

                foreach (var item in timeline.Sequence)
                {
                    var node = NodeModels[item.NodeID];

                    PositionMap[node.ID] = node;

                    int depth = timeline.FixedDepths[item.Depth];
                    float xPos = xOffset + nodeWidth * depth;

                    var area = new RectangleF(ScreenOffset.X + xPos, ScreenOffset.Y + yPos, nodeHeight, nodeWidth);

                    // only light up the latest node
                    if (!AddedNodes.Contains(node.ID))
                    {
                        node.RoomForLabel = true;
                        node.SetArea(area);
                    }

                    // extend this node down to the previous nodes (future node) depth
                    bool foundPrev = false;
                    for (int i = depth + 1; i < timeline.NodeDepths.Length; i++)
                    {
                        if (!foundPrev && timeline.NodeDepths[i] != null)
                        {
                            area.Height = timeline.NodeDepths[i].Area.Bottom - area.Top;
                            foundPrev = true;
                        }


                        timeline.NodeDepths[i] = null;
                    }

                    bool showHit = false;
                    if (item.EndTick == 0 || Utilities.TicksToSeconds(currentTick - item.StartTick) < 1.0) // of started in last second
                        showHit = true;

                    float labelX = ScreenOffset.X + xPos + nodeWidth + 3;
                    float labelWidth = colWidth - nodeWidth * (depth + 1) - 3;
                    var labelArea = new RectangleF(labelX, ScreenOffset.Y + yPos + 1, labelWidth, nodeHeight);
                    var entry = new ThreadlineNode() { Node = node, Area = area, LabelArea = labelArea, ShowHit = showHit };

                    if (timeline.NodeDepths[depth] == null)
                        timeline.NodeDepths[depth] = entry;

                    ThreadlineNodes.Add(entry);

                    yPos -= nodeHeight;
                }

                xOffset += colWidth;
            }
        }
    }

    public class Threadline
    {
        public int ThreadID;
        public string Name;
        public bool IsAlive;

        public int Order;
        public int Deepest; // dont reset, if we do columns start moving around whenever it changes

        public List<StackItem> Sequence = new List<StackItem>();
        public ThreadlineNode[] NodeDepths;
        public SortedSet<int> DepthSet = new SortedSet<int>();
        public int[] FixedDepths;


        public Threadline(ThreadFlow flow, int order)
        {
            ThreadID = flow.ThreadID;
            Order = order;
            Name = flow.Name;
            IsAlive = flow.IsAlive;
        }
    }

    public class ThreadlineNode
    {
        public NodeModel Node;
        public RectangleF Area;
        public RectangleF LabelArea;
        public bool ShowHit;
    }
}
