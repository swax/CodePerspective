namespace XLibrary
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.RedrawTimer = new System.Windows.Forms.Timer(this.components);
            this.RevalueTimer = new System.Windows.Forms.Timer(this.components);
            this.SearchTimer = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.MainToolStrip = new System.Windows.Forms.ToolStrip();
            this.OnOffButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.BackButton = new System.Windows.Forms.ToolStripButton();
            this.ForwardButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SearchTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.SearchToolButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.ClearSearchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SubsSearchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TabPanel = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.DisplayTab = new XLibrary.Panels.ViewPanel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.NamespaceTab = new XLibrary.UI.Panels.NamespacePanel();
            this.CodeTab = new XLibrary.Panels.CodePanel();
            this.InstanceTab = new XLibrary.InstancePanel();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.ConsoleTab = new XLibrary.Panels.ConsolePanel();
            this.ViewHostPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.MainToolStrip.SuspendLayout();
            this.TabPanel.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.ViewHostPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // RedrawTimer
            // 
            this.RedrawTimer.Interval = 30;
            this.RedrawTimer.Tick += new System.EventHandler(this.RedrawTimer_Tick);
            // 
            // RevalueTimer
            // 
            this.RevalueTimer.Interval = 1000;
            this.RevalueTimer.Tick += new System.EventHandler(this.RevalueTimer_Tick);
            // 
            // SearchTimer
            // 
            this.SearchTimer.Enabled = true;
            this.SearchTimer.Interval = 300;
            this.SearchTimer.Tick += new System.EventHandler(this.SearchTimer_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ViewHostPanel);
            this.splitContainer1.Panel1.Controls.Add(this.MainToolStrip);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitContainer1.Panel2.Controls.Add(this.TabPanel);
            this.splitContainer1.Size = new System.Drawing.Size(550, 584);
            this.splitContainer1.SplitterDistance = 360;
            this.splitContainer1.TabIndex = 4;
            // 
            // MainToolStrip
            // 
            this.MainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OnOffButton,
            this.toolStripSeparator2,
            this.BackButton,
            this.ForwardButton,
            this.toolStripSeparator1,
            this.SearchTextBox,
            this.SearchToolButton});
            this.MainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.MainToolStrip.Name = "MainToolStrip";
            this.MainToolStrip.Size = new System.Drawing.Size(550, 25);
            this.MainToolStrip.TabIndex = 4;
            this.MainToolStrip.Text = "toolStrip1";
            // 
            // OnOffButton
            // 
            this.OnOffButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OnOffButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OnOffButton.ForeColor = System.Drawing.Color.Green;
            this.OnOffButton.Image = ((System.Drawing.Image)(resources.GetObject("OnOffButton.Image")));
            this.OnOffButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OnOffButton.Name = "OnOffButton";
            this.OnOffButton.Size = new System.Drawing.Size(25, 22);
            this.OnOffButton.Text = "on";
            this.OnOffButton.Click += new System.EventHandler(this.OnOffButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // BackButton
            // 
            this.BackButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.BackButton.Image = ((System.Drawing.Image)(resources.GetObject("BackButton.Image")));
            this.BackButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(23, 22);
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // ForwardButton
            // 
            this.ForwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ForwardButton.Image = ((System.Drawing.Image)(resources.GetObject("ForwardButton.Image")));
            this.ForwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ForwardButton.Name = "ForwardButton";
            this.ForwardButton.Size = new System.Drawing.Size(23, 22);
            this.ForwardButton.Click += new System.EventHandler(this.ForwardButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(100, 25);
            this.SearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            // 
            // SearchToolButton
            // 
            this.SearchToolButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.SearchToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SearchToolButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearSearchMenuItem,
            this.SubsSearchMenuItem});
            this.SearchToolButton.Image = ((System.Drawing.Image)(resources.GetObject("SearchToolButton.Image")));
            this.SearchToolButton.Name = "SearchToolButton";
            this.SearchToolButton.Padding = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.SearchToolButton.Size = new System.Drawing.Size(35, 22);
            this.SearchToolButton.Text = "toolStripLabel1";
            // 
            // ClearSearchMenuItem
            // 
            this.ClearSearchMenuItem.Name = "ClearSearchMenuItem";
            this.ClearSearchMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ClearSearchMenuItem.Text = "Clear";
            this.ClearSearchMenuItem.Click += new System.EventHandler(this.ClearSearchMenuItem_Click);
            // 
            // SubsSearchMenuItem
            // 
            this.SubsSearchMenuItem.CheckOnClick = true;
            this.SubsSearchMenuItem.Name = "SubsSearchMenuItem";
            this.SubsSearchMenuItem.Size = new System.Drawing.Size(152, 22);
            this.SubsSearchMenuItem.Text = "Highlight Subs";
            this.SubsSearchMenuItem.CheckedChanged += new System.EventHandler(this.SubsSearchMenuItem_CheckedChanged);
            // 
            // TabPanel
            // 
            this.TabPanel.Controls.Add(this.tabPage1);
            this.TabPanel.Controls.Add(this.tabPage2);
            this.TabPanel.Controls.Add(this.tabPage5);
            this.TabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabPanel.Location = new System.Drawing.Point(0, 0);
            this.TabPanel.Name = "TabPanel";
            this.TabPanel.SelectedIndex = 0;
            this.TabPanel.Size = new System.Drawing.Size(550, 220);
            this.TabPanel.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.DisplayTab);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(542, 194);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Display";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // DisplayTab
            // 
            this.DisplayTab.AutoScroll = true;
            this.DisplayTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DisplayTab.Location = new System.Drawing.Point(3, 3);
            this.DisplayTab.Name = "DisplayTab";
            this.DisplayTab.Size = new System.Drawing.Size(536, 188);
            this.DisplayTab.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.NamespaceTab);
            this.tabPage2.Controls.Add(this.CodeTab);
            this.tabPage2.Controls.Add(this.InstanceTab);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(542, 194);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Details";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // NamespaceTab
            // 
            this.NamespaceTab.Location = new System.Drawing.Point(392, 14);
            this.NamespaceTab.Name = "NamespaceTab";
            this.NamespaceTab.Size = new System.Drawing.Size(142, 174);
            this.NamespaceTab.TabIndex = 2;
            // 
            // CodeTab
            // 
            this.CodeTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CodeTab.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CodeTab.Location = new System.Drawing.Point(209, 14);
            this.CodeTab.Name = "CodeTab";
            this.CodeTab.Size = new System.Drawing.Size(177, 174);
            this.CodeTab.TabIndex = 1;
            // 
            // InstanceTab
            // 
            this.InstanceTab.Location = new System.Drawing.Point(8, 14);
            this.InstanceTab.Name = "InstanceTab";
            this.InstanceTab.Size = new System.Drawing.Size(195, 172);
            this.InstanceTab.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.ConsoleTab);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(542, 194);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Console";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // ConsoleTab
            // 
            this.ConsoleTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleTab.Location = new System.Drawing.Point(0, 0);
            this.ConsoleTab.Name = "ConsoleTab";
            this.ConsoleTab.Size = new System.Drawing.Size(542, 194);
            this.ConsoleTab.TabIndex = 0;
            // 
            // ViewHostPanel
            // 
            this.ViewHostPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewHostPanel.Location = new System.Drawing.Point(0, 28);
            this.ViewHostPanel.Name = "ViewHostPanel";
            this.ViewHostPanel.Size = new System.Drawing.Size(550, 330);
            this.ViewHostPanel.TabIndex = 7;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 584);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "Ghost Viewer";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.MainToolStrip.ResumeLayout(false);
            this.MainToolStrip.PerformLayout();
            this.TabPanel.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.ViewHostPanel.ResumeLayout(false);
            this.ViewHostPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer RedrawTimer;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public System.Windows.Forms.TabControl TabPanel;
        public InstancePanel InstanceTab;
        private Panels.ViewPanel DisplayTab;
        private System.Windows.Forms.ToolStrip MainToolStrip;
        private System.Windows.Forms.TabPage tabPage5;
        private Panels.ConsolePanel ConsoleTab;
        private System.Windows.Forms.Timer RevalueTimer;
        private System.Windows.Forms.ToolStripButton BackButton;
        private System.Windows.Forms.ToolStripButton ForwardButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton OnOffButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripTextBox SearchTextBox;
        private System.Windows.Forms.Timer SearchTimer;
        public Panels.CodePanel CodeTab;
        private UI.Panels.NamespacePanel NamespaceTab;
        private System.Windows.Forms.ToolStripDropDownButton SearchToolButton;
        private System.Windows.Forms.ToolStripMenuItem ClearSearchMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SubsSearchMenuItem;
        private System.Windows.Forms.Panel ViewHostPanel;
    }
}