using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections;


namespace XLibrary.Remote
{
    public class XRemote
    {
        public RijndaelManaged Encryption = new RijndaelManaged();

        public List<XConnection> Connections = new List<XConnection>();

        public LinkedList<string> DebugLog = new LinkedList<string>();

        // listening
        int ListenPort = 4566;
        Socket ListenSocket;

        // logging
        public BandwidthLog Bandwidth = new BandwidthLog(10);
        public Queue<PacketLogEntry> LoggedPackets = new Queue<PacketLogEntry>();

        Dictionary<string, Action<XConnection, GenericPacket>> RouteGeneric = new Dictionary<string, Action<XConnection, GenericPacket>>();

        // downloading
        List<Download> Downloads = new List<Download>();
        const int DownloadChunkSize = 8 * 1024; // should be 8kb
       
        // sync
        public List<SyncClient> SyncClients = new List<SyncClient>();
        public int SyncsPerSecond;
        public int SyncCount;

        // client specific
        public string RemoteStatus = "";
        public string RemoteCachePath;
        public string RemoteDatHash;
        public long RemoteDatSize;
        public string LocalDatPath;
        public string LocalDatTempPath;
        public Stream LocalTempFile;


        public XRemote()
        {
            RouteGeneric["Ping"] = Receive_Ping;
            RouteGeneric["Pong"] = Receive_Pong;

            RouteGeneric["Bye"] = Receive_Bye;

            RouteGeneric["DatHashRequest"] = Receive_DatHashRequest;
            RouteGeneric["DatHashResponse"] = Receive_DatHashResponse;

            RouteGeneric["DatFileRequest"] = Receive_DatFileRequest;

            RouteGeneric["StartSync"] = Receive_StartSync;
        }

        public void StartListening()
        {
            // todo use key embedded with dat file
            Encryption.Key = Utilities.HextoBytes("43a6e878b76fc485698f2d3b2cfbd93b9f90907e1c81e8821dceac82d45252f3");

            try
            {
                if(ListenSocket == null)
                    ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ListenSocket.Bind(new IPEndPoint(System.Net.IPAddress.Any, ListenPort));

                ListenSocket.Listen(10);
                ListenSocket.BeginAccept(new AsyncCallback(ListenSocket_Accept), ListenSocket);

               Log("Listening for TCP on port {0}", ListenPort);
            }
            catch (Exception ex)
            {
               Log("TcpHandler::TcpHandler Exception: " + ex.Message);
            }
        }

        public void Shutdown()
        {
            try
            {
                Socket oldSocket = ListenSocket; // do this to prevent listen exception
                ListenSocket = null;

                if (oldSocket != null)
                    oldSocket.Close();

                lock (Connections)
                    foreach (var connection in Connections)
                        connection.CleanClose("Client shutting down");
            }
            catch (Exception ex)
            {
               Log("TcpHandler::Shudown Exception: " + ex.Message);
            }
        }

        public void SecondTimer()
        {
            // sync stat
            SyncsPerSecond = SyncCount;
            SyncCount = 0;

            // Run through socket connections
            var deadSockets = new List<XConnection>();

            lock (Connections)
                foreach (var socket in Connections)
                {
                    socket.SecondTimer();

                    // only let socket linger in connecting state for 10 secs
                    if (socket.State == TcpState.Closed)
                        deadSockets.Add(socket);
                }

            foreach (var socket in deadSockets)
            {
                Connections.Remove(socket);

                string message = "Connection to " + socket.ToString() + " Removed";
                if (socket.ByeMessage != null)
                    message += ", Reason: " + socket.ByeMessage;

               Log(message);

                // socket.TcpSocket = null; causing endrecv to fail on disconnect
            }
        }

        public XConnection MakeOutbound(IPAddress address, ushort tcpPort)
        {
            // only allow 1 outbound connection at a time
            if (Connections.Count != 0)
                return null;

            try
            {
               var outbound = new XConnection(this, address, tcpPort);
               Log("Attempting Connection to " + address.ToString() + ":" + tcpPort.ToString());

                lock (Connections)
                    Connections.Add(outbound);

                return outbound;
            }
            catch (Exception ex)
            {
                Log("TcpHandler::MakeOutbound Exception: " + ex.Message);
                return null;
            }
        }

        internal void Log(string text, params object[] args)
        {
            lock (DebugLog)
            {
                DebugLog.AddFirst(string.Format(text, args));

                while (DebugLog.Count > 100)
                    DebugLog.RemoveLast();
            }
        }

