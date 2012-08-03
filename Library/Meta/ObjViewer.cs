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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Reflection;

namespace Asmex.ObjViewer
{
	/// <summary>
	/// This is a custom attribute that can be used to indicate how the ObjViewer should treat a 
	/// property of any class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
	public class ObjViewerAttribute : Attribute
	{
		bool _hide;
		bool _rename;
		string _name;
		bool _hex;

		public bool Hide{get{return _hide;} set{_hide = value;}}
		public bool Hex{get{return _hex;} set{_hex = value;}}
		public bool Rename{get{return _rename;} set{_rename = value;}}
		public string Name{get{return _name;} set{_name = value;}}

		public ObjViewerAttribute(string name)
		{
			_hide = false;
			_name = name;
			_rename = true;
		}

		public ObjViewerAttribute(bool show)
		{
			_hide = !show;
			_rename = false;
		}

		public ObjViewerAttribute()
		{
			_hide = false;
			_rename = false;
		}
	}

	/// <summary>
	/// This is a simple yet incredibly useful control that, given an instance of any type, displays
	/// its properties.  If the type is one we control, we can use ObjViewerAttribute to control which
	/// props are displayed and how.  Simple yet brilliant -- it's tha perfect crime!
	/// </summary>
	public class ObjViewer : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ListView lvProps;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.ColumnHeader colName;
		private System.Windows.Forms.ColumnHeader colValue;
		private System.Windows.Forms.Label lblKind;
		private System.ComponentModel.Container components = null;

		public ObjViewer()
		{
			InitializeComponent();

		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lvProps = new System.Windows.Forms.ListView();
			this.colName = new System.Windows.Forms.ColumnHeader();
			this.colValue = new System.Windows.Forms.ColumnHeader();
			this.lblName = new System.Windows.Forms.Label();
			this.lblKind = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lvProps
			// 
			this.lvProps.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.lvProps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.colName,
																					  this.colValue});
			this.lvProps.FullRowSelect = true;
			this.lvProps.GridLines = true;
			this.lvProps.Location = new System.Drawing.Point(0, 72);
			this.lvProps.Name = "lvProps";
			this.lvProps.Size = new System.Drawing.Size(296, 296);
			this.lvProps.TabIndex = 0;
			this.lvProps.View = System.Windows.Forms.View.Details;
			// 
			// colName
			// 
			this.colName.Text = "Property";
			this.colName.Width = 120;
			// 
			// colValue
			// 
			this.colValue.Text = "Value";
			this.colValue.Width = 300;
			// 
			// lblName
			// 
			this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.lblName.Location = new System.Drawing.Point(8, 24);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(287, 48);
			this.lblName.TabIndex = 1;
			this.lblName.Text = "label1";
			// 
			// lblKind
			// 
			this.lblKind.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, (System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblKind.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.lblKind.Location = new System.Drawing.Point(8, 0);
			this.lblKind.Name = "lblKind";
			this.lblKind.Size = new System.Drawing.Size(277, 15);
			this.lblKind.TabIndex = 2;
			this.lblKind.Text = "label1";
			// 
			// ObjViewer
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.lblKind,
																		  this.lblName,
																		  this.lvProps});
			this.Name = "ObjViewer";
			this.Size = new System.Drawing.Size(296, 368);
			this.ResumeLayout(false);

		}
		#endregion


		public void ReadNode(object obj, string title, string subtitle)
		{
			Clear();

			if (obj == null)
			{
				return;
			}

			lblName.Text = subtitle;

			lblKind.Text = title;

			ShowObjectProps(obj);

		}

		void Clear()
		{
			lblName.Text="";
			lblKind.Text = "";
			lvProps.Items.Clear();
		}

		void AddProp(string k, string v)
		{
			lvProps.Items.Add(new ListViewItem(new string[] {k,v}));
		}

		void ShowObjectProps(object o)
		{
			Type type = o.GetType();

			PropertyInfo[] pi = type.GetProperties();

			for(int i=0; i < pi.Length; ++i)
			{
				object ret = null;
				object[] atts;
				try
				{
					atts = pi[i].GetCustomAttributes(typeof(ObjViewerAttribute), true);

					ret = type.InvokeMember(pi[i].Name, BindingFlags.GetProperty, null, o, new object [] {});

					if (atts.Length == 0)
					{
						AddProp(pi[i].Name, (ret==null)?"null":ret.ToString());
					}
					else
					{
						ObjViewerAttribute att = (ObjViewerAttribute)atts[0];
						if(att.Hide == false)
						{
							string sName, sVal;

							if (att.Rename)
							{
								sName = att.Name;
							}
							else
							{
								sName = pi[i].Name;
							}

							sVal = ret.ToString();

							if (att.Hex)
							{
								if (ret is Int16) sVal = "0x"+((Int16)ret).ToString("X");
								if (ret is Int32) sVal = "0x"+((Int32)ret).ToString("X8");
								if (ret is Int64) sVal = "0x"+((Int64)ret).ToString("X");
								if (ret is UInt16) sVal = "0x"+((UInt16)ret).ToString("X");
								if (ret is UInt32) sVal = "0x"+((UInt32)ret).ToString("X8");
								if (ret is UInt64) sVal = "0x"+((UInt64)ret).ToString("X");
							}

							AddProp(sName, sVal);
						}
					}
				}
				catch(Exception e)
				{
					ret = e;
				}
			}
		}
	}
}
