﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using XLibrary;
using XLibrary.Remote;


namespace XBuilder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            XDef.Test();
            PacketExts.Test();
            GenericPacket.Test();

            Pro.LoadFromDirectory(Application.StartupPath);

            XRay.BuilderVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
