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
    public partial class BuildStepOptions : UserControl
    {
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepOptions(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;


            TrackFlowCheckBox.AttachToolTip("Calls between functions are tracked so call graphs can work");

            TrackFieldsCheckBox.AttachToolTip("Set and get operations to class members are tracked");

            TrackInstancesCheckBox.AttachToolTip("Creation and deletion of classes are tracked, and class introspection is enabled");

            ReplaceOriginalCheckBox.AttachToolTip(
@"Unchecked: XRay copies and re-compiles the selected files then runs them side by side the originals.
           This case maintains the relative paths the original files had for configuration, etc...

Checked: XRay overwrites the original files with XRayed versions, originals are put in a backup directory.
           Namespaces are kept the same so referencing assemblies should still work.");

            DecompileCSharpCheckBox.AttachToolTip("Recompile time takes signifigantly longer with this option");
        }

        private void BuildStep2_Load(object sender, EventArgs e)
        {
            TrackFlowCheckBox.Checked = Model.TrackFlow;
            TrackExternalCheckBox.Checked = Model.TrackExternal;
            TrackAnonCheckBox.Checked = Model.TrackAnon;
            TrackFieldsCheckBox.Checked = Model.TrackFields;
            TrackInstancesCheckBox.Checked = Model.TrackInstances;
            ReplaceOriginalCheckBox.Checked = Model.ReplaceOriginal;
            RunVerifyCheckbox.Checked = Model.DoVerify;
            MsToolsCheckbox.Checked = Model.CompileWithMS;
            DecompileAgainCheckbox.Checked = Model.DecompileAgain;
            SaveMsilCheckBox.Checked = Model.SaveMsil;
            DecompileCSharpCheckBox.Checked = Model.DecompileCSharp;
        }

        void SaveToModel()
        {
            Model.TrackFlow = TrackFlowCheckBox.Checked;
            Model.TrackExternal = TrackExternalCheckBox.Checked;
            Model.TrackAnon = TrackAnonCheckBox.Checked;
            Model.TrackFields = TrackFieldsCheckBox.Checked;
            Model.TrackInstances = TrackInstancesCheckBox.Checked;
            Model.ReplaceOriginal = ReplaceOriginalCheckBox.Checked;
            Model.DoVerify = RunVerifyCheckbox.Checked;
            Model.CompileWithMS = MsToolsCheckbox.Checked;
            Model.DecompileAgain = DecompileAgainCheckbox.Checked;
            Model.SaveMsil = SaveMsilCheckBox.Checked;
            Model.DecompileCSharp = DecompileCSharpCheckBox.Checked;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.Files);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.ViewerOptions);
        }
    }
}
