using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
/*using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;*/
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
        public static Dictionary<int, XUI> UIs = new Dictionary<int, XUI>();

        public static XNodeIn RootNode;
        public static XNodeIn[] Nodes = new XNodeIn[] {};

        public static int FunctionCount;

        public static bool InitComplete;

        // settings
        public static Dictionary<string, string> Settings = new Dictionary<string, string>();
        public static bool EnableLocalViewer;
        public static bool ShowViewerOnStart;
        public static bool EnableIpcServer;

        public static bool EnableTcpServer;
        public static int TcpListenPort;
        public static string EncryptionKey;

        // cover
        public static bool CoverChange;
        public static bool CallChange;
        public static bool InstanceChange;

        public static BitArray CoveredNodes;

        // core thread
        public const int HitFrames = 30;
        public const int ShowTicks = HitFrames - 1; // first index
        public static int TargetFps = HitFrames;
        public const int NewHitTimeout = 1;

        public static Thread CoreThread;
        public static AutoResetEvent RunCoreEvent = new AutoResetEvent(false);
        public static Queue<Action> CoreMessages = new Queue<Action>();

        // tracking
        public static bool TrackFunctionHits = true;
        public static bool TrackMethodExit = false;
        public static bool InstanceTracking = false; // must be compiled in, can be ignored later
        public static bool ThreadTracking = false; // can be turned off library side
        public static bool FlowTracking = false; // must be compiled in, can be ignored later
        public static bool ClassTracking = false;
        public static bool FieldGetLeftToRight = true;

        public const int MaxStack = 512;
        public const int MaxThreadlineSize = 500;

        public static SharedDictionary<ThreadFlow> FlowMap = new SharedDictionary<ThreadFlow>(100);
        public static SharedDictionary<FunctionCall> CallMap = new SharedDictionary<FunctionCall>(1000);
        public static SharedDictionary<FunctionCall> ClassCallMap = new SharedDictionary<FunctionCall>(1000);
        public static SharedDictionary<FunctionCall> InitMap = new SharedDictionary<FunctionCall>(1000);

        public static bool ThreadlineEnabled = true;

        public static string AppDir;
        public static string DatPath;
        public static string DatHash;
        public static long DatSize;

        // errors
        public static HashSet<int> ErrorDupes = new HashSet<int>();
        public static List<string> ErrorLog = new List<string>();

        public static Stopwatch Watch = new Stopwatch();
        static uint CurrentSequence;

        public static DateTime StartTime;
        public static string BuilderVersion = "unknown";
        public static Random RndGen = new Random();

        public static int DashOffset;

        // Remote Connections
        public static bool RemoteViewer; // if false then this instance is a server (linked directly to xrayed code)
        public static XRemote Remote;



        // opens xray from the builder exe to analyze the dat
        public static void Analyze(string path)
        {
            AppDir = Path.GetDirectoryName(path);
            DatPath = path;

            // enable so call lines show
            FlowTracking = true;
            ThreadTracking = true;
            ClassTracking = true;

            if (LoadNodeMap())
            {
                ApplySettings();

                var ui = new XUI();
                ui.Thread = Thread.CurrentThread;
                ui.Window = new MainForm();

                UIs[ui.Thread.ManagedThreadId] = ui;

                ui.Window.Show();
            }
        }

        // called from re-compiled app's entrypoint
        public static void Init(string datPath, bool trackFlow, bool trackInstances, bool remoteViewer)
        {
            LogError("Entry point Init");

            if (InitComplete)
            {
                LogError("Init already called");
                return;
            }
            InitComplete = true;

            AppDir = Path.GetDirectoryName(datPath);
            DatPath = datPath;

            RemoteViewer = remoteViewer;
            if (remoteViewer)
                TargetFps = 10;

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
                if (!File.Exists(DatPath))
                    DatPath = "XRay.dat";

                // data file with node info should be along side ext
                DatHash = Utilities.MD5HashFile(DatPath);
                DatSize = new FileInfo(DatPath).Length;
                LoadNodeMap();
                ApplySettings();

                // init tracking structures
                CoveredNodes = new BitArray(FunctionCount);

                TrackMethodExit = (ThreadTracking && FlowTracking && TrackFunctionHits && Nodes != null); // nodes initd

                InitCoreThread();

                if (!remoteViewer)
                {
                    /*if(EnableIpcServer)
                        StartIpcServer();*/

                    if (EnableTcpServer)
                    {
                        Remote = new XRemote();
                        Remote.StartListening(TcpListenPort, EncryptionKey);
                    }

                    if (EnableLocalViewer && ShowViewerOnStart)
                        StartGui();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDir, "XRayError.txt"), "XRay::Init - " + ex.Message + ":\r\n" + ex.StackTrace);
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
            var resetWatch = new Stopwatch();
            var secondWatch = new Stopwatch();

            resetWatch.Start();
            secondWatch.Start();

            int frameMS = 1000 / TargetFps;

            while (true)
            {
                RunCoreEvent.WaitOne(frameMS);

                // second timer
                if (secondWatch.ElapsedMilliseconds >= 1000)
                {
                    if (Remote != null)
                        Remote.SecondTimer();

                    // if we are a server, update thread alive states for the program we're analyzing
                    if (!RemoteViewer)
                        UpdateThreadAlive();

                    // set target fps, might of changed
                    frameMS = 1000 / TargetFps;

                    TrackMethodExit = (ThreadTracking && FlowTracking && TrackFunctionHits && Nodes != null);

                    secondWatch.Reset();
                    secondWatch.Start();
                }

                // at reset interface iterate functions and call and decrease hit counters, save cpu when no UIs active
                if (UIs.Count > 0 && resetWatch.ElapsedMilliseconds >= frameMS)
                {         
                    ResetFunctionHits();

                    resetWatch.Reset();
                    resetWatch.Start();
                }

                // sync remotes
                if (Remote != null)
                    foreach (var client in Remote.SyncClients)
                        client.TrySync();

                // try to send more data, on send may have triggered this loop
                if (Remote != null)
                {
                    foreach (var connection in Remote.Connections)
                        connection.TrySend();

                    Remote.ProcessDownloads();
                }

                // run any outstanding functions on core thread
                lock (CoreMessages)
                    while (CoreMessages.Count > 0)
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

        private static void UpdateThreadAlive()
        {
            foreach (var thread in FlowMap)
            {
                if (thread.IsAlive == thread.Handle.IsAlive)
                    continue;

                thread.IsAlive = thread.Handle.IsAlive;

                // check if state is diff from sync
                if (Remote != null)
                    foreach (var client in Remote.SyncClients)
                        client.ThreadChanges.Add(thread.ThreadID, thread.IsAlive);
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
                if (call == null)
                    continue;

                if (call.Hit > 0)
                    call.Hit--;

                if (call.NewHit > 0)
                    call.NewHit--;

                //call.DashOffset -= FunctionCall.DashSize;
                //if (call.DashOffset < 0)
                //    call.DashOffset = FunctionCall.DashSpace;
            }
        }

        public static void StartGui()
        {
            XUI ui = new XUI();

            ui.Thread = new Thread(() =>
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    
                    ui.Window = new MainForm();
                    Application.Run(ui.Window);
                }
                catch (Exception ex)
                {
                    LogError("Gui Error: " + ex.Message);
                }

                UIs.Remove(Thread.CurrentThread.ManagedThreadId);
            });

            ui.Thread.SetApartmentState(ApartmentState.STA);
            ui.Thread.Start();

            UIs[ui.Thread.ManagedThreadId] = ui;
        }

        /*static IpcServerChannel XRayChannel;
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
        }*/

        static bool LoadNodeMap()
        {
            RootNode = null;
            FunctionCount = 0;

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

                            Settings[name] = value;

                            if (name == "FunctionCount")
                            {
                                FunctionCount = int.Parse(value);

                                Nodes = new XNodeIn[FunctionCount];
                            }
                        }
                        else if (type == XPacketType.Node)
                        {
                            XNodeIn node = XNodeIn.ReadNode(stream);
                            Nodes[node.ID] = node;

                            // first node read is the root
                            if (RootNode == null)
                                RootNode = node;

                            else if (Nodes[node.ParentID] != null)
                            {
                                node.Parent = Nodes[node.ParentID];
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

                            if (node.ObjType == XObjType.Internal)
                            {
                                node.Record = new InstanceRecord();
                                node.Record.Add(typeof(XRay));
                            }
                        }

                        else if (type == XPacketType.CallMap || type == XPacketType.InitMap)
                        {
                            stream.Read(4); // re-read total size
                            stream.Read(1); // re-read packet type


                            int count = BitConverter.ToInt32(stream.Read(4), 0);

                            for (int i = 0; i < count; i++)
                            {
                                int source = BitConverter.ToInt32(stream.Read(4), 0);
                                int dest = BitConverter.ToInt32(stream.Read(4), 0);

                                int hash = PairHash(source, dest);

                                if (type == XPacketType.CallMap)
                                    CreateNewCall(hash, source, Nodes[dest]);
                                else
                                    CheckCreateInit(Nodes[source], Nodes[dest]);
                            }
                        }

                        stream.Position = startPos + totalSize;
                    }
                }


                foreach (var from in dependenciesFrom.Keys)
                    Nodes[from].Independencies = dependenciesFrom[from].ToArray();


                return true;
            }
            catch(Exception ex)
            {
                File.WriteAllText(Path.Combine(AppDir, "XRayError.txt"), "XRay::LoadNode - " + ex.Message + ":\r\n" + ex.StackTrace);
            }

            return false;
        }

        public static void ApplySettings()
        {
            foreach (var name in Settings.Keys)
            {
                var value = Settings[name];

                switch (name)
                {
                    case "Version":
                        BuilderVersion = value;
                        break;

                    case "Pro":
                        Pro.LoadFromString(value);
                        break;

                    case "EnableLocalViewer":
                        EnableLocalViewer = bool.Parse(value);
                        break;

                    case "EnableIpcServer":
                        EnableIpcServer = bool.Parse(value);
                        break;

                    case "ShowViewerOnStart":
                        ShowViewerOnStart = bool.Parse(value);
                        break;

                    case "EnableTcpServer":
                        EnableTcpServer = bool.Parse(value);
                        break;

                    case "TcpListenPort":
                        TcpListenPort = int.Parse(value);
                        break;

                    case "EncryptionKey":
                        EncryptionKey = value;
                        break;
                }
            }
        }

        /*public static double GetSimBps()
        {
            double secs = DateTime.Now.Subtract(StartTime).TotalSeconds;

            return BytesSent / secs;
        }*/

        public static void MethodEnter(int nodeID)
        {
            if (!TrackFunctionHits)
                return;
            
            //BytesSent += 4 + 4 + 4 + 1; // type, functionID, threadID, null
    
            NodeHit(null, nodeID);
        }

        public static void MethodEnterWithParams(object[] parameters, int nodeID)
        {
              if (!TrackFunctionHits)
                return;
            
            NodeHit(parameters, nodeID);
        }

        private static XNodeIn NodeHit(object[] parameters, int nodeID, bool loadField=false)
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

            if (!node.ThreadIDs.Contains(thread))
            {
                node.ThreadIDs.Add(thread);

                if (Remote != null)
                    foreach (var client in Remote.SyncClients)
                        lock(client.NodeThreads)
                            client.NodeThreads.Add(new Tuple<int, int>(node.ID, thread));
            }

            //if (CallLogging)
            //    LogError("Thread {0}, Func {1}, Enter\r\n", thread, nodeID);

            // mark covered
            if (!CoveredNodes[nodeID])
                SetCovered(node);

            if (node.FunctionHit <= XRay.NewHitTimeout)
                node.FunctionNewHit = ShowTicks;
            
            node.FunctionHit = ShowTicks;

            if (Remote != null)
                foreach (var sync in Remote.SyncClients)
                    lock(sync.FunctionHits)
                        sync.FunctionHits.Add(nodeID);

            if (FlowTracking)
                TrackFunctionCall(nodeID, node, thread, parameters, loadField);

            return node;
        }

        private static void SetCovered(XNodeIn node)
        {
            node.HitSequence = CurrentSequence++;

            CoverChange = true;

            Utilities.IterateParents<XNode>(
                node,
                n => CoveredNodes[n.ID] = true,
                n => n.Parent);

            // clear cover change on paint
        }

        private static void TrackFunctionCall(int dest, XNodeIn node, int thread, object[] parameters, bool loadField=false)
        {
            // check that thread is in map
            ThreadFlow flow;
            if (!FlowMap.TryGetValue(thread, out flow))
            {
                flow = new ThreadFlow()
                {
                    ThreadID = thread,
                    Name = node.Name,
                    Handle = Thread.CurrentThread,
                    IsAlive = true
                };

                FlowMap.Add(thread, flow);

                if (Remote != null)
                    foreach (var client in Remote.SyncClients)
                        lock(client.NewThreads)
                            client.NewThreads[flow.ThreadID] = new Tuple<string,bool>(flow.Name, flow.IsAlive);
            }

            bool isMethod = (node.ObjType == XObjType.Method);

            if(isMethod)
                node.StillInside++;


            // if the first entry, return here
            if (flow.Pos == -1)
            {
                flow.CreateStackItem(dest, null, Watch.ElapsedTicks, isMethod, ThreadlineEnabled);
                node.EntryPoint++;
                return;
            }

            // if exceeded tracking max return
            if (flow.Pos >= flow.Stack.Length)
                return;

            // set the source, and put the dest in stack
            int source = flow.Stack[flow.Pos].NodeID;

            // if loading a fields the call goes from field -> node
            if (loadField && FieldGetLeftToRight)
            {
                int temp = source;
                source = dest;
                dest = temp;
            }

            int hash = PairHash(source, dest);

            FunctionCall call;
            if (!CallMap.TryGetValue(hash, out call))
                call = CreateNewCall(hash, source, node);

            if (source != call.Source || dest != call.Destination)
                LogError("Call mismatch  {0}->{1} != {2}->{3}\r\n", source, dest, call.Source, call.Destination);
            
            
            if (call.Hit < XRay.NewHitTimeout)
                call.NewHit = ShowTicks;
            
            call.Hit = ShowTicks;
            call.TotalHits++;
            call.LastParameters = parameters;

            if (!call.ThreadIDs.Contains(thread))
            {
                call.ThreadIDs.Add(thread);

                if (Remote != null)
                    foreach (var client in Remote.SyncClients)
                        lock(client.CallThreads)
                            client.CallThreads.Add(new Tuple<int, int>(call.ID, thread));
            }

            if (Remote != null)
                foreach (var sync in Remote.SyncClients)
                    lock(sync.CallHits)
                        sync.CallHits.Add(hash);

            // if a method
            if (isMethod) 
                call.StillInside++;

            flow.CreateStackItem(dest, call, Watch.ElapsedTicks, isMethod, ThreadlineEnabled);

            if(ClassTracking)
                TrackClassCall(call, thread);
        }

        public static FunctionCall CreateNewCall(int hash, int sourceID, XNodeIn dest)
        {
            var call = new FunctionCall() { ID = hash, Source = sourceID, Destination = dest.ID };
            CallMap.Add(hash, call);

            // add link to node that its been called
            dest.AddFunctionCall(ref dest.CalledIn, sourceID, call);

            var source = Nodes[sourceID];
            source.AddFunctionCall(ref source.CallsOut, dest.ID, call);

            //***** new location of create of class to class add function call

            CreateLayerCall(source, dest);

            if (Remote != null)
                foreach (var client in Remote.SyncClients)
                    lock(client.NewCalls)
                        client.NewCalls.Add(new Tuple<int, int>(sourceID, dest.ID));

            CallChange = true;

            if (!ClassTracking)
                return call;

            var srcNode = Nodes[call.Source];
            if (srcNode == null)
                return call;

            var sourceClass = GetContainingClass(srcNode) as XNodeIn;
            var destClass = GetContainingClass(dest) as XNodeIn;

            if (sourceClass == destClass)
                return call;

            if (destClass.ObjType != XObjType.Class || sourceClass.ObjType != XObjType.Class)
            {
                LogError("parent not class type, {0} and {1}", destClass.ObjType, sourceClass.ObjType);
                return call;
            }

            call.ClassCallHash = PairHash(sourceClass.ID, destClass.ID);

            //LogError("Adding to class map {0} -> {1} with hash {2}", sourceClass.ID, destClass.ID, hash);

            var classCall = new FunctionCall() { ID = hash, Source = sourceClass.ID, Destination = destClass.ID };
            ClassCallMap.Add(hash, classCall);

            destClass.AddFunctionCall(ref destClass.CalledIn, sourceClass.ID, classCall);
            sourceClass.AddFunctionCall(ref sourceClass.CallsOut, destClass.ID, classCall);

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

        public static void TrackClassCall(FunctionCall functionCall, int thread)
        {
            // save class hash in call to avoid lookup
            FunctionCall call;

            if(!ClassCallMap.TryGetValue(functionCall.ClassCallHash, out call))
                return;

            if (call.Hit < XRay.NewHitTimeout)
                call.NewHit = ShowTicks;
            
            call.Hit = ShowTicks;

            if (RemoteViewer)
                return;

            call.TotalHits++;
            
            if (!RemoteViewer && !call.ThreadIDs.Contains(thread))
                call.ThreadIDs.Add(thread);
    
            // remote infers class hit and thread by the function to function call
        }

        public static void MethodExit(int nodeID)
        {
            MethodExitWithValue(null, nodeID);
        }

        public static void MethodExitWithValue(object value, int nodeID)
        {
            // still run if disabled so turning xray on/off doesnt desync xray's understanding of the current state

            if(!TrackMethodExit)
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

                            var exitedCall = exited.Call;
                            if (exitedCall == null)
                                continue;

                            exitedCall.StillInside--;
                            exitedCall.TotalCallTime += (int)(ticks - exited.StartTick);
                            exitedCall.LastReturnValue = value;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += (int)(ticks - exited.StartTick);
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
            if (!TrackMethodExit)
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
                            exited.Call.TotalCallTime += (int)(ticks - exited.StartTick);

                            Nodes[exited.NodeID].ExceptionHit = ShowTicks;
                            if (Remote != null)
                                foreach (var sync in Remote.SyncClients)
                                    lock(sync.ExceptionHits)
                                        sync.ExceptionHits.Add(exited.NodeID);

                            if (exited.Call != null)
                                exited.Call.StillInside--;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += (int)(ticks - exited.StartTick);
                        }

                        break;
                    }
        }

        public static void Constructed(int index, Object obj)
        {
            if (!InstanceTracking)
                return;

            XNodeIn node = Nodes[index];
            
            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null)
                return;

            node.ConstructedHit = ShowTicks;
            if (Remote != null)
                foreach (var sync in Remote.SyncClients)
                    lock(sync.ConstructedHits)
                        sync.ConstructedHits.Add(node.ID);

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

            CheckCreateInit(sourceClass, node);
        }

        private static void CheckCreateInit(XNodeIn sourceClass, XNodeIn classNode)
        {
            int hash = PairHash(sourceClass.ID, classNode.ID);

            FunctionCall call;
            if (!InitMap.TryGetValue(hash, out call))
            {
                //LogError("Adding to init map {0} -> {1} with hash {2}", sourceClass.ID, node.ID, hash);

                call = new FunctionCall() { ID = hash, Source = sourceClass.ID, Destination = classNode.ID };
                InitMap.Add(hash, call);

                if (classNode.InitsBy == null)
                    classNode.InitsBy = new HashSet<int>();

                if (sourceClass.InitsOf == null)
                    sourceClass.InitsOf = new HashSet<int>();

                classNode.InitsBy.Add(sourceClass.ID);
                sourceClass.InitsOf.Add(classNode.ID);

                if (Remote != null)
                    foreach (var client in Remote.SyncClients)
                        lock(client.Inits)
                            client.Inits.Add(new Tuple<int, int>(sourceClass.ID, classNode.ID));
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

            if (node.Record.Remove(obj))
            {
                InstanceChange = true;

                node.DisposeHit = ShowTicks;
                if (Remote != null)
                    foreach (var sync in Remote.SyncClients)
                        lock (sync.DisposeHits)
                            sync.DisposeHits.Add(index);
            }
        }

        public static void LoadField(int nodeID)
        {
            if (!TrackFunctionHits)
                return;

            var node = NodeHit(null, nodeID, true);
            if (node != null)
                node.LastFieldOp = FieldOp.Get;
        }

        public static void SetField(int nodeID)
        {
            if (!TrackFunctionHits)
                return;

            var node = NodeHit(null, nodeID);
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

        internal static void RemoteSync(SyncPacket packet)
        {
            if (packet.FunctionHits != null)
                foreach (var id in packet.FunctionHits)
                {
                    var node = Nodes[id];

                    if (node.FunctionHit < XRay.NewHitTimeout)
                        node.FunctionNewHit = ShowTicks;
                    
                    node.FunctionHit = ShowTicks;

                    // mark covered
                    if (!CoveredNodes[id])
                        SetCovered(node);
                }

            if (packet.ExceptionHits != null)
                foreach (var id in packet.ExceptionHits)
                    Nodes[id].ExceptionHit = ShowTicks;

            if (packet.ConstructedHits != null)
                foreach (var id in packet.ConstructedHits)
                    Nodes[id].ConstructedHit = ShowTicks;

            if (packet.DisposeHits != null)
                foreach (var id in packet.DisposeHits)
                    Nodes[id].DisposeHit = ShowTicks;

            if (packet.NewCalls != null)
                foreach (var newCall in packet.NewCalls)
                {
                    int source = newCall.Item1;
                    int dest = newCall.Item2;
                    int hash = PairHash(source, dest);

                    if (!CallMap.Contains(hash))
                        CreateNewCall(hash, source, Nodes[dest]);
                    else
                    {
                        // on re-connect this will happen
                        //Debug.Assert(false, "New call already added in sync");
                    }
                }

            if (packet.CallHits != null)
                foreach (var hash in packet.CallHits)
                {
                    FunctionCall call;
                    if (!CallMap.TryGetValue(hash, out call))
                    {
                        Debug.Assert(false, "Call not found in sync");
                        continue;
                    }

                    if (call.Hit < XRay.NewHitTimeout)
                        call.NewHit = ShowTicks;
                    
                    call.Hit = ShowTicks;

                    if (ClassTracking)
                        TrackClassCall(call, 0);
                }

            if (packet.CallStats != null)
            {
                foreach (var stat in packet.CallStats)
                {
                    FunctionCall call;
                    if (!CallMap.TryGetValue(stat.ID, out call))
                    {
                        Debug.Assert(false, "Call not found in sync");
                        continue;
                    }

                    call.TotalHits = stat.TotalHits;
                    call.TotalCallTime = stat.TotalCallTime;
                    call.TotalTimeOutsideDest = stat.TotalTimeOutsideDest;
                }
            }

            if (packet.Inits != null)
                foreach (var init in packet.Inits)
                {
                    var source = Nodes[init.Item1];
                    var initClass = Nodes[init.Item2];

                    CheckCreateInit(source, initClass);
                }

            if(packet.NewThreads != null)
                foreach (var thread in packet.NewThreads)
                {
                    int id = thread.Key;
                    string name = thread.Value.Item1;
                    bool alive = thread.Value.Item2;

                    if (!FlowMap.Contains(id))
                        FlowMap.Add(id, new ThreadFlow() { ThreadID = id, Name = name, IsAlive = alive });
                    else
                    {
                        // on re-connect this will happen
                        //Debug.Assert(false, "Flow already contains thread on sync");
                    }
                }

            if(packet.ThreadChanges != null)
                foreach (var thread in packet.ThreadChanges)
                {
                    ThreadFlow flow;
                    if (FlowMap.TryGetValue(thread.Key, out flow))
                        flow.IsAlive = thread.Value;
                    else
                        Debug.Assert(false, "Thread not found on sync");
                }

            if(packet.NodeThreads != null)
                foreach (var nodeThread in packet.NodeThreads)
                {
                    var node = Nodes[nodeThread.Item1];

                    if (node.ThreadIDs == null)
                        node.ThreadIDs = new HashSet<int>();

                    node.ThreadIDs.Add(nodeThread.Item2);
                }

            if (packet.CallThreads != null)
                foreach (var callThread in packet.CallThreads)
                {
                    FunctionCall call;
                    if (!CallMap.TryGetValue(callThread.Item1, out call))
                    {
                        Debug.Assert(false, "Call thread not found on sync");
                        continue;
                    }

                    if (call.ThreadIDs == null)
                        call.ThreadIDs = new HashSet<int>();

                    call.ThreadIDs.Add(callThread.Item2);

                    // add thread to class class (might not exist for internal calls)
                    if (!ClassCallMap.TryGetValue(call.ClassCallHash, out call))
                        continue;

                    if (call.ThreadIDs == null)
                        call.ThreadIDs = new HashSet<int>();

                    call.ThreadIDs.Add(callThread.Item2);
                }

            if (packet.ThreadStacks != null)
                foreach (var threadstack in packet.ThreadStacks)
                {
                    ThreadFlow flow;
                    if (!FlowMap.TryGetValue(threadstack.Key, out flow))
                        Debug.Assert(false, "Thread not found on sync");

                    foreach (var itemPos in threadstack.Value)
                    {
                        var item = GetStackItem(itemPos);

                        flow.Stack[item.Depth] = item;
                    }
                }

            if(packet.Threadlines != null)
                foreach (var threadline in packet.Threadlines)
                {
                    ThreadFlow flow;
                    if (!FlowMap.TryGetValue(threadline.Key, out flow))
                        Debug.Assert(false, "Thread not found on sync");

                    // list sent to us latest first, add oldest first
                    threadline.Value.Reverse();

                    foreach (var itemPos in threadline.Value)
                    {
                        var item = GetStackItem(itemPos);

                        flow.AddStackItem(item);
                    }
                }
        }

        private static StackItem GetStackItem(Tuple<int, int> itemPos)
        {
            int id = itemPos.Item1;
            int pos = itemPos.Item2;

            FunctionCall call = null;
            int nodeID = 0;

            if (pos == 0)
            {
                nodeID = id;
            }
            else
            {
                if (!CallMap.TryGetValue(id, out call))
                    Debug.Assert(false, "Thread call not found on sync");

                nodeID = call.Destination;
            }

            var newItem = new StackItem()
            {
                NodeID = nodeID,
                Call = call,
                StartTick = XRay.Watch.ElapsedTicks,
                Depth = pos
            };
            return newItem;
        }

        public static int PairHash(int a, int b)
        {
            // this really only works until ~50,000 functions, then wraps around - still pretty unique
            // was using FunctionCount before instead of 50,000 but might as well use the max range for what we can
            // gaurentee will be unique

            // cantor pairing is the real solution - or using longs
            // cantor is this - p(k1, k2) = 1/2(k1 + k2)(k1 + k2 + 1) + k2

            return a * 50000 + b;
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
        public bool IsAlive;

        public int Pos = -1; // current position on the stack
        public StackItem[] Stack = new StackItem[XRay.MaxStack];

        public int ThreadlinePos = -1;
        public StackItem[] Threadline = new StackItem[XRay.MaxThreadlineSize]; // 200 lines, 16px high, like 3000px record


        public void CreateStackItem(int nodeID, FunctionCall call, long startTick, bool isMethod, bool ThreadlineEnabled)
        {
            Pos++;

            Handle = Thread.CurrentThread; // not sure if id can stay the same while handle object changes if thread id resurrected  

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

            AddStackItem(newItem);
        }

        public void AddStackItem(StackItem newItem)
        {
            ThreadlinePos++;
            if (ThreadlinePos >= Threadline.Length)
                ThreadlinePos = 0;

            Threadline[ThreadlinePos] = newItem;

            if (XRay.Remote != null && !XRay.RemoteViewer)
                foreach (var client in XRay.Remote.SyncClients)
                    if (client.NewStackItems.ContainsKey(ThreadID))
                        client.NewStackItems[ThreadID]++;
                    else
                        client.NewStackItems[ThreadID] = 0;
        }

        public IEnumerable<StackItem> EnumerateThreadline(int max = int.MaxValue)
        {
            // iterate back from current pos timeline until back to start, or start time is newer than start pos start time

            int startPos = ThreadlinePos; // start with the newest position
            if (startPos == -1)// havent started, or disabled
                yield break;

            int i = startPos;
            int count = 0;

            while (true)
            {
                // iterate to next node
                var item = Threadline[i];
                if (item == null)
                    break;

                yield return item;

                count++;
                if (count >= max)
                    break;

                // iterate to previous item in time
                i--;
                if (i < 0)
                    i = Threadline.Length - 1;

                if (i == startPos)
                    break;
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
        public int ID; // hash of source and dest
        public int Source;
        public int Destination;

        public int Hit;
        public int StillInside;
        public int NewHit;

        public int TotalHits;
        public int TotalCallTime;
        public int TotalTimeOutsideDest;

        public int ClassCallHash;

        public HashSet<int> ThreadIDs = new HashSet<int>();

        public object[] LastParameters;
        public object LastReturnValue;


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

    
    /*public class IpcQuery : MarshalByRefObject
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
            if(XRay.EnableLocalViewer)
                XRay.StartGui();
        }

        public bool Tracking
        {
            get
            {
                return XRay.TrackFunctionHits;
            }
            set
            {
                XRay.TrackFunctionHits = value;
            }
        }
    }*/

    public class XUI
    {
        public Thread Thread;
        public MainForm Window;
        public InstanceModel CurrentInstance;
    }
}
