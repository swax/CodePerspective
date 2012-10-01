using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XLibrary;


namespace XBuilder.Panels
{
    public partial class BuildStepBuildOptions : UserControl
    {        
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepBuildOptions(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;

            ReplaceOriginalCheckBox.AttachToolTip(
@"Unchecked: XRay copies and re-compiles the selected files then runs them side by side the originals.
           This case maintains the relative paths the original files had for configuration, etc...

Checked: XRay overwrites the original files with XRayed versions, originals are put in a backup directory.
           Namespaces are kept the same so referencing assemblies should still work.");

            DecompileCSharpCheckBox.AttachToolTip("Recompile time takes signifigantly longer with this option");
        }

        private void BuildStepBuildOptions_Load(object sender, EventArgs e)
        {
            ReplaceOriginalCheckBox.Checked = Model.ReplaceOriginal;
            MsToolsCheckbox.Checked = Model.CompileWithMS;
            DecompileAgainCheckbox.Checked = Model.DecompileAgain;
            SaveMsilCheckBox.Checked = Model.SaveMsil;
            DecompileCSharpCheckBox.Checked = Model.DecompileCSharp;

            PathTextBox.Text = Model.DatDir;
        }

        private void SaveToModel()
        {
            Model.ReplaceOriginal = ReplaceOriginalCheckBox.Checked;
            Model.CompileWithMS = MsToolsCheckbox.Checked;
            Model.DecompileAgain = DecompileAgainCheckbox.Checked;
            Model.SaveMsil = SaveMsilCheckBox.Checked;
            Model.DecompileCSharp = DecompileCSharpCheckBox.Checked;
            Model.DatDir = PathTextBox.Text;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.TrackingOptions);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.ViewerOptions);
        }
    }
}
