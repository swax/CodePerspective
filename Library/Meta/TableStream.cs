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
using System.Text;
using Asmex.ObjViewer;

namespace Asmex.FileViewer
{
	/// <summary>
	/// 'Types' in .NET metadata may be simple types, coded token types, or tables.
	/// Thus this enum describes all tables and codedtoken types as well as describing all types.
	/// </summary>
	public enum Types
	{
		//Tables
		Module = 0,
		TypeRef = 1,
		TypeDef = 2,
		FieldPtr = 3, 
		Field = 4,
		MethodPtr = 5,
		Method = 6,
		ParamPtr = 7, 
		Param = 8,
		InterfaceImpl = 9,
		MemberRef = 10,
		Constant = 11, 
		CustomAttribute = 12,
		FieldMarshal = 13,
		Permission = 14,
		ClassLayout = 15, 
		FieldLayout = 16,
		StandAloneSig = 17,
		EventMap = 18,
		EventPtr = 19, 
		Event = 20,
		PropertyMap = 21,
		PropertyPtr = 22,
		Property = 23, 
		MethodSemantics = 24,
		MethodImpl = 25,
		ModuleRef = 26,
		TypeSpec = 27, 
		ImplMap = 28, //lidin book is wrong again here?  It has enclog at 28
		FieldRVA = 29,
		ENCLog = 30,
		ENCMap = 31, 
		Assembly = 32,
		AssemblyProcessor= 33,
		AssemblyOS = 34,
		AssemblyRef = 35, 
		AssemblyRefProcessor = 36,
		AssemblyRefOS = 37,
		File = 38,
		ExportedType = 39, 
		ManifestResource = 40,
		NestedClass = 41,
		TypeTyPar = 42,
		MethodTyPar = 43,

		//Coded Token Types
		TypeDefOrRef = 64,
		HasConstant = 65,
		CustomAttributeType = 66,
		HasSemantic = 67,
		ResolutionScope = 68,
		HasFieldMarshal = 69,
		HasDeclSecurity = 70,
		MemberRefParent = 71,
		MethodDefOrRef = 72,
		MemberForwarded = 73,
		Implementation = 74,
		HasCustomAttribute = 75,

		//Simple
		UInt16 = 97,
		UInt32 = 99,
		String = 101,
		Blob = 102,
		Guid = 103,
		UserString = 112
	}

	/// <summary>
	/// The information that specifies one MD Table column
	/// </summary>
	public class ColDesc
	{
		Types _type;
		string _name;

		public ColDesc(Types type, string name)
		{
			_type = type;
			_name = name;
		}

		public string Name{get{return _name;}}
		public Types Type{get{return _type;}}
	}
		
	/// <summary>
	/// An MD table.  Includes the schema (a coldesc array) and some rows that are 
	/// accessed via the indexer
	/// </summary>
	public class Table : IEnumerable
	{
		Types _type;
		ColDesc[] _colDescs;
		MDTables _helper;
		Row[] _rows;
		int _rowSize;
		Byte[] data;
		 
		public Table(Types type, Types[] colTypes, String[] colNames, MDTables helper, BinaryReader reader)
		{
			_type = type;
			_helper = helper;
		
			_colDescs = new ColDesc[colTypes.Length];

			for (int i = 0; i < _colDescs.Length; i++)
			{
				_colDescs[i] = new ColDesc(colTypes[i], colNames[i]);
			}

			_rows = new Row[helper.GetTableRows(type)];

			_rowSize = 0;
			foreach (ColDesc cd in _colDescs)
			{
				_rowSize += _helper.SizeOfType(cd.Type);
			}

			data = reader.ReadBytes(_rowSize * Count); 
		}
		
		public Types Type{get{return _type;}}
		
		[ObjViewer(false)]
		public ColDesc[] ColDescs{get{return _colDescs;}}
		
		public int Count{get{return _rows.Length;}}
		

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

        // Source code change by Andrea Bertolotto - 2006/08/22 - Start
        [ObjViewer(false)]
        public Byte[] RawData { get { return this.data; } }

        [ObjViewer(false)]
        public int RowSize { get { return this._rowSize; } }
        // Source code change by Andrea Bertolotto - 2006/08/22 - End

		public Row this[int index]
		{
			get 
			{ 
				if (_rows[index] == null)
				{
					_rows[index] = GetRow(index);
				}
				return _rows[index]; 
			}
		}

