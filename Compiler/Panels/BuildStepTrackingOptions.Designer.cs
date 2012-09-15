namespace XBuilder.Panels
{
    partial class BuildStepTrackingOptions
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.TrackInstancesCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackFieldsCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackExternalCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackFlowCheckBox = new System.Windows.Forms.CheckBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.TrackAnonCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackFunctionsCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 20);
            this.label1.TabIndex = 23;
            this.label1.Text = "Tracking Options";
            // 
            // TrackInstancesCheckBox
            // 
            this.TrackInstancesCheckBox.AutoSize = true;
            this.TrackInstancesCheckBox.Location = new System.Drawing.Point(7, 170);
            this.TrackInstancesCheckBox.Name = "TrackInstancesCheckBox";
            this.TrackInstancesCheckBox.Size = new System.Drawing.Size(320, 17);
            this.TrackInstancesCheckBox.TabIndex = 31;
            this.TrackInstancesCheckBox.Text = "Track class instance construction and disposal (low overhead)";
            this.TrackInstancesCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFieldsCheckBox
            // 
            this.TrackFieldsCheckBox.AutoSize = true;
            this.TrackFieldsCheckBox.Location = new System.Drawing.Point(32, 90);
            this.TrackFieldsCheckBox.Name = "TrackFieldsCheckBox";
            this.TrackFieldsCheckBox.Size = new System.Drawing.Size(234, 17);
            this.TrackFieldsCheckBox.TabIndex = 30;
            this.TrackFieldsCheckBox.Text = "Track get/set operations on class properties";
            this.TrackFieldsCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackExternalCheckBox
            // 
            this.TrackExternalCheckBox.AutoSize = true;
            this.TrackExternalCheckBox.Location = new System.Drawing.Point(32, 113);
            this.TrackExternalCheckBox.Name = "TrackExternalCheckBox";
            this.TrackExternalCheckBox.Size = new System.Drawing.Size(349, 17);
            this.TrackExternalCheckBox.TabIndex = 28;
            this.TrackExternalCheckBox.Text = "Track calls to external assemblies (ones not chosen in previous step)";
            this.TrackExternalCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFlowCheckBox
            // 
            this.TrackFlowCheckBox.AutoSize = true;
            this.TrackFlowCheckBox.Location = new System.Drawing.Point(32, 67);
            this.TrackFlowCheckBox.Name = "TrackFlowCheckBox";
            this.TrackFlowCheckBox.Size = new System.Drawing.Size(273, 17);
            this.TrackFlowCheckBox.TabIndex = 26;
            this.TrackFlowCheckBox.Text = "Track function stack (adds hooks to method returns)";
            this.TrackFlowCheckBox.UseVisualStyleBackColor = true;
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Location = new System.Drawing.Point(299, 314);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 34;
            this.NextButton.Text = "Next >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // BackButton
            // 
            this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackButton.Location = new System.Drawing.Point(218, 314);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(75, 23);
            this.BackButton.TabIndex = 35;
            this.BackButton.Text = "< Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // TrackAnonCheckBox
            // 
            this.TrackAnonCheckBox.AutoSize = true;
            this.TrackAnonCheckBox.Location = new System.Drawing.Point(32, 136);
            this.TrackAnonCheckBox.Name = "TrackAnonCheckBox";
            this.TrackAnonCheckBox.Size = new System.Drawing.Size(213, 17);
            this.TrackAnonCheckBox.TabIndex = 36;
            this.TrackAnonCheckBox.Text = "Track anonymous classes and methods";
            this.TrackAnonCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrackFunctionsCheckBox
            // 
            this.TrackFunctionsCheckBox.AutoSize = true;
            this.TrackFunctionsCheckBox.Location = new System.Drawing.Point(7, 44);
            this.TrackFunctionsCheckBox.Name = "TrackFunctionsCheckBox";
            this.TrackFunctionsCheckBox.Size = new System.Drawing.Size(255, 17);
            this.TrackFunctionsCheckBox.TabIndex = 37;
            this.TrackFunctionsCheckBox.Text = "Track function hits (adds hooks to method enter)";
            this.TrackFunctionsCheckBox.UseVisualStyleBackColor = true;
            this.TrackFunctionsCheckBox.CheckedChanged += new System.EventHandler(this.TrackFunctionsCheckBox_CheckedChanged);
            // 
            // BuildStepTrackingOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TrackFunctionsCheckBox);
            this.Controls.Add(this.TrackAnonCheckBox);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.TrackInstancesCheckBox);
            this.Controls.Add(this.TrackFieldsCheckBox);
            this.Controls.Add(this.TrackExternalCheckBox);
            this.Controls.Add(this.TrackFlowCheckBox);
            this.Controls.Add(this.label1);
            this.Name = "BuildStepTrackingOptions";
            this.Size = new System.Drawing.Size(377, 340);
            this.Load += new System.EventHandler(this.BuildStep2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox TrackInstancesCheckBox;
        private System.Windows.Forms.CheckBox TrackFieldsCheckBox;
        private System.Windows.Forms.CheckBox TrackExternalCheckBox;
        private System.Windows.Forms.CheckBox TrackFlowCheckBox;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.CheckBox TrackAnonCheckBox;
        private System.Windows.Forms.CheckBox TrackFunctionsCheckBox;
    }
}
