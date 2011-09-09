using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace XLibrary
{
    public partial class InstancesForm : Form
    {
        public InstancesForm(XNodeIn node)
        {
            InitializeComponent();

            NavigateTo(node);
        }

        private void NavigateTo(XNodeIn node)
        {
            if (node.Record != null)
            {
                var record = node.Record;

                StaticLabel.Text = "Static Created: " + record.StaticCreated.ToString();
                CreatedLabel.Text = "Created: " + record.Created.ToString();
                DeletedLabel.Text = "Deleted: " + record.Deleted.ToString();
                ActiveLabel.Text = "Active: " + record.Active.Count.ToString();

                InfoLabel.Text = "No active objects found";

                LoadInfo(record);
            }
            else
            {
                StaticLabel.Text = "No record of instance of " + node.Name + " type being created";
            }  
        }

        void LoadInfo(InstanceRecord record)
        {
            Object instance = null;

            var weakRef = record.Active.FirstOrDefault();
            if (weakRef != null)
                instance = weakRef.Target;     

            string info = "";// instanceType.GetFields().Length.ToString();

            foreach (var field in record.InstanceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
            {
                if(field.IsStatic || instance != null)
                    info += field.FieldType.Name.ToString() + " " + field.Name + " = " + field.GetValue(instance) + "\r\n";
            }

            InfoLabel.Text = info;
        }
    }


}