		Row GetRow(int idx)
		{
			MemoryStream stream = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(stream);
			reader.BaseStream.Position = _rowSize * idx;
					
			TableCell[] cells = new TableCell[_colDescs.Length];
			for (int i = 0; i < _colDescs.Length; i++)
			{
				ColDesc cd = _colDescs[i];
				//uint valu = ReadSingleValue(_colDescs[i].Type, reader);
				//cells[i] = new TableCell(valu, _helper);
				cells[i] = new TableCell(_colDescs[i].Type, reader, _helper);
			}
		
			stream.Close();

			return new Row(cells, this);
			
		}
	
		public override String ToString()
		{
			StringBuilder sb = new StringBuilder(100);
			sb.Append (_type.ToString());
			sb.Append (" (");
			sb.Append (_rows.Length);
			sb.Append (")    (");
			
			for(int i=0; i < this._colDescs.Length;++i)
			{
				sb.Append(_colDescs[i].Name);
				if(i < _colDescs.Length-1) sb.Append("  --   ");
			}

			sb.Append(")");

			return sb.ToString();
		}
		
		class Enumerator : IEnumerator
		{
			Table _table;
			int _curr;
		
			public Enumerator(Table table)
			{
				_table = table;
				Reset();
			}	
					
			public void Reset()
			{
				_curr = -1;
			}
					
			public Boolean MoveNext()
			{
				_curr++;
				return (_curr < _table.Count);
			}
					
			public Object Current
			{
				get { return _table[_curr]; }
			}
		}
	}
		
	/// <summary>
	/// An MD Table row
	/// </summary>
	public class Row
	{
		Table _table;
		TableCell[] _cells;
					
		public Row(TableCell[] cells, Table table)
		{
			_cells = cells;
			_table = table;
		}
					
		public Table Table{get{return _table;}}
					
		public TableCell this[int index]{get{return _cells[index];}}

		public string RawString()
		{
			StringBuilder sb = new StringBuilder(100);
			
			for(int i=0; i < _cells.Length;++i)
			{
				sb.Append(_cells[i].RawData.ToString("X8"));
				if(i < _cells.Length-1) sb.Append("   --    ");
			}

			return sb.ToString();
		}

		public string CookedString()
		{
			StringBuilder sb = new StringBuilder(100);
			
			for(int i=0; i < this._cells.Length;++i)
			{
				sb.Append(_cells[i].Data.ToString());
				if(i < _cells.Length-1) sb.Append("   --    ");
			}

			return sb.ToString();
		}
				
		public override String ToString()
		{	
			return RawString();
		}
	}
		
	/// <summary>
	/// A cell in an MD table
	/// </summary>
	public class TableCell
	{
		uint _rawData;
		Types _type;
		object _data;
				
		public TableCell(Types type, BinaryReader reader, MDTables helper)
		{
			_type = type;

			try
			{

				// Fixed
				if (type == Types.UInt16) _rawData = reader.ReadUInt16();
				if (type == Types.UInt32) _rawData = reader.ReadUInt32();
					
				// Heap
				if (type == Types.String) _rawData =  ((helper.GetStringIndexSize() == 2) ? (uint) reader.ReadUInt16() : reader.ReadUInt32());
				if (type == Types.Guid) _rawData =  ((helper.GetGuidIndexSize() == 2) ? (uint) reader.ReadUInt16() : reader.ReadUInt32());
				if (type == Types.Blob) _rawData =  ((helper.GetBlobIndexSize() == 2) ? (uint) reader.ReadUInt16() : reader.ReadUInt32());
		
				// Rid
				if ((int)type < 64)
				{
					Table table = helper.GetTable(type);
					_rawData =  (uint) (((uint) type << 24) | ((table.Count < 65536) ? (uint) reader.ReadUInt16() : reader.ReadUInt32()));
				}
		
				// Coded token (may need to be uncompressed from 2-byte form)
				else if ((int) type < 97)
				{
					int codedToken = (helper.SizeOfType(type) == 2) ? reader.ReadUInt16() : reader.ReadInt32();
					Types[] referredTables = (Types[]) helper.CodedTokenTypes(type);

					int tableIndex = (int) (codedToken & ~(-1 << helper.CodedTokenBits(referredTables.Length)));
					int index = (int) (codedToken >> helper.CodedTokenBits(referredTables.Length));

					int token = helper.ToToken(referredTables[tableIndex], index - 1);
					_rawData =  (uint) token;
				}

				_data = CreateData(helper);

			}
			catch(Exception)
			{
				_type = Types.String;
				_data = "Unreadable cell: " + type + " " + _rawData;
			}
		}

