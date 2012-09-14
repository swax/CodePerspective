namespace XBuilder.Panels
{
    partial class BuildStepFiles
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
            this.RemoveLink = new System.Windows.Forms.LinkLabel();
            this.AddLink = new System.Windows.Forms.LinkLabel();
            this.ResetLink = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.NextButton = new System.Windows.Forms.Button();
            this.OutputLink = new System.Windows.Forms.LinkLabel();
            this.FileList = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // RemoveLink
            // 
            this.RemoveLink.AutoSize = true;
            this.RemoveLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.RemoveLink.Location = new System.Drawing.Point(36, 51);
            this.RemoveLink.Name = "RemoveLink";
            this.RemoveLink.Size = new System.Drawing.Size(47, 13);
            this.RemoveLink.TabIndex = 24;
            this.RemoveLink.TabStop = true;
            this.RemoveLink.Text = "Remove";
            this.RemoveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveLink_LinkClicked);
            // 
            // AddLink
            // 
            this.AddLink.AutoSize = true;
            this.AddLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.AddLink.Location = new System.Drawing.Point(4, 51);
            this.AddLink.Name = "AddLink";
            this.AddLink.Size = new System.Drawing.Size(26, 13);
            this.AddLink.TabIndex = 23;
            this.AddLink.TabStop = true;
            this.AddLink.Text = "Add";
            this.AddLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AddLink_LinkClicked);
            // 
            // ResetLink
            // 
            this.ResetLink.AutoSize = true;
            this.ResetLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.ResetLink.Location = new System.Drawing.Point(89, 51);
            this.ResetLink.Name = "ResetLink";
            this.ResetLink.Size = new System.Drawing.Size(35, 13);
            this.ResetLink.TabIndex = 25;
            this.ResetLink.TabStop = true;
            this.ResetLink.Text = "Reset";
            this.ResetLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetLink_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 20);
            this.label1.TabIndex = 22;
            this.label1.Text = "Add Assemblies to XRay";
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Enabled = false;
            this.NextButton.Location = new System.Drawing.Point(283, 272);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 26;
            this.NextButton.Text = "Next >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // OutputLink
            // 
            this.OutputLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputLink.AutoEllipsis = true;
            this.OutputLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.OutputLink.Location = new System.Drawing.Point(4, 277);
            this.OutputLink.Name = "OutputLink";
            this.OutputLink.Size = new System.Drawing.Size(273, 13);
            this.OutputLink.TabIndex = 27;
            this.OutputLink.TabStop = true;
            this.OutputLink.Text = "Output: Path";
            this.OutputLink.Visible = false;
            this.OutputLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OutputLink_LinkClicked);
            // 
            // FileList
            // 
            this.FileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileList.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileList.FullRowSelect = true;
            this.FileList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.FileList.Location = new System.Drawing.Point(3, 67);
            this.FileList.Name = "FileList";
            this.FileList.Size = new System.Drawing.Size(355, 199);
            this.FileList.TabIndex = 28;
            this.FileList.UseCompatibleStateImageBehavior = false;
            this.FileList.View = System.Windows.Forms.View.List;
            // 
            // BuildStepFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FileList);
            this.Controls.Add(this.OutputLink);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.RemoveLink);
            this.Controls.Add(this.AddLink);
            this.Controls.Add(this.ResetLink);
            this.Controls.Add(this.label1);
            this.Name = "BuildStepFiles";
            this.Size = new System.Drawing.Size(361, 298);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel RemoveLink;
        private System.Windows.Forms.LinkLabel AddLink;
        private System.Windows.Forms.LinkLabel ResetLink;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.LinkLabel OutputLink;
        private System.Windows.Forms.ListView FileList;
    }
}
