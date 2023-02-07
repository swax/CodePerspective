using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using XLibrary;
using System.Diagnostics;

namespace XBuilder.Panels
{
    public partial class BuildStepRun : UserControl
    {
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepRun(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            Frame.SetStep(BuildStep.Compile);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            Frame.SetStep(BuildStep.Files);
        }

        private void AnalzeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string path = Path.Combine(Model.OutputDir, "XRay.dat");

                XRay.Analyze(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LaunchButton_Click(object sender, EventArgs e)
        {
                if (Model.Files.Count == 0)
            {
                MessageBox.Show("Nothing to launch");
                return;
            }

            // execute selected assembly
            var item = Model.Files.FirstOrDefault(i => i.FileName.EndsWith(".exe"));

            if (item == null)
            {
                var filepath = Model.Files.First().FilePath;
                var filedir = Path.GetDirectoryName(filepath);
                Process.Start(new ProcessStartInfo(filedir) { UseShellExecute = true });

                //MessageBox.Show("Can only launch exes, for dlls launch the exe that uses the dll.");
                return;
            }

            if (item.RecompiledPath == null)
            {
                MessageBox.Show(item.ToString() + " has not been re-compiled yet");
                return;
            }

            var info = new ProcessStartInfo(item.RecompiledPath);
            info.WorkingDirectory = Path.GetDirectoryName(item.RecompiledPath);

            Process.Start(info);
        }

        private void VerifyLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string errorLog = "";

            foreach (var item in Model.Files)
            {
                try
                {
                    XDecompile.Verify(item.RecompiledPath);
                }
                catch (CompileError ex)
                {
                    errorLog += item.FileName + "\r\n" + ex.Summary + "\r\n--------------------------------------------------------\r\n";
                }
            }

            if (errorLog.Length == 0)
            {
                MessageBox.Show("Verified");
            }
            else
            {
                errorLog = errorLog.Replace("Unexpected type on the stack.",
                    "Unexpected type on the stack. (Ignore)");

                errorLog = errorLog.Replace("Unmanaged pointers are not a verifiable type.",
                    "Unmanaged pointers are not a verifiable type. (Ignore)");

                if (!Model.ReplaceOriginal)
                    errorLog = errorLog.Replace("Unable to resolve token.",
                        "Unable to resolve token. (Try turning off side by side)");

                //todo token error - turn off side by side

                string logPath = Path.Combine(Application.StartupPath, "errorlog.txt");
                File.WriteAllText(logPath, errorLog);
                Process.Start(logPath);
            }
        }

    }
}
