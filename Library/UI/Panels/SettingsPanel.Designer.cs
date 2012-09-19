namespace XLibrary.UI.Panels
{
    partial class SettingsPanel
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.FpsLabel = new System.Windows.Forms.Label();
            this.TargetFpsLink = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.CompileSettingsList = new System.Windows.Forms.ListBox();
            this.ConnectionList = new System.Windows.Forms.ListBox();
            this.ModeLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel8 = new System.Windows.Forms.FlowLayoutPanel();
            this.TrackFunctionsCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackCalls = new System.Windows.Forms.CheckBox();
            this.TrackInstances = new System.Windows.Forms.CheckBox();
            this.TrackProfiling = new System.Windows.Forms.CheckBox();
            this.TrackThreadlines = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SecondTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Frame Rate";
            // 
            // FpsLabel
            // 
            this.FpsLabel.Location = new System.Drawing.Point(3, 23);
            this.FpsLabel.Name = "FpsLabel";
            this.FpsLabel.Size = new System.Drawing.Size(103, 69);
            this.FpsLabel.TabIndex = 1;
            this.FpsLabel.Text = "Current: 15 fps...";
            // 
            // TargetFpsLink
            // 
            this.TargetFpsLink.AutoSize = true;
            this.TargetFpsLink.Location = new System.Drawing.Point(3, 92);
            this.TargetFpsLink.Name = "TargetFpsLink";
            this.TargetFpsLink.Size = new System.Drawing.Size(93, 13);
            this.TargetFpsLink.TabIndex = 3;
            this.TargetFpsLink.TabStop = true;
            this.TargetFpsLink.Text = "Target Rate 15fps";
            this.TargetFpsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TargetFpsLink_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(224, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Compile Settings";
            // 
            // CompileSettingsList
            // 
            this.CompileSettingsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.CompileSettingsList.FormattingEnabled = true;
            this.CompileSettingsList.IntegralHeight = false;
            this.CompileSettingsList.Location = new System.Drawing.Point(227, 16);
            this.CompileSettingsList.Name = "CompileSettingsList";
            this.CompileSettingsList.Size = new System.Drawing.Size(184, 141);
            this.CompileSettingsList.TabIndex = 7;
            // 
            // ConnectionList
            // 
            this.ConnectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionList.FormattingEnabled = true;
            this.ConnectionList.IntegralHeight = false;
            this.ConnectionList.Location = new System.Drawing.Point(420, 16);
            this.ConnectionList.Name = "ConnectionList";
            this.ConnectionList.Size = new System.Drawing.Size(250, 141);
            this.ConnectionList.TabIndex = 9;
            // 
            // ModeLabel
            // 
            this.ModeLabel.AutoSize = true;
            this.ModeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModeLabel.Location = new System.Drawing.Point(417, 0);
            this.ModeLabel.Name = "ModeLabel";
            this.ModeLabel.Size = new System.Drawing.Size(117, 13);
            this.ModeLabel.TabIndex = 8;
            this.ModeLabel.Text = "Server/Client Mode";
            // 
            // flowLayoutPanel8
            // 
            this.flowLayoutPanel8.Controls.Add(this.TrackFunctionsCheckBox);
            this.flowLayoutPanel8.Controls.Add(this.TrackCalls);
            this.flowLayoutPanel8.Controls.Add(this.TrackInstances);
            this.flowLayoutPanel8.Controls.Add(this.TrackProfiling);
            this.flowLayoutPanel8.Controls.Add(this.TrackThreadlines);
            this.flowLayoutPanel8.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel8.Location = new System.Drawing.Point(112, 20);
            this.flowLayoutPanel8.Name = "flowLayoutPanel8";
            this.flowLayoutPanel8.Size = new System.Drawing.Size(107, 123);
            this.flowLayoutPanel8.TabIndex = 41;
            // 
            // TrackFunctionsCheckBox
            // 
            this.TrackFunctionsCheckBox.AutoSize = true;
            this.TrackFunctionsCheckBox.Location = new System.Drawing.Point(3, 3);
            this.TrackFunctionsCheckBox.Name = "TrackFunctionsCheckBox";
            this.TrackFunctionsCheckBox.Size = new System.Drawing.Size(88, 17);
            this.TrackFunctionsCheckBox.TabIndex = 12;
            this.TrackFunctionsCheckBox.Text = "Function Hits";
            this.TrackFunctionsCheckBox.UseVisualStyleBackColor = true;
            this.TrackFunctionsCheckBox.CheckedChanged += new System.EventHandler(this.TrackFunctionHits_CheckedChanged);
            // 
            // TrackCalls
            // 
            this.TrackCalls.AutoSize = true;
            this.TrackCalls.Location = new System.Drawing.Point(3, 26);
            this.TrackCalls.Name = "TrackCalls";
            this.TrackCalls.Size = new System.Drawing.Size(74, 17);
            this.TrackCalls.TabIndex = 13;
            this.TrackCalls.Text = "Call Stack";
            this.TrackCalls.UseVisualStyleBackColor = true;
            this.TrackCalls.CheckedChanged += new System.EventHandler(this.TrackCalls_CheckedChanged);
            // 
            // TrackInstances
            // 
            this.TrackInstances.AutoSize = true;
            this.TrackInstances.Location = new System.Drawing.Point(3, 49);
            this.TrackInstances.Name = "TrackInstances";
            this.TrackInstances.Size = new System.Drawing.Size(72, 17);
            this.TrackInstances.TabIndex = 16;
            this.TrackInstances.Text = "Instances";
            this.TrackInstances.UseVisualStyleBackColor = true;
            this.TrackInstances.CheckedChanged += new System.EventHandler(this.TrackInstances_CheckedChanged);
            // 
            // TrackProfiling
            // 
            this.TrackProfiling.AutoSize = true;
            this.TrackProfiling.Location = new System.Drawing.Point(3, 72);
            this.TrackProfiling.Name = "TrackProfiling";
            this.TrackProfiling.Size = new System.Drawing.Size(63, 17);
            this.TrackProfiling.TabIndex = 14;
            this.TrackProfiling.Text = "Profiling";
            this.TrackProfiling.UseVisualStyleBackColor = true;
            this.TrackProfiling.CheckedChanged += new System.EventHandler(this.TrackProfiling_CheckedChanged);
            // 
            // TrackThreadlines
            // 
            this.TrackThreadlines.AutoSize = true;
            this.TrackThreadlines.Location = new System.Drawing.Point(3, 95);
            this.TrackThreadlines.Name = "TrackThreadlines";
            this.TrackThreadlines.Size = new System.Drawing.Size(81, 17);
            this.TrackThreadlines.TabIndex = 15;
            this.TrackThreadlines.Text = "Threadlines";
            this.TrackThreadlines.UseVisualStyleBackColor = true;
            this.TrackThreadlines.CheckedChanged += new System.EventHandler(this.TrackThreadlines_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(109, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "Tracking";
            // 
            // SecondTimer
            // 
            this.SecondTimer.Enabled = true;
            this.SecondTimer.Interval = 1000;
            this.SecondTimer.Tick += new System.EventHandler(this.SecondTimer_Tick);
            // 
            // SettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel8);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ConnectionList);
            this.Controls.Add(this.ModeLabel);
            this.Controls.Add(this.CompileSettingsList);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.TargetFpsLink);
            this.Controls.Add(this.FpsLabel);
            this.Controls.Add(this.label1);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(673, 160);
            this.Load += new System.EventHandler(this.SettingsPanel_Load);
            this.flowLayoutPanel8.ResumeLayout(false);
            this.flowLayoutPanel8.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel TargetFpsLink;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox CompileSettingsList;
        private System.Windows.Forms.ListBox ConnectionList;
        private System.Windows.Forms.Label ModeLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel8;
        public System.Windows.Forms.CheckBox TrackFunctionsCheckBox;
        public System.Windows.Forms.CheckBox TrackCalls;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label FpsLabel;
        private System.Windows.Forms.Timer SecondTimer;
        public System.Windows.Forms.CheckBox TrackProfiling;
        public System.Windows.Forms.CheckBox TrackThreadlines;
        public System.Windows.Forms.CheckBox TrackInstances;
    }
}
