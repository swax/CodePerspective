using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace XBuilder.Panels
{
    public partial class BuildStepFiles : UserControl
    {
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepFiles(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;

            Sync();
        }

        private void AddLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = ".Net Assemblies|*.exe;*.dll";
            open.Multiselect = true;

            if (open.ShowDialog() != DialogResult.OK)
                return;

            Model.AddFiles(open.FileNames);

            Sync();
        }

        public void Sync()
        {
            Model.UpdateOutputPath();

            // update list
            FileList.BeginUpdate();

            FileList.Items.Clear();

            foreach (var file in Model.Files)
                FileList.Items.Add(new XFileItem(file));

            FileList.EndUpdate();

            // update output path
            if (Model.OutputDir != null)
            {
                OutputLink.Text = "Output: " + Model.OutputDir;
                OutputLink.Visible = true;
                NextButton.Enabled = true;
            }
            else
            {
                OutputLink.Visible = false;
                NextButton.Enabled = false;
            }
        }

        private void RemoveLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (var item in FileList.SelectedItems.Cast<XFileItem>())
                Model.Files.Remove(item.File);

            Sync();
        }

        private void ResetLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Model.Files.Clear();

            Sync();
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            Frame.SetStep(BuildStep.TrackingOptions);
        }

        private void OutputLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(Model.OutputDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class XFileItem : ListViewItem
    {
        public XRayedFile File;

        public XFileItem(XRayedFile file)
        {
            File = file;
            Text = file.ToString();
        }
    }
}