		public uint RawData{get{return _rawData;}}

		public object Data{get{return _data;}}

		object CreateData(MDTables helper)
		{
			
			switch(_type)
			{
				case Types.UInt16:
					return (ushort)_rawData;;
				case Types.UInt32:
					return (uint)_rawData;
				case Types.String:
					return helper.GetString((int)_rawData); 
				case Types.Guid:
					return helper.GetGuid((int)_rawData);
				case Types.Blob:
					return helper.GetBlob((int)_rawData); 
					//case Types.UserString:
					//	return _helper.GetBlob((int)_rawData); 
				default:
					break;
			}

			if ((int)_type < 64)
			{
				return new RID(_type, _rawData);
			}

			return new CodedToken(_type, _rawData, helper);
		}

	}


	/// <summary>
	/// An RID. 'type' is not part of the RID's actual content but rather a note saying what
	/// sort of column the RID was found in and thus what table it must refer to
	/// </summary>
	public class RID
	{
		uint _rawData;
		Types _type;

		public RID(Types type, uint raw)
		{
			_rawData = raw;
			_type = type;
		}

		public uint Raw{get{return _rawData;}}

		public override string ToString()
		{
			return _type.ToString() + " " + _rawData.ToString("X8");
		}
	}

	/// <summary>
	/// A coded token.  As with the RID class, 'type' is not actually data held in the coded token but a note
	/// telling us what kind of column the ct was found in and thus what kind of ct it must be
	/// </summary>
	public class CodedToken
	{
		uint _rawData;
		Types _type;

		public CodedToken(Types type, uint raw, MDTables helper)
		{
			_rawData = raw;
			_type = type;
		}

		public uint Raw{get{return _rawData;}}

		public override string ToString()
		{
			Types t = (Types)((_rawData & 0xff000000) >> 24);

			return t.ToString() + " " +( _rawData & 0x00ffffff).ToString("X8");
		}
	}
		


	/// <summary>
	/// The collection of all md tables in the file
	/// </summary>
	public class MDTables
	{
		Hashtable _ctok;
		Table[] _td;
		int[] _codedTokenBits;
		MModule _mod;

		public MDTables(BinaryReader reader, MModule mod)
		{
			_mod = mod;

			SetupSchema(reader);

			//reader may now be accessed randomly to fill in actual strings etc.
		}

		public Table[] Tables{get{return _td;}}

		public Types[] CodedTokenTypes(Types t)
		{
			return (Types[])_ctok[t];
		}

		public int CodedTokenBits(Types t)
		{
			return _codedTokenBits[(int)t];
		}

		public int CodedTokenBits(int i)
		{
			return _codedTokenBits[i];
		}

		public Table GetTable(int token)
		{
			int idx = token >> 24;
			if (idx >= _td.Length) throw new ModException("MDTables:  No such table");
			return _td[idx];
		}
		
		public Table GetTable(Types type)
		{
			int idx = (int) type;
			if (idx >= _td.Length) throw new ModException("MDTables:  No such table");
			return _td[idx];
		}

		public uint GetTableRows(Types t)
		{
			int idx = (int)t;
			if (idx < 0 || idx > _mod.ModHeaders.MetaDataTableHeader.TableLengths.Length) throw new ModException("MDHelper:  Tried to get length of nonexistant table");
			return _mod.ModHeaders.MetaDataTableHeader.TableLengths[(int)t];
		}

		public int ToToken(Types tableType, int index)
		{
			int type = (int) tableType;
			index++;
			if (index < 0) return -1;
			return ((type << 24) | index);
		}



		public string GetString(int offset)
		{
			if (offset < 0 || offset > _mod.StringHeap.Length) throw new ModException("MDTables:  string offs out of range.");
			string s = (string)_mod.StringHeap.GetByOffset(offset);
			return s;
		}

		public MDBlob GetBlob(int offset)
		{
			if (offset < 0 || offset > _mod.BlobHeap.Length) throw new ModException("MDTables:  blob offs out of range.");
			return (MDBlob)_mod.BlobHeap.GetByOffset(offset);
		}

