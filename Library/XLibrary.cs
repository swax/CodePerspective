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
using XLibrary.Remote;
using System.Security.Cryptography;


namespace XLibrary
{
    public static class XRay
    {
        static MainForm MainForm;

        public static Dictionary<int, Thread> UIs = new Dictionary<int, Thread>();

        public static XNodeIn RootNode;
        public static XNodeIn[] Nodes = new XNodeIn[] {};

        public static int FunctionCount;

        public static bool XRayEnabled = true;

        public static bool CoverChange;
        public static bool CallChange;
        public static bool InstanceChange;

        public static BitArray CoveredNodes;

        // core thread
        public const int HitFrames = 30;
        public const int ShowTicks = HitFrames - 1; // first index
        public static Thread CoreThread;
        public static AutoResetEvent RunCoreEvent = new AutoResetEvent(false);
        public static Queue<Action> CoreMessages = new Queue<Action>();

        // tracking
        public static bool InstanceTracking = false; // must be compiled in, can be ignored later
       
        public static bool ThreadTracking = false; // can be turned off library side
        
        public static bool FlowTracking = false; // must be compiled in, can be ignored later
        public static bool ClassTracking = false;
        public const int MaxStack = 512;
        public const int MaxThreadlineSize = 1000;

        public static SharedDictionary<ThreadFlow> FlowMap = new SharedDictionary<ThreadFlow>(100);
        public static SharedDictionary<FunctionCall> CallMap = new SharedDictionary<FunctionCall>(1000);
        public static SharedDictionary<FunctionCall> ClassCallMap = new SharedDictionary<FunctionCall>(1000);
        public static SharedDictionary<FunctionCall> InitMap = new SharedDictionary<FunctionCall>(1000);

        public static bool ThreadlineEnabled = true;

        public static string DatPath;
        public static string DatHash;
        public static long DatSize;

        //public static bool CallLogging;
        public static HashSet<int> ErrorDupes = new HashSet<int>();
        public static List<string> ErrorLog = new List<string>();

        public static bool InitComplete;

        public static Stopwatch Watch = new Stopwatch();

        static uint CurrentSequence;

        public static DateTime StartTime;
        //public static double BytesSent;
        public static string BuilderVersion = "unknown";
        public static Random RndGen = new Random();

        public static int DashOffset;

        // Remote Connections
        public static XRemote Remote = new XRemote();


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
        public static void Init(string datPath, bool showUiOnStart, bool trackFlow, bool trackInstances, bool remoteClient)
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

            //System.Windows.Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //AppDomain.CurrentDomain.FirstChanceException +=new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
            try
            {
                // reset path to root path if not found (user moved exe folder, or distributing demo
                if (!File.Exists(datPath))
                    datPath = "XRay.dat";

                // data file with node info should be along side ext
                DatHash = Utilities.MD5HashFile(datPath);
                DatSize = new FileInfo(datPath).Length;
                LoadNodeMap(datPath);

                // init tracking structures
                CoveredNodes = new BitArray(FunctionCount);

                InitCoreThread();

                if (!remoteClient)
                {
                    StartIpcServer();

                    Remote.StartListening();
                }

                if (showUiOnStart)
                    StartGui();
            }
            catch (Exception ex)
            {
                MessageBox.Show("XRay::Init Exception: " + ex.Message);
            }
        }

        public static void InitCoreThread()
        {
            if (CoreThread != null)
                return;

            CoreThread = new Thread(RunEventLoop);
            CoreThread.IsBackground = true;
            CoreThread.Start();
        }

        /* static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogUnhandledException("Current_DispatcherUnhandledException: " + e.Exception.ToString());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogUnhandledException("CurrentDomain_UnhandledException: " + e.ExceptionObject.ToString());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogUnhandledException("Application_ThreadException: " + e.Exception.ToString());

            throw e.Exception;
        }*/

        /*static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
        {
            LogUnhandledException("Application_ThreadException: " + args.Exception.ToString());
        }*/

        static void LogUnhandledException(string excString)
        {
            LogMessage(excString);

            File.AppendAllText("XError.log", excString);
        }


        static void RunEventLoop(object state)
        {
            int frameMS = 1000 / HitFrames;

            var resetWatch = new Stopwatch();
            var secondWatch = new Stopwatch();

            resetWatch.Start();
            secondWatch.Start();

            while (true)
            {
                RunCoreEvent.WaitOne(frameMS);

                if(resetWatch.ElapsedMilliseconds >= frameMS)
                {
                    ResetFunctionHits();

                    resetWatch.Reset();
                    resetWatch.Start();
                }

                if (Remote != null)
                {
                    if (secondWatch.ElapsedMilliseconds >= 1000)
                    {
                        Remote.SecondTimer();

                        secondWatch.Reset();
                        secondWatch.Start();
                    }

                    Remote.RunEventLoop();
                }

                lock (CoreMessages)
                    if (CoreMessages.Count > 0)
                    {
                        try
                        {
                            var method = CoreMessages.Dequeue();
                            method.Invoke();
                        }
                        catch (Exception ex)
                        {
                            LogError("Error Processing Message", ex.Message);
                        }
                    }
            }
        }

        public static bool IsInvokeRequired()
        {
            if (CoreThread == null)
                return false;

            return CoreThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId;
        }

