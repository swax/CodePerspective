using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;


namespace XLibrary
{
    public static class XRay
    {
        static MainForm MainForm;

        internal static Dictionary<int, Thread> UIs = new Dictionary<int, Thread>();

        internal static XNodeIn RootNode;
        internal static XNodeIn[] Nodes;

        public static int FunctionCount;

        public static bool XRayEnabled = true;

        internal static bool CoverChange;
        internal static bool CallChange;
        internal static bool InstanceChange;

        internal static BitArray CoveredNodes;
        internal static bool ShowAllCalls;

        internal const int HitFrames = 30;
        internal const int ShowTicks = HitFrames - 1; // first index
        static System.Threading.Timer ResetTimer;

        internal static bool InstanceTracking = false; // must be compiled in, can be ignored later
       
        internal static bool ThreadTracking = false; // can be turned off library side
        
        internal static bool FlowTracking = false; // must be compiled in, can be ignored later
        internal static bool ClassTracking = false;
        internal const int MaxStack = 512;
        internal static SharedDictionary<ThreadFlow> FlowMap = new SharedDictionary<ThreadFlow>(100);
        internal static SharedDictionary<FunctionCall> CallMap = new SharedDictionary<FunctionCall>(1000);
        internal static SharedDictionary<FunctionCall> ClassCallMap = new SharedDictionary<FunctionCall>(1000);
        internal static SharedDictionary<FunctionCall> InitMap = new SharedDictionary<FunctionCall>(1000);

        internal static bool ThreadlineEnabled = true;

        internal static bool TrackCallGraph = true; // turning this off would save mem/cpu - todo test impact

        internal static string DatPath;
        //internal static bool CallLogging;
        internal static HashSet<int> ErrorDupes = new HashSet<int>();
        internal static List<string> ErrorLog = new List<string>();

        static bool InitComplete;

        public static Stopwatch Watch = new Stopwatch();

        static uint CurrentSequence;

        public static DateTime StartTime;
        //public static double BytesSent;

        public static Random RndGen = new Random();

        public static int DashOffset;


        // opens xray from the builder exe to analyze the dat
        public static void Analyze(string path)
        {
            if (LoadNodeMap(path))
            {
                MainForm = new MainForm();
                MainForm.Show();
            }
        }           
            
        // called from re-compiled app's entrypoint
        public static void Init(string datPath, bool showUiOnStart, bool trackFlow, bool trackInstances)
        {
            LogError("Entry point Init");

            if (InitComplete)
            {
                LogError("Init already called");
                return;
            }
            InitComplete = true;

            StartTime = DateTime.Now;

            Watch.Start();
              
            // read compiled settings
            if (trackFlow)
            {
                FlowTracking = true;
                ThreadTracking = true;
                ClassTracking = true;
            }
            InstanceTracking = trackInstances;

            try
            {
                // data file with node info should be along side ext
                LoadNodeMap(datPath);

                // init tracking structures
                CoveredNodes = new BitArray(FunctionCount);

                StartResetThread();

                StartIpcServer();

                if(showUiOnStart)
                    StartGui();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static void StartResetThread()
        {
            int period = 1000 / HitFrames;
            ResetTimer = new System.Threading.Timer(ResetFunctionHits, null, period, period);
        }

        static void  ResetFunctionHits(object state)
        {
            // save cpu when no UIs active
            if (UIs.Count == 0)
                return;

            foreach (var node in Nodes)
                node.DecrementHits();

            // reset
            if (FlowTracking)
            {
                // time out function calls
                ResetCallHits(CallMap);
                ResetCallHits(ClassCallMap);
            }
        }

        static void ResetCallHits(SharedDictionary<FunctionCall> callMap)
        {
            foreach (var call in callMap)
            {
                if (call == null || call.Hit <= 0)
                    continue;

                call.Hit--;

                //call.DashOffset -= FunctionCall.DashSize;
                //if (call.DashOffset < 0)
                //    call.DashOffset = FunctionCall.DashSpace;
            }
        }

        public static void StartGui()
        {
            //if (Gui != null)
            //    return;

            int id = RndGen.Next();

            var uiThread = new Thread(() =>
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    
                    MainForm = new MainForm();
                    Application.Run(MainForm);
                }
                catch (Exception ex)
                {
                    LogError("Gui Error: " + ex.Message);
                }

                UIs.Remove(id);
            });
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            UIs[id] = uiThread;
        }

        static IpcServerChannel XRayChannel;
        static IpcQuery XRayQuery;

