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
            this.LicenseLabel = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.NameLabel = new System.Windows.Forms.Label();
            this.CompanyLabel = new System.Windows.Forms.Label();
            this.DateLabel = new System.Windows.Forms.Label();
            this.CodePerspectiveLink = new System.Windows.Forms.LinkLabel();
            this.ProBox = new System.Windows.Forms.GroupBox();
            this.IdLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.GoProLink = new System.Windows.Forms.LinkLabel();
            this.NewsLink = new System.Windows.Forms.LinkLabel();
            this.CopyrightLink = new System.Windows.Forms.LinkLabel();
            this.NewsBrowser = new DeOps.Interface.Views.WebBrowserEx();
            this.ProBox.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LicenseLabel
            // 
            this.LicenseLabel.AutoSize = true;
            this.LicenseLabel.Location = new System.Drawing.Point(18, 52);
            this.LicenseLabel.Name = "LicenseLabel";
            this.LicenseLabel.Size = new System.Drawing.Size(78, 13);
            this.LicenseLabel.TabIndex = 3;
            this.LicenseLabel.Text = "License: AGPL";
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(18, 32);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(60, 13);
            this.VersionLabel.TabIndex = 4;
            this.VersionLabel.Text = "Version 1.0";
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NameLabel.Location = new System.Drawing.Point(17, 22);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(64, 13);
            this.NameLabel.TabIndex = 8;
            this.NameLabel.Text = "Name: John";
            // 
            // CompanyLabel
            // 
            this.CompanyLabel.AutoSize = true;
            this.CompanyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CompanyLabel.Location = new System.Drawing.Point(17, 42);
            this.CompanyLabel.Name = "CompanyLabel";
            this.CompanyLabel.Size = new System.Drawing.Size(91, 13);
            this.CompanyLabel.TabIndex = 9;
            this.CompanyLabel.Text = "Company: Firesoft";
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateLabel.Location = new System.Drawing.Point(17, 62);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(99, 13);
            this.DateLabel.TabIndex = 10;
            this.DateLabel.Text = "Date: July 11, 2012";
            // 
            // CodePerspectiveLink
            // 
            this.CodePerspectiveLink.AutoSize = true;
            this.CodePerspectiveLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodePerspectiveLink.Location = new System.Drawing.Point(3, 9);
            this.CodePerspectiveLink.Name = "CodePerspectiveLink";
            this.CodePerspectiveLink.Size = new System.Drawing.Size(107, 13);
            this.CodePerspectiveLink.TabIndex = 11;
            this.CodePerspectiveLink.TabStop = true;
            this.CodePerspectiveLink.Text = "Code Perspective";
            this.CodePerspectiveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CodePerspectiveLink_LinkClicked);
            // 
            // ProBox
            // 
            this.ProBox.Controls.Add(this.IdLabel);
            this.ProBox.Controls.Add(this.NameLabel);
            this.ProBox.Controls.Add(this.CompanyLabel);
            this.ProBox.Controls.Add(this.DateLabel);
            this.ProBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProBox.Location = new System.Drawing.Point(6, 75);
            this.ProBox.Name = "ProBox";
            this.ProBox.Size = new System.Drawing.Size(210, 105);
            this.ProBox.TabIndex = 12;
            this.ProBox.TabStop = false;
            this.ProBox.Text = "Pro Details";
            // 
            // IdLabel
            // 
            this.IdLabel.AutoSize = true;
            this.IdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IdLabel.Location = new System.Drawing.Point(17, 82);
            this.IdLabel.Name = "IdLabel";
            this.IdLabel.Size = new System.Drawing.Size(60, 13);
            this.IdLabel.TabIndex = 11;
            this.IdLabel.Text = "ID: 234234";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.NewsLink);
            this.splitContainer1.Panel2.Controls.Add(this.CopyrightLink);
            this.splitContainer1.Panel2.Controls.Add(this.NewsBrowser);
            this.splitContainer1.Size = new System.Drawing.Size(515, 191);
            this.splitContainer1.SplitterDistance = 225;
            this.splitContainer1.TabIndex = 13;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.GoProLink);
            this.panel1.Controls.Add(this.CodePerspectiveLink);
            this.panel1.Controls.Add(this.ProBox);
            this.panel1.Controls.Add(this.VersionLabel);
            this.panel1.Controls.Add(this.LicenseLabel);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(225, 185);
            this.panel1.TabIndex = 2;
            // 
            // GoProLink
            // 
            this.GoProLink.AutoSize = true;
            this.GoProLink.Location = new System.Drawing.Point(102, 52);
            this.GoProLink.Name = "GoProLink";
            this.GoProLink.Size = new System.Drawing.Size(40, 13);
            this.GoProLink.TabIndex = 2;
            this.GoProLink.TabStop = true;
            this.GoProLink.Text = "Go Pro";
            this.GoProLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoProLink_LinkClicked);
            // 
            // NewsLink
            // 
            this.NewsLink.AutoSize = true;
            this.NewsLink.Location = new System.Drawing.Point(5, 12);
            this.NewsLink.Name = "NewsLink";
            this.NewsLink.Size = new System.Drawing.Size(98, 13);
            this.NewsLink.TabIndex = 3;
            this.NewsLink.TabStop = true;
            this.NewsLink.Text = "News and Updates";
            this.NewsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NewsLink_LinkClicked);
            // 
            // CopyrightLink
            // 
            this.CopyrightLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyrightLink.AutoSize = true;
            this.CopyrightLink.Location = new System.Drawing.Point(193, 12);
            this.CopyrightLink.Name = "CopyrightLink";
            this.CopyrightLink.Size = new System.Drawing.Size(90, 13);
            this.CopyrightLink.TabIndex = 2;
            this.CopyrightLink.TabStop = true;
            this.CopyrightLink.Text = "Copyright Notices";
            this.CopyrightLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CopyrightLink_LinkClicked);
            // 
            // NewsBrowser
            // 
            this.NewsBrowser.AllowWebBrowserDrop = false;
            this.NewsBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NewsBrowser.IsWebBrowserContextMenuEnabled = false;
            this.NewsBrowser.Location = new System.Drawing.Point(8, 28);
            this.NewsBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.NewsBrowser.Name = "NewsBrowser";
            this.NewsBrowser.Size = new System.Drawing.Size(275, 160);
            this.NewsBrowser.TabIndex = 0;
            this.NewsBrowser.Url = new System.Uri("", System.UriKind.Relative);
            this.NewsBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // AboutPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "AboutPanel";
            this.Size = new System.Drawing.Size(515, 191);
            this.Resize += new System.EventHandler(this.AboutPanel_Resize);
            this.ProBox.ResumeLayout(false);
            this.ProBox.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DeOps.Interface.Views.WebBrowserEx NewsBrowser;
        private System.Windows.Forms.Label LicenseLabel;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label CompanyLabel;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.LinkLabel CodePerspectiveLink;
        private System.Windows.Forms.GroupBox ProBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label IdLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.LinkLabel GoProLink;
        private System.Windows.Forms.LinkLabel NewsLink;
        private System.Windows.Forms.LinkLabel CopyrightLink;
    }
}
