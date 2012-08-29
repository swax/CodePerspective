using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace XLibrary.Remote
{
    public static class PacketType
    {
        public const byte Padding = 0x10;
        public const byte Generic = 0x20;
        public const byte Dat = 0x30;
        public const byte Sync = 0x40;
    }

    public class GenericPacket : G2Packet
    {
        const byte Packet_Item = 0x10;
        const byte Packet_Key = 0x20;
        const byte Packet_Value = 0x30;


        public string Name;
        public Dictionary<string, string> Data;

        public GenericPacket()
        {
        }
        
        public GenericPacket(string name)
        {
            Name = name;
        }

        public override byte[] Encode(G2Protocol protocol)
        {
            lock (protocol.WriteSection)
            {
                var packet = protocol.WritePacket(null, PacketType.Generic, UTF8Encoding.UTF8.GetBytes(Name));

                if(Data != null)
                    foreach (var item in Data)
                    {
                        var keyValuePair = protocol.WritePacket(packet, Packet_Item, null);
                        protocol.WritePacket(keyValuePair, Packet_Key, UTF8Encoding.UTF8.GetBytes(item.Key));
                        protocol.WritePacket(keyValuePair, Packet_Value, UTF8Encoding.UTF8.GetBytes(item.Value));
                    }               

                return protocol.WriteFinish();
            }
        }

        public static GenericPacket Decode(G2Header root)
        {
            var generic = new GenericPacket();

            if (G2Protocol.ReadPayload(root))
                generic.Name = UTF8Encoding.UTF8.GetString(root.Data, root.PayloadPos, root.PayloadSize);

            foreach(var child in G2Protocol.EnumerateChildren(root))
            {
                if(generic.Data == null)
                    generic.Data = new Dictionary<string, string>();

                string key = null;
                string value = null;

                foreach(var sub in G2Protocol.EnumerateChildren(child))
                    if (sub.Name == Packet_Key)
                        key = UTF8Encoding.UTF8.GetString(sub.Data, sub.PayloadPos, sub.PayloadSize);
                    else if(sub.Name == Packet_Value)
                        value = UTF8Encoding.UTF8.GetString(sub.Data, sub.PayloadPos, sub.PayloadSize);

                generic.Data[key] = value;           
            }

            return generic;
        }
    }

    public class DatPacket : G2Packet
    {
        const byte Packet_Pos = 0x10;
        const byte Packet_Data = 0x20;

        public long Pos;
        public byte[] Data;

        public DatPacket()
        {
        }

        public DatPacket(long pos, byte[] data)
        {
            Pos = pos;
            Data = data;
        }

        public override byte[] Encode(G2Protocol protocol)
        {
            lock (protocol.WriteSection)
            {
                var dat = protocol.WritePacket(null, PacketType.Dat, null);

                protocol.WritePacket(dat, Packet_Pos, BitConverter.GetBytes(Pos));
                protocol.WritePacket(dat, Packet_Data, Data);

                return protocol.WriteFinish();
            }
        }

        public static DatPacket Decode(G2Header root)
        {
            var dat = new DatPacket();

            foreach(var child in G2Protocol.EnumerateChildren(root))
            {
                switch (child.Name)
                {
                    case Packet_Pos:
                        dat.Pos = BitConverter.ToInt64(child.Data, child.PayloadPos);
                        break;

                    case Packet_Data:
                        dat.Data = Utilities.ExtractBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;
                }
            }

            return dat;
        }
    }

    public class SyncPacket : G2Packet
    {
        const byte Packet_FunctionHit = 0x10;
        const byte Packet_ExceptionHit = 0x20;
        const byte Packet_ConstructedHit = 0x30;
        const byte Packet_DisposedHit = 0x40;
        const byte Packet_NewCalls = 0x50;
        const byte Packet_CallHits = 0x60;
        const byte Packet_Inits = 0x70;
        const byte Packet_NewThreads = 0x80;
        const byte Packet_ThreadChanges = 0x90;

        const byte Packet_ThreadID = 0x10;
        const byte Packet_ThreadName = 0x20;
        const byte Packet_ThreadAlive = 0x30;


        public HashSet<int> FunctionHits;
        public HashSet<int> ExceptionHits;
        public HashSet<int> ConstructedHits;
        public HashSet<int> DisposeHits;

        public PairList NewCalls;
        public HashSet<int> CallHits;

        public PairList Inits;

        public Dictionary<int, Tuple<string, bool>> NewThreads;
        public Dictionary<int, bool> ThreadChanges;


        public override byte[] Encode(G2Protocol protocol)
        {
            lock (protocol.WriteSection)
            {
                var sync = protocol.WritePacket(null, PacketType.Sync, null);

                AddSet(protocol, sync, Packet_FunctionHit, FunctionHits);
                AddSet(protocol, sync, Packet_ExceptionHit, ExceptionHits);
                AddSet(protocol, sync, Packet_ConstructedHit, ConstructedHits);
                AddSet(protocol, sync, Packet_DisposedHit, DisposeHits);

                AddPairs(protocol, sync, Packet_NewCalls, NewCalls);
                AddSet(protocol, sync, Packet_CallHits, CallHits);

                AddPairs(protocol, sync, Packet_Inits, Inits);

                if (NewThreads != null)
                    foreach (var item in NewThreads)
                    {
                        var keyValuePair = protocol.WritePacket(sync, Packet_NewThreads, null);
                        protocol.WritePacket(keyValuePair, Packet_ThreadID, BitConverter.GetBytes(item.Key));
                        protocol.WritePacket(keyValuePair, Packet_ThreadName, UTF8Encoding.UTF8.GetBytes(item.Value.Item1));
                        protocol.WritePacket(keyValuePair, Packet_ThreadAlive, BitConverter.GetBytes(item.Value.Item2));
                    }

                if (ThreadChanges != null)
                    foreach (var item in ThreadChanges)
                    {
                        var keyValuePair = protocol.WritePacket(sync, Packet_ThreadChanges, null);
                        protocol.WritePacket(keyValuePair, Packet_ThreadID, BitConverter.GetBytes(item.Key));
                        protocol.WritePacket(keyValuePair, Packet_ThreadAlive, BitConverter.GetBytes(item.Value));
                    }

                return protocol.WriteFinish();
            }
        }

        private void AddSet(G2Protocol protocol, G2Frame sync, byte name, HashSet<int> set)
        {
            if (set != null && set.Count > 0)
                protocol.WritePacket(sync, name, set.ToBytes());
        }

        private void AddPairs(G2Protocol protocol, G2Frame sync, byte name, PairList pairs)
        {
            if (pairs != null && pairs.Count > 0)
                protocol.WritePacket(sync, name, pairs.ToBytes());
        }

        public static SyncPacket Decode(G2Header root)
        {
            var sync = new SyncPacket();

            foreach(var child in G2Protocol.EnumerateChildren(root))
            {
                switch (child.Name)
                {
                    case Packet_FunctionHit:
                        sync.FunctionHits = HashSetExt.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_ExceptionHit:
                        sync.ExceptionHits = HashSetExt.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_ConstructedHit:
                        sync.ConstructedHits = HashSetExt.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_DisposedHit:
                        sync.DisposeHits = HashSetExt.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_NewCalls:
                        sync.NewCalls = PairList.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_CallHits:
                        sync.CallHits = HashSetExt.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_Inits:
                        sync.Inits = PairList.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_NewThreads:
                        if (sync.NewThreads == null)
                            sync.NewThreads = new Dictionary<int, Tuple<string, bool>>(); 

                        int id = 0;
                        string name = null;
                        bool alive = false;

                        foreach (var sub in G2Protocol.EnumerateChildren(child))
                            if (sub.Name == Packet_ThreadID)
                                id = BitConverter.ToInt32(sub.Data, sub.PayloadPos);
                            else if (sub.Name == Packet_ThreadName)
                                name = UTF8Encoding.UTF8.GetString(sub.Data, sub.PayloadPos, sub.PayloadSize);
                            else if (sub.Name == Packet_ThreadAlive)
                                alive = BitConverter.ToBoolean(sub.Data, sub.PayloadPos);

                        sync.NewThreads[id] = new Tuple<string,bool>(name, alive);
                        break;

                    case Packet_ThreadChanges:
                        if (sync.ThreadChanges == null)
                            sync.ThreadChanges = new Dictionary<int, bool>();

                        int id2 = 0;
                        bool alive2 = false;

                        foreach (var sub in G2Protocol.EnumerateChildren(child))
                            if (sub.Name == Packet_ThreadID)
                                id2 = BitConverter.ToInt32(sub.Data, sub.PayloadPos);
                            else if (sub.Name == Packet_ThreadAlive)
                                alive2 = BitConverter.ToBoolean(sub.Data, sub.PayloadPos);

                        sync.ThreadChanges[id2] = alive2;
                        break;
                }
            }

            return sync;
        }
    }

    public static class HashSetExt
    {
        public static byte[] ToBytes(this HashSet<int> set)
        {
            if (set.Count == 0)
                return null;

            byte[] result = new byte[set.Count * 4];
            int i = 0;

            foreach (int id in set)
            {
                BitConverter.GetBytes(id).CopyTo(result, i * 4);
                i++;
            }

            return result;
        }

        public static HashSet<int> FromBytes(byte[] data, int pos, int size)
        {
            HashSet<int> result = new HashSet<int>();

            for (int i = pos; i < pos + size; i += 4)
                result.Add(BitConverter.ToInt32(data, i));

            return result;
        }

                public static void Test()
        {
            HashSet<int> set = new HashSet<int>();
            set.Add(1); set.Add(22); set.Add(433); set.Add(766);

            var bytes = set.ToBytes();

            var checkSet = FromBytes(bytes, 0, bytes.Length);
        }
    }

    public class CryptPadding : G2Packet
    {
        public byte[] Filler;


        public override byte[] Encode(G2Protocol protocol)
        {
            lock (protocol.WriteSection)
            {
                protocol.WritePacket(null, PacketType.Padding, Filler);
                return protocol.WriteFinish();
            }
        }

        public static CryptPadding Decode(G2ReceivedPacket packet)
        {
            CryptPadding padding = new CryptPadding();
            return padding;
        }
    }
}
