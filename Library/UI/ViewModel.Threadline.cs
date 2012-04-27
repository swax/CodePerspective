using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace XLibrary
{
    public partial class ViewModel
    {
        int ThreadOrder = 0;
        public Dictionary<int, Threadline> Threadlines = new Dictionary<int, Threadline>();
        public List<ThreadlineNode> ThreadlineNodes = new List<ThreadlineNode>();
        public HashSet<int> AddedNodes = new HashSet<int>();
        public List<StackItem> UnfinishedItems = new List<StackItem>();

        public void DrawTheadline(Graphics buffer)
        {
            PositionMap.Clear();

            // clear thread map so order of items is kept and we dont get threads switching around
            foreach (var timeline in Threadlines.Values)
                timeline.Sequence.Clear();

            ThreadlineNodes.Clear();
            AddedNodes.Clear();
            UnfinishedItems.Clear();

            // iterate back from current pos timeline until back to start, or start time is newer than start pos start time
            int startPos= XRay.ThreadlinePos; // start with the newest position
            if (startPos == -1)// havent started, or disabled
                return;

            int i = startPos;
            long startTick = 0;

            long currentTick = XRay.Watch.ElapsedTicks;

            while (true)
            {
                // iterate to next node
                var item = XRay.Threadline[i];
                if (item == null)
                    break;

                if (startTick == 0)
                    startTick = item.StartTick;

                // we should be reading into the past, if the next item is 
                // in the future, then we are reading space that the app as logged 
                // since this process was started (its nice cause we avoid locks on the timeline
                if (item.StartTick > startTick)
                    break;


                // do stuff with item
                Threadline timeline = null;
                if (!Threadlines.ContainsKey(item.ThreadID))
                {
                    timeline = new Threadline(item.ThreadID, ThreadOrder++);
                    Threadlines[item.ThreadID] = timeline;
                }
                else
                    timeline = Threadlines[item.ThreadID];

                if (item.EndTick == 0)
                    UnfinishedItems.Add(item);
                else
                    timeline.Sequence.Add(item);

                if (item.Depth > timeline.Deepest)
                    timeline.Deepest = item.Depth;

                // iterate to previous item in time
                i--;
                if (i < 0)
                    i = XRay.Threadline.Length - 1;

                if (i == startPos)
                    break;
            }

            // add unfinshed items, we do this separetly because they are out of time order in xray
            foreach (var item in UnfinishedItems.OrderByDescending(ui => ui.StartTick))
                Threadlines[item.ThreadID].Sequence.Add(item);

            // remove empty threads
            var removeTimelines = Threadlines.Values.Where(t => t.Sequence.Count == 0).ToArray();
            foreach (var timeline in removeTimelines)
                Threadlines.Remove(timeline.ThreadID);

            // go through each thread object and position nodes
            float xOffset = 0;
            float nodeHeight = 18;
            float nodeWidth = 18;

            foreach(var timeline in Threadlines.Values.OrderBy(t => t.ThreadID))
            {
                timeline.NodeDepths = new ThreadlineNode[timeline.Deepest + 2]; // an extra level to prevent outside bounds exc when checking lower level

                float yPos = ScreenSize.Height - nodeHeight - 16;

                buffer.DrawString("Thread " + timeline.ThreadID.ToString(), TextFont, new SolidBrush(Color.Black), new PointF(ScreenOffset.X + xOffset + 2, ScreenOffset.Y + yPos + nodeHeight + 2));

                float colWidth = timeline.Deepest * nodeWidth + 100;

                foreach(var item in timeline.Sequence)
                {
                    float xPos = xOffset + nodeWidth * item.Depth;

                    // draw
                    var node = XRay.Nodes[item.NodeID];
                    PositionMap[node.ID] = node;

                    if (node.External && !ShowExternal)
                        continue;

                    var area = new RectangleF(ScreenOffset.X + xPos, ScreenOffset.Y + yPos, nodeHeight, nodeWidth);

                    // only light up the latest node
                    if (!AddedNodes.Contains(node.ID))
                    {
                        node.RoomForLabel = true;
                        node.SetArea(area);
                    }

                    // extend this node down to the previous nodes (future node) depth
                    bool foundPrev = false;
                    for (i = item.Depth + 1; i < timeline.NodeDepths.Length; i++)
                    {
                        if (!foundPrev && timeline.NodeDepths[i] != null)
                        {
                            area.Height = timeline.NodeDepths[i].Area.Bottom - area.Top;
                            foundPrev = true;
                        }

                        timeline.NodeDepths[i] = null;
                    }

                    bool showHit = false;
                    if (item.EndTick == 0 || Xtensions.TicksToSeconds(currentTick - item.StartTick) < 1.0) // of started in last second
                        showHit = true;

                    float labelX = ScreenOffset.X + xPos + nodeWidth + 3;
                    float labelWidth = colWidth - nodeWidth * (item.Depth + 1) - 3;
                    var labelArea = new RectangleF(labelX, ScreenOffset.Y + yPos + 1, labelWidth, nodeHeight);
                    var entry = new ThreadlineNode() { Node = node, Area = area, LabelArea = labelArea, ShowHit = showHit };

                    if(timeline.NodeDepths[item.Depth] == null)             
                        timeline.NodeDepths[item.Depth] = entry;

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
        public int Order;
        public int Deepest; // dont reset, if we do columns start moving around whenever it changes

        public List<StackItem> Sequence = new List<StackItem>();
        public ThreadlineNode[] NodeDepths;


        public Threadline(int threadID, int order)
        {
            ThreadID = threadID;
            Order = order;
        }
    }

    public class ThreadlineNode
    {
        public XNodeIn Node;
        public RectangleF Area;
        public RectangleF LabelArea;
        public bool ShowHit;
    }
}