        // be careful if calling this with loop objects, reference will be changed by the time async executes
        public static void RunInCoreAsync(Action code)
        {
            lock (CoreMessages)
            {
                if (CoreMessages.Count < 1000)
                    CoreMessages.Enqueue(code);
                else
                    LogError("CoreMessages Overload");
            }

            RunCoreEvent.Set();
        }


        static void  ResetFunctionHits()
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
                        long startPos = stream.Position;

                        int totalSize;
                        XPacketType type = XNodeIn.ReadNextPacket(stream, out totalSize);

                        stream.Position = startPos;

                        if (type == XPacketType.Setting)
                        {
                            string name, value;
                            XNodeIn.ReadSetting(stream, out name, out value);

                            if (name == "Version")
                                BuilderVersion = value;
                            if (name == "Pro")
                                Pro.LoadFromString(value);
                        }
                        else if (type == XPacketType.Node)
                        {
                            XNodeIn node = XNodeIn.ReadNode(stream);
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
                            if (node.Dependencies != null)
                                foreach (var to in node.Dependencies)
                                {
                                    if (!dependenciesFrom.ContainsKey(to))
                                        dependenciesFrom[to] = new List<int>();

                                    dependenciesFrom[to].Add(node.ID);
                                }
                        }

                        stream.Position = startPos + totalSize;
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

            if (Remote != null)
                foreach (var sync in Remote.SyncClients)
                {
                    sync.HitArray[nodeID] = true;
                    sync.HitArrayAlt.Add(nodeID);
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
                flow = new ThreadFlow() { ThreadID = thread, Name = node.Name };
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
                call = CreateNewCall(hash, source, node);

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
                TrackClassCall(node, source, thread);
        }

        public static FunctionCall CreateNewCall(int hash, int sourceID, XNodeIn dest)
        {
            var call = new FunctionCall() { Source = sourceID, Destination = dest.ID };
            CallMap.Add(hash, call);

            // add link to node that its been called
            dest.AddFunctionCall(ref dest.CalledIn, sourceID, call);

            var source = Nodes[sourceID];
            source.AddFunctionCall(ref source.CallsOut, dest.ID, call);

            //***** new location of create of class to class add function call

            CreateLayerCall(source, dest);


            CallChange = true;

            return call;
        }

        private static void CreateLayerCall(XNodeIn source, XNodeIn dest)
        {
            var sourceChain = source.GetParentChain();
            var destChain = dest.GetParentChain();

            for (int i = 0; i < sourceChain.Length; i++)
                for (int x = 0; x < destChain.Length; x++)
                    if (sourceChain[i] == destChain[x])
                    {
                        if (i == 0 || x == 0)
                        {
                            LogError(string.Format("Error trying to find common link between {0} and {1}, common had 0 index", sourceChain[i].Name, destChain[x].Name));
                            return;
                        }

                        var sourceLayer = sourceChain[i - 1];
                        var destLayer = destChain[x - 1];

                        if (sourceLayer.LayerOut == null)
                            sourceLayer.LayerOut = new HashSet<int>();

                        if (destLayer.LayerIn == null)
                            destLayer.LayerIn = new HashSet<int>();

                        sourceLayer.LayerOut.Add(destLayer.ID);
                        destLayer.LayerIn.Add(sourceLayer.ID);

                        return;
                    }
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

        public static void TrackClassCall(XNodeIn node, int source, int thread)
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

            call.ThreadIDs.Add(thread);
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

                if (node.InitsBy == null)
                    node.InitsBy = new HashSet<int>();

                if (sourceClass.InitsOf == null)
                    sourceClass.InitsOf = new HashSet<int>();

                node.InitsBy.Add(sourceClass.ID);
                sourceClass.InitsOf.Add(node.ID);
            }
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
            if (ErrorDupes.Contains(text.GetHashCode()))
                return;

            ErrorDupes.Add(text.GetHashCode());

            ErrorLog.Add(string.Format(text, args));
        }

        static internal void LogMessage(string text, params object[] args)
        {
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

    public class ThreadFlow
    {
        public int ThreadID;
        public string Name;
        public Thread Handle;

        public int Pos = -1; // current position on the stack
        public StackItem[] Stack = new StackItem[XRay.MaxStack];

        public int ThreadlinePos = -1;
        public StackItem[] Threadline = new StackItem[XRay.MaxThreadlineSize]; // 200 lines, 16px high, like 3000px record


        public void AddStackItem(int nodeID, FunctionCall call, long startTick, bool isMethod, bool ThreadlineEnabled)
        {
            Pos++;

            Handle = Thread.CurrentThread;  

            if (Pos >= XRay.MaxStack)
                return;

            var newItem = new StackItem() 
            { 
                NodeID = nodeID, 
                Call = call, 
                StartTick = startTick, 
                Depth = Pos 
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
        public int NodeID;
        public FunctionCall Call;
        public long StartTick;
        public long EndTick;
        public int Depth;
    }

    public class FunctionCall
    {
        public int Source;
        public int Destination;

        public int Hit;
        public int StillInside;

        public int TotalHits;
        public long TotalCallTime;
        public long TotalTimeOutsideDest;

        public HashSet<int> ThreadIDs = new HashSet<int>();


        public long TotalTimeInsideDest
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

        public string GetLog(int count)
        {
            return String2.Join("\r\n", XRay.ErrorLog.Last(50));
        }

        public bool GuiVisible
        {
            get
            {
                return XRay.UIs.Any();
            }
        }

        public void OpenViewer()
        {
            XRay.StartGui();
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
