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
        static TreeForm MainForm;

        static Thread Gui;

        internal static XNodeIn RootNode;
        internal static XNodeIn[] Nodes;

        static int FunctionCount;

        internal static bool ShowOnlyHit;
        internal static bool CoverChange;
        internal static BitArray CoveredFunctions;

        internal const int HitFrames = 15;
        internal const int ShowTicks = HitFrames - 1; // first index

        internal static bool InstanceTracking = false; // must be compiled in, can be ignored later
        internal static byte[] InstanceCount;

       
        internal static bool ThreadTracking = false; // can be turned off library side
        
        internal static bool FlowTracking = false; // must be compiled in, can be ignored later
        internal const int MaxStack = 512;
        internal static SharedDictionary<ThreadFlow> FlowMap = new SharedDictionary<ThreadFlow>();
        internal static SharedDictionary<FunctionCall> CallMap = new SharedDictionary<FunctionCall>();

        internal static bool CallLogging;
        internal static StringBuilder DebugLog = new StringBuilder(4096);

        static int EdgeHashOffset;


        public static void TestInit(string path)
        {
            LoadNodeMap(path);

            MainForm = new TreeForm();
            MainForm.Show();
        }


        public static void Init(bool trackFlow, bool trackInstances)
        {
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

            MainForm = new TreeForm();
            Application.Run(MainForm);
        }

        static void LoadNodeMap(string path)
        {
            FunctionCount = 0;
            Dictionary<int, XNodeIn> map = new Dictionary<int, XNodeIn>();
            
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    while (stream.Position < stream.Length)
                    {
                        XNodeIn node = XNodeIn.Read(stream);
                        map[node.ID] = node;

                        if (RootNode == null)
                            RootNode = node;

                        if (map.ContainsKey(node.ParentID))
                        {
                            node.Parent = map[node.ParentID];
                            node.Parent.Nodes.Add(node);
                        }

                        if (node.ID > FunctionCount)
                            FunctionCount = node.ID;
                    }
                }

                FunctionCount++; // so id can be accessed in 0 based index

                Nodes = new XNodeIn[FunctionCount];
                foreach (var node in map.Values)
                    Nodes[node.ID] = node;
            }
            catch(Exception ex)
            {
                MessageBox.Show("XRay data file not found: " + ex.Message); // would this even work? test
            }
        }

        public static void MethodEnter(int method)
        {
            XNodeIn node = Nodes[method];

            // prevent having to check multiple times in mark hit and flow tracking
            if (node == null)
                return;

            int thread = 0;
            if (ThreadTracking)
                thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                DebugLog.AppendFormat("Thread {0}, Func {1}, Enter\r\n", thread, method);


            MarkHit(thread, node);

            if (!FlowTracking)
                return;

            // check that thread is in map
            ThreadFlow flow;
            if (!FlowMap.TryGetValue(thread, out flow))
            {
                flow = new ThreadFlow() { ThreadID = thread };
                FlowMap.Add(thread, flow);

                //todo - check map size here, and clear oldest entries
            }

            node.StillInside++;

            // if the first entry, return here
            if (flow.Pos == -1)
            {
                flow.Pos = 0;
                flow.Stack[0] = new StackItem() { Method = method };
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
                DebugLog.AppendFormat("Call mismatch  {0}->{1} != {2}->{3}\r\n",
                    source, method, call.Source, call.Destination);

            flow.Pos++;
            flow.Stack[flow.Pos] = new StackItem() { Method = method, Call = call };

            call.Hit = ShowTicks;
            call.StillInside++;
        }

        public static void MethodExit(int method)
        {
            if (!ThreadTracking || !FlowTracking || Nodes[method] == null)
                return;

            int thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                DebugLog.AppendFormat("Thread {0}, Func {1}, Exit\r\n", thread, method);


            // only should be called if flow tracking is enabled

            // move back through stack array and find function
            // if a function threw then a lot of functions will be skipped

            ThreadFlow flow;
            if (FlowMap.TryGetValue(thread, out flow))
                for (int i = flow.Pos; i >= 0; i--)
                    if (flow.Stack[i].Method == method)
                    {
                        // mark functions called as well as this function as not insde
                        for (int x = i; x <= flow.Pos; x++)
                        {
                            StackItem exited = flow.Stack[x];

                            Nodes[exited.Method].StillInside--;

                            if (exited.Call != null)
                                exited.Call.StillInside--;
                        }

                        if(i == 0)
                            Nodes[method].EntryPoint--;

                        flow.Pos = i - 1;
                        break;
                    }

            // need a way to freeze app and debug these structures, perfect case for xray live reflection interfaces
            // solves the problem of constant output debug, can surf structure live and manip variables
        }

        public static void MethodCatch(int method)
        {
            if (!ThreadTracking || !FlowTracking || Nodes[method] == null)
                return;

            int thread = Thread.CurrentThread.ManagedThreadId;

            if (CallLogging)
                DebugLog.AppendFormat("Thread {0}, Func {1}, Catch\r\n", thread, method);

            ThreadFlow flow;
            if (FlowMap.TryGetValue(thread, out flow))
                for (int i = flow.Pos; i >= 0; i--)
                    if (flow.Stack[i].Method == method)
                    {
                        // mark functions called by this as not inside any longer
                        for (int x = i + 1; x <= flow.Pos; x++)
                        {
                            StackItem exited = flow.Stack[x];

                            Nodes[exited.Method].StillInside--;
                            Nodes[exited.Method].ExceptionHit = ShowTicks;

                            if (exited.Call != null)
                                exited.Call.StillInside--;
                        }

                        flow.Pos = i;
                    }
        }

        public static void MarkHit(int thread, XNodeIn node)
        {
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

            if (ThreadTracking && thread != 0)
            {
                if (node.LastCallingThread != 0 && node.LastCallingThread != thread)
                   node.ConflictHit = ShowTicks;

                node.LastCallingThread = thread;
            }

            // keep 6 lists around for diff threads
            // map thread id to list pos
            // keep track of how often each thread list is updated

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
    }

    class ThreadFlow
    {
        internal int ThreadID;

        internal int Pos = -1;
        internal StackItem[] Stack = new StackItem[XRay.MaxStack];
    }

    class StackItem
    {
        internal int Method;
        internal FunctionCall Call;
    }

    class FunctionCall
    {
        internal int Source;
        internal int Destination;

        internal int Hit;
        internal int DashOffset;

        internal int StillInside;
    }

    // this is a dictionary where values can be added, for fast look up dynamically without needing a lock
    // one thread needs to be able to write values fast
    // while another threads need to be able to read values fast
    class SharedDictionary<T> 
        where T : class
    {
        internal int Length;
        internal T[] Values = new T[10000];
        Dictionary<int, int> Map = new Dictionary<int, int>();
        bool Resizing;

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
            // should probably lock this... speed or low chance errors? a dangerous game
            int index = Length;
            Map[hash] = index;
            Values[index] = call;

            Length++;

            // need to figure a better way... ideally without locking everytime

            // maybe 2 identical functions, one that locks on that doesnt that only triggers when need a resize

            if (Length >= Values.Length / 2 && !Resizing)
            {
                Resizing = true;

                T[] newValues = new T[Values.Length * 2];

                for (int i = 0; i < Values.Length; i++)
                    newValues[i] = Values[i];

                Values = newValues;

                Resizing = false;
            }
        }   
    }

}
