using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace XLibrary.UI
{
    public partial class DetailsNav : UserControl
    {
        MainForm Main;


        public DetailsNav()
        {
            InitializeComponent();
        }

        public void Init(MainForm main)
        {
            Main = main;
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            Main.NavigatePanelUp();
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            Main.NavigatePanel(true);
        }

        private void ForwardButton_Click(object sender, EventArgs e)
        {
            Main.NavigatePanel(false);
        }
    }
}