        void ListenSocket_Accept(IAsyncResult asyncResult)
        {
            if (ListenSocket == null)
                return;

            try
            {
                Socket tempSocket = ListenSocket.EndAccept(asyncResult); // do first to catch

                OnAccept(tempSocket, (IPEndPoint)tempSocket.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                Log("TcpHandler::ListenSocket_Accept:1 Exception: " + ex.Message);
            }

            // exception handling not combined because endreceive can fail legit, still need begin receive to run
            try
            {
                ListenSocket.BeginAccept(new AsyncCallback(ListenSocket_Accept), ListenSocket);
            }
            catch (Exception ex)
            {
                Log("TcpHandler::ListenSocket_Accept:2 Exception: " + ex.Message);
            }
        }

        public XConnection OnAccept(Socket socket, IPEndPoint source)
        {
            var inbound = new XConnection(this);

            inbound.TcpSocket = socket;
            inbound.RemoteIP = source.Address;
            inbound.SetConnected();

            // it's not until the host sends us traffic that we can send traffic back because we don't know
            // connecting node's dhtID (and hence encryption key) until ping is sent

            lock (Connections)
                Connections.Add(inbound);

           Log("Accepted Connection from {0}", inbound);

            return inbound;
        }

        internal void OnConnected(XConnection connection)
        {
            if (XRay.IsInvokeRequired())
            {
                XRay.RunInCoreAsync(() => OnConnected(connection));
                return;
            }
            // runs when client connects to server, not the other way around (that's what OnAccept is for)
            RemoteStatus = "Requesting Dat Hash";

            connection.SendPacket(new GenericPacket("DatHashRequest"));
        }

        internal void IncomingPacket(XConnection connection, G2ReceivedPacket packet)
        {
            if (XRay.IsInvokeRequired())
            {
                XRay.RunInCoreAsync(() => IncomingPacket(connection, packet));
                return;
            }

            switch (packet.Root.Name)
            {
                case PacketType.Padding:
                    Log("Crypt Padding Received");
                    break;

                case PacketType.Generic:

                    var generic = GenericPacket.Decode(packet.Root);

                    Log("Generic Packet Received: " + generic.Name);

                    if(RouteGeneric.ContainsKey(generic.Name))
                        RouteGeneric[generic.Name](connection, generic);
                    else
                       Log("Unknown generic packet: " + generic.Name);

                    break;

                case PacketType.Dat:
                    Receive_DatPacket(connection, packet);
                    break;

                case PacketType.Sync:
                    Receive_Sync(connection, packet);
                    break;

                default:
                    Log("Unknown Packet Type: " + packet.Root.Name.ToString());
                    break;
            }
        }

        void Receive_Ping(XConnection connection, GenericPacket ping)
        {
            Log("Pong Sent");
            connection.SendPacket(new GenericPacket("Pong"));
        }

        void Receive_Pong(XConnection connection, GenericPacket pong)
        {
            // lower level socket on receiving data marks connection as alive
        }

        void Receive_Bye(XConnection connection, GenericPacket bye)
        {
            Log("Received bye from {0}: {1}", connection, bye.Data["Reason"]);
            connection.Disconnect();
        }

        void Receive_DatHashRequest(XConnection connection, GenericPacket request)
        {
            var response = new GenericPacket("DatHashResponse");

            response.Data = new Dictionary<string, string>
            {
                {"Hash", XRay.DatHash},
                {"Size", XRay.DatSize.ToString()}
            };

            Log("Sending Dat Hash");

            connection.SendPacket(response);
        }

        void Receive_DatHashResponse(XConnection connection, GenericPacket response)
        {
            // only one instance type per builder instance because xray is static
            if (XRay.InitComplete && RemoteDatHash != null && RemoteDatHash != response.Data["Hash"])
            {
                RemoteStatus = "Open a new builder instance to connect to a new server";
                connection.Disconnect();
                return;
            }

            // check if we have this hash.dat file locally, if not then request download
            RemoteDatHash = response.Data["Hash"];
            RemoteDatSize = long.Parse(response.Data["Size"]);

            LocalDatPath = Path.Combine(RemoteCachePath, RemoteDatHash + ".dat");
            LocalDatTempPath = Path.Combine(RemoteCachePath, RemoteDatHash + ".tmp");

            if (RemoteDatSize == 0)
                RemoteStatus = "Error - Remote Dat Empty";

            else if (File.Exists(LocalDatPath))
                Send_StartSync(connection);
            
            else
            {
                Log("Requesting Dat File, size: " + RemoteDatSize.ToString());

                RemoteStatus = "Requesting Dat File";

                var request = new GenericPacket("DatFileRequest");

                connection.SendPacket(request);
            }
        }

        void Receive_DatFileRequest(XConnection connection, GenericPacket request)
        {
            // received by server from client

            Log("Creating download for connection, size: " + XRay.DatSize.ToString());

            Downloads.Add(new Download()
            {
                Connection = connection,
                Stream = File.OpenRead(XRay.DatPath),
                FilePos = 0
            });
            
        }

        public void ProcessDownloads()
        {
            if (Downloads.Count == 0)
                return;

            var removeDownloads = new List<Download>();

            foreach (var download in Downloads)
            {
                if (download.Connection.State != TcpState.Connected)
                {
                    removeDownloads.Add(download);
                    continue;
                }

                // while connection has 8kb in buffer free
                while (download.Connection.SendReady) // read 8k, 200b overflow buffer
                {
                    // read 8k of file
                    long readSize = XRay.DatSize - download.FilePos;
                    if (readSize > DownloadChunkSize)
                        readSize = DownloadChunkSize;

                    var chunk = new DatPacket(download.FilePos, download.Stream.Read((int)readSize));
       
                    Log("Sending dat pos: {0}, length: {1}", chunk.Pos, chunk.Data.Length); //todo delete

                    // send
                    if(chunk.Data.Length > 0)
                    {
                        int bytesSent = download.Connection.SendPacket(chunk);
                        if(bytesSent < 0)
                            break;
                    }

                    download.FilePos += chunk.Data.Length;

                    // remove when complete
                    if (download.FilePos >= XRay.DatSize)
                    {
                        removeDownloads.Add(download);
                        break;
                    }
                }
            }

            foreach (var download in removeDownloads)
            {
                download.Stream.Close();
                Downloads.Remove(download);
            }
        }

        void Receive_DatPacket(XConnection connection, G2ReceivedPacket packet)
        {
            // received by client from server
            var chunk = DatPacket.Decode(packet.Root);

            // write to tmp file
            if (LocalTempFile == null)
            {
                LocalTempFile = File.Create(LocalDatTempPath);
                LocalTempFile.SetLength(0);
            }

            Log("Received dat pos: {0}, length: {1}", chunk.Pos, chunk.Data.Length); //todo delete

            LocalTempFile.Write(chunk.Data);

            var percentComplete = LocalTempFile.Length * 100 / RemoteDatSize;

            RemoteStatus = string.Format("Downloading Dat File - {0}% Complete", percentComplete);

            // hash when complete
            if (LocalTempFile.Length >= RemoteDatSize)
            {
                LocalTempFile.Close();
                LocalTempFile = null;

                var checkHash = Utilities.MD5HashFile(LocalDatTempPath);

                if (checkHash == RemoteDatHash)
                {
                    File.Move(LocalDatTempPath, LocalDatPath);
                    Send_StartSync(connection);
                }
                else
                    RemoteStatus = string.Format("Dat integrity check failed - Expecting {0}, got {1}", RemoteDatHash, checkHash);
            }

        }

        void Send_StartSync(XConnection connection)
        {
            RemoteStatus = "Starting Sync";

            // send packet telling server to start syncing us

            XRay.Init(LocalDatPath, true, true, true, true);

            connection.SendPacket(new GenericPacket("StartSync"));
        }

        void Receive_StartSync(XConnection connection, GenericPacket packet)
        {
            var client = new SyncClient();
            client.Connection = connection;

            Log("Sync client added");
            SyncClients.Add(client);

            // do after state added so new calls get queued to be sent as well
            foreach(var call in XRay.CallMap)
                client.NewCalls.Add(new Tuple<int, int>(call.Source, call.Destination));

            foreach (var init in XRay.InitMap)
                client.Inits.Add(new Tuple<int, int>(init.Source, init.Destination));

            foreach (var flow in XRay.FlowMap)
                client.NewThreads.Add(flow.ThreadID, new Tuple<string, bool>(flow.Name, flow.IsAlive));

            foreach (var node in XRay.Nodes)
                if (node.ThreadIDs != null)
                    foreach (var id in node.ThreadIDs)
                        client.NodeThreads.Add(new Tuple<int, int>(node.ID, id));

            foreach (var call in XRay.CallMap)
                if (call.ThreadIDs != null)
                    foreach (var id in call.ThreadIDs)
                        client.CallThreads.Add(new Tuple<int, int>(call.ID, id));

            // past threadlines will be added automatically when sync packet is sent
        }

        private void Receive_Sync(XConnection connection, G2ReceivedPacket packet)
        {
            var sync = SyncPacket.Decode(packet.Root);

            Log("Sync packet received");

            SyncCount++;

            XRay.RemoteSync(sync);
        }
    }

