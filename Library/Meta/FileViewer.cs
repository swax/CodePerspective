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

namespace Asmex.FileViewer
{
	public class ModHeaders
	{
		OSHeaders _hdr;
		MetaDataHeaders _md;
		COR20Header _cor20;
		MetaDataTableHeader _mdth;

		public ModHeaders(BinaryReader reader)
		{
			//read coff, pe, section headers
			_hdr = new OSHeaders(reader);

			//find and read COR20 header
			try
			{
				reader.BaseStream.Position = Rva2Offset(_hdr.PEHeader.DataDirs[14].Rva);
				_cor20 = new COR20Header(reader);
			}
			catch(Exception)
			{
				return;
			}

			//find and read md headers
			try
			{
				reader.BaseStream.Position = Rva2Offset(_cor20.MetaData.Rva);
				_md = new MetaDataHeaders(reader);
			}
			catch(Exception)
			{
				return;
			}

			try
			{
				reader.BaseStream.Position = _md.TableStreamHeader.Offset + _md.StorageSigAndHeader.Start;
				_mdth = new MetaDataTableHeader(reader);
			}
			catch(Exception)
			{
				return;
			}
		}

		public uint Rva2Offset(uint rva)
		{
			for (int i = 0; i < _hdr.SectionHeaders.Length; i++)
			{
				SectionHeader sh = _hdr.SectionHeaders[i];
				if ((sh.VirtualAddress <= rva) && (sh.VirtualAddress + sh.SizeOfRawData > rva))
					return (sh.PointerToRawData + (rva - sh.VirtualAddress));
			}
		
			throw new ModException("Module:  Invalid RVA address.");
		}

		public OSHeaders OSHeaders{get{return _hdr;}}
		public MetaDataHeaders MetaDataHeaders{get{return _md;}}
		public COR20Header COR20Header{get{return _cor20;}}
		public MetaDataTableHeader MetaDataTableHeader{get{return _mdth;}}

	}



	public class MModule
	{
		ModHeaders _mh;
		MDStringHeap _stringHeap;
		MDBlobHeap _blobHeap;
		MDGUIDHeap _GUIDHeap;
		MDBlobHeap _USHeap;
		MDTables _tables;
		ImpExports _imp;
		Relocations _fix;

		public MModule(BinaryReader reader)
		{
			try
			{
				_mh = new ModHeaders(reader);
			}
			catch(Exception)
			{
				return;
			}

			//imports
			try
			{
				_imp = new ImpExports(reader, this);
			}
			catch(Exception)
			{
				return;
			}

			//relocs
			try
			{
				_fix = new Relocations(reader, this);
			}
			catch(Exception)
			{
				return;
			}

			//heaps
			try
			{
				_stringHeap = new MDStringHeap(reader, _mh.MetaDataHeaders.StringStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start, _mh.MetaDataHeaders.StringStreamHeader.Size, _mh.MetaDataHeaders.StringStreamHeader.Name);
				_blobHeap = new MDBlobHeap(reader, _mh.MetaDataHeaders.BlobStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start, _mh.MetaDataHeaders.BlobStreamHeader.Size, _mh.MetaDataHeaders.BlobStreamHeader.Name);
				_GUIDHeap = new MDGUIDHeap(reader, _mh.MetaDataHeaders.GUIDStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start, _mh.MetaDataHeaders.GUIDStreamHeader.Size, _mh.MetaDataHeaders.GUIDStreamHeader.Name);

				if (_mh.MetaDataHeaders.USStreamHeader != null)
					_USHeap = new MDBlobHeap(reader, _mh.MetaDataHeaders.USStreamHeader.Offset + _mh.MetaDataHeaders.StorageSigAndHeader.Start, _mh.MetaDataHeaders.USStreamHeader.Size, _mh.MetaDataHeaders.USStreamHeader.Name);
			}
			catch(Exception)
			{
				return;
			}

			//tables
			try
			{
				reader.BaseStream.Position = _mh.MetaDataTableHeader.End;
				_tables = new MDTables(reader, this);
			}
			catch(Exception)
			{
				return;
			}

		}

		public ModHeaders ModHeaders{get{return _mh;}}
		public MDStringHeap StringHeap{get{return _stringHeap;}}
		public MDBlobHeap BlobHeap{get{return _blobHeap;}}
		public MDGUIDHeap GUIDHeap{get{return _GUIDHeap;}}
		public MDBlobHeap USHeap{get{return _USHeap;}}
		public MDTables MDTables{get{return _tables;}}
		public ImpExports ImportExport{get{return _imp;}}
		public Relocations Relocations{get{return _fix;}}

		public static int DecodeInt32(BinaryReader reader)
		{
			int length = reader.ReadByte();
			if ((length & 0x80) == 0) return length;
			if ((length & 0xC0) == 0x80) return ((length & 0x3F) << 8) | reader.ReadByte();
			return ((length & 0x3F) << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();
		}

		public string StringFromRVA(BinaryReader reader, uint rva)
		{
			long offs = reader.BaseStream.Position;
			string s;

			try
			{
				s = "";
				uint nameOffs = ModHeaders.Rva2Offset(rva);
				reader.BaseStream.Position = nameOffs;
				byte b = reader.ReadByte();
				while(b != 0)
				{
					s += (char)b;
					b = reader.ReadByte();
				}
			}
			catch(Exception)
			{
				s = "Unable to find string";
			}
			finally
			{
				reader.BaseStream.Position = offs;
			}
			return s;
		}

	}


	public class ModException : Exception
	{
		public ModException(string ex, Exception e) : base(ex, e) {}
		public ModException(string ex) : base(ex) {}
	}
}
