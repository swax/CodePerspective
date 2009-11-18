using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XBuilder
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }


        private void SetButton_Click(object sender, EventArgs e)
        {
            try
            {
                testPanel1.ResetText(int.Parse(NodesBox.Text), int.Parse(EdgesBox.Text));
            }
            catch
            {
            }
        }

        private void LayoutButton_Click(object sender, EventArgs e)
        {
            testPanel1.LayoutGraph();
        }

        private void UncrossButton_Click(object sender, EventArgs e)
        {
            testPanel1.Uncross();
        }
    }
}