        private static void StartIpcServer()
        {
            // url - ipc://xray_1/query
            try
            {
                string unique = RndGen.Next().ToString();
                string host = "xray_" + unique;
                string queryUri = "query";
                string url = "ipc://" + host + "/" + queryUri;

                XRayChannel = new IpcServerChannel(host);

                ChannelServices.RegisterChannel(XRayChannel, false);

                RemotingConfiguration.RegisterWellKnownServiceType(typeof(IpcQuery), queryUri, WellKnownObjectMode.Singleton);

                XRayQuery = new IpcQuery();
                RemotingServices.Marshal(XRayQuery, queryUri);
            }
            catch (Exception ex)
            {
                LogError("Error starting IPC server: " + ex.Message);
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
                var dependenciesFrom = new Dictionary<int, List<int>>();

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

                        // create converse dependency edges
                        if(node.Dependencies != null)
                            foreach (var to in node.Dependencies)
                            {
                                if (!dependenciesFrom.ContainsKey(to))
                                    dependenciesFrom[to] = new List<int>();

                                dependenciesFrom[to].Add(node.ID);
                            }
                    }
                }

                FunctionCount++; // so id can be accessed in 0 based index

                Nodes = new XNodeIn[FunctionCount];
                foreach (var node in map.Values)
                    Nodes[node.ID] = node;

                foreach (var from in dependenciesFrom.Keys)
                    Nodes[from].Independencies = dependenciesFrom[from].ToArray();


                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("XRay data file not found: " + ex.Message); // would this even work? test
            }

