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
using System.Collections;
using Asmex.ObjViewer;

namespace Asmex.FileViewer
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class Region
	{
		long _start;
		long _len;

		public Region()
		{
			
		}

		[ObjViewer(false)]
		public long Start
		{
			get{return _start;}
			set{_start = value;}
		}

		[ObjViewer(false)]
		public long Length
		{
			get{return _len;}
			set{_len = value;}
		}

		[ObjViewer(false)]
		public long End
		{
			get{return _start + _len;}
		}

		public override string ToString()
		{
			return this.GetType().Name + "  {" + _start.ToString("X8") + " - " + (_len + _start).ToString("X8") + "}";
		}

	}

	public abstract class MDHeap : Region, IEnumerable, ICollection
	{

		protected SortedList _data;
		protected string _name;

		public MDHeap()
		{
			_data = new SortedList();
		}

		public object GetByOffset(int i)
		{
			return _data[i];
		}
		
		public int Count
		{
			get{return _data.Count;}
		}

		public void CopyTo(Array arr, int idx)
		{
			for(int i=0; i < _data.Count; ++i)
			{
				arr.SetValue(_data[i], i+idx);
			}
		}

		public object SyncRoot
		{
			get{return null;}
		}

		public bool IsSynchronized
		{
			get{return false;}
		}

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(_data.GetValueList());
		}

		public class Enumerator : IEnumerator
		{
			int _curr;
			IList _data;
		
			public Enumerator(IList data)
			{
				_data = data;
				Reset();
			}	
					
			public void Reset()
			{
				_curr = -1;
			}
					
			public bool MoveNext()
			{
				_curr++;
				return (_curr < _data.Count);
			}
					
			public Object Current
			{
				get { return _data[_curr]; }
			}
		}


		public object this[int idx]
		{
			get
			{
				return _data.GetByIndex(idx);
			}
		}

		public override string ToString()
		{
			return _name + "  {" + Start.ToString("X8") + " - " + (Length + Start).ToString("X8") + "}";
		}
	}
}
