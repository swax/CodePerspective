using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.IO;


namespace XLibrary.Remote
{
    public class XRemote
    {
        public RijndaelManaged Encryption = new RijndaelManaged();

        public List<XConnection> Connections = new List<XConnection>();

        public List<string> DebugLog = new List<string>();

        // listening
        int ListenPort = 4566;
        Socket ListenSocket;

        // logging
        public BandwidthLog Bandwidth = new BandwidthLog(10);
        public Queue<PacketLogEntry> LoggedPackets = new Queue<PacketLogEntry>();

        Dictionary<string, Action<XConnection, GenericPacket>> RouteGeneric = new Dictionary<string, Action<XConnection, GenericPacket>>();

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
            RouteGeneric["Ping"] = Ping;
            RouteGeneric["Pong"] = Pong;

            RouteGeneric["DatHashRequest"] = DatHashRequest;
            RouteGeneric["DatHashResponse"] = DatHashResponse;

            RouteGeneric["DatFileRequest"] = DatFileRequest;
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
            DebugLog.Add(string.Format(text, args));
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
                case PacketType.Generic:

                    var generic = GenericPacket.Decode(packet.Root);

                    Log("Generic Packet Received: " + generic.Name);

                    if(RouteGeneric.ContainsKey(generic.Name))
                        RouteGeneric[generic.Name](connection, generic);
                    else
                       Log("Unknown generic packet: " + generic.Name);

                    break;

                case PacketType.Dat:
                    ReceiveDatPacket(connection, packet);
                    break;
            }
        }

        void Ping(XConnection connection, GenericPacket ping)
        {
            connection.SendPacket(new GenericPacket("Pong"));
        }

        void Pong(XConnection connection, GenericPacket pong)
        {
            // lower level socket on receiving data marks connection as alive
        }

        void DatHashRequest(XConnection connection, GenericPacket request)
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

        void DatHashResponse(XConnection connection, GenericPacket response)
        {
            // check if we have this hash.dat file locally, if not then request download
            RemoteDatHash = response.Data["Hash"];
            RemoteDatSize = long.Parse(response.Data["Size"]);

            LocalDatPath = Path.Combine(RemoteCachePath, RemoteDatHash + ".dat");
            LocalDatTempPath = Path.Combine(RemoteCachePath, RemoteDatHash + ".tmp");

            if (RemoteDatSize == 0)
                RemoteStatus = "Error - Remote Dat Empty";

            else if (File.Exists(LocalDatPath))
                StartSync();
            
            else
            {
                Log("Requesting Dat File, size: " + RemoteDatSize.ToString());

                RemoteStatus = "Requesting Dat File";

                var request = new GenericPacket("DatFileRequest");

                connection.SendPacket(request);
            }
        }

        List<Download> Downloads = new List<Download>();

        void DatFileRequest(XConnection connection, GenericPacket request)
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

        const int DownloadChunkSize = XConnection.BUFF_SIZE / 2; // should be 8kb
        List<Download> RemoveDownloads = new List<Download>();

        public void ProcessDownloads()
        {
            if (Downloads.Count == 0)
                return;

            foreach (var download in Downloads)
            {
                if (download.Connection.State != TcpState.Connected)
                {
                    RemoveDownloads.Add(download);
                    continue;
                }

                // while connection has 8kb in buffer free
                while (download.Connection.SendBufferBytesAvailable > DownloadChunkSize + 200) // read 8k, 200b overflow buffer
                {
                    // read 8k of file
                    long readSize = XRay.DatSize - download.FilePos;
                    if (readSize > DownloadChunkSize)
                        readSize = DownloadChunkSize;

                    var chunk = new DatPacket(download.FilePos, download.Stream.Read((int)readSize));
                    download.FilePos += chunk.Data.Length;

                    Log("Sending dat pos: {0}, length: {1}", chunk.Pos, chunk.Data.Length); //todo delete

                    // send
                    if(chunk.Data.Length > 0)
                        download.Connection.SendPacket(chunk);

                    // remove when complete
                    if (download.FilePos >= XRay.DatSize)
                    {
                        RemoveDownloads.Add(download);
                        break;
                    }
                }
            }

            foreach (var download in RemoveDownloads)
            {
                download.Stream.Close();
                Downloads.Remove(download);
            }
            RemoveDownloads.Clear();
        }

        void ReceiveDatPacket(XConnection connection, G2ReceivedPacket packet)
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
                    StartSync();
                }
                else
                    RemoteStatus = string.Format("Dat integrity check failed - Expecting {0}, got {1}", RemoteDatHash, checkHash);
            }

        }

        void StartSync()
        {
            RemoteStatus = "Starting Sync";

            // send packet telling server to start syncing us
        }
    }

    class Download
    {
        public XConnection Connection;
        public FileStream Stream;
        public long FilePos;
    }
}