            return false;
        }

        /*public static double GetSimBps()
        {
            double secs = DateTime.Now.Subtract(StartTime).TotalSeconds;

            return BytesSent / secs;
        }*/

        public static void MethodEnter(int nodeID)
        {
            if (!XRayEnabled)
                return;
            
            //BytesSent += 4 + 4 + 4 + 1; // type, functionID, threadID, null
    
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
           
            if (node.ThreadIDs == null)
                node.ThreadIDs = new HashSet<int>();
            node.ThreadIDs.Add(thread);

            //if (CallLogging)
            //    LogError("Thread {0}, Func {1}, Enter\r\n", thread, nodeID);

            // mark covered
            if (!CoveredNodes[nodeID])
            {
                node.HitSequence = CurrentSequence++;

                CoverChange = true;

                Utilities.IterateParents<XNode>(
                    node, 
                    n => CoveredNodes[n.ID] = true, 
                    n => n.Parent);

                // clear cover change on paint
            }

            node.FunctionHit = ShowTicks;

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

            bool isMethod = (node.ObjType == XObjType.Method);

            if(isMethod)
                node.StillInside++;


            // if the first entry, return here
            if (flow.Pos == -1)
            {
                flow.AddStackItem(nodeID, null, Watch.ElapsedTicks, isMethod, ThreadlineEnabled);
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
                    node.AddFunctionCall(ref node.CalledIn, source, call);

                    var srcNode = Nodes[source];
                    if (srcNode == null)
                        return;

                    srcNode.AddFunctionCall(ref srcNode.CallsOut, nodeID, call);
                }

                CallChange = true;
            }

            if (source != call.Source || nodeID != call.Destination)
                LogError("Call mismatch  {0}->{1} != {2}->{3}\r\n", source, nodeID, call.Source, call.Destination);

            call.ThreadIDs.Add(thread);

            call.Hit = ShowTicks;
            call.TotalHits++;

            // if a method
            if (isMethod) 
                call.StillInside++;

            flow.AddStackItem(nodeID, call, Watch.ElapsedTicks, isMethod, ThreadlineEnabled);

            if(ClassTracking)
                TrackClassCall(nodeID, node, source);
        }

        public static XNode GetContainingClass(XNode node)
        {
            while (node != null)
            {
                if (node.ObjType == XObjType.Class)
                    return node;

                node = node.Parent;
            }

            return null;
        }

        public static void TrackClassCall(int nodeID, XNodeIn node, int source)
        {
            var srcNode = Nodes[source];
            if (srcNode == null)
                return;

            var sourceClass = GetContainingClass(srcNode) as XNodeIn;
            var destClass = GetContainingClass(node) as XNodeIn;

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
                //LogError("Adding to class map {0} -> {1} with hash {2}", sourceClass.ID, destClass.ID, hash);

                call = new FunctionCall() { Source = sourceClass.ID, Destination = destClass.ID };
                ClassCallMap.Add(hash, call);

                destClass.AddFunctionCall(ref destClass.CalledIn, sourceClass.ID, call);
               
                sourceClass.AddFunctionCall(ref sourceClass.CallsOut, destClass.ID, call);
            }

            call.Hit = ShowTicks;
            call.TotalHits++;
        }

        public static void MethodExit(int nodeID)
        {
            // still run if disabled so turning xray on/off doesnt desync xray's understanding of the current state

            if (!ThreadTracking || !FlowTracking || Nodes == null || Nodes[nodeID] == null)
                return;

            //BytesSent += 4 + 4 + 4 + 1; // type, functionID, threadID, null

            int thread = Thread.CurrentThread.ManagedThreadId;

            //if (CallLogging)
            //    LogError("Thread {0}, Func {1}, Exit\r\n", thread, nodeID);


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
                            exited.EndTick = ticks;

                            Nodes[exited.NodeID].StillInside--;

                            if (exited.Call == null)
                                continue;

                            exited.Call.StillInside--;
                            exited.Call.TotalCallTime += ticks - exited.StartTick;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += ticks - exited.StartTick;
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

            //BytesSent += 4 + 4 + 4 + 1; // type, functionID, threadID, null

            int thread = Thread.CurrentThread.ManagedThreadId;

            //if (CallLogging)
            //    LogError("Thread {0}, Func {1}, Catch\r\n", thread, nodeID);
           
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
                            exited.EndTick = ticks;

                            Nodes[exited.NodeID].StillInside--;
                            exited.Call.TotalCallTime += ticks - exited.StartTick;

                            Nodes[exited.NodeID].ExceptionHit = ShowTicks;

                            if (exited.Call != null)
                                exited.Call.StillInside--;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += ticks - exited.StartTick;
                        }

                        break;
                    }
        }

        public static void Constructed(int index, Object obj)
        {
            if (!XRayEnabled)
                return;

            XNodeIn node = Nodes[index];
            
            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null)
                return;

            node.ConstructedHit = ShowTicks;

            // of obj is system.runtimeType thats the flag that this is a static class's constructor running

            if (node.Record == null)
                node.Record = new InstanceRecord();

            if(node.Record.Add(obj))
                InstanceChange = true;


            // track who constructed this object
            TrackInit(node);
        }

        private static void TrackInit(XNodeIn node)
        {
            int thread = 0;
            if (ThreadTracking)
                thread = Thread.CurrentThread.ManagedThreadId;

            ThreadFlow flow;
            if (!FlowMap.TryGetValue(thread, out flow))
            {
                LogError("Init Error 1");
                return;
            }

            if (flow.Pos < 0)
            {
                LogError("Init Error 2");
                return;
            }

            XNodeIn sourceClass = null;

            // travel up stack until we find something we know and mark that as the source of the init
            for (int i = flow.Pos; i >= 0; i--)
            {
                int source = flow.Stack[i].NodeID;

                var srcNode = Nodes[source];
                if (srcNode == null)
                    continue;

                sourceClass = GetContainingClass(srcNode) as XNodeIn;
                if (sourceClass != null && sourceClass != node)
                    break;
            }

            if (sourceClass == null)
            {
                LogError("Init Error 3");
                return;
            }

            if (node.ObjType != XObjType.Class)
            {
                LogError("Init Error 4");
                return;
            }

            if (sourceClass == node)
            {
                LogError("Init Error 5 " + node.Name); //? class could create itself...
                return;
            }

            // link 
            int hash = sourceClass.ID * FunctionCount + node.ID;

            FunctionCall call;
            if (!InitMap.TryGetValue(hash, out call))
            {
                //LogError("Adding to init map {0} -> {1} with hash {2}", sourceClass.ID, node.ID, hash);

                call = new FunctionCall() { Source = sourceClass.ID, Destination = node.ID };
                InitMap.Add(hash, call);

                node.AddFunctionCall(ref node.InitsBy, sourceClass.ID, call);
                sourceClass.AddFunctionCall(ref sourceClass.InitsOf, node.ID, call);
            }
        }

        static void form_KeyPress(object sender, KeyPressEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void Deconstructed(int index, Object obj)
        {
            // still run if disabled so turning xray on/off doesnt desync xray's understanding of the current state

            XNodeIn node = Nodes[index];

            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null || node.Record == null)
            {
                LogError("Deconstructed instance not found of type " + obj.GetType().ToString());
                return;
            }

            node.DisposeHit = ShowTicks;

            if (node.Record.Remove(obj))
                InstanceChange = true;
        }

        public static void LoadField(int nodeID)
        {
            if (!XRayEnabled)
                return;

            var node = NodeHit(nodeID);
            if (node != null)
                node.LastFieldOp = FieldOp.Get;
        }

        public static void SetField(int nodeID)
        {
            if (!XRayEnabled)
                return;

            var node = NodeHit(nodeID);
            if (node != null)
                node.LastFieldOp = FieldOp.Set;
        }

        static internal void LogError(string text, params object[] args)
        {
            LogError2(text, true, args);

        }

        static internal void LogError2(string text, bool filterDupes, params object[] args)
        {
            if (filterDupes)
            {
                if (ErrorDupes.Contains(text.GetHashCode()))
                    return;

                ErrorDupes.Add(text.GetHashCode());
            }

            ErrorLog.Add(string.Format(text, args));
        }
    }

    public class InstanceRecord
    {
        public long Created;
        public long Deleted;
        public List<ActiveRecord> Active = new List<ActiveRecord>();
        
        /// <summary>
        /// Returns true if new instance created
        /// </summary>
        public bool Add(object obj)
        {
            Created++;

            lock (Active)
            {
                Active.Add(new ActiveRecord(Created, obj));

                return (Active.Count == 1);
            }
        }

        /// <summary>
        /// Return true if active count went for something to nothing
        /// </summary>
        public bool Remove(object obj)
        {
            Deleted++;

            // iterate through objects and delete null target
            // cant use hash code to ident object, because some classes override it and do crazy things like construct themselves again causing deadlock

            lock (Active)
            {
                var removeRef = Active.FirstOrDefault(a => a.Ref != null && a.Ref.Target == null);
                if (removeRef == null)
                {
                   XRay.LogError("Deleted instance wasnt logged of type " + obj.GetType().ToString());
                    return false;
                }

                Active.Remove(removeRef);

                return (Active.Count == 0);
            }
        }
    }

    public class ActiveRecord
    {
        public long Number;
        public DateTime Created = DateTime.Now;
        public WeakReference Ref;
        public Type InstanceType;
        public bool IsStatic;

        public ActiveRecord(long num, object obj)
        {
            Number = num;

            if (obj is System.Type)
            {
                InstanceType = obj as System.Type;
                IsStatic = true;
            }
            else
            {
                InstanceType = obj.GetType();
                Ref = new WeakReference(obj, false);
            }
        }

        internal FieldInfo GetField(string name)
        {
            FieldInfo field = null;

            Type instanceBase = InstanceType;
            while (field == null)
            {
                field = InstanceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                                        .FirstOrDefault(f => f.Name == name);

                instanceBase = instanceBase.BaseType;
                if (instanceBase == null)
                    break;
            }

            return field;
        }
    }

    class ThreadFlow
    {
        internal int ThreadID;

        internal int Pos = -1; // current position on the stack
        internal StackItem[] Stack = new StackItem[XRay.MaxStack];

        internal int ThreadlinePos = -1;
        internal StackItem[] Threadline = new StackItem[200]; // 200 lines, 16px high, like 3000px record


        internal void AddStackItem(int nodeID, FunctionCall call, long startTick, bool isMethod, bool ThreadlineEnabled)
        {
            Pos++;

            if (Pos >= XRay.MaxStack)
                return;

            var newItem = new StackItem() 
            { 
                NodeID = nodeID, 
                Call = call, 
                StartTick = startTick, 
                Depth = Pos, 
                ThreadID = ThreadID 
            };

            if (isMethod)
                Stack[Pos] = newItem;
            else
            {
                newItem.EndTick = startTick;
                Pos--;
            }      

            if (!ThreadlineEnabled)
                return;

            // dont over write items in timeline that haven't ended yet
            while (true)
            {
                ThreadlinePos++;
                if (ThreadlinePos >= Threadline.Length)
                    ThreadlinePos = 0;

                var overwrite = Threadline[ThreadlinePos];

                if (overwrite == null || overwrite.EndTick != 0)
                {
                    Threadline[ThreadlinePos] = newItem;
                    break;
                }
            }
        }
    }

    public class StackItem
    {
        internal int NodeID;
        internal FunctionCall Call;
        internal long StartTick;
        internal long EndTick;
        internal int ThreadID;
        internal int Depth;
    }

    class FunctionCall
    {
        internal int Source;
        internal int Destination;

        internal int Hit;
        internal int StillInside;

        internal int TotalHits;
        internal long TotalCallTime;
        internal long TotalTimeOutsideDest;

        internal HashSet<int> ThreadIDs = new HashSet<int>();


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

    
    public class IpcQuery : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            // Make sure the object exists "forever"
            return null;
        }

        public string GetName()
        {
            return Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
        }

        public bool GuiVisible
        {
            get
            {
                return XRay.UIs.Any();
            }
            set
            {
                XRay.StartGui();
            }
        }

        public bool Tracking
        {
            get
            {
                return XRay.XRayEnabled;
            }
            set
            {
                XRay.XRayEnabled = value;
            }
        }
    }
}
