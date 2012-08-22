using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Generic;


namespace XLibrary.Remote
{
    public enum ProxyType { Unset, Server, ClientBlocked, ClientNAT };
    public enum TcpState { Connecting, Connected, Closed };

    public class XConnection
    {
        public XRemote Remote;

        // client info
        public IPAddress RemoteIP;
        public ushort TcpPort;

        // socket info
        public Socket TcpSocket = null;
        public TcpState State = TcpState.Connecting;
        public int Age;
        public bool Outbound;
        public string ByeMessage;


        // bandwidth
        public int BytesReceivedinSec;
        public int BytesSentinSec;

        int SecondsDead;

        const int BUFF_SIZE = 16 * 1024;

        // sending
        ICryptoTransform Encryptor;
        byte[] SendBuffer;
        int SendBuffSize;
        byte[] FinalSendBuffer;
        int FinalSendBuffSize;


        // receiving
        ICryptoTransform Decryptor;
        public byte[] RecvBuffer;
        public int RecvBuffSize;
        public byte[] FinalRecvBuffer;
        public int FinalRecvBuffSize;

        // bandwidth
        public BandwidthLog Bandwidth;

        G2Protocol Protocol = new G2Protocol();


        // inbound
        public XConnection(XRemote remote)
        {
            Remote = remote;
            Bandwidth = new BandwidthLog(5);
        }

        // outbound
        public XConnection(XRemote remote, IPAddress ip, ushort tcpPort)
        {
            Remote = remote;
            Bandwidth = new BandwidthLog(5);

            Outbound = true;

            RemoteIP = ip;
            TcpPort = tcpPort;

            try
            {
                IPEndPoint endpoint = new IPEndPoint(RemoteIP, TcpPort);

                TcpSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                TcpSocket.BeginConnect((EndPoint)endpoint, new AsyncCallback(Socket_Connect), TcpSocket);
            }
            catch (Exception ex)
            {
                Log("TcpSocket", ex.Message);
                Disconnect();
            }
        }

        public void SecondTimer()
        {
            if (State == TcpState.Closed)
                return;

            // update bandwidth
            SecondsDead = (BytesReceivedinSec > 0) ? 0 : SecondsDead + 1;

            BytesSentinSec = 0;
            BytesReceivedinSec = 0;

            Remote.Bandwidth.InPerSec += Bandwidth.InPerSec;
            Remote.Bandwidth.OutPerSec += Bandwidth.OutPerSec;
            Bandwidth.NextSecond();

            if (Age < 60)
                Age++;

            // if proxy not set after 10 secs disconnect
            if (Age > 10 && State == TcpState.Connecting)
            {
                CleanClose("Timed out");
                return;
            }

            // send ping if dead for x secs
            if (SecondsDead > 30 && SecondsDead % 5 == 0)
            {
                SendPacket(new GenericPacket("Ping"));
            }
            else if (SecondsDead > 60)
            {
                CleanClose("Minute dead");
                return;
            }

            // flush send buffer
            TrySend();
        }

        public void Socket_Connect(IAsyncResult asyncResult)
        {
            try
            {
                TcpSocket.EndConnect(asyncResult);

                OnConnect();
            }
            catch (Exception ex)
            {
                Log("Socket_Connect", ex.Message);
                Disconnect();
            }
        }

        public void OnConnect()
        {
            Log("OnConnect", "Connected to " + ToString());

            SetConnected();

            CreateEncryptor();

            Remote.OnConnected(this);
        }

        private void CreateEncryptor()
        {
            RijndaelManaged crypt = new RijndaelManaged();
            crypt.Key = Remote.Encryption.Key;
            crypt.Padding = PaddingMode.None;

            Encryptor = crypt.CreateEncryptor();

            crypt.IV.CopyTo(FinalSendBuffer, 0);
            FinalSendBuffSize = crypt.IV.Length;
        }

        public void SetConnected()
        {
            SendBuffer = new byte[BUFF_SIZE];
            RecvBuffer = new byte[BUFF_SIZE];
            FinalRecvBuffer = new byte[BUFF_SIZE];
            FinalSendBuffer = new byte[BUFF_SIZE];

            State = TcpState.Connected;

            try
            {
                TcpSocket.BeginReceive(RecvBuffer, RecvBuffSize, RecvBuffer.Length, SocketFlags.None, new AsyncCallback(Socket_Receive), TcpSocket);
            }
            catch (Exception ex)
            {
                Log("SetConnected", ex.Message);
                Disconnect();
            }
        }

