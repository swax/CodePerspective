/***
 * 
 *  ASMEX by RiskCare Ltd.
 * 
 * This source is copyright (C) 2002 RiskCare Ltd. All rights reserved.
 * 
 * Disclaimer:
 * This code is provided 'as is', with absolutely no warranty expressed or
 * implied.  Any use of this code is at your own risk.
 *   
 * You are hereby granted the right to redistribute this source unmodified
 * in its original archive. 
 * You are hereby granted the right to use this code, or code based on it,
 * provided that you acknowledge RiskCare Ltd somewhere in the documentation
 * of your application. 
 * You are hereby granted the right to distribute changes to this source, 
 * provided that:
 * 
 * 1 -- This copyright notice is retained unchanged 
 * 2 -- Your changes are clearly marked 
 * 
 * Enjoy!
 * 
 * --------------------------------------------------------------------
 * 
 * If you use this code or have comments on it, please mail me at 
 * support@jbrowse.com or ben.peterson@riskcare.com
 * 
 */


using System;
using System.IO;
using System.Collections;
using Asmex.ObjViewer;

namespace Asmex.FileViewer
{
	/// <summary>
	/// Represents a COR20 header
	/// </summary>
	public class COR20Header : Region
	{
		uint _CB;
		ushort _MajorRuntimeVersion;
		ushort _MinorRuntimeVersion;
		DataDir _MetaData;
		uint _Flags;
		uint _EntryPointToken;
		DataDir _Resources;
		DataDir _StrongNameSignature;
		DataDir _CodeManagerTable;
		DataDir _VTableFixups;
		DataDir _ExportAddressTableJumps;
		DataDir _ManagedNativeHeader;

		public uint CB{get{return _CB;}}
		public ushort MajorRuntimeVersion{get{return _MajorRuntimeVersion;}}
		public ushort MinorRuntimeVersion{get{return _MinorRuntimeVersion;}}
		public DataDir MetaData{get{return _MetaData;}}
		[ObjViewer(Hex=true)]
		public uint Flags{get{return _Flags;}}
		[ObjViewer(Hex=true)]
		public uint EntryPointToken{get{return _EntryPointToken;}}
		public DataDir Resources{get{return _Resources;}}
		public DataDir StrongNameSignature{get{return _StrongNameSignature;}}
		public DataDir CodeManagerTable{get{return _CodeManagerTable;}}
		public DataDir VTableFixups{get{return _VTableFixups;}}
		public DataDir ExportAddressTableJumps{get{return _ExportAddressTableJumps;}}
		public DataDir ManagedNativeHeader{get{return _ManagedNativeHeader;}}

		public COR20Header(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			_CB = reader.ReadUInt32();
			_MajorRuntimeVersion = reader.ReadUInt16();
			_MinorRuntimeVersion = reader.ReadUInt16();
			_MetaData = new DataDir(reader, "MetaDataDir");
			_Flags = reader.ReadUInt32();
			_EntryPointToken = reader.ReadUInt32();
			_Resources = new DataDir(reader, "ResourcesDir");
			_StrongNameSignature = new DataDir(reader, "StrongNameSignatureDir");
			_CodeManagerTable = new DataDir(reader, "CodeManagerTableDir");
			_VTableFixups = new DataDir(reader, "VTableFixupsDir");
			_ExportAddressTableJumps = new DataDir(reader, "ExportAddressTableJumpsDir");
			_ManagedNativeHeader = new DataDir(reader, "ManagedNativeHeaderDir");

			Length = reader.BaseStream.Position - Start;

		}

	}

	/// <summary>
	/// Represents the header that tells us where to find metadata streams
	/// </summary>
	public class StorageSigAndHeader : Region
	{
		ushort _MajorVersion;
		ushort _MinorVersion;
		string _VersionString;
		ushort _NumOfStreams;

		public ushort MajorVersion{get{return _MajorVersion;}}
		public ushort MinorVersion{get{return _MinorVersion;}}
		public string VersionString{get{return _VersionString;}}
		public ushort NumOfStreams{get{return _NumOfStreams;}}
		
		public StorageSigAndHeader(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			//storage signature

			uint sig = reader.ReadUInt32();
			if (sig != 0x424A5342) throw new ModException("MetaData:  Incorrect signature.");
			_MajorVersion = reader.ReadUInt16();
			_MinorVersion = reader.ReadUInt16();
			reader.ReadUInt32();//extradata (unused)
			uint versionLength = reader.ReadUInt32();

			for(int i=0;i<versionLength;++i)
			{
				_VersionString += (char)reader.ReadByte();
			}

			if ((versionLength % 4) != 0) reader.BaseStream.Position += 4 - (versionLength % 4); //padding


			//storage header
			reader.ReadByte();//flags(unused)
			reader.ReadByte();//padding
			_NumOfStreams = reader.ReadUInt16();

			Length = reader.BaseStream.Position - Start;
			
		}
	}

