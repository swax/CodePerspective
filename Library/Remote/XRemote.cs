using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using System.Diagnostics;


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
        public SharedDictionary<SyncClient> SyncClients = new SharedDictionary<SyncClient>(10); // special cause it can be iterated without being locked

        // client specific
        public XConnection ServerConnection;
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

            RouteGeneric["RequestInstance"] = Receive_RequestInstance;
            RouteGeneric["RequestField"] = Receive_RequestField;
            RouteGeneric["RequestInstanceRefresh"] = Receive_RequestInstanceRefresh;

            RouteGeneric["Settings"] = Receive_Settings;
        }

        public void StartListening(int port, string key)
        {
            // todo use key embedded with dat file
            Encryption.Key = Utilities.HextoBytes(key);

            try
            {
                if(ListenSocket == null)
                    ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ListenSocket.Bind(new IPEndPoint(System.Net.IPAddress.Any, port));

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
            foreach (var client in SyncClients)
                client.SecondTimer();

            // Run through socket connections
            lock (Connections)
                foreach (var socket in Connections)
                    socket.SecondTimer();

            var deadSockets = Connections.Where(c => c.State == TcpState.Closed).ToList();

            foreach (var socket in deadSockets)
            {
                Connections.Remove(socket);

                string message = "Connection to " + socket.ToString() + " Removed";
                if (socket.ByeMessage != null)
                    message += ", Reason: " + socket.ByeMessage;

               Log(message);

               SyncClients.Remove(socket);

               if (socket == ServerConnection)
                   ServerConnection = null;

               // socket.TcpSocket = null; causing endrecv to fail on disconnect
            }

        }

        public void ConnectToServer(IPAddress address, ushort tcpPort)
        {
            // only allow 1 outbound connection at a time
            if (ServerConnection != null)
                return;

            try
            {
               ServerConnection = new XConnection(this, address, tcpPort);
               Log("Attempting Connection to " + address.ToString() + ":" + tcpPort.ToString());

                lock (Connections)
                    Connections.Add(ServerConnection);
            }
            catch (Exception ex)
            {
                Log("TcpHandler::MakeOutbound Exception: " + ex.Message);
            }
        }

        internal void Log(string text, params object[] args)
        {
            lock (DebugLog)
            {
                DebugLog.AddLast(string.Format(text, args));

                while (DebugLog.Count > 100)
                    DebugLog.RemoveFirst();
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
                    //Log("Crypt Padding Received");
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

                case PacketType.Instance:
                    Receive_Instance(connection, packet);
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
            XRay.Init(LocalDatPath, true, true, true);

            XRay.StartGui();

            connection.SendPacket(new GenericPacket("StartSync"));

            SendClientSettings(); // need to send after sync started

            ServerConnection = connection;
        }

        void Receive_StartSync(XConnection connection, GenericPacket packet)
        {
            // received by server from client
         
            var client = new SyncClient();
            client.Connection = connection;

            Log("Sync client added");
            SyncClients.Add(client.Connection.GetHashCode(), client);

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
            // received by client from server

            var sync = SyncPacket.Decode(packet.Root);

            //Log("Sync packet received");

            connection.SyncCount++;

            XRay.RemoteSync(sync);
        }

        void Receive_RequestInstance(XConnection connection, GenericPacket request)
        {
            // received by server from client

            SyncClient client;
            if (!SyncClients.TryGetValue(connection.GetHashCode(), out client))
            {
                Log("Request Instance: Sync client not found");
                return;
            }

            var threadID = int.Parse(request.Data["ThreadID"]);

            var node = XRay.Nodes[int.Parse(request.Data["NodeID"])];

            string filter = null;
            request.Data.TryGetValue("Filter", out filter);

            var model = new InstanceModel(node, filter);
            client.SelectedInstances[threadID] = model;
            Log("Request Instance: Model added for thread " + threadID.ToString());

            model.BeginUpdateTree(false);

            // send back details, columns, and nodes
            var response = new InstancePacket()
            {
                Type = InstancePacketType.Root,
                ThreadID = threadID,
                Details = model.DetailsLabel,
                Columns = model.Columns,
                Fields = model.RootNodes
            };

            client.Connection.SendPacket(response);
        }

        void Receive_RequestInstanceRefresh(XConnection connection, GenericPacket request)
        {       
            // received by server from client

            SyncClient client;
            if (!SyncClients.TryGetValue(connection.GetHashCode(), out client))
            {
                Log("Request Instance Refresh: Sync client not found");
                return;
            }

            var threadID = int.Parse(request.Data["ThreadID"]);

            InstanceModel instance;
            if (!client.SelectedInstances.TryGetValue(threadID, out instance))
            {
                Log("Request field: instance not found " + threadID.ToString());
                return;
            }

            instance.BeginUpdateTree(true);

            // send columns if updated
            var response = new InstancePacket();
            response.Type = InstancePacketType.Refresh;
            response.ThreadID = threadID;
            // flag as a refresh?

            if (instance.ColumnsUpdated)
                response.Columns = instance.Columns;

            // iterate fields, send any marked as changed
            response.Fields = instance.FieldMap.Values.Where(f => instance.UpdatedFields.Contains(f.ID)).ToList();

            client.Connection.SendPacket(response);
        }

        void Receive_RequestField(XConnection connection, GenericPacket request)
        {
            // received by server from client

            // get client
            SyncClient client;
            if (!SyncClients.TryGetValue(connection.GetHashCode(), out client))
            {
                Log("Request field: Instance request received, but sync client not found");
                return;
            }

            // get thread - client can have multiple ui threads viewing instances
            var threadID = int.Parse(request.Data["ThreadID"]);

            InstanceModel instance;
            if (!client.SelectedInstances.TryGetValue(threadID, out instance))
            {
                Log("Request field: instance not found " + threadID.ToString());
                return;
            }

            // get field
            var fieldID = int.Parse(request.Data["FieldID"]);

            IFieldModel field;
            if (!instance.FieldMap.TryGetValue(fieldID, out field))
            {
                Log("Request field: field not found " + fieldID.ToString());
                return;
            }

            field.ExpandField();

            // create packet with expanded results and send
            var response = new InstancePacket()
            {
                Type = InstancePacketType.Field,
                ThreadID = threadID,
                FieldID = fieldID,
                Fields = field.Nodes
            };

            client.Connection.SendPacket(response);
        }

        private void Receive_Instance(XConnection connection, G2ReceivedPacket rawPacket)
        {
            // received by client from server

            var packet = InstancePacket.Decode(rawPacket.Root);

            XUI ui;
            if (!XRay.UIs.TryGetValue(packet.ThreadID, out ui))
            {
                Log("Receive Instance: UI not found for instance result");
                return;
            }

            if (ui.CurrentInstance == null)
            {
                Log("Receive Instance: Field not set");
                return;
            }

            bool updateTree = false;
            bool updateFields = false;
            IFieldModel expandField = null;

            var instance = ui.CurrentInstance;

            if (packet.Details != null)
                instance.DetailsLabel = packet.Details;

            if (packet.Columns != null)
            {
                instance.Columns = packet.Columns;
                updateTree = true;
            }
            
            if (packet.Type == InstancePacketType.Root)
            {
                instance.RootNodes = packet.Fields;
                updateTree = true;
                updateFields = true;
            }
            else if(packet.Type == InstancePacketType.Field)
            {
                IFieldModel field;
                if (instance.FieldMap.TryGetValue(packet.FieldID, out field))
                    field.Nodes = packet.Fields;

                updateFields = true;
                expandField = field;
            }
            else if (packet.Type == InstancePacketType.Refresh)
            {
                foreach (var field in packet.Fields)
                    if (instance.FieldMap.ContainsKey(field.ID))
                        instance.FieldMap[field.ID].Cells = field.Cells;

                updateTree = true; // cause recursive update of cells
                // dont update fields because already added, and would orphen fields associated with nodes
            }

            if (updateFields)
                foreach (var field in packet.Fields)
                    instance.FieldMap[field.ID] = field;

            // if we're geting fresh info, or refresh info with new columns
            if (updateTree && instance.UpdatedTree != null)
                ui.Window.BeginInvoke(new Action(() => instance.UpdatedTree()));

            if (expandField != null && instance.ExpandedField != null)
                ui.Window.BeginInvoke(new Action(() => instance.ExpandedField(expandField)));
        }

        internal void SendClientSettings()
        {
            if (XRay.IsInvokeRequired())
            {
                XRay.RunInCoreAsync(() => SendClientSettings());
                return;
            }

            if (ServerConnection == null)
                return;

            var packet = new GenericPacket("Settings");

            packet.Data = new Dictionary<string, string>()
            {
                {"TargetFps", XRay.TargetFps.ToString()}
            };

            ServerConnection.SendPacket(packet);
        }

        void Receive_Settings(XConnection connection, GenericPacket request)
        {
            // received by server from client
       
            SyncClient client;
            if (!SyncClients.TryGetValue(connection.GetHashCode(), out client))
            {
                Log("Request field: Settings received, but sync client not found");
                return;
            }

            if (request.Data.ContainsKey("TargetFps"))
                client.TargetFps = int.Parse(request.Data["TargetFps"]);
        }

    }

    class Download
    {
        public XConnection Connection;
        public FileStream Stream;
        public long FilePos;
    }
}