		//hmm.  'offsets' in the guid heap actually seem to be 1-based indexes?  And an index of 0 means empty?  
		//ITS NOT LIKE THIS IS ACTUALLY ***DOCUMENTED*** ANYWHERE AFTER ALL.
		public MDGUID GetGuid(int offset)
		{
			if (offset ==0) return MDGUID.Empty;
			if (offset < 1 || offset > _mod.GUIDHeap.Length / 16) throw new ModException("MDTables:  GUID offs out of range.");
			return (MDGUID)_mod.GUIDHeap[offset-1];
		}


		public int GetStringIndexSize()
		{
			return ((_mod.ModHeaders.MetaDataTableHeader.HeapOffsetSizes & 0x01) != 0) ? 4 : 2;
		}
		
		public int GetGuidIndexSize()
		{
			return ((_mod.ModHeaders.MetaDataTableHeader.HeapOffsetSizes & 0x02) != 0) ? 4 : 2;
		}
		
		public int GetBlobIndexSize()
		{
			return ((_mod.ModHeaders.MetaDataTableHeader.HeapOffsetSizes & 0x04) != 0) ? 4 : 2;
		}

		//Note that the size of a type cannot be known until at least table sizes are loaded from file
		public int SizeOfType(Types type)
		{
			// Fixed
			if (type == Types.UInt16) return 2;
			if (type == Types.UInt32) return 4;
					
			// Heap
			if (type == Types.String) return GetStringIndexSize();
			if (type == Types.Blob) return GetBlobIndexSize();
			if (type == Types.Guid) return GetGuidIndexSize();
					
			// RID
			if ((int)type < 64)
			{
				uint rows = GetTableRows(type);
				return (rows < 65536) ? 2 : 4;
			}
		
			// CodedToken
			Types[] referredTypes = (Types[]) CodedTokenTypes(type);
			if (referredTypes != null)
			{
				uint maxRows = 0;
				foreach (Types referredType in referredTypes)
				{
					if (referredType != Types.UserString)//but what if there is a large user string table?
					{
						uint rows = GetTableRows(referredType);
						if (maxRows < rows)
							maxRows = rows;
					}
				}
			
				maxRows = maxRows << CodedTokenBits(referredTypes.Length);
				return (maxRows < 65536) ? 2 : 4;
			} 
			
			throw new ModException("Table:  Sizeof invalid token type.");
		}



