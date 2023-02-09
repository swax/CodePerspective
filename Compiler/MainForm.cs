using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using XLibrary;


namespace XBuilder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            ScannerPanel.Init(this);
        }
    }
}
