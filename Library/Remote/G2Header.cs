using System;
using System.Net;
using System.Net.Sockets;


namespace XLibrary.Remote
{
	public class G2ReceivedPacket
	{
		public G2Header    Root;
		public XConnection  Tcp;
    }

	public enum DirectionType {In, Out};
	
	public class PacketLogEntry
	{
        public DateTime Time;
		public DirectionType Direction;
        public string    Address;
		public byte[]        Data;

        public PacketLogEntry(DateTime time, DirectionType direction, string address, byte[] data)
		{
            Time = time;
			Direction = direction;
			Address   = address;
			Data      = data;
		}

        public override string ToString()
        {
            return Address + " " + Direction.ToString();
        }
	}
}
