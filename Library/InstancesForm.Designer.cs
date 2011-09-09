namespace XLibrary
{
    partial class InstancesForm
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
            this.CreatedLabel = new System.Windows.Forms.Label();
            this.DeletedLabel = new System.Windows.Forms.Label();
            this.ActiveLabel = new System.Windows.Forms.Label();
            this.StaticLabel = new System.Windows.Forms.Label();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CreatedLabel
            // 
            this.CreatedLabel.AutoSize = true;
            this.CreatedLabel.Location = new System.Drawing.Point(21, 48);
            this.CreatedLabel.Name = "CreatedLabel";
            this.CreatedLabel.Size = new System.Drawing.Size(50, 13);
            this.CreatedLabel.TabIndex = 0;
            this.CreatedLabel.Text = "Created: ";
            // 
            // DeletedLabel
            // 
            this.DeletedLabel.AutoSize = true;
            this.DeletedLabel.Location = new System.Drawing.Point(21, 77);
            this.DeletedLabel.Name = "DeletedLabel";
            this.DeletedLabel.Size = new System.Drawing.Size(47, 13);
            this.DeletedLabel.TabIndex = 1;
            this.DeletedLabel.Text = "Deleted:";
            // 
            // ActiveLabel
            // 
            this.ActiveLabel.AutoSize = true;
            this.ActiveLabel.Location = new System.Drawing.Point(21, 108);
            this.ActiveLabel.Name = "ActiveLabel";
            this.ActiveLabel.Size = new System.Drawing.Size(43, 13);
            this.ActiveLabel.TabIndex = 2;
            this.ActiveLabel.Text = "Active: ";
            // 
            // StaticLabel
            // 
            this.StaticLabel.AutoSize = true;
            this.StaticLabel.Location = new System.Drawing.Point(21, 24);
            this.StaticLabel.Name = "StaticLabel";
            this.StaticLabel.Size = new System.Drawing.Size(74, 13);
            this.StaticLabel.TabIndex = 3;
            this.StaticLabel.Text = "Static Created";
            // 
            // InfoLabel
            // 
            this.InfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoLabel.Location = new System.Drawing.Point(21, 144);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(322, 119);
            this.InfoLabel.TabIndex = 4;
            this.InfoLabel.Text = "Info";
            // 
            // InstancesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 272);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.StaticLabel);
            this.Controls.Add(this.ActiveLabel);
            this.Controls.Add(this.DeletedLabel);
            this.Controls.Add(this.CreatedLabel);
            this.Name = "InstancesForm";
            this.Text = "InstancesForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label CreatedLabel;
        private System.Windows.Forms.Label DeletedLabel;
        private System.Windows.Forms.Label ActiveLabel;
        private System.Windows.Forms.Label StaticLabel;
        private System.Windows.Forms.Label InfoLabel;

    }
}