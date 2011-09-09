using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace XLibrary
{
    public static class XRay
    {
        static MainForm MainForm;

        static Thread Gui;

        internal static XNodeIn RootNode;
        internal static XNodeIn[] Nodes;

        static int FunctionCount;

        internal static bool CoverChange;
        internal static bool CallChange;
        internal static bool InstanceChange;

        internal static BitArray CoveredFunctions;
        internal static bool ShowAllCalls;

        internal const int HitFrames = 15;
        internal const int ShowTicks = HitFrames - 1; // first index

        internal static bool InstanceTracking = false; // must be compiled in, can be ignored later
       
        internal static bool ThreadTracking = false; // can be turned off library side
        
        internal static bool FlowTracking = false; // must be compiled in, can be ignored later
        internal static bool ClassTracking = false;
        internal const int MaxStack = 512;
        internal static SharedDictionary<ThreadFlow> FlowMap = new SharedDictionary<ThreadFlow>(100);
        internal static SharedDictionary<FunctionCall> CallMap = new SharedDictionary<FunctionCall>(1000);
        internal static SharedDictionary<FunctionCall> ClassCallMap = new SharedDictionary<FunctionCall>(1000);

        internal static bool TrackCallGraph = true; // turning this off would save mem/cpu - todo test impact

        internal static string DatPath;
        internal static bool CallLogging;
        internal static Dictionary<string, bool> ErrorMap = new Dictionary<string, bool>();

        static bool InitComplete;

        static Stopwatch Watch = new Stopwatch();

        static uint CurrentSequence;


        public static void TestInit(string path)
        {
            if (LoadNodeMap(path))
            {
                MainForm = new MainForm();
                MainForm.Show();
            }
        }           
            
        public static void Init(bool trackFlow, bool trackInstances)
        {
            if (InitComplete)
            {
                LogError("Init already called");
                return;
            }

            Init2(trackFlow, trackInstances);
        }

        public static void Init2(bool trackFlow, bool trackInstances)
        {
            try
            {
                Watch.Start();

                // read compiled settings
                if (trackFlow)
                {
                    FlowTracking = true;
                    ThreadTracking = true;
                }
                InstanceTracking = trackInstances;

                // data file with node info should be along side ext
                string path = Path.Combine(Application.StartupPath, "XRay.dat");

                LoadNodeMap(path);

                // init tracking structures
                CoveredFunctions = new BitArray(FunctionCount);

                InitComplete = true;

                // boot up the xray gui
                if (Gui != null)
                    return;

                Gui = new Thread(() =>
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    MainForm = new MainForm();
                    Application.Run(MainForm);
                });
                Gui.SetApartmentState(ApartmentState.STA);
                Gui.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static bool LoadNodeMap(string path)
        {
            RootNode = null;
            DatPath = path;
            FunctionCount = 0;
            Dictionary<int, XNodeIn> map = new Dictionary<int, XNodeIn>();
            
            try
            {
                using (FileStream stream = new FileStream(DatPath, FileMode.Open))
                {
                    while (stream.Position < stream.Length)
                    {
                        XNodeIn node = XNodeIn.Read(stream);
                        map[node.ID] = node;

                        // first node read is the root
                        if (RootNode == null)
                            RootNode = node;

                        else if (map.ContainsKey(node.ParentID))
                        {
                            node.Parent = map[node.ParentID];
                            node.Parent.Nodes.Add(node);
                        }
                        else
                            LogError("Could not find parent {0} or {1}", node.Parent, node.Name);

                        if (node.ID > FunctionCount)
                            FunctionCount = node.ID;
                    }
                }

                FunctionCount++; // so id can be accessed in 0 based index

                Nodes = new XNodeIn[FunctionCount];
                foreach (var node in map.Values)
                    Nodes[node.ID] = node;

                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("XRay data file not found: " + ex.Message); // would this even work? test
            }

            return false;
        }

        public static void MethodEnter(int nodeID)
        {
            NodeHit(nodeID);
        }

        private static XNodeIn NodeHit(int nodeID)
        {
            /*if (!InitComplete)
            {

                lock (Thread.CurrentThread)
                {
                    if (!InitComplete)
                        Init2(true, false);

                    InitComplete = true;
                }
            }*/

            if (Nodes == null) // static classes init'ing in the entry points class can cause this
                return null;

            if (nodeID >= Nodes.Length)
            {
                LogError("Method not in node array Func {0}, Array Size {1}\r\n", nodeID, Nodes.Length);
                return null;
            }

            XNodeIn node = Nodes[nodeID];

            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null)
                return null;

            int thread = 0;
            if (ThreadTracking)
                thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                LogError("Thread {0}, Func {1}, Enter\r\n", thread, nodeID);

            // mark covered, should probably check if show covered is checked
            if (!CoveredFunctions[nodeID])
            {
                node.HitSequence = CurrentSequence++;

                CoverChange = true;

                XNode check = node;
                while (check != null)
                {
                    CoveredFunctions[check.ID] = true;
                    check = check.Parent;
                }
                // clear cover change on paint
            }

            node.FunctionHit = ShowTicks;

            if (node.ObjType != XObjType.Method)
                LogError("{0} {1} Hit", node.ObjType, nodeID);

            if (ThreadTracking && thread != 0)
            {
                if (node.LastCallingThread != 0 && node.LastCallingThread != thread)
                    node.ConflictHit = ShowTicks;

                node.LastCallingThread = thread;
            }

            if (FlowTracking)
                TrackFunctionCall(nodeID, node, thread);

            return node;
        }

        private static void TrackFunctionCall(int nodeID, XNodeIn node, int thread)
        {
            // check that thread is in map
            ThreadFlow flow;
            if (!FlowMap.TryGetValue(thread, out flow))
            {
                flow = new ThreadFlow() { ThreadID = thread };
                FlowMap.Add(thread, flow);
            }

            if(node.ObjType == XObjType.Method)
                node.StillInside++;

            // if the first entry, return here
            if (flow.Pos == -1)
            {
                flow.Pos = 0;
                flow.Stack[0] = new StackItem() { NodeID = nodeID, Ticks = Watch.ElapsedTicks };
                node.EntryPoint++;
                return;
            }

            // if exceeded tracking max return
            if (flow.Pos >= flow.Stack.Length)
                return;

            // set the source, and put the dest in stack
            int source = flow.Stack[flow.Pos].NodeID;

            // the ids are small and auto-inc based on the # of funcs
            // just hashing together is not unique enough, and many conflicts because the numbers 
            // are small and close together. so expand the number to a larger domain.
            // also ensure s->d != d->s
            int hash = source * FunctionCount + nodeID;

            FunctionCall call;
            if (!CallMap.TryGetValue(hash, out call))
            {
                call = new FunctionCall() { Source = source, Destination = nodeID };
                CallMap.Add(hash, call);

                // add link to node that its been called
                if (TrackCallGraph)
                {
                    node.AddCallIn(source, call);

                    var srcNode = Nodes[source];
                    if (srcNode == null)
                        return;

                    srcNode.AddCallOut(nodeID, call);
                }

                CallChange = true;
            }

            if (source != call.Source || nodeID != call.Destination)
                LogError("Call mismatch  {0}->{1} != {2}->{3}\r\n", source, nodeID, call.Source, call.Destination);

            call.Hit = ShowTicks;
            call.TotalHits++;

            if (node.ObjType == XObjType.Method)
            {
                call.StillInside++;

                flow.Pos++;
                flow.Stack[flow.Pos] = new StackItem() { NodeID = nodeID, Call = call, Ticks = Watch.ElapsedTicks };
            }

            if(ClassTracking)
                TrackClassCall(nodeID, node, source);
        }

        public static void TrackClassCall(int nodeID, XNodeIn node, int source)
        {
            var srcNode = Nodes[source];
            if (srcNode == null)
                return;

            var sourceClass = srcNode.Parent as XNodeIn;
            var destClass = node.Parent as XNodeIn;

            if (sourceClass == destClass)
                return;

            if (destClass.ObjType != XObjType.Class || sourceClass.ObjType != XObjType.Class)
            {
                LogError("parent not class type, {0} and {1}", destClass.ObjType, sourceClass.ObjType);
                return;
            }

            int hash = sourceClass.ID * FunctionCount + destClass.ID;

            FunctionCall call;
            if (!ClassCallMap.TryGetValue(hash, out call))
            {
                LogError("Adding to class map {0} -> {1} with hash {2}", sourceClass.ID, destClass.ID, hash);

                call = new FunctionCall() { Source = sourceClass.ID, Destination = destClass.ID };
                ClassCallMap.Add(hash, call);

                destClass.AddCallIn(sourceClass.ID, call);

                sourceClass.AddCallOut(destClass.ID, call);
            }

            call.Hit = ShowTicks;
            call.TotalHits++;
        }

        public static void MethodExit(int nodeID)
        {
            if (!ThreadTracking || !FlowTracking || Nodes == null || Nodes[nodeID] == null)
                return;

            int thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                LogError("Thread {0}, Func {1}, Exit\r\n", thread, nodeID);


            // only should be called if flow tracking is enabled

            // move back through stack array and find function
            // if a function threw then a lot of functions will be skipped
            long ticks = Watch.ElapsedTicks;

            ThreadFlow flow;
            if (FlowMap.TryGetValue(thread, out flow))
                // work backwards from position on stack to position of the exit
                for (int i = flow.Pos; i >= 0; i--)
                    if (flow.Stack[i].NodeID == nodeID)
                    {
                        int exit = flow.Pos;
                        flow.Pos = i - 1; // set current position asap

                        // mark functions called as well as this function as not insde
                        for (int x = i; x <= exit; x++)
                        {
                            StackItem exited = flow.Stack[x];

                            Nodes[exited.NodeID].StillInside--;

                            if (exited.Call == null)
                                continue;

                            exited.Call.StillInside--;
                            exited.Call.TotalCallTime += ticks - exited.Ticks;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += ticks - exited.Ticks;
                        }

                        if (i == 0)
                            Nodes[nodeID].EntryPoint--;

                        break;
                    }
    
            // need a way to freeze app and debug these structures, perfect case for xray live reflection interfaces
            // solves the problem of constant output debug, can surf structure live and manip variables
        }

        public static void MethodCatch(int nodeID)
        {
            if (!ThreadTracking || !FlowTracking || Nodes == null || Nodes[nodeID] == null)
                return;

            int thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                LogError("Thread {0}, Func {1}, Catch\r\n", thread, nodeID);

            long ticks = Watch.ElapsedTicks;

            ThreadFlow flow;
            if (FlowMap.TryGetValue(thread, out flow))
                // work backwards from position on stack to position of the catch
                for (int i = flow.Pos; i >= 0; i--)
                    if (flow.Stack[i].NodeID == nodeID)
                    {
                        int exit = flow.Pos;
                        flow.Pos = i;

                        // mark functions called by this as not inside any longer
                        for (int x = i + 1; x <= exit; x++)
                        {
                            StackItem exited = flow.Stack[x];

                            Nodes[exited.NodeID].StillInside--;
                            exited.Call.TotalCallTime += ticks - exited.Ticks;

                            Nodes[exited.NodeID].ExceptionHit = ShowTicks;

                            if (exited.Call != null)
                                exited.Call.StillInside--;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += ticks - exited.Ticks;
                        }

                        break;
                    }
        }

        public static void Constructed(int index, Object obj)
        {
            XNodeIn node = Nodes[index];

            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null)
                return;

            var record = node.Record;
            if (record == null)
            {
                record = new InstanceRecord();
                node.Record = record;
                record.InstanceType = (obj is System.Type) ? obj as System.Type : obj.GetType();
            }

            if (obj is System.Type)
            {
                record.StaticCreated++;
                InstanceChange = true;
            }
            else
            {
                record.Created++;

                if (record.Active.Count == 0)
                    InstanceChange = true;

                record.Active.Add(new WeakReference(obj, false));
            }
        }

        public static void Deconstructed(int index, Object obj)
        {
            XNodeIn node = Nodes[index];

            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null || node.Record == null)
            {
                LogError("Deconstructed instance not found of type " + obj.GetType().ToString());
                return;
            }

            var record = node.Record;

            record.Deleted++;

            // iterate through objects and delete null target
            // cant use hash code to ident object, because some classes override it and do crazy things like construct themselves again causing deadlock

            var removeRef = record.Active.FirstOrDefault(a => a.Target == null);

            if (removeRef != null)
            {
                record.Active.Remove(removeRef);

                if (record.Active.Count == 0)
                    InstanceChange = true;
            }
            else
                LogError("Deleted instance wasnt logged of type " + obj.GetType().ToString()); 
        }

        public static void LoadField(int nodeID)
        {
            var node = NodeHit(nodeID);
            if (node != null)
                node.LastFieldOp = FieldOp.Get;
        }

        public static void SetField(int nodeID)
        {
            var node = NodeHit(nodeID);
            if (node != null)
                node.LastFieldOp = FieldOp.Set;
        }

        static internal void LogError(string text, params object[] args)
        {
            ErrorMap[string.Format(text, args)] = true;
        }
    }

    internal class InstanceRecord
    {
        public Type InstanceType;
        public long Created;
        public long StaticCreated;
        public long Deleted;
        public List<WeakReference> Active = new List<WeakReference>();
    }

    class ThreadFlow
    {
        internal int ThreadID;

        internal int Pos = -1; // current position on the stack
        internal StackItem[] Stack = new StackItem[XRay.MaxStack];
    }

    class StackItem
    {
        internal int NodeID;
        internal FunctionCall Call;
        internal long Ticks;
    }

    class FunctionCall
    {
        internal int Source;
        internal int Destination;

        internal int Hit;

        internal const int DashSize = 3;
        internal const int DashSpace = 6;
        internal int DashOffset;

        internal int StillInside;

        internal int TotalHits;
        internal long TotalCallTime;
        internal long TotalTimeOutsideDest;

        internal List<XNodeIn> Intermediates; // only draw edge if this is null or empty


        internal long TotalTimeInsideDest
        {
            get
            {
                // if still inside called function return 0 for this
                if (TotalTimeOutsideDest > TotalCallTime)
                    return 0;

                return TotalCallTime - TotalTimeOutsideDest;
            }
        }
    }

    // this is a dictionary where values can be added, for fast look up dynamically without needing a lock
    // one thread needs to be able to write values fast
    // while another threads need to be able to read values fast
    class SharedDictionary<T>  : IEnumerable<T>
        where T : class
    {
        internal int Length;
        internal T[] Values;

        Dictionary<int, int> Map = new Dictionary<int, int>();

        internal SharedDictionary(int keyMax)
        {
            Values = new T[keyMax];
        }

        internal bool Contains(int hash)
        {
            return Map.ContainsKey(hash);
        }

        internal bool TryGetValue(int hash, out T call)
        {
            int index;
            if (Map.TryGetValue(hash, out index))
            {
                call = Values[index];
                return true;
            }

            call = null;
            return false;
        }

        internal void Add(int hash, T call)
        {
            // locking isnt so bad because once app is running, add won't be called so much
            lock (this)
            {
                if (Length >= Values.Length)
                {
                    T[] resized = new T[Values.Length * 2];
                    Values.CopyTo(resized, 0);
                    Values = resized;
                }

                int index = Length;
                Map[hash] = index;
                Values[index] = call;

                Length++;
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return Values[i];
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return Values[i];
        }

        #endregion
    }

}
