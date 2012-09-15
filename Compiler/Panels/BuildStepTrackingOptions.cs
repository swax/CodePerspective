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
    public partial class BuildStepTrackingOptions : UserControl
    {
        BuildFrame Frame;
        BuildModel Model;

        public BuildStepTrackingOptions(BuildFrame frame, BuildModel model)
        {
            InitializeComponent();

            Frame = frame;
            Model = model;


            TrackFlowCheckBox.AttachToolTip("Calls between functions are tracked so call graphs can work");

            TrackFieldsCheckBox.AttachToolTip("Set and get operations to class members are tracked");

            TrackInstancesCheckBox.AttachToolTip("Creation and deletion of classes are tracked, and class introspection is enabled");

        }

        private void BuildStep2_Load(object sender, EventArgs e)
        {
            TrackFunctionsCheckBox.Checked = Model.TrackFunctions;
            TrackFlowCheckBox.Checked = Model.TrackFlow;
            TrackExternalCheckBox.Checked = Model.TrackExternal;
            TrackAnonCheckBox.Checked = Model.TrackAnon;
            TrackFieldsCheckBox.Checked = Model.TrackFields;
            TrackInstancesCheckBox.Checked = Model.TrackInstances;

            TrackFunctionsCheckBox_CheckedChanged(this, null);
        }

        private void TrackFunctionsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TrackFlowCheckBox.Enabled = TrackFunctionsCheckBox.Checked;
            TrackExternalCheckBox.Enabled = TrackFunctionsCheckBox.Checked;
            TrackAnonCheckBox.Enabled = TrackFunctionsCheckBox.Checked;
            TrackFieldsCheckBox.Enabled = TrackFunctionsCheckBox.Checked;
        }

        void SaveToModel()
        {
            Model.TrackFunctions = TrackFunctionsCheckBox.Checked;
            Model.TrackFlow = TrackFunctionsCheckBox.Checked && TrackFlowCheckBox.Checked;
            Model.TrackExternal = TrackFunctionsCheckBox.Checked && TrackExternalCheckBox.Checked;
            Model.TrackAnon = TrackFunctionsCheckBox.Checked && TrackAnonCheckBox.Checked;
            Model.TrackFields = TrackFunctionsCheckBox.Checked && TrackFieldsCheckBox.Checked;

            Model.TrackInstances = TrackInstancesCheckBox.Checked;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.Files);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            SaveToModel();

            Frame.SetStep(BuildStep.BuildOptions);
        }
    }
}
