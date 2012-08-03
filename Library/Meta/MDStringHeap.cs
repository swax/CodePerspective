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

namespace Asmex.FileViewer
{
	
	public class MDStringHeap : MDHeap
	{
		public MDStringHeap(BinaryReader reader, long offs, long size, string name) : base()
		{
			reader.BaseStream.Position = offs;
			Start = offs;
			Length = size;
			_name = name;

			string s = "";
			int reloffs = (int)(reader.BaseStream.Position - offs);

			for(long i=offs; i < offs + size; ++i)
			{
				

				//BinaryReader is utf8 by default and that's what we want.
				char c = reader.ReadChar();

				if (c == '\0')
				{
					_data.Add(reloffs, s);
					s = "";
					reloffs = (int)(reader.BaseStream.Position - offs);
				}
				else
				{
					s += c;
				}
			}
		}
	}

	public class MDBlob
	{
		public byte[] _data;
		int _length;

		public MDBlob(BinaryReader reader)
		{
			//read length indicator
			_length = MModule.DecodeInt32(reader);

			_data = reader.ReadBytes(_length);
		}

		public int Length{get{return _length;}}
		public override string ToString()
		{
			int l = 32;
			if (_length < l) l = _length;

			string sAsc = "", s = "";

			for (int i=0; i < l; ++i)
			{
				s += _data[i].ToString("X2") + " ";
				if (_data[i] >= 0x20 && _data[i] < 127)
				{
					sAsc += (char)_data[i];
				}
				else
				{
					sAsc += ".";
				}
			}

			if (_length > 32) 
			{
				sAsc += "...";
				s += "...";
			}

			return /*"[" + sAsc + "]       " +*/ s;
		}	
	
		public byte[] ToBytes()
		{
			return _data;
		}

	}

	public class MDBlobHeap : MDHeap
	{
		public MDBlobHeap(BinaryReader reader, long offs, long size, string name) : base()
		{
			reader.BaseStream.Position = offs;
			Start = offs;
			Length = size;
			_name = name;

            while (reader.BaseStream.Position < offs + size)
            {
                int reloffs = (int)(reader.BaseStream.Position - offs);
                MDBlob mdb = new MDBlob(reader);
                _data.Add(reloffs, mdb);
            }
		}
	}

	public class MDGUID
	{
		Guid _g;

		public static MDGUID Empty = new MDGUID();

		public MDGUID()
		{
			_g = Guid.Empty;
		}

		public MDGUID(BinaryReader reader)
		{
			_g = new Guid(reader.ReadBytes(16));
		}

		public override string ToString()
		{
			return "{" + _g.ToString() + "}";
		}

		public Guid ToGuid()
		{
			return _g;
		}


	}

	public class MDGUIDHeap : MDHeap
	{
		public MDGUIDHeap(BinaryReader reader, long offs, long size, string name) : base()
		{
			reader.BaseStream.Position = offs;
			Start = offs;
			Length = size;
			_name = name;

			while (reader.BaseStream.Position < offs + size)
			{
				int reloffs = (int)(reader.BaseStream.Position - offs);
				MDGUID mdg = new MDGUID(reader);
				_data.Add(reloffs, mdg);
			}
		}
	}
}
