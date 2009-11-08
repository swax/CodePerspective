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
    internal enum ShowHitMode { All, Hit, Unhit }

    public static class XRay
    {
        static MainForm MainForm;

        static Thread Gui;

        internal static XNodeIn RootNode;
        internal static XNodeIn[] Nodes;

        static int FunctionCount;

        internal static ShowHitMode ShowHit = ShowHitMode.All;
        internal static bool CoverChange;
        internal static BitArray CoveredFunctions;

        internal static bool ShowAllCalls;

        internal const int HitFrames = 15;
        internal const int ShowTicks = HitFrames - 1; // first index

        internal static bool InstanceTracking = false; // must be compiled in, can be ignored later
        internal static byte[] InstanceCount;

       
        internal static bool ThreadTracking = false; // can be turned off library side
        
        internal static bool FlowTracking = false; // must be compiled in, can be ignored later
        internal const int MaxStack = 512;
        internal static SharedDictionary<ThreadFlow> FlowMap = new SharedDictionary<ThreadFlow>(1000);
        internal static SharedDictionary<FunctionCall> CallMap = new SharedDictionary<FunctionCall>(100000);

        internal static string DatPath;
        internal static bool CallLogging;
        internal static Dictionary<string, bool> ErrorMap = new Dictionary<string, bool>();

        static int EdgeHashOffset;
        static bool InitComplete;

        static Stopwatch Watch = new Stopwatch();


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

            Watch.Start();

            // read compiled settings
            if (trackFlow)
            {
                FlowTracking = true;
                ThreadTracking = true;
            }
            InstanceTracking = trackInstances;

            // data file with node info should be along side ext
            string path = Path.Combine(Application.StartupPath , "XRay.dat");

            LoadNodeMap(path);

            // init tracking structures
            CoveredFunctions = new BitArray(FunctionCount);

            if (InstanceTracking)
                InstanceCount = new byte[FunctionCount];

            EdgeHashOffset = int.MaxValue / (FunctionCount + 1);
            
            InitComplete = true;

            // boot up the xray gui
            if (Gui == null)
            {
                Gui = new Thread(ShowGui);
                Gui.SetApartmentState(ApartmentState.STA);
                Gui.Start();
            }
        }

        public static void ShowGui()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm = new MainForm();
            Application.Run(MainForm);
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

        public static void MethodEnter(int method)
        {
            if (Nodes == null) // static classes init'ing in the entry points class can cause this
                return;

            if (method >= Nodes.Length)
            {
                LogError("Method not in node array Func {0}, Array Size {1}\r\n", method, Nodes.Length);
                return;
            }

            XNodeIn node = Nodes[method];

            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null)
                return;

            int thread = 0;
            if (ThreadTracking)
                thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                LogError("Thread {0}, Func {1}, Enter\r\n", thread, method);

            // mark covered, should probably check if show covered is checked
            if (!CoveredFunctions[node.ID])
            {
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
                LogError("{0} {1} Hit", node.ObjType, node.ID);

            if (ThreadTracking && thread != 0)
            {
                if (node.LastCallingThread != 0 && node.LastCallingThread != thread)
                    node.ConflictHit = ShowTicks;

                node.LastCallingThread = thread;
            }


            if (!FlowTracking)
                return;

            // check that thread is in map
            ThreadFlow flow;
            if (!FlowMap.TryGetValue(thread, out flow))
            {
                flow = new ThreadFlow() { ThreadID = thread };
                FlowMap.Add(thread, flow);
            }

            node.StillInside++;

            // if the first entry, return here
            if (flow.Pos == -1)
            {
                flow.Pos = 0;
                flow.Stack[0] = new StackItem() { Method = method, Ticks = Watch.ElapsedTicks };
                node.EntryPoint++;
                return;
            }

            // if exceeded tracking max return
            if (flow.Pos >= flow.Stack.Length)
                return;

            // set the source, and put the dest in stack
            int source = flow.Stack[flow.Pos].Method;

            // the ids are small and auto-inc based on the # of funcs
            // just hashing together is not unique enough, and many conflicts because the numbers 
            // are small and close together. so expand the number to a larger domain.
            // also ensure s->d != d->s
            int hash = source * EdgeHashOffset + method;

            FunctionCall call;
            if (!CallMap.TryGetValue(hash, out call))
            {
                call = new FunctionCall() { Source = source, Destination = method };
                CallMap.Add(hash, call);
            }

            if (source != call.Source || method != call.Destination)
                LogError("Call mismatch  {0}->{1} != {2}->{3}\r\n",
                    source, method, call.Source, call.Destination);

            flow.Pos++;
            flow.Stack[flow.Pos] = new StackItem() { Method = method, Call = call, Ticks = Watch.ElapsedTicks };

            call.Hit = ShowTicks;
            call.TotalHits++;
            call.StillInside++;
        }

        public static void MethodExit(int method)
        {
            if (!ThreadTracking || !FlowTracking || Nodes == null || Nodes[method] == null)
                return;

            int thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                LogError("Thread {0}, Func {1}, Exit\r\n", thread, method);


            // only should be called if flow tracking is enabled

            // move back through stack array and find function
            // if a function threw then a lot of functions will be skipped
            long ticks = Watch.ElapsedTicks;

            ThreadFlow flow;
            if (FlowMap.TryGetValue(thread, out flow))
                // work backwards from position on stack to position of the exit
                for (int i = flow.Pos; i >= 0; i--)
                    if (flow.Stack[i].Method == method)
                    {
                        int exit = flow.Pos;
                        flow.Pos = i - 1; // set current position asap

                        // mark functions called as well as this function as not insde
                        for (int x = i; x <= exit; x++)
                        {
                            StackItem exited = flow.Stack[x];

                            Nodes[exited.Method].StillInside--;

                            if (exited.Call == null)
                                continue;

                            exited.Call.StillInside--;
                            exited.Call.TotalCallTime += ticks - exited.Ticks;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += ticks - exited.Ticks;
                        }

                        if (i == 0)
                            Nodes[method].EntryPoint--;

                        break;
                    }
    
            // need a way to freeze app and debug these structures, perfect case for xray live reflection interfaces
            // solves the problem of constant output debug, can surf structure live and manip variables
        }

        public static void MethodCatch(int method)
        {
            if (!ThreadTracking || !FlowTracking || Nodes == null || Nodes[method] == null)
                return;

            int thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                LogError("Thread {0}, Func {1}, Catch\r\n", thread, method);

            long ticks = Watch.ElapsedTicks;

            ThreadFlow flow;
            if (FlowMap.TryGetValue(thread, out flow))
                // work backwards from position on stack to position of the catch
                for (int i = flow.Pos; i >= 0; i--)
                    if (flow.Stack[i].Method == method)
                    {
                        int exit = flow.Pos;
                        flow.Pos = i;

                        // mark functions called by this as not inside any longer
                        for (int x = i + 1; x <= exit; x++)
                        {
                            StackItem exited = flow.Stack[x];

                            Nodes[exited.Method].StillInside--;
                            exited.Call.TotalCallTime += ticks - exited.Ticks;

                            Nodes[exited.Method].ExceptionHit = ShowTicks;

                            if (exited.Call != null)
                                exited.Call.StillInside--;

                            if (x > 0 && flow.Stack[x - 1].Call != null)
                                flow.Stack[x - 1].Call.TotalTimeOutsideDest += ticks - exited.Ticks;
                        }

                        break;
                    }
        }

        public static void Constructed(int index)
        {
            InstanceCount[index]++;
        }

        public static void Deconstructed(int index)
        {
            InstanceCount[index]--;

            // below happens if app calls finalize multiple times (should not happen)
            if (InstanceCount[index] < 0)
            {
                InstanceCount[index] = 0;
                Debug.Assert(false);
            }
        }

        static void LogError(string text, params object[] args)
        {
            ErrorMap[string.Format(text, args)] = true;
        }
    }

    class ThreadFlow
    {
        internal int ThreadID;

        internal int Pos = -1; // current position on the stack
        internal StackItem[] Stack = new StackItem[XRay.MaxStack];
    }

    class StackItem
    {
        internal int Method;
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

        internal long TotalTimeInsideDest { get { return TotalCallTime - TotalTimeOutsideDest; } }
    }

    // this is a dictionary where values can be added, for fast look up dynamically without needing a lock
    // one thread needs to be able to write values fast
    // while another threads need to be able to read values fast
    class SharedDictionary<T> 
        where T : class
    {
        internal int Length;
        internal T[] Values;

        int KeyMax = 100000;
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
            if (Length >= Values.Length)
            {
                // todo log error, in future user should specify call map size in build
                return;
            }

            // should probably lock this... speed or low chance errors? a dangerous game
            int index = Length;
            Map[hash] = index;
            Values[index] = call;

            Length++;
        }   
    }

}
