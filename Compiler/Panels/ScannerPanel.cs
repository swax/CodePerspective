using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XLibrary;
using System.IO;
using System.Threading;
using System.Reflection;

namespace XBuilder
{
    public partial class ScannerPanel : UserControl
    {
        Thread ScanThread;
        int FoundCount;
        int ScanCount;
        StringBuilder ErrorLog = new StringBuilder();
        bool StopThread;
        MainForm Main;


        public ScannerPanel()
        {
            InitializeComponent();

            ListViewHelper.EnableDoubleBuffer(FilesList);

            ScanButton.AttachToolTip("Scans selected directory for files that can be xrayed");
        }

        public void Init(MainForm main)
        {
            Main = main;
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            if (ScanThread != null)
            {
                ScanButton.Text = "Stopping";
                StopThread = true;
                return;
            }

            string path = PathTextBox.Text;

            var info = new DirectoryInfo(path);

            FilesList.Items.Clear();

            ScanCount = 0;
            FoundCount = 0;
            ScanButton.Text = "Stop";
            ErrorLog.Clear();

            ScanThread = new Thread(o =>
            {
                ScanDirectory(info);

                StopThread = false;
                ScanThread = null;

                BeginInvoke(new Action(() =>
                {
                    ScanButton.Text = "Scan";
                    //MessageBox.Show("Scan Finished\r\n" + ErrorLog.ToString());
                }));
                
            });
            ScanThread.Start();
        }

        private void ScanDirectory(DirectoryInfo info)
        {
            if (StopThread)
                return;

            try
            {
                foreach(var dir in info.GetDirectories())
                    ScanDirectory(dir);

                foreach (var file in info.GetFiles())
                {
                    if (StopThread)
                        return;

                    ScanCount++;

                    if (file.Name.EndsWith(".dll") || file.Name.EndsWith(".exe"))
                        IsDotNet(file);
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Append("Error at " + info.FullName + " - " + ex.Message + "\r\n");
            }
        }

        private void IsDotNet(FileInfo file)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(file.FullName);
                byte[] asmToken = asm.GetName().GetPublicKeyToken();

                FoundCount++;

                BeginInvoke(new Action(() =>
                {
                    FilesList.Items.Add(new ListViewItem(new string[] { 
                        file.Name, 
                        (asmToken.Length > 0) ? "Yes" : "No", 
                        file.FullName }));
                }));
            }
            catch (Exception ex)
            {
                ErrorLog.Append("File scan error at " + file.FullName + " - " + ex.Message + "\r\n");
            }
        }

        private void PathLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            dialog.Description = "Select folder to scan";
            dialog.SelectedPath = PathTextBox.Text;
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == DialogResult.OK)
                PathTextBox.Text = dialog.SelectedPath;
        }

        private void ScanCountTimer_Tick(object sender, EventArgs e)
        {
            if (ScanCount > 0)
                ResultsLabel.Text = String.Format("{0} .net assemblies found, {1} scanned", FoundCount, ScanCount);
            else
                ResultsLabel.Text = "";
        }

        private void AddToBuildLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var paths = FilesList.SelectedItems.Cast<ListViewItem>().Select(i => i.SubItems[2].Text).ToArray();

            Main.BuildPanel.AddFilesToList(paths);

            Main.MainTabs.SelectedTab = Main.MainTabs.TabPages[0];
        }
    }
}
