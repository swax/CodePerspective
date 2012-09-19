using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

namespace XLibrary.Remote
{
    public static class PacketType
    {
        public const byte Padding = 0x10;
        public const byte Generic = 0x20;
        public const byte Dat = 0x30;
        public const byte Sync = 0x40;
        public const byte Instance = 0x50;
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

        public static void Test()
        {
            // create
            var packet = new GenericPacket("name");
            packet.Data = new Dictionary<string, string>()
            {
                {"test1", "val1"},
                {"test2", "val2"}
            };

            // send
            var protocol = new G2Protocol();
            var encoded = packet.Encode(protocol);
            
            // recv
            var recvPacket = new G2Header(encoded);
            int start = 0;
            int size = encoded.Length;
            var streamStatus = G2Protocol.ReadNextPacket(recvPacket, ref start, ref size);

            // decode
            var check = GenericPacket.Decode(recvPacket);

            Debug.Assert(check.Data["test1"] == "val1");
            Debug.Assert(check.Data["test2"] == "val2");
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
        const byte Packet_NodeThreads = 0xA0;
        const byte Packet_CallThreads = 0xB0;
        const byte Packet_ClassCallThreads = 0xC0;
        const byte Packet_Threadlines = 0xD0;
        const byte Packet_ThreadStacks = 0xE0;
        const byte Packet_CallStats = 0xF0;

        const byte ChildPacket_ThreadID = 0x10;
        const byte ChildPacket_ThreadName = 0x20;
        const byte ChildPacket_ThreadAlive = 0x30;
        const byte ChildPacket_PairList = 0x40;

        public HashSet<int> FunctionHits;
        public HashSet<int> ExceptionHits;
        public HashSet<int> ConstructedHits;
        public HashSet<int> DisposeHits;

        public PairList NewCalls;
        public HashSet<int> CallHits;
        public List<CallStat> CallStats;

        public PairList Inits;

        public Dictionary<int, Tuple<string, bool>> NewThreads;
        public Dictionary<int, bool> ThreadChanges;
        public PairList NodeThreads;
        public PairList CallThreads;
        public Dictionary<int, PairList> Threadlines;
        public Dictionary<int, PairList> ThreadStacks;


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

                if (CallStats != null)
                    protocol.WritePacket(sync, Packet_CallStats, CallStats.ToBytes());

                AddPairs(protocol, sync, Packet_Inits, Inits);

                if (NewThreads != null)
                    foreach (var item in NewThreads)
                    {
                        var keyValuePair = protocol.WritePacket(sync, Packet_NewThreads, null);
                        protocol.WritePacket(keyValuePair, ChildPacket_ThreadID, BitConverter.GetBytes(item.Key));
                        protocol.WritePacket(keyValuePair, ChildPacket_ThreadName, UTF8Encoding.UTF8.GetBytes(item.Value.Item1));
                        protocol.WritePacket(keyValuePair, ChildPacket_ThreadAlive, BitConverter.GetBytes(item.Value.Item2));
                    }

                if (ThreadChanges != null)
                    foreach (var item in ThreadChanges)
                    {
                        var keyValuePair = protocol.WritePacket(sync, Packet_ThreadChanges, null);
                        protocol.WritePacket(keyValuePair, ChildPacket_ThreadID, BitConverter.GetBytes(item.Key));
                        protocol.WritePacket(keyValuePair, ChildPacket_ThreadAlive, BitConverter.GetBytes(item.Value));
                    }

                AddPairs(protocol, sync, Packet_NodeThreads, NodeThreads);
                AddPairs(protocol, sync, Packet_CallThreads, CallThreads);

                AddPairMap(protocol, sync, Packet_Threadlines, Threadlines);
                AddPairMap(protocol, sync, Packet_ThreadStacks, ThreadStacks);

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

        private void AddPairMap(G2Protocol protocol, G2Frame sync, byte name, Dictionary<int, PairList> map)
        {
            if (map != null && map.Count > 0)
                foreach (var pairList in map)
                {
                    var keyValuePair = protocol.WritePacket(sync, name, null);
                    protocol.WritePacket(keyValuePair, ChildPacket_ThreadID, BitConverter.GetBytes(pairList.Key));
                    protocol.WritePacket(keyValuePair, ChildPacket_PairList, pairList.Value.ToBytes());
                }
        }

        public static SyncPacket Decode(G2Header root)
        {
            var sync = new SyncPacket();

            foreach (var child in G2Protocol.EnumerateChildren(root))
            {
                switch (child.Name)
                {
                    case Packet_FunctionHit:
                        sync.FunctionHits = PacketExts.HashSetFromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_ExceptionHit:
                        sync.ExceptionHits = PacketExts.HashSetFromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_ConstructedHit:
                        sync.ConstructedHits = PacketExts.HashSetFromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_DisposedHit:
                        sync.DisposeHits = PacketExts.HashSetFromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_NewCalls:
                        sync.NewCalls = PairList.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_CallHits:
                        sync.CallHits = PacketExts.HashSetFromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;
                        
                    case Packet_CallStats:
                        sync.CallStats = PacketExts.StatsFromBytes(child.Data, child.PayloadPos, child.PayloadSize);
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
                            if (sub.Name == ChildPacket_ThreadID)
                                id = BitConverter.ToInt32(sub.Data, sub.PayloadPos);
                            else if (sub.Name == ChildPacket_ThreadName)
                                name = UTF8Encoding.UTF8.GetString(sub.Data, sub.PayloadPos, sub.PayloadSize);
                            else if (sub.Name == ChildPacket_ThreadAlive)
                                alive = BitConverter.ToBoolean(sub.Data, sub.PayloadPos);

                        sync.NewThreads[id] = new Tuple<string, bool>(name, alive);
                        break;

                    case Packet_ThreadChanges:
                        if (sync.ThreadChanges == null)
                            sync.ThreadChanges = new Dictionary<int, bool>();

                        int id2 = 0;
                        bool alive2 = false;

                        foreach (var sub in G2Protocol.EnumerateChildren(child))
                            if (sub.Name == ChildPacket_ThreadID)
                                id2 = BitConverter.ToInt32(sub.Data, sub.PayloadPos);
                            else if (sub.Name == ChildPacket_ThreadAlive)
                                alive2 = BitConverter.ToBoolean(sub.Data, sub.PayloadPos);

                        sync.ThreadChanges[id2] = alive2;
                        break;

                    case Packet_NodeThreads:
                        sync.NodeThreads = PairList.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_CallThreads:
                        sync.CallThreads = PairList.FromBytes(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_Threadlines:
                        ReadPairListMap(ref sync.Threadlines, sync, child);
                        break;

                    case Packet_ThreadStacks:
                        ReadPairListMap(ref sync.ThreadStacks, sync, child);
                        break;
                }
            }

            return sync;
        }

        private static void ReadPairListMap(ref Dictionary<int, PairList> map, SyncPacket sync, G2Header child)
        {
            if (map == null)
                map = new Dictionary<int, PairList>();

            int id = 0;
            PairList list = new PairList();

            foreach (var sub in G2Protocol.EnumerateChildren(child))
                if (sub.Name == ChildPacket_ThreadID)
                    id = BitConverter.ToInt32(sub.Data, sub.PayloadPos);
                else if (sub.Name == ChildPacket_PairList)
                    list = PairList.FromBytes(sub.Data, sub.PayloadPos, sub.PayloadSize);

            map[id] = list;
        }
    }

    public class CallStat
    {
        public int ID;
        public int TotalHits;
        public int TotalCallTime;
        public int TotalTimeOutsideDest;

        public CallStat()
        { }

        public CallStat(FunctionCall call)
        {
            ID = call.ID;
            TotalHits = call.TotalHits;
            TotalCallTime = call.TotalCallTime;
            TotalTimeOutsideDest = call.TotalTimeOutsideDest;
        }
    }

    public enum InstancePacketType { Root = 1, Field = 2, Refresh = 3 }

    public class InstancePacket : G2Packet
    {
        const byte Packet_Details = 0x10;
        const byte Packet_Column = 0x20;
        const byte Packet_Field = 0x30;
        const byte Packet_Cell = 0x40;
        const byte Packet_SubNodesFlag = 0x50;
        const byte Packet_ThreadID = 0x60;
        const byte Packet_FieldID = 0x70;
        const byte Packet_Type = 0x80;


        public InstancePacketType Type;
        public int ThreadID;
        public int FieldID;
        public string Details;
        public List<string> Columns;
        public List<IFieldModel> Fields;


        public override byte[] Encode(G2Protocol protocol)
        {
            lock (protocol.WriteSection)
            {
                var instance = protocol.WritePacket(null, PacketType.Instance, null);

                protocol.WritePacket(instance, Packet_Type, BitConverter.GetBytes((int)Type));
                protocol.WritePacket(instance, Packet_ThreadID, BitConverter.GetBytes(ThreadID));
                protocol.WritePacket(instance, Packet_FieldID, BitConverter.GetBytes(FieldID));

                if(Details != null)
                    protocol.WritePacket(instance, Packet_Details, UTF8Encoding.UTF8.GetBytes(Details));

                if (Columns != null)
                    foreach(var column in Columns)
                        protocol.WritePacket(instance, Packet_Column, UTF8Encoding.UTF8.GetBytes(column));

                if (Fields != null)
                    foreach (var field in Fields)
                    {
                        var fieldPacket = protocol.WritePacket(instance, Packet_Field, null);

                        protocol.WritePacket(fieldPacket, Packet_SubNodesFlag, BitConverter.GetBytes(field.PossibleSubNodes));
                        protocol.WritePacket(fieldPacket, Packet_FieldID, BitConverter.GetBytes(field.ID));

                        foreach(var cell in field.Cells)
                            protocol.WritePacket(fieldPacket, Packet_Cell, UTF8Encoding.UTF8.GetBytes(cell));
                    }

                return protocol.WriteFinish();
            }
        }

        public static InstancePacket Decode(G2Header root)
        {
            var instance = new InstancePacket();
            
            instance.Fields = new List<IFieldModel>(); // needs to be initd

            foreach (var child in G2Protocol.EnumerateChildren(root))
                switch (child.Name)
                {
                    case Packet_Type:
                        instance.Type = (InstancePacketType) BitConverter.ToInt32(child.Data, child.PayloadPos);
                        break;

                    case Packet_ThreadID:
                        instance.ThreadID = BitConverter.ToInt32(child.Data, child.PayloadPos);
                        break;

                    case Packet_FieldID:
                        instance.FieldID = BitConverter.ToInt32(child.Data, child.PayloadPos);
                        break;

                    case Packet_Details:
                        instance.Details = UTF8Encoding.UTF8.GetString(child.Data, child.PayloadPos, child.PayloadSize);
                        break;

                    case Packet_Column:
                        if (instance.Columns == null)
                            instance.Columns = new List<string>();

                        instance.Columns.Add(UTF8Encoding.UTF8.GetString(child.Data, child.PayloadPos, child.PayloadSize));
                        break;

                    case Packet_Field:
                        if(instance.Fields == null)
                            instance.Fields = new List<IFieldModel>();

                        var field = new RemoteFieldModel();

                        foreach (var sub in G2Protocol.EnumerateChildren(child))
                            switch (sub.Name)
                            {
                                case Packet_SubNodesFlag:
                                    field.PossibleSubNodes = BitConverter.ToBoolean(sub.Data, sub.PayloadPos);
                                    break;

                                case Packet_FieldID:
                                    field.ID = BitConverter.ToInt32(sub.Data, sub.PayloadPos);
                                    break;

                                case Packet_Cell:
                                    if(field.Cells == null)
                                        field.Cells = new List<string>();

                                    field.Cells.Add(UTF8Encoding.UTF8.GetString(sub.Data, sub.PayloadPos, sub.PayloadSize));
                                    break;
                            }

                        instance.Fields.Add(field);
                    
                        break;
                }

            return instance;
        }
    }

    public static class PacketExts
    {
        public static byte[] ToBytes(this ICollection<int> set)
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

        public static HashSet<int> HashSetFromBytes(byte[] data, int pos, int size)
        {
            var result = new HashSet<int>();

            for (int i = pos; i < pos + size; i += 4)
                result.Add(BitConverter.ToInt32(data, i));

            return result;
        }

        public static List<int> ListFromBytes(byte[] data, int pos, int size)
        {
            var results = new List<int>();

            for (int i = pos; i < pos + size; i += 4)
                results.Add(BitConverter.ToInt32(data, i));

            return results;
        }

        public static byte[] ToBytes(this List<CallStat> list)
        {
            byte[] data = new byte[list.Count * 16];

            for (int i = 0; i < list.Count; i++)
            {
                var stat = list[i];
                var pos = i * 16;

                BitConverter.GetBytes(stat.ID).CopyTo(data, pos);
                BitConverter.GetBytes(stat.TotalHits).CopyTo(data, pos + 4);
                BitConverter.GetBytes(stat.TotalCallTime).CopyTo(data, pos + 8);
                BitConverter.GetBytes(stat.TotalTimeOutsideDest).CopyTo(data, pos + 12);
            }

            return data;
        }

        public static List<CallStat> StatsFromBytes(byte[] data, int pos, int size)
        {
            var results = new List<CallStat>();

            for (int i = pos; i < pos + size; i += 16)
            {
                var stat = new CallStat();
                stat.ID = BitConverter.ToInt32(data, i);
                stat.TotalHits = BitConverter.ToInt32(data, i + 4);
                stat.TotalCallTime = BitConverter.ToInt32(data, i + 8);
                stat.TotalTimeOutsideDest = BitConverter.ToInt32(data, i + 12);

                results.Add(stat);
            }

            return results;
        }

        public static void Test()
        {
            HashSet<int> set = new HashSet<int>();
            set.Add(1); set.Add(22); set.Add(433); set.Add(766);

            var bytes = set.ToBytes();

            var checkSet = HashSetFromBytes(bytes, 0, bytes.Length);
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