        public void CleanClose(string reason)
        {
            CleanClose(reason, false);
        }

        public void CleanClose(string reason, bool reconnect)
        {
            if (State == TcpState.Connecting)
                ByeMessage = reason;

            if (State == TcpState.Connected)
            {
                ByeMessage = reason;

                var bye = new GenericPacket("Bye");

                bye.Data = new Dictionary<string, string>
                {
                    {"Message", reason}
                };

                SendPacket(bye);

                Log("CleanClose", "Closing connection: " + reason);
            }

            State = TcpState.Closed;
        }

        public void Disconnect()
        {
            if (State != TcpState.Closed)
            {
                try
                {
                    TcpSocket.Close();
                }
                catch (Exception ex)
                {
                    Log("Disconnect", ex.Message);
                }
            }

            State = TcpState.Closed;
        }

        public int SendPacket(G2Packet packet)
        {
            if (State != TcpState.Connected)
                return 0;

            if (Encryptor == null)
                CreateEncryptor();

            try
            {
                byte[] encoded = packet.Encode(Protocol);
                PacketLogEntry logEntry = new PacketLogEntry(DateTime.Now, DirectionType.Out, ToString(), encoded);
                LogPacket(logEntry);


                lock (FinalSendBuffer)
                {
                    // fill up final buffer, keep encrypt buffer clear
                    if (BUFF_SIZE - FinalSendBuffSize < encoded.Length + 128)
                        throw new Exception("SendBuff Full"); //crit check packet log

                    // encrypt
                    encoded.CopyTo(SendBuffer, SendBuffSize);
                    SendBuffSize += encoded.Length;

                    int remainder = SendBuffSize % Encryptor.InputBlockSize;
                    if (remainder > 0)
                    {
                        CryptPadding padding = new CryptPadding();

                        int fillerNeeded = Encryptor.InputBlockSize - remainder;

                        if (fillerNeeded > 2)
                            padding.Filler = new byte[fillerNeeded - 2];

                        encoded = padding.Encode(Protocol);
                        encoded.CopyTo(SendBuffer, SendBuffSize);
                        SendBuffSize += encoded.Length;
                    }

                    int tryTransform = SendBuffSize - (SendBuffSize % Encryptor.InputBlockSize);
                    if (tryTransform == 0)
                        return 0;

                    int tranformed = Encryptor.TransformBlock(SendBuffer, 0, tryTransform, FinalSendBuffer, FinalSendBuffSize);
                    if (tranformed == 0)
                        return 0;

                    FinalSendBuffSize += tranformed;
                    SendBuffSize -= tranformed;
                    Buffer.BlockCopy(SendBuffer, tranformed, SendBuffer, 0, SendBuffSize);
                }

                TrySend();

                // record bandwidth
                return encoded.Length;
            }
            catch (Exception ex)
            {
                Log("SendPacket", ex.Message);
            }

            return 0;
        }

        private void LogPacket(PacketLogEntry logEntry)
        {
            lock (Remote.LoggedPackets)
            {
                Remote.LoggedPackets.Enqueue(logEntry);

                while (Remote.LoggedPackets.Count > 50)
                    Remote.LoggedPackets.Dequeue();
            }
        }

        public void TrySend()
        {
            if (FinalSendBuffSize == 0)
                return;

            try
            {
                lock (FinalSendBuffer)
                {
                    int bytesSent = 0;

                    //Core.UpdateConsole("Begin Send " + SendBufferSize.ToString());
                
                    TcpSocket.Blocking = false;
                    bytesSent = TcpSocket.Send(FinalSendBuffer, FinalSendBuffSize, SocketFlags.None);


                    if (bytesSent > 0)
                    {
                        FinalSendBuffSize -= bytesSent;
                        BytesSentinSec += bytesSent;

                        Bandwidth.OutPerSec += bytesSent;

                        if (FinalSendBuffSize < 0)
                            throw new Exception("Tcp SendBuff size less than zero");

                        // realign send buffer
                        if (FinalSendBuffSize > 0)
                            lock (FinalSendBuffer)
                                Buffer.BlockCopy(FinalSendBuffer, bytesSent, FinalSendBuffer, 0, FinalSendBuffSize);
                    }
                }
            }

            catch (Exception ex)
            {
                Log("TrySend", ex.Message);
                Disconnect();
            }
        }


