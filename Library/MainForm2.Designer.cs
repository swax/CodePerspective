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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.BottomStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomStrip
            // 
            this.BottomStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectedLabel});
            this.BottomStrip.Location = new System.Drawing.Point(0, 381);
            this.BottomStrip.Name = "BottomStrip";
            this.BottomStrip.Size = new System.Drawing.Size(586, 22);
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
            this.menuStrip1.Size = new System.Drawing.Size(586, 24);
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
            this.ViewOutsideMenuItem.Size = new System.Drawing.Size(152, 22);
            this.ViewOutsideMenuItem.Text = "Outside zoom";
            this.ViewOutsideMenuItem.Click += new System.EventHandler(this.ViewOutsideMenuItem_Click);
            // 
            // ViewExternalMenuItem
            // 
            this.ViewExternalMenuItem.CheckOnClick = true;
            this.ViewExternalMenuItem.Name = "ViewExternalMenuItem";
            this.ViewExternalMenuItem.Size = new System.Drawing.Size(152, 22);
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
            this.callsToolStripMenuItem.Click += new System.EventHandler(this.callsToolStripMenuItem_Click);
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
            this.ViewHostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ViewHostPanel.Location = new System.Drawing.Point(0, 0);
            this.ViewHostPanel.Name = "ViewHostPanel";
            this.ViewHostPanel.Size = new System.Drawing.Size(418, 326);
            this.ViewHostPanel.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.Color.White;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 52);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.splitContainer1.Panel1.Controls.Add(this.comboBox1);
            this.splitContainer1.Panel1.Controls.Add(this.radioButton5);
            this.splitContainer1.Panel1.Controls.Add(this.radioButton3);
            this.splitContainer1.Panel1.Controls.Add(this.radioButton4);
            this.splitContainer1.Panel1.Controls.Add(this.radioButton2);
            this.splitContainer1.Panel1.Controls.Add(this.radioButton1);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox2);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ViewHostPanel);
            this.splitContainer1.Size = new System.Drawing.Size(586, 326);
            this.splitContainer1.SplitterDistance = 164;
            this.splitContainer1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "layout";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 235);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "size by";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "view";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(23, 82);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(88, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "outside zoom";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(23, 100);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(100, 17);
            this.checkBox2.TabIndex = 6;
            this.checkBox2.Text = "non-xrayed asm";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "hits";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(20, 198);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(118, 24);
            this.button1.TabIndex = 9;
            this.button1.Text = "reset whats been hit";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(23, 29);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(66, 17);
            this.radioButton1.TabIndex = 10;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "tree map";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(23, 46);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(71, 17);
            this.radioButton2.TabIndex = 11;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "call graph";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(20, 158);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(64, 17);
            this.radioButton3.TabIndex = 13;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "show hit";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(20, 141);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(63, 17);
            this.radioButton4.TabIndex = 12;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "show all";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(20, 175);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(82, 17);
            this.radioButton5.TabIndex = 14;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "show not hit";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Constant",
            "Method size",
            "Total time",
            "Hits",
            "Time per hit"});
            this.comboBox1.Location = new System.Drawing.Point(23, 251);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(115, 21);
            this.comboBox1.TabIndex = 15;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripSeparator2,
            this.toolStripButton2,
            this.toolStripButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(586, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(39, 22);
            this.toolStripLabel1.Text = "Zoom";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 403);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.BottomStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.BottomStrip.ResumeLayout(false);
            this.BottomStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
    }
}