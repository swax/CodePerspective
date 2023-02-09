namespace XLibrary.UI.Panels
{
    partial class AboutPanel
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
            this.VersionLabel = new System.Windows.Forms.Label();
            this.CodePerspectiveLink = new System.Windows.Forms.LinkLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.NewsBrowser = new DeOps.Interface.Views.WebBrowserEx();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(13, 40);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(80, 20);
            this.VersionLabel.TabIndex = 4;
            this.VersionLabel.Text = "Version 1.0";
            // 
            // CodePerspectiveLink
            // 
            this.CodePerspectiveLink.AutoSize = true;
            this.CodePerspectiveLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CodePerspectiveLink.Location = new System.Drawing.Point(13, 72);
            this.CodePerspectiveLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CodePerspectiveLink.Name = "CodePerspectiveLink";
            this.CodePerspectiveLink.Size = new System.Drawing.Size(48, 16);
            this.CodePerspectiveLink.TabIndex = 11;
            this.CodePerspectiveLink.TabStop = true;
            this.CodePerspectiveLink.Text = "GitHub";
            this.CodePerspectiveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CodePerspectiveLink_LinkClicked);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.NewsBrowser);
            this.splitContainer1.Size = new System.Drawing.Size(687, 294);
            this.splitContainer1.SplitterDistance = 225;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 13;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.CodePerspectiveLink);
            this.panel1.Controls.Add(this.VersionLabel);
            this.panel1.Location = new System.Drawing.Point(4, 5);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(300, 285);
            this.panel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(13, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "Code Perspective";
            // 
            // NewsBrowser
            // 
            this.NewsBrowser.AllowWebBrowserDrop = false;
            this.NewsBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NewsBrowser.IsWebBrowserContextMenuEnabled = false;
            this.NewsBrowser.Location = new System.Drawing.Point(4, 5);
            this.NewsBrowser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.NewsBrowser.MinimumSize = new System.Drawing.Size(27, 31);
            this.NewsBrowser.Name = "NewsBrowser";
            this.NewsBrowser.Size = new System.Drawing.Size(446, 284);
            this.NewsBrowser.TabIndex = 0;
            this.NewsBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
            this.NewsBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // AboutPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "AboutPanel";
            this.Size = new System.Drawing.Size(687, 294);
            this.Resize += new System.EventHandler(this.AboutPanel_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DeOps.Interface.Views.WebBrowserEx NewsBrowser;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.LinkLabel CodePerspectiveLink;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
    }
}
