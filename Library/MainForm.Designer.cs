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
            this.BottomStrip = new System.Windows.Forms.StatusStrip();
            this.SelectedLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ResetTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.layoutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.TreeMapMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CallGraphMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SizesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConstantMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MethodSizeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TimeInMethodMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SizeHitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TimePerHitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZoomMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewOutsideMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewExternalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowAllHitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowHitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowUnhitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.callsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowRTCallsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowAllCallsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewHostPanel = new System.Windows.Forms.Panel();
            this.BottomStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomStrip
            // 
            this.BottomStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectedLabel});
            this.BottomStrip.Location = new System.Drawing.Point(0, 287);
            this.BottomStrip.Name = "BottomStrip";
            this.BottomStrip.Size = new System.Drawing.Size(423, 22);
            this.BottomStrip.TabIndex = 1;
            this.BottomStrip.Text = "statusStrip1";
            // 
            // SelectedLabel
            // 
            this.SelectedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectedLabel.Name = "SelectedLabel";
            this.SelectedLabel.Size = new System.Drawing.Size(108, 17);
            this.SelectedLabel.Text = "Selected Object";
            // 
            // ResetTimer
            // 
            this.ResetTimer.Interval = 30;
            this.ResetTimer.Tick += new System.EventHandler(this.ResetTimer_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.layoutToolStripMenuItem1,
            this.SizesMenuItem,
            this.ZoomMenuItem,
            this.ViewMenuItem,
            this.HitsMenuItem,
            this.callsToolStripMenuItem,
            this.DebugMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(423, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // layoutToolStripMenuItem1
            // 
            this.layoutToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TreeMapMenuItem,
            this.CallGraphMenuItem});
            this.layoutToolStripMenuItem1.Name = "layoutToolStripMenuItem1";
            this.layoutToolStripMenuItem1.Size = new System.Drawing.Size(55, 20);
            this.layoutToolStripMenuItem1.Text = "Layout";
            this.layoutToolStripMenuItem1.DropDownOpening += new System.EventHandler(this.LayoutMenu_DropDownOpening);
            // 
            // TreeMapMenuItem
            // 
            this.TreeMapMenuItem.Name = "TreeMapMenuItem";
            this.TreeMapMenuItem.Size = new System.Drawing.Size(152, 22);
            this.TreeMapMenuItem.Text = "TreeMap";
            this.TreeMapMenuItem.Click += new System.EventHandler(this.TreeMapMenuItem_Click);
            // 
            // CallGraphMenuItem
            // 
            this.CallGraphMenuItem.Name = "CallGraphMenuItem";
            this.CallGraphMenuItem.Size = new System.Drawing.Size(152, 22);
            this.CallGraphMenuItem.Text = "Call Graph";
            this.CallGraphMenuItem.Click += new System.EventHandler(this.CallGraphMenuItem_Click);
            // 
            // SizesMenuItem
            // 
            this.SizesMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConstantMenuItem,
            this.MethodSizeMenuItem,
            this.TimeInMethodMenuItem,
            this.SizeHitsMenuItem,
            this.TimePerHitMenuItem});
            this.SizesMenuItem.Name = "SizesMenuItem";
            this.SizesMenuItem.Size = new System.Drawing.Size(44, 20);
            this.SizesMenuItem.Text = "Sizes";
            this.SizesMenuItem.Click += new System.EventHandler(this.SizesMenu_DropDownOpening);
            // 
            // ConstantMenuItem
            // 
            this.ConstantMenuItem.Name = "ConstantMenuItem";
            this.ConstantMenuItem.Size = new System.Drawing.Size(159, 22);
            this.ConstantMenuItem.Text = "Constant";
            this.ConstantMenuItem.Click += new System.EventHandler(this.ConstantMenuItem_Click);
            // 
            // MethodSizeMenuItem
            // 
            this.MethodSizeMenuItem.Name = "MethodSizeMenuItem";
            this.MethodSizeMenuItem.Size = new System.Drawing.Size(159, 22);
            this.MethodSizeMenuItem.Text = "Method Size";
            this.MethodSizeMenuItem.Click += new System.EventHandler(this.MethodSizeMenuItem_Click);
            // 
            // TimeInMethodMenuItem
            // 
            this.TimeInMethodMenuItem.Name = "TimeInMethodMenuItem";
            this.TimeInMethodMenuItem.Size = new System.Drawing.Size(159, 22);
            this.TimeInMethodMenuItem.Text = "Time in Method";
            this.TimeInMethodMenuItem.Click += new System.EventHandler(this.TimeInMethodMenuItem_Click);
            // 
            // SizeHitsMenuItem
            // 
            this.SizeHitsMenuItem.Name = "SizeHitsMenuItem";
            this.SizeHitsMenuItem.Size = new System.Drawing.Size(159, 22);
            this.SizeHitsMenuItem.Text = "Hits";
            this.SizeHitsMenuItem.Click += new System.EventHandler(this.SizeHitsMenuItem_Click);
            // 
            // TimePerHitMenuItem
            // 
            this.TimePerHitMenuItem.Name = "TimePerHitMenuItem";
            this.TimePerHitMenuItem.Size = new System.Drawing.Size(159, 22);
            this.TimePerHitMenuItem.Text = "Time per Hit";
            this.TimePerHitMenuItem.Click += new System.EventHandler(this.TimePerHitMenuItem_Click);
            // 
            // ZoomMenuItem
            // 
            this.ZoomMenuItem.Name = "ZoomMenuItem";
            this.ZoomMenuItem.Size = new System.Drawing.Size(51, 20);
            this.ZoomMenuItem.Text = "Zoom";
            this.ZoomMenuItem.DropDownOpening += new System.EventHandler(this.ZoomMenuItem_DropDownOpening);
            // 
            // ViewMenuItem
            // 
            this.ViewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewOutsideMenuItem,
            this.ViewExternalMenuItem});
            this.ViewMenuItem.Name = "ViewMenuItem";
            this.ViewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.ViewMenuItem.Text = "View";
            // 
            // ViewOutsideMenuItem
            // 
            this.ViewOutsideMenuItem.CheckOnClick = true;
            this.ViewOutsideMenuItem.Name = "ViewOutsideMenuItem";
            this.ViewOutsideMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ViewOutsideMenuItem.Text = "Outside zoom";
            this.ViewOutsideMenuItem.Click += new System.EventHandler(this.ViewOutsideMenuItem_Click);
            // 
            // ViewExternalMenuItem
            // 
            this.ViewExternalMenuItem.CheckOnClick = true;
            this.ViewExternalMenuItem.Name = "ViewExternalMenuItem";
            this.ViewExternalMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ViewExternalMenuItem.Text = "Not XRayed";
            this.ViewExternalMenuItem.Click += new System.EventHandler(this.ViewExternalMenuItem_Click);
            // 
            // HitsMenuItem
            // 
            this.HitsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowAllHitsMenuItem,
            this.ShowHitMenuItem,
            this.ShowUnhitMenuItem,
            this.ResetMenuItem});
            this.HitsMenuItem.Name = "HitsMenuItem";
            this.HitsMenuItem.Size = new System.Drawing.Size(40, 20);
            this.HitsMenuItem.Text = "Hits";
            this.HitsMenuItem.DropDownOpening += new System.EventHandler(this.HitsMenuItem_DropDownOpening);
            // 
            // ShowAllHitsMenuItem
            // 
            this.ShowAllHitsMenuItem.Name = "ShowAllHitsMenuItem";
            this.ShowAllHitsMenuItem.Size = new System.Drawing.Size(185, 22);
            this.ShowAllHitsMenuItem.Text = "Show all";
            this.ShowAllHitsMenuItem.Click += new System.EventHandler(this.ShowAllHitsMenuItem_Click);
            // 
            // ShowHitMenuItem
            // 
            this.ShowHitMenuItem.Name = "ShowHitMenuItem";
            this.ShowHitMenuItem.Size = new System.Drawing.Size(185, 22);
            this.ShowHitMenuItem.Text = "Show only hit";
            this.ShowHitMenuItem.Click += new System.EventHandler(this.ShowHitMenuItem_Click);
            // 
            // ShowUnhitMenuItem
            // 
            this.ShowUnhitMenuItem.Name = "ShowUnhitMenuItem";
            this.ShowUnhitMenuItem.Size = new System.Drawing.Size(185, 22);
            this.ShowUnhitMenuItem.Text = "Show only unhit";
            this.ShowUnhitMenuItem.Click += new System.EventHandler(this.ShowUnhitMenuItem_Click);
            // 
            // ResetMenuItem
            // 
            this.ResetMenuItem.Name = "ResetMenuItem";
            this.ResetMenuItem.Size = new System.Drawing.Size(185, 22);
            this.ResetMenuItem.Text = "Reset what\'s been hit";
            this.ResetMenuItem.Click += new System.EventHandler(this.ResetMenuItem_Click);
            // 
            // callsToolStripMenuItem
            // 
            this.callsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowRTCallsMenuItem,
            this.ShowAllCallsMenuItem});
            this.callsToolStripMenuItem.Name = "callsToolStripMenuItem";
            this.callsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.callsToolStripMenuItem.Text = "Calls";
            // 
            // ShowRTCallsMenuItem
            // 
            this.ShowRTCallsMenuItem.CheckOnClick = true;
            this.ShowRTCallsMenuItem.Name = "ShowRTCallsMenuItem";
            this.ShowRTCallsMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ShowRTCallsMenuItem.Text = "Show real time";
            this.ShowRTCallsMenuItem.Click += new System.EventHandler(this.ShowRTMenuItem_Click);
            // 
            // ShowAllCallsMenuItem
            // 
            this.ShowAllCallsMenuItem.CheckOnClick = true;
            this.ShowAllCallsMenuItem.Name = "ShowAllCallsMenuItem";
            this.ShowAllCallsMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ShowAllCallsMenuItem.Text = "Show all";
            this.ShowAllCallsMenuItem.Click += new System.EventHandler(this.ShowAllCallsMenuItem_Click);
            // 
            // DebugMenuItem
            // 
            this.DebugMenuItem.Name = "DebugMenuItem";
            this.DebugMenuItem.Size = new System.Drawing.Size(54, 20);
            this.DebugMenuItem.Text = "Debug";
            this.DebugMenuItem.Click += new System.EventHandler(this.DebugMenuItem_Click);
            // 
            // ViewHostPanel
            // 
            this.ViewHostPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewHostPanel.Location = new System.Drawing.Point(0, 27);
            this.ViewHostPanel.Name = "ViewHostPanel";
            this.ViewHostPanel.Size = new System.Drawing.Size(423, 260);
            this.ViewHostPanel.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 309);
            this.Controls.Add(this.ViewHostPanel);
            this.Controls.Add(this.BottomStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.BottomStrip.ResumeLayout(false);
            this.BottomStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ToolStripStatusLabel SelectedLabel;
        private System.Windows.Forms.StatusStrip BottomStrip;
        private System.Windows.Forms.Timer ResetTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem HitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowHitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ResetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DebugMenuItem;
        private System.Windows.Forms.ToolStripMenuItem callsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowRTCallsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowAllCallsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewOutsideMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewExternalMenuItem;
        private System.Windows.Forms.Panel ViewHostPanel;
        private System.Windows.Forms.ToolStripMenuItem ShowUnhitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowAllHitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem TreeMapMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CallGraphMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SizesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ConstantMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MethodSizeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TimeInMethodMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SizeHitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TimePerHitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ZoomMenuItem;
    }
}