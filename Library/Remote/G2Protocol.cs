using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace XLibrary.Remote
{
	public enum G2ReadResult { PACKET_GOOD, PACKET_INCOMPLETE, PACKET_ERROR, STREAM_END };

	/// <summary>
	/// Summary description for G2Protocol.
	/// </summary>
	public class G2Protocol
	{
		const int MAX_FRAMES     = 100000;
		const int MAX_WRITE_SIZE = 512 * 1024;
		const int MAX_FINAL_SIZE = 512 * 1024;
        const int G2_PACKET_BUFF = (MAX_FINAL_SIZE + 1024);

		int    WriteOffset;
		byte[] WriteData = new byte[MAX_WRITE_SIZE];


        LinkedList<G2Frame> Frames = new LinkedList<G2Frame>();

		int    FinalSize;
		byte[] FinalPacket = new byte[MAX_FINAL_SIZE];
		
        public object WriteSection = new object();


		public G2Protocol()
		{
		}

		public G2Frame WritePacket(G2Frame root, byte name, byte[] payload)
		{
			// If new packet
			if(root == null)
			{
				if(WriteOffset != 0)
				{
					Frames.Clear();
					WriteOffset = 0;

					// caused by error building packet before
					throw new Exception("Packet Frames not clear");//Debug.Assert(false); // Careful, can be caused by previous assert further down call stack
				}

				FinalSize = 0;
			}

			if(Frames.Count > MAX_FRAMES)
			{
				Debug.Assert(false);
				return null;
			}

			// Create new frame
			G2Frame packet = new G2Frame();

			packet.Parent = root;
			packet.Name   = name;
	
			if(payload != null)
			{
				if(WriteOffset + payload.Length > MAX_WRITE_SIZE)
				{
					Debug.Assert(false);
					return null;
				}

				packet.PayloadLength = payload.Length;
				packet.PayloadPos    = WriteOffset;
                payload.CopyTo(WriteData, WriteOffset);
				WriteOffset += payload.Length;
			}

			Frames.AddLast(packet);

			return packet;
		}

		public byte[] WriteFinish()
		{
            // Reverse iterate through packet structure backwards, set lengths
            LinkedListNode<G2Frame> current = Frames.Last;
            while(current != null)
            {
                G2Frame packet = current.Value;

                if (packet.InternalLength > 0 && packet.PayloadLength > 0)
                    packet.InternalLength += 1; // For endstream byte

                packet.PayloadOffset = packet.InternalLength;
                packet.InternalLength += packet.PayloadLength;

                packet.LenLen = 0;
                while (packet.InternalLength >= Math.Pow(256, packet.LenLen))
                    packet.LenLen++;

                Debug.Assert(packet.LenLen < 8);

                packet.HeaderLength = 1 + packet.LenLen + 1;

                if (packet.Parent != null)
                {
                    packet.Parent.InternalLength += packet.HeaderLength + packet.InternalLength;
                    packet.Parent.Compound = 1;
                }

                current = current.Previous; 
            }

			// Iterate through packet stucture forwards, build packet
			foreach(G2Frame packet in Frames)
			{
				int nextByte = 0;

				if( packet.Parent != null)
				{
					Debug.Assert(packet.Parent.NextChild != 0);
					nextByte = packet.Parent.NextChild;
					packet.Parent.NextChild += packet.HeaderLength + packet.InternalLength;
				}
				else // beginning of packet
				{
					FinalSize = packet.HeaderLength + packet.InternalLength;
				}

				byte control = 0;
				control |= (byte) (packet.LenLen << 5);
                control |= (byte) (1 << 3);
				control |= (byte) (packet.Compound << 2);
		
				Buffer.SetByte(FinalPacket, nextByte, control);
				nextByte += 1;

				// DNA should not pass packets greater than 4096, though pass through packets could be bigger
				if(packet.HeaderLength + packet.InternalLength > MAX_WRITE_SIZE)
				{
					Debug.Assert(false);

					Frames.Clear();
					WriteOffset = 0;
					FinalSize   = 0;

					return null;
				}

				byte [] lenData = BitConverter.GetBytes(packet.InternalLength);
				Buffer.BlockCopy(lenData, 0, FinalPacket, nextByte, packet.LenLen);
				nextByte += packet.LenLen;

                FinalPacket[nextByte] = packet.Name;
				nextByte += 1;

				if(packet.Compound == 1)
					packet.NextChild = nextByte;

				if( packet.PayloadLength != 0)
				{
					int finalPos = nextByte + packet.PayloadOffset;
					Buffer.BlockCopy(WriteData, packet.PayloadPos, FinalPacket, finalPos, packet.PayloadLength);

					if(packet.Compound == 1) // Set stream end
					{
						finalPos -= 1;
						Buffer.SetByte(FinalPacket, finalPos, 0);
					}
				}
			}

			Debug.Assert(FinalSize != 0);

			Frames.Clear();
			WriteOffset = 0;

            return Utilities.ExtractBytes(FinalPacket, 0, FinalSize);
		}

		public static G2ReadResult ReadNextPacket( G2Header packet, ref int readPos, ref int readSize )
		{
			if( readSize == 0 )
				return G2ReadResult.PACKET_INCOMPLETE;

			int beginPos   = readPos;
			int beginSize  = readSize;

			packet.PacketPos = readPos;
			
			// Read Control Byte
			byte control = Buffer.GetByte(packet.Data, readPos);

			readPos  += 1;
			readSize -= 1;

			if ( control == 0 ) 
				return G2ReadResult.STREAM_END;

			byte lenLen  = (byte) ( (control & 0xE0) >> 5); // 11100000
			byte nameLen = (byte) ( (control & 0x18) >> 3); // 00011000 
			byte flags   = (byte)   (control & 0x07);       // 00000111

			bool bigEndian  = (flags & 0x02) != 0; 
			bool isCompound = (flags & 0x04) != 0; 

			if( bigEndian )
				return G2ReadResult.PACKET_ERROR;

			packet.HasChildren = isCompound;
			
			// Read Packet Length
			packet.InternalSize = 0;
			if( lenLen != 0)
			{	
				if(readSize < lenLen)
				{
					readPos  = beginPos;
					readSize = beginSize;
					return G2ReadResult.PACKET_INCOMPLETE;
				}
				
				byte[] lenData = new byte[8]; // create here because lenLen is less than 8 in size
				Buffer.BlockCopy(packet.Data, readPos, lenData, 0, lenLen);

				packet.InternalSize = BitConverter.ToInt32(lenData, 0); // only 4 bytes supported so far

				Debug.Assert(MAX_FINAL_SIZE < G2_PACKET_BUFF);
                if (packet.InternalSize < 0 || MAX_FINAL_SIZE < packet.InternalSize)
				{
					Debug.Assert(false);
					return G2ReadResult.PACKET_ERROR;
				}

				readPos  += lenLen;
				readSize -= lenLen;
			}

			// Read Packet Name
			if(readSize < nameLen)
			{
				readPos  = beginPos;
				readSize = beginSize;
				return G2ReadResult.PACKET_INCOMPLETE;
			}

            if(nameLen != 1)
                return G2ReadResult.PACKET_ERROR;

			/*if(packet.Name.Length + 1 + nameLen > MAX_NAME_SIZE - 1)
			{
				Debug.Assert(false);
				packet.Name = "ERROR";
			}
			else
			{
				packet.Name += "/" + StringEnc.GetString(packet.Data, readPos, nameLen);
			}*/

            packet.Name = packet.Data[readPos];

			readPos  += nameLen;
			readSize -= nameLen;

			// Check if full packet length available in stream
			if(readSize < packet.InternalSize)
			{
				readPos  = beginPos;
				readSize = beginSize;
				return G2ReadResult.PACKET_INCOMPLETE;
			}

			packet.InternalPos = (packet.InternalSize > 0) ? readPos : 0;
			
			packet.NextBytePos   = packet.InternalPos;
			packet.NextBytesLeft = packet.InternalSize;

			readPos  += packet.InternalSize;
			readSize -= packet.InternalSize;

			packet.PacketSize = 1 + lenLen + nameLen + packet.InternalSize;

			return G2ReadResult.PACKET_GOOD;
		}

		public static bool ReadPayload(G2Header packet)
		{
			ResetPacket(packet);

			G2Header child = new G2Header(packet.Data);

			G2ReadResult streamStatus = G2ReadResult.PACKET_GOOD;
			while( streamStatus == G2ReadResult.PACKET_GOOD )
				streamStatus = ReadNextChild(packet, child);

            bool found = false;

			if(streamStatus == G2ReadResult.STREAM_END)
			{
				if( packet.NextBytesLeft > 0)
				{
					packet.PayloadPos  = packet.NextBytePos;
					packet.PayloadSize = packet.NextBytesLeft;

                    found = true;
				}
			}
			else if( packet.NextBytesLeft > 0)
			{
				// Payload Read Error
				//m_pG2Comm->m_pCore->DebugLog("G2 Network", "Payload Read Error: " + HexDump(packet.Packet, packet.PacketSize));
			}

            packet.NextBytePos = packet.InternalPos;
            packet.NextBytesLeft = packet.InternalSize;

			return found;
		}

		public static void ResetPacket(G2Header packet)
		{
			packet.NextBytePos   = packet.InternalPos;
			packet.NextBytesLeft = packet.InternalSize;

			packet.PayloadPos  = 0;
			packet.PayloadSize = 0;
		}

		public static G2ReadResult ReadNextChild( G2Header root, G2Header child)
		{
			if( !root.HasChildren )
				return G2ReadResult.STREAM_END;

			return ReadNextPacket(child, ref root.NextBytePos, ref root.NextBytesLeft);
		}

        public static IEnumerable<G2Header> EnumerateChildren(G2Header root)
        {
             G2Header child = new G2Header(root.Data);

             while (G2Protocol.ReadNextChild(root, child) == G2ReadResult.PACKET_GOOD)
             {
                 // set payload pos vars
                 G2Protocol.ReadPayload(child);

                 yield return child;
             }
        }

        public static bool ReadPacket(G2Header root)
        {
            int start  = 0;
            int length = root.Data.Length;

            if (G2ReadResult.PACKET_GOOD == ReadNextPacket(root, ref start, ref length))
                return true;

            return false;
        }

        public int WriteToFile(G2Packet packet, Stream stream)
        {
            byte[] data = packet.Encode(this);

            stream.Write(data, 0, data.Length);

            return data.Length;
        }
    }

	/// <summary>
	/// Summary description for G2Frame.
	/// </summary>
	public class G2Frame
	{
		public G2Frame Parent;

		public int HeaderLength;
        public int InternalLength;
	
		public byte Name;

		public byte LenLen;
		public byte Compound;

		public int   NextChild;

		public int   PayloadPos;
		public int   PayloadLength;
        public int   PayloadOffset;


		public G2Frame()
		{
			
		}
	}

    public class G2Header
    {
        public byte[] Data;

        public int PacketPos;
        public int PacketSize;

        public byte Name;

        public bool HasChildren;

        public int InternalPos;
        public int InternalSize;

        public int PayloadPos;
        public int PayloadSize;

        public int NextBytePos;
        public int NextBytesLeft;

        public G2Header(byte[] data)
        {
            Data = data;
        }
    }

    public class G2Packet
    {
        public G2Packet()
        {
        }

        public virtual byte[] Encode(G2Protocol protocol)
        {
            return null;
        }
    }

    public class PacketStream
    {
        G2Protocol Protocol;
        Stream     ParentStream;
        FileAccess Access;

        byte[] ReadBuffer;
        int    ReadSize;
        int    Start;
        int Pos;

        public int ParentPos;

        G2ReadResult ReadStatus = G2ReadResult.PACKET_INCOMPLETE;


        public PacketStream(Stream stream, G2Protocol protocol, FileAccess access)
        {
            ParentStream = stream;
            Protocol = protocol;
            Access = access;

            if(access == FileAccess.Read)
                ReadBuffer = new byte[4096]; // break/resume relies on 4kb buffer
        }

        public bool ReadPacket(ref G2Header root)
        {
            root = new G2Header(ReadBuffer);

            // continue from left off, read another goo packete
            if (ReadNext(root))
                return true;

            if (ReadStatus != G2ReadResult.PACKET_INCOMPLETE)
                return false;

            // re-align
            if (ReadSize > 0)
                Buffer.BlockCopy(ReadBuffer, Start, ReadBuffer, 0, ReadSize);

            // incomplete, or just started, read some more from file
            Start = 0;
            
            int read = ParentStream.Read(ReadBuffer, ReadSize, ReadBuffer.Length - ReadSize);
            Pos += read;
            ReadSize += read;

            if (ReadNext(root))
                return true;

            return false;
        }

        private bool ReadNext(G2Header root)
        {
            if (ReadSize > 0)
            {
                int prevStart = Start;

                ReadStatus = G2Protocol.ReadNextPacket(root, ref Start, ref ReadSize);

                ParentPos += (Start - prevStart);

                if (ReadStatus == G2ReadResult.PACKET_GOOD)
                    return true;
            }

            // hit the exact end of the buffer read in, signal to read the next buffer in
            else
                ReadStatus = G2ReadResult.PACKET_INCOMPLETE;

            return false;
        }

        public int WritePacket(G2Packet packet)
        {
            byte[] data = packet.Encode(Protocol);
            ParentStream.Write(data, 0, data.Length);
            return data.Length;
        }

        public byte[] Break()
        {
            byte[] remaining = Utilities.ExtractBytes(ReadBuffer, Start, ReadSize);

            ReadSize = 0;
            ReadStatus = G2ReadResult.PACKET_INCOMPLETE;

            return remaining;
        }

        public void Resume(byte[] data, int size)
        {
            Start = 0;
            ReadSize = size;
            data.CopyTo(ReadBuffer, 0);
            ReadStatus = G2ReadResult.PACKET_INCOMPLETE;
        }
    }

    public static class CompactNum
    {
        // unsigned to bytes
        public static byte[] GetBytes(byte num)
        {
            return new byte[] { num };
        }

        public static byte[] GetBytes(ushort num)
        {
            if (byte.MinValue <= num && num <= byte.MaxValue)
                return GetBytes((byte)num);
            else
                return BitConverter.GetBytes(num);
        }

        public static byte[] GetBytes(uint num)
        {
            if (ushort.MinValue <= num && num <= ushort.MaxValue)
                return GetBytes((ushort)num);
            else
                return BitConverter.GetBytes(num);
        }

        public static byte[] GetBytes(ulong num)
        {
            if (uint.MinValue <= num && num <= uint.MaxValue)
                return GetBytes((uint)num);
            else
                return BitConverter.GetBytes(num);
        }

        // signed to bytes
        public static byte[] GetBytes(sbyte num)
        {
            return new byte[] { (byte) num };
        }

        public static byte[] GetBytes(short num)
        {
            if (sbyte.MinValue <= num && num <= sbyte.MaxValue)
                return GetBytes((sbyte)num);
            else
                return BitConverter.GetBytes(num);
        }

        public static byte[] GetBytes(int num)
        {
            if (short.MinValue <= num && num <= short.MaxValue)
                return GetBytes((short)num);
            else
                return BitConverter.GetBytes(num);
        }

        public static byte[] GetBytes(long num)
        {
            if (int.MinValue <= num && num <= int.MaxValue)
                return GetBytes((int)num);
            else
                return BitConverter.GetBytes(num);
        }

        // unsigned from bytes
        public static byte ToUInt8(byte[] data, int pos, int size)
        {
            return data[pos];
        }

        public static ushort ToUInt16(byte[] data, int pos, int size)
        {
            if (size == 1)
                return ToUInt8(data, pos, size);
            else
                return BitConverter.ToUInt16(data, pos);
        }

        public static uint ToUInt32(byte[] data, int pos, int size)
        {
            if (size <= 2)
                return ToUInt16(data, pos, size);
            else
                return BitConverter.ToUInt32(data, pos);
        }

        public static ulong ToUInt64(byte[] data, int pos, int size)
        {
            if (size <= 4)
                return ToUInt32(data, pos, size);
            else
                return BitConverter.ToUInt64(data, pos);
        }

        // signed from bytes
        public static sbyte ToInt8(byte[] data, int pos, int size)
        {
            return (sbyte) data[pos];
        }

        public static short ToInt16(byte[] data, int pos, int size)
        {
            if (size == 1)
                return ToInt8(data, pos, size);
            else
                return BitConverter.ToInt16(data, pos);
        }

        public static int ToInt32(byte[] data, int pos, int size)
        {
            if (size <= 2)
                return ToInt16(data, pos, size);
            else
                return BitConverter.ToInt32(data, pos);
        }

        public static long ToInt64(byte[] data, int pos, int size)
        {
            if (size <= 4)
                return ToInt32(data, pos, size);
            else
                return BitConverter.ToInt64(data, pos);
        }
    }
}