    class Download
    {
        public XConnection Connection;
        public FileStream Stream;
        public long FilePos;
    }

    public class SyncClient
    {
        public XConnection Connection;

        public HashSet<int> FunctionHits = new HashSet<int>();
        public HashSet<int> ExceptionHits = new HashSet<int>();
        public HashSet<int> ConstructedHits = new HashSet<int>();
        public HashSet<int> DisposeHits = new HashSet<int>();

        public PairList NewCalls = new PairList();  
        public HashSet<int> CallHits = new HashSet<int>();

        public PairList Inits = new PairList();

        public Dictionary<int, Tuple<string, bool>> NewThreads = new Dictionary<int, Tuple<string, bool>>();
        public Dictionary<int, bool> ThreadChanges = new Dictionary<int, bool>();
        public PairList NodeThreads = new PairList();
        public PairList CallThreads = new PairList();
        public Dictionary<int, PairList> Threadlines = new Dictionary<int, PairList>();
        const int MaxStackItemsPerThreadSent = 100;

        public void DoSync()
        {

            if (Connection.State != TcpState.Connected)
                return;

            if (!Connection.SendReady)
                return;

            {
                // save current set and create a new one so other threads dont get tripped up
                var packet = new SyncPacket();

                AddSet(ref FunctionHits, ref packet.FunctionHits);
                AddSet(ref ExceptionHits, ref packet.ExceptionHits);
                AddSet(ref ConstructedHits, ref packet.ConstructedHits);
                AddSet(ref DisposeHits, ref packet.DisposeHits);
     
                AddPairs(ref NewCalls, ref packet.NewCalls);
                AddSet(ref CallHits, ref packet.CallHits);

                AddPairs(ref Inits, ref packet.Inits);

                if (NewThreads.Count > 0)
                {
                    packet.NewThreads = NewThreads;
                    NewThreads = new Dictionary<int, Tuple<string, bool>>();
                }

                if (ThreadChanges.Count > 0)
                {
                    packet.ThreadChanges = ThreadChanges;
                    ThreadChanges = new Dictionary<int, bool>();
                }

                AddPairs(ref NodeThreads, ref packet.NodeThreads);
                AddPairs(ref CallThreads, ref packet.CallThreads);

                AddThreadlines(packet);

                // check that there's space in the send buffer to send state
                Connection.SendPacket(packet);
            }
        }