		/// <summary>
		/// .NET expects the consumer of the metadata to know the schema of the metadata database.
		/// that schema is represented here as an array of 'table' objs which will be filled with actual
		/// rows elsewhere.
		/// </summary>
		/// <param name="reader"></param>
		void SetupSchema(BinaryReader reader)
		{
			//number of bits in coded token tag for a coded token that refers to n tables.
			//values 5-17 are not used :I
			_codedTokenBits = new int[] { 0, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };

			//Hash telling us what tables a given coded token type can refer to
			_ctok = new Hashtable();
			_ctok[Types.TypeDefOrRef] = new Types[] { Types.TypeDef, Types.TypeRef, Types.TypeSpec};
			_ctok[Types.HasConstant] = new Types[] { Types.Field, Types.Param, Types.Property };
			_ctok[Types.CustomAttributeType] = new Types[] { Types.TypeRef, Types.TypeDef, Types.Method, Types.MemberRef, Types.UserString };
			_ctok[Types.HasSemantic] = new Types[] { Types.Event, Types.Property };
			_ctok[Types.ResolutionScope] = new Types[] { Types.Module, Types.ModuleRef, Types.AssemblyRef, Types.TypeRef };
			_ctok[Types.HasFieldMarshal] = new Types[] { Types.Field, Types.Param };
			_ctok[Types.HasDeclSecurity] = new Types[] { Types.TypeDef, Types.Method, Types.Assembly };
			_ctok[Types.MemberRefParent] = new Types[] { Types.TypeDef, Types.TypeRef, Types.ModuleRef, Types.Method, Types.TypeSpec };
			_ctok[Types.MethodDefOrRef] = new Types[] { Types.Method, Types.MemberRef };
			_ctok[Types.MemberForwarded] = new Types[] { Types.Field, Types.Method };
			_ctok[Types.Implementation] = new Types[] { Types.File, Types.AssemblyRef, Types.ExportedType };
			_ctok[Types.HasCustomAttribute] = new Types[] { Types.Method, Types.Field, Types.TypeRef, Types.TypeDef, Types.Param, Types.InterfaceImpl, Types.MemberRef, Types.Module, Types.Permission, Types.Property, Types.Event, Types.StandAloneSig, Types.ModuleRef, Types.TypeSpec, Types.Assembly, Types.AssemblyRef, Types.File, Types.ExportedType, Types.ManifestResource };

			//md db schema is represented in the arguments to table constructors...
			_td = new Table[0x2C];
			_td[0x00] = new Table(Types.Module, new Types[] { Types.UInt16, Types.String, Types.Guid, Types.Guid, Types.Guid }, new String[] { "Generation", "Name", "Mvid", "EncId", "EncBaseId" }, this, reader);
			_td[0x01] = new Table(Types.TypeRef, new Types[] { Types.ResolutionScope, Types.String, Types.String }, new String[] { "ResolutionScope", "Name", "Namespace" }, this, reader);
			_td[0x02] = new Table(Types.TypeDef, new Types[] { Types.UInt32, Types.String, Types.String, Types.TypeDefOrRef, Types.Field, Types.Method }, new String[] { "Flags", "Name", "Namespace", "Extends", "FieldList", "MethodList" }, this, reader);
			_td[0x03] = new Table(Types.FieldPtr, new Types[] { Types.Field }, new String[] { "Field" }, this, reader);
			_td[0x04] = new Table(Types.Field, new Types[] { Types.UInt16, Types.String, Types.Blob }, new String[] { "Flags", "Name", "Signature" }, this, reader);
			_td[0x05] = new Table(Types.MethodPtr, new Types[] { Types.Method }, new String[] { "Method" }, this, reader);
			_td[0x06] = new Table(Types.Method, new Types[] { Types.UInt32, Types.UInt16, Types.UInt16, Types.String, Types.Blob, Types.Param }, new String[] { "RVA", "ImplFlags", "Flags", "Name", "Signature", "ParamList" }, this, reader);
			_td[0x07] = new Table(Types.ParamPtr, new Types[] { Types.Param }, new String[] { "Param" }, this, reader);
			_td[0x08] = new Table(Types.Param, new Types[] { Types.UInt16, Types.UInt16, Types.String }, new String[] { "Flags", "Sequence", "Name" }, this, reader);
			_td[0x09] = new Table(Types.InterfaceImpl, new Types[] { Types.TypeDef, Types.TypeDefOrRef }, new String[] { "Class", "Interface" }, this, reader);
			_td[0x0A] = new Table(Types.MemberRef, new Types[] { Types.MemberRefParent, Types.String, Types.Blob }, new String[] { "Class", "Name", "Signature" }, this, reader);
			_td[0x0B] = new Table(Types.Constant, new Types[] { Types.UInt16, Types.HasConstant, Types.Blob }, new String[] { "Type", "Parent", "Value" }, this, reader);
			_td[0x0C] = new Table(Types.CustomAttribute, new Types[] { Types.HasCustomAttribute, Types.CustomAttributeType, Types.Blob }, new String[] { "Type", "Parent", "Value" }, this, reader);
			_td[0x0D] = new Table(Types.FieldMarshal, new Types[] { Types.HasFieldMarshal, Types.Blob }, new String[] { "Parent", "Native" }, this, reader);
			_td[0x0E] = new Table(Types.Permission, new Types[] { Types.UInt16, Types.HasDeclSecurity, Types.Blob }, new String[] { "Action", "Parent", "PermissionSet" }, this, reader);
			_td[0x0F] = new Table(Types.ClassLayout, new Types[] { Types.UInt16, Types.UInt32, Types.TypeDef }, new String[] { "PackingSize", "ClassSize", "Parent" }, this, reader);
			_td[0x10] = new Table(Types.FieldLayout, new Types[] { Types.UInt32, Types.Field }, new String[] { "Offset", "Field" }, this, reader);
			_td[0x11] = new Table(Types.StandAloneSig, new Types[] { Types.Blob }, new String[] { "Signature" }, this, reader);
			_td[0x12] = new Table(Types.EventMap, new Types[] { Types.TypeDef, Types.Event }, new String[] { "Parent", "EventList" }, this, reader);
			_td[0x13] = new Table(Types.EventPtr, new Types[] { Types.Event }, new String[] { "Event" }, this, reader);
			_td[0x14] = new Table(Types.Event, new Types[] { Types.UInt16, Types.String, Types.TypeDefOrRef }, new String[] { "EventFlags", "Name", "EventType" }, this, reader);
			_td[0x15] = new Table(Types.PropertyMap, new Types[] { Types.TypeDef, Types.Property }, new String[] { "Parent", "PropertyList" }, this, reader);
			_td[0x16] = new Table(Types.PropertyPtr, new Types[] { Types.Property }, new String[] { "Property" }, this, reader);
			_td[0x17] = new Table(Types.Property, new Types[] { Types.UInt16, Types.String, Types.Blob }, new String[] { "PropFlags", "Name", "Type" }, this, reader);
			_td[0x18] = new Table(Types.MethodSemantics, new Types[] { Types.UInt16, Types.Method, Types.HasSemantic }, new String[] { "Semantic", "Method", "Association" }, this, reader);
			_td[0x19] = new Table(Types.MethodImpl, new Types[] { Types.TypeDef, Types.MethodDefOrRef, Types.MethodDefOrRef }, new String[] { "Class", "MethodBody", "MethodDeclaration" }, this, reader);
			_td[0x1A] = new Table(Types.ModuleRef, new Types[] { Types.String }, new String[] { "Name" }, this, reader);
			_td[0x1B] = new Table(Types.TypeSpec, new Types[] { Types.Blob }, new String[] { "Signature" }, this, reader); 
			_td[0x1C] = new Table(Types.ImplMap, new Types[] { Types.UInt16, Types.MemberForwarded, Types.String, Types.ModuleRef }, new String[] { "MappingFlags", "MemberForwarded", "ImportName", "ImportScope" }, this, reader);
			_td[0x1D] = new Table(Types.FieldRVA, new Types[] { Types.UInt32, Types.Field }, new String[] { "RVA", "Field" }, this, reader);
			_td[0x1E] = new Table(Types.ENCLog, new Types[] { Types.UInt32, Types.UInt32 }, new String[] { "Token", "FuncCode" }, this, reader);
			_td[0x1F] = new Table(Types.ENCMap, new Types[] { Types.UInt32 }, new String[] { "Token" }, this, reader);
			_td[0x20] = new Table(Types.Assembly, new Types[] { Types.UInt32, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt32, Types.Blob, Types.String, Types.String }, new String[] { "HashAlgId", "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKey", "Name", "Locale" }, this, reader);
			_td[0x21] = new Table(Types.AssemblyProcessor, new Types[] { Types.UInt32 }, new String[] { "Processor" }, this, reader);
			_td[0x22] = new Table(Types.AssemblyOS, new Types[] { Types.UInt32, Types.UInt32, Types.UInt32 }, new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion" }, this, reader);
			_td[0x23] = new Table(Types.AssemblyRef, new Types[] { Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt32, Types.Blob, Types.String, Types.String, Types.Blob }, new String[] { "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKeyOrToken", "Name", "Locale", "HashValue" }, this, reader);
			_td[0x24] = new Table(Types.AssemblyRefProcessor, new Types[] { Types.UInt32, Types.AssemblyRef }, new String[] { "Processor", "AssemblyRef" }, this, reader);
			_td[0x25] = new Table(Types.AssemblyRefOS, new Types[] { Types.UInt32, Types.UInt32, Types.UInt32, Types.AssemblyRef }, new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion", "AssemblyRef" }, this, reader);
			_td[0x26] = new Table(Types.File, new Types[] { Types.UInt32, Types.String, Types.Blob }, new String[] { "Flags", "Name", "HashValue" }, this, reader);
			_td[0x27] = new Table(Types.ExportedType, new Types[] { Types.UInt32, Types.UInt32, Types.String, Types.String, Types.Implementation }, new String[] { "Flags", "TypeDefId", "TypeName", "TypeNamespace", "TypeImplementation" }, this, reader);
			_td[0x28] = new Table(Types.ManifestResource, new Types[] { Types.UInt32, Types.UInt32, Types.String, Types.Implementation }, new String[] { "Offset", "Flags", "Name", "Implementation" }, this, reader);
			_td[0x29] = new Table(Types.NestedClass, new Types[] { Types.TypeDef, Types.TypeDef }, new String[] { "NestedClass", "EnclosingClass"}, this, reader);
			//unused TyPar tables taken from Roeder's reflector... are these documented anywhere?  Since they are always empty, does it matter
			_td[0x2A] = new Table(Types.TypeTyPar, new Types[] { Types.UInt16, Types.TypeDef, Types.TypeDefOrRef, Types.String }, new String[] { "Number", "Class", "Bound", "Name" }, this, reader);
			_td[0x2B] = new Table(Types.MethodTyPar, new Types[] { Types.UInt16, Types.Method, Types.TypeDefOrRef, Types.String }, new String[] { "Number", "Method", "Bound", "Name" }, this, reader);

		}
	}
}