	/// <summary>
	/// The header that describes each metadata stream
	/// </summary>
	public class MDStreamHeader : Region
	{
		uint _Offset;
		uint _Size;
		string _Name;
		StreamType _Type;

		[ObjViewer(Hex=true)]
		public uint Offset{get{return _Offset;}}
		[ObjViewer(Hex=true)]
		public uint Size{get{return _Size;}}
		public string Name{get{return _Name;}}
		public StreamType Type{get{return _Type;}}

		public enum StreamType{StrType, BlobType, GUIDType, TableType};

		
		public MDStreamHeader(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			_Offset = reader.ReadUInt32();
			_Size = reader.ReadUInt32();

			//How IS the name stored anyway?  It seems subtly different for stream names... it's certainly not what the book says.
			char[] chars = new char[32];
			int index = 0;
			byte character = 0;
			while ((character = reader.ReadByte()) != 0) 
				chars[index++] = (char) character;
	
			index++;
			int padding = ((index % 4) != 0) ? (4 - (index % 4)) : 0;
			reader.ReadBytes(padding);
	
			_Name = new String(chars).Trim(new Char[] {'\0'});
			
			if (Name == "#Strings") 
			{
				_Type = StreamType.StrType;
			}
			else if (Name == "#US" || Name == "#Blob")
			{
				_Type = StreamType.BlobType;
			}
			else if (Name == "#GUID")
			{
				_Type = StreamType.GUIDType;
			}
			else
			{
				_Type = StreamType.TableType;
			}

			Length = reader.BaseStream.Position - Start;

		}


	}


	/// <summary>
	/// This class bundles all the MD headers, ie the main header and the header of each stream
	/// </summary>
	public class MetaDataHeaders : Region
	{
		StorageSigAndHeader _ssah;
		MDStreamHeader _strstr;
		MDStreamHeader _blobstr;
		MDStreamHeader _guidstr;
		MDStreamHeader _usstr;
		MDStreamHeader _tablestr;
		
		public MetaDataHeaders(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			_ssah = new StorageSigAndHeader(reader);

			for(int i=0; i< _ssah.NumOfStreams;++i)
			{
				MDStreamHeader mds = new MDStreamHeader(reader);
				if (mds.Name == "#Strings")
				{
					_strstr = mds;
				}
				else if (mds.Name == "#Blob")
				{
					_blobstr = mds;
				}
				else if (mds.Name == "#GUID")
				{
					_guidstr = mds;
				}
				else if (mds.Name == "#US")
				{
					_usstr = mds;
				}
				else if (mds.Name == "#~")
				{
					_tablestr = mds;
				}
				else 
				{
					_tablestr = mds;
				}
			}

			Length = reader.BaseStream.Position - Start;

		}

		public StorageSigAndHeader StorageSigAndHeader{get{return _ssah;}}

		public MDStreamHeader StringStreamHeader{get{return _strstr;}}
		public MDStreamHeader BlobStreamHeader{get{return _blobstr;}}
		public MDStreamHeader GUIDStreamHeader{get{return _guidstr;}}
		public MDStreamHeader USStreamHeader{get{return _usstr;}}
		public MDStreamHeader TableStreamHeader{get{return _tablestr;}}
		
	}

	/// <summary>
	/// The header that describes all the MD tables
	/// </summary>
	public class MetaDataTableHeader : Region
	{
		uint _Reserved;
		byte _MajorVersion;
		byte _MinorVersion;
		byte _HeapOffsetSizes;
		byte _RIDPlaceholder;
		ulong _MaskValid;
		ulong _MaskSorted;
		uint[] _TableLengths;

		public uint Reserved{get{return _Reserved;}}
		public byte MajorVersion{get{return _MajorVersion;}}
		public byte MinorVersion{get{return _MinorVersion;}}
		public byte HeapOffsetSizes{get{return _HeapOffsetSizes;}}
		[ObjViewer(Hex=true)]
		public byte RIDPlaceholder{get{return _RIDPlaceholder;}}
		[ObjViewer(Hex=true)]
		public ulong MaskValid{get{return _MaskValid;}}
		[ObjViewer(Hex=true)]
		public ulong MaskSorted{get{return _MaskSorted;}}
		[ObjViewer(false)]
		public uint[] TableLengths{get{return _TableLengths;}}
		

		public MetaDataTableHeader(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			_TableLengths = new uint[64];

			_Reserved = reader.ReadUInt32();
			_MajorVersion = reader.ReadByte();
			_MinorVersion = reader.ReadByte(); 
			_HeapOffsetSizes = reader.ReadByte();
			_RIDPlaceholder = reader.ReadByte();
			_MaskValid = reader.ReadUInt64();
			_MaskSorted = reader.ReadUInt64();
		
			//read as many uints as there are bits set in maskvalid
			for (int i = 0; i < 64; i++)
			{
				uint count = (uint)(  (((_MaskValid >> i) & 1) == 0) ? 0 : reader.ReadInt32()  );
				_TableLengths[i] = count;
			}

			Length = reader.BaseStream.Position - Start;
		}

	}
}
