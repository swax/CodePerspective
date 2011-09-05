using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
            lock (XRay.Instances)
            {
                InstanceRecord record;
                if (XRay.Instances.TryGetValue(node.ID, out record))
                {
                    StaticLabel.Text = "Static Created: " + record.StaticCreated.ToString();
                    CreatedLabel.Text = "Created: " + record.Created.ToString();
                    DeletedLabel.Text = "Deleted: " + record.Deleted.ToString();
                    ActiveLabel.Text = "Active: " + record.Active.Count.ToString();
                }
                else
                {
                    StaticLabel.Text = "No record of instance of " + node.Name + " type being created";
                }
            }
        }
    }
}
