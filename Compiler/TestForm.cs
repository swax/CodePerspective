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
                testPanel1.Reset(int.Parse(NodesBox.Text), int.Parse(EdgesBox.Text), int.Parse(WeightBox.Text));
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

        private void MinDistButton_Click(object sender, EventArgs e)
        {
            testPanel1.MinDistance();
        }

        private void AutoButton_Click(object sender, EventArgs e)
        {
            testPanel1.LayoutGraph();

            for (int i = 0; i < 7; i++)
                testPanel1.Uncross();

            for (int i = 0; i < 7; i++)
                testPanel1.MinDistance();
        }
    }
}