        void Socket_Receive(IAsyncResult asyncResult)
        {
            try
            {
                int recvLength = TcpSocket.EndReceive(asyncResult);
                //Core.UpdateConsole(recvLength.ToString() + " received");

                if (recvLength <= 0)
                {
                    Disconnect();
                    return;
                }

                OnReceive(recvLength);
            }
            catch (Exception ex)
            {
                Log("Socket_Receive:1", ex.Message);
                Disconnect();
            }

            try
            {
                if (State == TcpState.Connected)
                    TcpSocket.BeginReceive(RecvBuffer, RecvBuffSize, RecvBuffer.Length, SocketFlags.None, new AsyncCallback(Socket_Receive), TcpSocket);
            }
            catch (Exception ex)
            {
                Log("Socket_Receive:2", ex.Message);
                Disconnect();
            }
        }

        public void OnReceive(int length)
        {
            if (State != TcpState.Connected)
                return;

            if (length <= 0)
            {
                Disconnect();
                return;
            }

            Bandwidth.InPerSec += length;
            BytesReceivedinSec += length;
            RecvBuffSize += length;

            // transfer to final recv buffer

            //create decryptor
            if (Decryptor == null)
            {
                int ivlen = 16;

                if (RecvBuffSize < ivlen)
                    return;

                RijndaelManaged crypt = new RijndaelManaged();
                crypt.Key = Remote.Encryption.Key;
                crypt.IV = Utilities.ExtractBytes(RecvBuffer, 0, ivlen);
                crypt.Padding = PaddingMode.None;

                Decryptor = crypt.CreateDecryptor();

                RecvBuffSize -= ivlen;

                if (RecvBuffSize == 0)
                    return;

                Buffer.BlockCopy(RecvBuffer, ivlen, RecvBuffer, 0, RecvBuffSize);
            }

            // decrypt
            int tryTransform = RecvBuffSize - (RecvBuffSize % Decryptor.InputBlockSize);
            if (tryTransform == 0)
                return;

            int transformed = Decryptor.TransformBlock(RecvBuffer, 0, tryTransform, FinalRecvBuffer, FinalRecvBuffSize);
            if (transformed == 0)
                return;

            FinalRecvBuffSize += transformed;
            RecvBuffSize -= transformed;
            Buffer.BlockCopy(RecvBuffer, transformed, RecvBuffer, 0, RecvBuffSize);


            ReceivePackets();
        }

        G2ReceivedPacket LastPacket; //crit delete

        void ReceivePackets()
        {
            int Start = 0;
            G2ReadResult streamStatus = G2ReadResult.PACKET_GOOD;

            while (streamStatus == G2ReadResult.PACKET_GOOD)
            {
                G2ReceivedPacket packet = new G2ReceivedPacket();
                packet.Root = new G2Header(FinalRecvBuffer);

                streamStatus = G2Protocol.ReadNextPacket(packet.Root, ref Start, ref FinalRecvBuffSize);

                if (streamStatus != G2ReadResult.PACKET_GOOD)
                    break;

                packet.Tcp = this;

                // extract data from final recv buffer so it can be referenced without being overwritten by this thread
                byte[] extracted = Utilities.ExtractBytes(packet.Root.Data, packet.Root.PacketPos, packet.Root.PacketSize);
                packet.Root = new G2Header(extracted);
                G2Protocol.ReadPacket(packet.Root);

                LastPacket = packet;

                PacketLogEntry logEntry = new PacketLogEntry(DateTime.Now, DirectionType.In, ToString(), packet.Root.Data);
                LogPacket(logEntry);

                Remote.IncomingPacket(this, packet);
            }

            // re-align buffer
            if (Start > 0 && FinalRecvBuffSize > 0)
            {
                Buffer.BlockCopy(FinalRecvBuffer, Start, FinalRecvBuffer, 0, FinalRecvBuffSize);
                //Network.UpdateConsole(PacketBytesReady.ToString() + " bytes moved to front of receive buffer");
            }
        }

        void Log(string where, string message)
        {
            Remote.Log("XConnection(" + ToString() + ")::" + where + ": " + message);
        }

        public override string ToString()
        {
            return RemoteIP.ToString() + ":" + TcpPort.ToString();
        }
    }
}