        void AddSet(ref HashSet<int> localSet, ref HashSet<int> packetSet)
        {
            if (localSet.Count > 0)
            {
                packetSet = localSet;
                localSet = new HashSet<int>();
            }
        }

        void AddPairs(ref PairList localPairs, ref PairList packetPairs)
        {
            if (localPairs.Count > 0)
            {
                packetPairs = localPairs;
                localPairs = new PairList();
            }
        }

        internal void AddThreadlines(SyncPacket packet)
        {
            foreach (var flow in XRay.FlowMap)
            {
                if (flow.NewItems == 0)
                    continue;

                PairList threadline;
                if (!Threadlines.TryGetValue(flow.ThreadID, out threadline))
                {
                    threadline = new PairList();
                    Threadlines[flow.ThreadID] = threadline;
                }

                int sendCount = Math.Min(flow.NewItems, MaxStackItemsPerThreadSent);
                foreach (var item in flow.EnumerateThreadline(sendCount))
                    threadline.Add(new Tuple<int,int>((item.Call == null) ? item.NodeID : item.Call.ID, item.Depth));


                // set new items to 0, iterate new items on threadline add
                flow.NewItems = 0;
            }

            if (Threadlines.Count > 0)
            {
                packet.Threadlines = Threadlines;
                Threadlines = new Dictionary<int, PairList>();
            }
        }
    }
}
