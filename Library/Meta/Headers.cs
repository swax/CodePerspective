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
	/// The structure with which PE files start, the DOS stub
	/// </summary>
	public class DOSStub : Region
	{
		uint _PEPos;
		public uint PEHeaderOffset{get{return _PEPos;}}

		public DOSStub(BinaryReader reader)
		{
			if (reader.ReadUInt16() != 0x5A4D) 
				throw new ModException("DOSStub: Invalid DOS header.");

			reader.BaseStream.Position = 0x3c;
			_PEPos = reader.ReadUInt32();

			Start = 0;
			Length = 64;
		}

		[ObjViewer(Hex=true)]
		public uint PEPos{get{return _PEPos;}}
	}

	/// <summary>
	/// A single PE data directory
	/// </summary>
	public class DataDir : Region
	{
		uint _Rva;
		uint _Size;
		string _Name;

		[ObjViewer(Hex=true)]
		public uint Rva{get{return _Rva;}}
		[ObjViewer(Hex=true)]
		public uint Size{get{return _Size;}}
		public string Name{get{return _Name;}}

		public DataDir(BinaryReader reader, string name)
		{
			Start = reader.BaseStream.Position;
			Length = 8;
			_Name = name;

			_Rva = reader.ReadUInt32();
			_Size = reader.ReadUInt32();
		}

		public override string ToString()
		{
			return base.ToString() + " " + Name + " points to {" + Rva.ToString("X8") + " - " + (Rva + Size).ToString("X8") + "}";
		}
	}

	/// <summary>
	/// The PE header plus the so-called optional header
	/// </summary>
	public class PEHeader : Region
	{
		uint _Magic;
		byte _MajorLinkerVersion;
		byte _MinorLinkerVersion;
		uint _SizeOfCode;
		uint _SizeOfInitializedData;
		uint _SizeOfUninitializedData;
		uint _AddressOfEntryPoint;
		uint _BaseOfCode;
		uint _BaseOfData;
		uint _ImageBase;
		uint _SectionAlignment;
		uint _FileAlignment;
		ushort _OsMajor;
		ushort _OsMinor;
		ushort _UserMajor;
		ushort _UserMinor;
		ushort _SubSysMajor;
		ushort _SubSysMinor;
		uint _Reserved;
		uint _ImageSize;
		uint _HeaderSize;
		uint _FileChecksum;
		ushort _SubSystem;
		ushort _DllFlags;
		uint _StackReserveSize;
		uint _StackCommitSize;
		uint _HeapReserveSize;
		uint _HeapCommitSize;
		uint _LoaderFlags;
		uint _NumberOfDataDirectories;
		DataDir[] _DataDirs;

		[ObjViewer(Hex=true)]
		public uint Magic{get{return _Magic;}}
		public byte MajorLinkerVersion{get{return _MajorLinkerVersion;}}
		public byte MinorLinkerVersion{get{return _MinorLinkerVersion;}}
		public uint SizeOfCode{get{return _SizeOfCode;}}
		public uint SizeOfInitializedData{get{return _SizeOfInitializedData;}}
		public uint SizeOfUninitializedData{get{return _SizeOfUninitializedData;}}
		[ObjViewer(Hex=true)]
		public uint AddressOfEntryPoint{get{return _AddressOfEntryPoint;}}
		[ObjViewer(Hex=true)]
		public uint BaseOfCode{get{return _BaseOfCode;}}
		[ObjViewer(Hex=true)]
		public uint BaseOfData{get{return _BaseOfData;}}
		[ObjViewer(Hex=true)]
		public uint ImageBase{get{return _ImageBase;}}
		[ObjViewer(Hex=true)]
		public uint SectionAlignment{get{return _SectionAlignment;}}
		[ObjViewer(Hex=true)]
		public uint FileAlignment{get{return _FileAlignment;}}
		public ushort OsMajor{get{return _OsMajor;}}
		public ushort OsMinor{get{return _OsMinor;}}
		public ushort UserMajor{get{return _UserMajor;}}
		public ushort UserMinor{get{return _UserMinor;}}
		public ushort SubSysMajor{get{return _SubSysMajor;}}
		public ushort SubSysMinor{get{return _SubSysMinor;}}
		[ObjViewer(Hex=true)]
		public uint Reserved{get{return _Reserved;}}
		public uint ImageSize{get{return _ImageSize;}}
		public uint HeaderSize{get{return _HeaderSize;}}
		public uint FileChecksum{get{return _FileChecksum;}}
		public ushort SubSystem{get{return _SubSystem;}}
		public ushort DllFlags{get{return _DllFlags;}}
		public uint StackReserveSize{get{return _StackReserveSize;}}
		public uint StackCommitSize{get{return _StackCommitSize;}}
		public uint HeapReserveSize{get{return _HeapReserveSize;}}
		public uint HeapCommitSize{get{return _HeapCommitSize;}}
		[ObjViewer(Hex=true)]
		public uint LoaderFlags{get{return _LoaderFlags;}}
		public uint NumberOfDataDirectories{get{return _NumberOfDataDirectories;}}

		[ObjViewer(false)]
		public DataDir[] DataDirs{get{return _DataDirs;}}

		public PEHeader(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			// Read Standard fields
			_Magic = reader.ReadUInt16();
			_MajorLinkerVersion = reader.ReadByte();
			_MinorLinkerVersion = reader.ReadByte();
			_SizeOfCode = reader.ReadUInt32();
			_SizeOfInitializedData = reader.ReadUInt32();
			_SizeOfUninitializedData = reader.ReadUInt32();
			_AddressOfEntryPoint = reader.ReadUInt32();
			_BaseOfCode = reader.ReadUInt32();
			_BaseOfData = reader.ReadUInt32();
				
			// Read NT-specific fields
			_ImageBase = reader.ReadUInt32();
			_SectionAlignment = reader.ReadUInt32();
			_FileAlignment = reader.ReadUInt32();
			_OsMajor = reader.ReadUInt16();
			_OsMinor = reader.ReadUInt16();
			_UserMajor = reader.ReadUInt16();
			_UserMinor = reader.ReadUInt16();
			_SubSysMajor = reader.ReadUInt16();
			_SubSysMinor = reader.ReadUInt16();
			_Reserved = reader.ReadUInt32();
			_ImageSize = reader.ReadUInt32();
			_HeaderSize = reader.ReadUInt32();
			_FileChecksum = reader.ReadUInt32();
			_SubSystem = reader.ReadUInt16();
			_DllFlags = reader.ReadUInt16();
			_StackReserveSize = reader.ReadUInt32();
			_StackCommitSize = reader.ReadUInt32();
			_HeapReserveSize = reader.ReadUInt32();
			_HeapCommitSize = reader.ReadUInt32();
			_LoaderFlags = reader.ReadUInt32();
			_NumberOfDataDirectories = reader.ReadUInt32();
			if (NumberOfDataDirectories < 16) 
				throw new ModException("PEHeader:  Invalid number of data directories in file header.");

			_DataDirs = new DataDir[NumberOfDataDirectories];

			string[] PEDirNames = new String[16] { "Export Table", "Import Table", "Resource Table", "Exception Table", "Certificate Table", "Base Relocation Table", "Debug", "Copyright", "Global Ptr", "TLS Table", "Load Config Table", "Bound Import", "IAT", "Delay Import Descriptor", "CLI Header", "Reserved"};
		

			for (int i=0; i < NumberOfDataDirectories; ++i)
			{
				_DataDirs[i] = new DataDir(reader, (i < 16)?PEDirNames[i]:"Unknown");
			}

			Length = reader.BaseStream.Position - Start;

		}

	}


	public class COFFHeader : Region
	{
		ushort _Machine;
		ushort _NumberOfSections;
		uint _TimeDateStamp;
		uint _SymbolTablePointer;
		uint _NumberOfSymbols;
		ushort _OptionalHeaderSize;
		ushort _Characteristics;

		public ushort Machine{get{return _Machine;}}
		public ushort NumberOfSections{get{return _NumberOfSections;}}
		[ObjViewer(Hex=true)]
		public uint TimeDateStamp{get{return _TimeDateStamp;}}
		[ObjViewer(Hex=true)]
		public uint SymbolTablePointer{get{return _SymbolTablePointer;}}
		public uint NumberOfSymbols{get{return _NumberOfSymbols;}}
		public ushort OptionalHeaderSize{get{return _OptionalHeaderSize;}}
		public ushort Characteristics{get{return _Characteristics;}}

		public COFFHeader(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;
			Length = 20;

			_Machine = reader.ReadUInt16();
			_NumberOfSections = reader.ReadUInt16();
			_TimeDateStamp = reader.ReadUInt32();
			_SymbolTablePointer = reader.ReadUInt32();
			_NumberOfSymbols = reader.ReadUInt32();
			_OptionalHeaderSize = reader.ReadUInt16();
			_Characteristics = reader.ReadUInt16();
		}

	}


	/// <summary>
	/// Describes a PE file section
	/// </summary>
	public class SectionHeader : Region
	{
		string _Name;
		uint _Misc;
		uint _VirtualAddress;
		uint _SizeOfRawData;
		uint _PointerToRawData;
		uint _PointerToRelocations;
		uint _PointerToLinenumbers;
		ushort _NumberOfRelocations;
		ushort _NumberOfLinenumbers;
		uint _Characteristics;

		public string Name{get{return _Name;}}
		public uint Misc{get{return _Misc;}}
		[ObjViewer(Hex=true)]
		public uint VirtualAddress{get{return _VirtualAddress;}}
		public uint SizeOfRawData{get{return _SizeOfRawData;}}
		[ObjViewer(Hex=true)]
		public uint PointerToRawData{get{return _PointerToRawData;}}
		[ObjViewer(Hex=true)]
		public uint PointerToRelocations{get{return _PointerToRelocations;}}
		[ObjViewer(Hex=true)]
		public uint PointerToLinenumbers{get{return _PointerToLinenumbers;}}
		public ushort NumberOfRelocations{get{return _NumberOfRelocations;}}
		public ushort NumberOfLinenumbers{get{return _NumberOfLinenumbers;}}
		[ObjViewer(Hex=true)]
		public uint Characteristics{get{return _Characteristics;}}

		public SectionHeader(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;
			Length = 40;

			for(int i=0; i < 8; ++i)
			{
				byte b = reader.ReadByte();
				if (b != 0)
					_Name += (char)b;
			}

			_Misc = reader.ReadUInt32();
			_VirtualAddress = reader.ReadUInt32();
			_SizeOfRawData = reader.ReadUInt32();
			_PointerToRawData = reader.ReadUInt32();
			_PointerToRelocations = reader.ReadUInt32();
			_PointerToLinenumbers = reader.ReadUInt32();
			_NumberOfRelocations = reader.ReadUInt16();
			_NumberOfLinenumbers = reader.ReadUInt16();
			_Characteristics = reader.ReadUInt32();

		}

		public override string ToString()
		{
			return base.ToString() + " " + Name + " raw data at offsets {" + PointerToRawData.ToString("X8") + " - " + (PointerToRawData + SizeOfRawData).ToString("X8") + "}";
		}

	}



	/// <summary>
	/// All the non-.NET specific headers in a PE file are gathered in this object
	/// </summary>
	public class OSHeaders : Region
	{
		DOSStub _stub;
		COFFHeader _coh;
		PEHeader _peh;

		SectionHeader[] _sech;

		long _dataSectionsOffset;

		public OSHeaders(BinaryReader reader)
		{
			Start = 0;

			_stub = new DOSStub(reader);
			reader.BaseStream.Position = _stub.PEPos;

			// Read "PE\0\0" signature
			if (reader.ReadUInt32() != 0x00004550) throw new ModException("File is not a portable executable.");
		
			_coh = new COFFHeader(reader);

			// Compute data sections offset
			_dataSectionsOffset = reader.BaseStream.Position + _coh.OptionalHeaderSize;

			_peh = new PEHeader(reader);

			reader.BaseStream.Position = _dataSectionsOffset;

			_sech = new SectionHeader[_coh.NumberOfSections];

			for (int i = 0; i < _sech.Length; i++)
			{
				_sech[i] = new SectionHeader(reader);
			}

			Length = reader.BaseStream.Position;

		}


		public DOSStub DOSStub{get{return _stub;}}
		public COFFHeader COFFHeader{get{return _coh;}}
		public PEHeader PEHeader{get{return _peh;}}
		public SectionHeader[] SectionHeaders{get{return _sech;}}
	}

	/// <summary>
	/// An entry in a PE import table
	/// </summary>
	public class ImportAddress
	{
		bool _ByOrdinal;
		uint _Ordinal;
		string _Name;

		public bool ByOrdinal{get{return _ByOrdinal;} set{_ByOrdinal = value;}}
		public uint Ordinal{get{return _Ordinal;}set{_Ordinal = value;}}
		public string Name{get{return _Name;}set{_Name = value;}}

		public ImportAddress(uint n, BinaryReader reader, MModule mod)
		{
			if ((n & 0x80000000) != 0)
				_ByOrdinal = true;
			else
				_ByOrdinal = false;

			if (_ByOrdinal)
			{
				_Ordinal = n & 0x7fffffff;
			}
			else
			{
				uint nameOffs = n & 0x7fffffff;
				long offs = reader.BaseStream.Position;
				reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(nameOffs);
				reader.ReadInt16();//hint

				byte b = reader.ReadByte();

				while(b != 0)
				{
					_Name += (char) b;
					b = reader.ReadByte();
				}

				reader.BaseStream.Position = offs;

			}
		}

		public override string ToString()
		{
			if (_ByOrdinal)
			{
				return "By Ordinal: " + _Ordinal;
			}
			else
			{
				return "By Name: " + _Name;
			}
		}
	}

	public class ImportDirectoryEntry : Region
	{
		ImportAddress[] _ImportLookupTable = new ImportAddress[0];
		uint _DateTimeStamp;
		uint _ForwarderChain;
		string _Name;
		uint[] _ImportAddressTable = new uint[0];


		[ObjViewer(false)]
		public ImportAddress[] ImportLookupTable{get{return _ImportLookupTable;}}
		[ObjViewer(Hex=true)]
		public uint DateTimeStamp{get{return _DateTimeStamp;}}
		[ObjViewer(Hex=true)]
		public uint ForwarderChain{get{return _ForwarderChain;}}
		public string Name{get{return _Name;}}
		[ObjViewer(false)]
		public uint[] ImportAddressTable{get{return _ImportAddressTable;}}

		public ImportDirectoryEntry(BinaryReader reader, MModule mod)
		{
			Start = reader.BaseStream.Position;

			uint iltRVA = reader.ReadUInt32();
			_DateTimeStamp = reader.ReadUInt32();
			_ForwarderChain = reader.ReadUInt32();
			uint nameRVA = reader.ReadUInt32();
			_Name = mod.StringFromRVA(reader, nameRVA);
			uint iatRVA = reader.ReadUInt32(); //can also get this from the PEHeader's data dirs

			Length = reader.BaseStream.Position - Start;

			long offs = reader.BaseStream.Position; // remember our position at the end of the imp dir entry record

			if (nameRVA == 0)
			{
				//indicate that this is not valid, because we reached the null terminating record
				//or because we are hopelessly lost
				_Name = null;
				return;
			}

			try
			{
				

				//get imp look table from RVA
				ArrayList arr;
				uint tableOffs, field;

				if (iltRVA != 0)
				{
				
					arr = new ArrayList();
					tableOffs = mod.ModHeaders.Rva2Offset(iltRVA);
					reader.BaseStream.Position = tableOffs;
					field = reader.ReadUInt32();
					while(field != 0)
					{
						arr.Add(new ImportAddress(field, reader, mod));
						field = reader.ReadUInt32();
					}

					_ImportLookupTable = (ImportAddress[])arr.ToArray(typeof(ImportAddress));
				}


				//get imp Addr table from RVA
				if (iatRVA != 0)
				{
					arr = new ArrayList();
					tableOffs = mod.ModHeaders.Rva2Offset(iatRVA);
					reader.BaseStream.Position = tableOffs;
					field = reader.ReadUInt32();
					while(field != 0)
					{
						arr.Add(field);
						field = reader.ReadUInt32();
					}

					_ImportAddressTable = (uint[])arr.ToArray(typeof(uint));
				}


			}
			catch(Exception)
			{
			}
			finally
			{
				//restore stream pos
				reader.BaseStream.Position = offs;
			}

		}

	}

	public class ExportDirTable : Region
	{
		uint _ExportFlags;
		uint _TimeStamp;
		ushort _MajorVersion;
		ushort _MinorVersion;
		string _Name;
		uint _OrdinalBase;
		uint _AddressTableEntries;
		uint _NamePointerCount;
		uint _ExportAddressTableRVA;
		uint _NamePointerRVA;
		uint _OrdinalRVA;

		public uint ExportFlags{get{return _ExportFlags;}}
		public uint TimeStamp{get{return _TimeStamp;}}
		public ushort MajorVersion{get{return _MajorVersion;}}
		public ushort MinorVersion{get{return _MinorVersion;}}
		public string Name{get{return _Name;}}
		public uint OrdinalBase{get{return _OrdinalBase;}}
		public uint AddressTableEntries{get{return _AddressTableEntries;}}
		public uint NamePointerCount{get{return _NamePointerCount;}}
		[ObjViewer(Hex=true)]
		public uint ExportAddressTableRVA{get{return _ExportAddressTableRVA;}}
		[ObjViewer(Hex=true)]
		public uint NamePointerRVA{get{return _NamePointerRVA;}}
		[ObjViewer(Hex=true)]
		public uint OrdinalRVA{get{return _OrdinalRVA;}}

		public ExportDirTable(BinaryReader reader, MModule mod)
		{
			_ExportFlags = reader.ReadUInt32();
			_TimeStamp = reader.ReadUInt32();
			_MajorVersion = reader.ReadUInt16();
			_MinorVersion = reader.ReadUInt16();
			_Name = mod.StringFromRVA(reader, reader.ReadUInt32());
			_OrdinalBase = reader.ReadUInt32();
			_AddressTableEntries = reader.ReadUInt32();
			_NamePointerCount = reader.ReadUInt32();
			_ExportAddressTableRVA = reader.ReadUInt32();
			_NamePointerRVA = reader.ReadUInt32();
			_OrdinalRVA = reader.ReadUInt32();
		}
	}

	
	/// <summary>
	/// In PE files, there are 3 different tables for ordinals, addresses and names of exports.
	/// This class gathers those three things into one type for simplicity
	/// </summary>
	public class ExportRecord
	{
		uint _ord;
		uint _addr;
		string _name;

		public ExportRecord(uint ord, uint addr, string name)
		{
			_ord=ord;
			_addr=addr;
			_name=name;
		}

		public string Name{get{return _name;}}
		[ObjViewer(Hex=true)]
		public uint Address{get{return _addr;}}
		public uint Ordinal{get{return _ord;}}


		public override string ToString()
		{
			return _name + ": Ordinal " + _ord + ", Addr " + _addr.ToString("X8");
		}
	}

	/// <summary>
	/// Contains misc information about the files imports and exports
	/// </summary>
	public class ImpExports
	{
		ImportDirectoryEntry[] _ith;
		ExportDirTable _extab;
		uint[] _expAddrTab;
		string[] _expNameTab;
		uint[] _expOrdTab;
		ExportRecord[] _exports;

		[ObjViewer(false)]
		public ImportDirectoryEntry[] ImportDirectoryEntries{get{return _ith;}}
		[ObjViewer(false)]
		public ExportDirTable ExportDirectoryTable{get{return _extab;}}
		//public uint[] ExportAddressTable{get{return _expAddrTab;}}
		//public string[] ExportNameTable{get{return _expNameTab;}}
		//public uint[] ExportOrdinalTable{get{return _expOrdTab;}}
		[ObjViewer(false)]
		public ExportRecord[] Exports{get{return _exports;}}

		public ImpExports(BinaryReader reader, MModule mod)
		{
			ArrayList ides = new ArrayList();
			ImportDirectoryEntry ide = null;

			_exports = new ExportRecord[0];
			_ith = new ImportDirectoryEntry[0];

			//imports 

			if (mod.ModHeaders.OSHeaders.PEHeader.DataDirs[1].Rva != 0)
			{

				uint start, end;
				start = mod.ModHeaders.Rva2Offset(mod.ModHeaders.OSHeaders.PEHeader.DataDirs[1].Rva);
				end = mod.ModHeaders.OSHeaders.PEHeader.DataDirs[1].Size + start;

				reader.BaseStream.Position = start;

				while (reader.BaseStream.Position < end)
				{
					ide = new ImportDirectoryEntry(reader, mod);

					//in older PEs it seems there is no null terminating entry, but in .NET ones there is.
					if (ide.Name == null)
					{
						break;
					}
					else
					{
						ides.Add(ide);
					}
				}

				_ith = (ImportDirectoryEntry[])ides.ToArray(typeof(ImportDirectoryEntry));
			}

			//exports

			if (mod.ModHeaders.OSHeaders.PEHeader.DataDirs[0].Rva != 0)
			{
			
				reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(mod.ModHeaders.OSHeaders.PEHeader.DataDirs[0].Rva);
				_extab = new ExportDirTable(reader, mod);

				_expAddrTab = new uint[_extab.AddressTableEntries];
				_expNameTab = new string[_extab.NamePointerCount];
				_expOrdTab = new uint[_extab.NamePointerCount];

				reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(_extab.ExportAddressTableRVA);

				for(int i=0; i< _extab.AddressTableEntries; ++i)
				{
					_expAddrTab[i] = reader.ReadUInt32();
				}

				reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(_extab.OrdinalRVA);

				for(int i=0; i< _extab.NamePointerCount; ++i)
				{
					_expOrdTab[i] = reader.ReadUInt16();
				}

				reader.BaseStream.Position = mod.ModHeaders.Rva2Offset(_extab.NamePointerRVA);

				for(int i=0; i< _extab.NamePointerCount; ++i)
				{
					_expNameTab[i] = mod.StringFromRVA(reader, reader.ReadUInt32());
				}

				//assemble array of exportrecords
				uint len = _extab.AddressTableEntries;
				if (len > _extab.NamePointerCount) len = _extab.NamePointerCount;
				_exports = new ExportRecord[len];
				for(int i=0; i < len; ++i)
				{
					_exports[i] = new ExportRecord(_expOrdTab[i], _expAddrTab[i], _expNameTab[i]);
				}
			}

		}
	}

	/// <summary>
	/// One relocation table entry
	/// </summary>
	public class Relocation
	{
		int _type;
		int _offs;

		public int Type{get{return _type;}}
		[ObjViewer(Hex=true)]
		public int Offset{get{return _offs;}}
		public string TypeName
		{
			get
			{
				switch(_type)
				{
					case 0:return "Absolute"; 
					case 1:return "High"; 
					case 2:return "Low"; 
					case 3:return "HighLow"; 
					case 4:return "HighAdj"; 
					case 5:return "Mips Jump"; 
					case 6:return "Section"; 
					case 7:return "Rel32"; 
					case 9:return "Mips Jump 16"; 
					case 10:return "Dir64"; 
					case 11:return "High32Adj"; 
					default: return "Unknown"; 
				}

			}
		}

		public Relocation(ushort n)
		{
			_type = (n & 0xf000) >> 12;
			_offs = n & 0x0fff;
		}

		public override string ToString()
		{
			return _offs.ToString("X8") + " " + TypeName;
		}
	}

	/// <summary>
	/// A PE relocation block
	/// </summary>
	public class RelocationBlock : Region
	{
		uint _PageRVA;
		uint _BlockSize;
		Relocation[] _entries;

		[ObjViewer(Hex=true)]
		public uint PageRVA{get{return _PageRVA;}}
		public uint BlockSize{get{return _BlockSize;}}
		[ObjViewer(false)]
		public Relocation[] Relocations{get{return _entries;}}

		public RelocationBlock(BinaryReader reader)
		{
			Start = reader.BaseStream.Position;

			_PageRVA = reader.ReadUInt32();
			_BlockSize = reader.ReadUInt32();

			uint numEntries = (_BlockSize / 2 - 4);
			
			_entries = new Relocation[numEntries];

			for(int i=0; i < numEntries; ++i)
			{
				_entries[i] = new Relocation(reader.ReadUInt16());
			}

			Length = reader.BaseStream.Position - Start;
		}

	}

	/// <summary>
	/// The collection of all relocation blocks in a PE file
	/// </summary>
	public class Relocations : Region
	{
		RelocationBlock[] _blox;

		public RelocationBlock[] Blocks{get{return _blox;}}

		public Relocations(BinaryReader reader, MModule mod)
		{
			uint start, end; 

			if (mod.ModHeaders.OSHeaders.PEHeader.DataDirs[5].Rva == 0) return;

			start = mod.ModHeaders.Rva2Offset(mod.ModHeaders.OSHeaders.PEHeader.DataDirs[5].Rva);
			end = start + mod.ModHeaders.OSHeaders.PEHeader.DataDirs[5].Size;

			//fill in Region props
			Start = start;
			Length = end-start;

			reader.BaseStream.Position = start;

			RelocationBlock block;
			ArrayList arr = new ArrayList();

			while(reader.BaseStream.Position < end)
			{

				block = new RelocationBlock(reader);
				arr.Add(block);
			}

			_blox = (RelocationBlock[])arr.ToArray(typeof(RelocationBlock));

		}


	}
}
