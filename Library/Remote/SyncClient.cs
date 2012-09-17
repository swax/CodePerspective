using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace XLibrary.Remote
{
    public class SyncClient
    {
        public XConnection Connection;

        bool DataToSend;

        public HashSet<int> FunctionHits = new HashSet<int>();
        public HashSet<int> ExceptionHits = new HashSet<int>();
        public HashSet<int> ConstructedHits = new HashSet<int>();
        public HashSet<int> DisposeHits = new HashSet<int>();

        public PairList NewCalls = new PairList();
        public HashSet<int> CallHits = new HashSet<int>();
        public HashSet<int> CallStats = new HashSet<int>();

        public PairList Inits = new PairList();

        public Dictionary<int, Tuple<string, bool>> NewThreads = new Dictionary<int, Tuple<string, bool>>();
        public Dictionary<int, bool> ThreadChanges = new Dictionary<int, bool>();
        public PairList NodeThreads = new PairList();
        public PairList CallThreads = new PairList();
        public Dictionary<int, PairList> Threadlines = new Dictionary<int, PairList>(); // history of the thread
        public Dictionary<int, PairList> ThreadStacks = new Dictionary<int, PairList>(); // current stack of thread, top level unchanged and pushed off history so we have to sync it
        const int MaxStackItemsPerThreadSent = 100;
        public Dictionary<int, int> NewStackItems = new Dictionary<int, int>();

        public Dictionary<int, InstanceModel> SelectedInstances = new Dictionary<int, InstanceModel>();

        const int SendStatsInterval = 4;
        int SendStatsCounter = 0;

        // settings
        Stopwatch SyncWatch = new Stopwatch();
        public int TargetFps;


        public SyncClient()
        {
            TargetFps = XRay.TargetFps;
            SyncWatch.Start();
        }

        public void SecondTimer()
        {
            SendStatsCounter++;
        }

        public void TrySync()
        {
            if (SyncWatch.ElapsedMilliseconds >= 1000 / TargetFps)
                if (DoSync())
                {
                    SyncWatch.Reset();
                    SyncWatch.Start();
                }
        }

        public bool DoSync()
        {
            if (Connection.State != TcpState.Connected)
                return false;

            // this is how we throttle the connection to available bandwidth
            if (!Connection.SendReady)
                return false;

            DataToSend = false;

            // save current set and create a new one so other threads dont get tripped up
            var packet = new SyncPacket();

            AddSet(ref FunctionHits, ref packet.FunctionHits);
            AddSet(ref ExceptionHits, ref packet.ExceptionHits);
            AddSet(ref ConstructedHits, ref packet.ConstructedHits);
            AddSet(ref DisposeHits, ref packet.DisposeHits);

            AddPairs(ref NewCalls, ref packet.NewCalls);
            AddSet(ref CallHits, ref packet.CallHits);

            if (packet.CallHits != null)
                foreach (var id in packet.CallHits)
                    CallStats.Add(id); // copy over to stats for bulk send

            if (SendStatsCounter > SendStatsInterval && CallStats.Count > 0)
            {
                packet.CallStats = new List<CallStat>();

                foreach (var hash in CallStats)
                    packet.CallStats.Add(new CallStat(XRay.CallMap[hash]));

                CallStats = new HashSet<int>();
                SendStatsCounter = 0;
                DataToSend = true;
            }

            AddPairs(ref Inits, ref packet.Inits);

            if (NewThreads.Count > 0)
            {
                packet.NewThreads = NewThreads;
                NewThreads = new Dictionary<int, Tuple<string, bool>>();
                DataToSend = true;
            }

            if (ThreadChanges.Count > 0)
            {
                packet.ThreadChanges = ThreadChanges;
                ThreadChanges = new Dictionary<int, bool>();
                DataToSend = true;
            }

            AddPairs(ref NodeThreads, ref packet.NodeThreads);
            AddPairs(ref CallThreads, ref packet.CallThreads);

            AddThreadlines(packet);

            // check that there's space in the send buffer to send state
            if (DataToSend)
            {
                Connection.SendPacket(packet);
                Connection.SyncCount++;
                return true;
            }

            return false;
        }

        void AddSet(ref HashSet<int> localSet, ref HashSet<int> packetSet)
        {
            if (localSet.Count > 0)
            {
                packetSet = localSet;
                localSet = new HashSet<int>();
                DataToSend = true;
            }
        }

        void AddPairs(ref PairList localPairs, ref PairList packetPairs)
        {
            if (localPairs.Count > 0)
            {
                packetPairs = localPairs;
                localPairs = new PairList();
                DataToSend = true;
            }
        }

        internal void AddThreadlines(SyncPacket packet)
        {
            foreach (var flow in XRay.FlowMap)
            {
                if (!NewStackItems.ContainsKey(flow.ThreadID) || NewStackItems[flow.ThreadID] == 0)
                    continue;

                PairList threadline;
                if (!Threadlines.TryGetValue(flow.ThreadID, out threadline))
                {
                    threadline = new PairList();
                    Threadlines[flow.ThreadID] = threadline;
                }

                int newItems = NewStackItems[flow.ThreadID];
                int minDepth = int.MaxValue;
                int sendCount = Math.Min(newItems, MaxStackItemsPerThreadSent);

                foreach (var item in flow.EnumerateThreadline(sendCount))
                {
                    if (item.Depth < minDepth)
                        minDepth = item.Depth;

                    threadline.Add(new Tuple<int, int>((item.Call == null) ? item.NodeID : item.Call.ID, item.Depth));
                }
                // set new items to 0, iterate new items on threadline add
                NewStackItems[flow.ThreadID] = 0;

                // send top level of stack, so remote node is in sync (these are often pushed off the threadline)
                PairList threadstack;
                if (!ThreadStacks.TryGetValue(flow.ThreadID, out threadstack))
                {
                    threadstack = new PairList();
                    ThreadStacks[flow.ThreadID] = threadstack;
                }
                for (int i = minDepth - 1; i >= 0; i--)
                {
                    var item = flow.Stack[i];
                    threadstack.Add(new Tuple<int, int>((item.Call == null) ? item.NodeID : item.Call.ID, item.Depth));
                }
            }

            if (Threadlines.Count > 0)
            {
                packet.Threadlines = Threadlines;
                Threadlines = new Dictionary<int, PairList>();
                DataToSend = true;
            }

            if (ThreadStacks.Count > 0)
            {
                packet.ThreadStacks = ThreadStacks;
                ThreadStacks = new Dictionary<int, PairList>();
                DataToSend = true;
            }
        }
    }
}
