namespace XLibrary
{
    partial class DebugForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RefreshButton = new System.Windows.Forms.Button();
            this.DebugOutput = new System.Windows.Forms.TextBox();
            this.ResolveBox = new System.Windows.Forms.TextBox();
            this.ResolveLink = new System.Windows.Forms.LinkLabel();
            this.ResolveLabel = new System.Windows.Forms.Label();
            this.CallLogCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // RefreshButton
            // 
            this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshButton.Location = new System.Drawing.Point(366, 343);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(73, 22);
            this.RefreshButton.TabIndex = 1;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // DebugOutput
            // 
            this.DebugOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DebugOutput.Location = new System.Drawing.Point(12, 12);
            this.DebugOutput.Multiline = true;
            this.DebugOutput.Name = "DebugOutput";
            this.DebugOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DebugOutput.Size = new System.Drawing.Size(427, 324);
            this.DebugOutput.TabIndex = 0;
            // 
            // ResolveBox
            // 
            this.ResolveBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResolveBox.Location = new System.Drawing.Point(12, 345);
            this.ResolveBox.Name = "ResolveBox";
            this.ResolveBox.Size = new System.Drawing.Size(51, 20);
            this.ResolveBox.TabIndex = 3;
            // 
            // ResolveLink
            // 
            this.ResolveLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResolveLink.AutoSize = true;
            this.ResolveLink.Location = new System.Drawing.Point(69, 348);
            this.ResolveLink.Name = "ResolveLink";
            this.ResolveLink.Size = new System.Drawing.Size(60, 13);
            this.ResolveLink.TabIndex = 4;
            this.ResolveLink.TabStop = true;
            this.ResolveLink.Text = "Resolve ID";
            this.ResolveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResolveLink_LinkClicked);
            // 
            // ResolveLabel
            // 
            this.ResolveLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResolveLabel.AutoSize = true;
            this.ResolveLabel.Location = new System.Drawing.Point(135, 348);
            this.ResolveLabel.Name = "ResolveLabel";
            this.ResolveLabel.Size = new System.Drawing.Size(0, 13);
            this.ResolveLabel.TabIndex = 5;
            // 
            // CallLogCheckBox
            // 
            this.CallLogCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CallLogCheckBox.AutoSize = true;
            this.CallLogCheckBox.Location = new System.Drawing.Point(265, 347);
            this.CallLogCheckBox.Name = "CallLogCheckBox";
            this.CallLogCheckBox.Size = new System.Drawing.Size(84, 17);
            this.CallLogCheckBox.TabIndex = 7;
            this.CallLogCheckBox.Text = "Call Logging";
            this.CallLogCheckBox.UseVisualStyleBackColor = true;
            this.CallLogCheckBox.CheckedChanged += new System.EventHandler(this.CallLogCheckBox_CheckedChanged);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 377);
            this.Controls.Add(this.CallLogCheckBox);
            this.Controls.Add(this.ResolveLabel);
            this.Controls.Add(this.ResolveLink);
            this.Controls.Add(this.ResolveBox);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.DebugOutput);
            this.Name = "DebugForm";
            this.Text = "Debug";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.TextBox DebugOutput;
        private System.Windows.Forms.TextBox ResolveBox;
        private System.Windows.Forms.LinkLabel ResolveLink;
        private System.Windows.Forms.Label ResolveLabel;
        private System.Windows.Forms.CheckBox CallLogCheckBox;
    }
}