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
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyHitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewPanel = new System.Windows.Forms.Panel();
            this.BottomStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomStrip
            // 
            this.BottomStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectedLabel});
            this.BottomStrip.Location = new System.Drawing.Point(0, 249);
            this.BottomStrip.Name = "BottomStrip";
            this.BottomStrip.Size = new System.Drawing.Size(292, 22);
            this.BottomStrip.TabIndex = 1;
            this.BottomStrip.Text = "statusStrip1";
            // 
            // SelectedLabel
            // 
            this.SelectedLabel.Name = "SelectedLabel";
            this.SelectedLabel.Size = new System.Drawing.Size(89, 17);
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
            this.optionsToolStripMenuItem,
            this.ViewMenuItem,
            this.DebugMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(292, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showOnlyHitToolStripMenuItem,
            this.ResetMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // showOnlyHitToolStripMenuItem
            // 
            this.showOnlyHitToolStripMenuItem.CheckOnClick = true;
            this.showOnlyHitToolStripMenuItem.Name = "showOnlyHitToolStripMenuItem";
            this.showOnlyHitToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.showOnlyHitToolStripMenuItem.Text = "Show only hit";
            this.showOnlyHitToolStripMenuItem.Click += new System.EventHandler(this.ShowOnlyHitToolStripMenuItem_Click);
            // 
            // ResetMenuItem
            // 
            this.ResetMenuItem.Name = "ResetMenuItem";
            this.ResetMenuItem.Size = new System.Drawing.Size(146, 22);
            this.ResetMenuItem.Text = "Reset hit";
            this.ResetMenuItem.Click += new System.EventHandler(this.ResetMenuItem_Click);
            // 
            // ViewMenuItem
            // 
            this.ViewMenuItem.Name = "ViewMenuItem";
            this.ViewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.ViewMenuItem.Text = "View";
            this.ViewMenuItem.DropDownOpening += new System.EventHandler(this.ViewMenuItem_DropDownOpening);
            // 
            // DebugMenuItem
            // 
            this.DebugMenuItem.Name = "DebugMenuItem";
            this.DebugMenuItem.Size = new System.Drawing.Size(54, 20);
            this.DebugMenuItem.Text = "Debug";
            this.DebugMenuItem.Click += new System.EventHandler(this.DebugMenuItem_Click);
            // 
            // ViewPanel
            // 
            this.ViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewPanel.Location = new System.Drawing.Point(0, 27);
            this.ViewPanel.Name = "ViewPanel";
            this.ViewPanel.Size = new System.Drawing.Size(292, 219);
            this.ViewPanel.TabIndex = 3;
            // 
            // TreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.Controls.Add(this.ViewPanel);
            this.Controls.Add(this.BottomStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TreeForm";
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TreeForm_FormClosing);
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
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyHitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ResetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuItem;
        private System.Windows.Forms.Panel ViewPanel;
        private System.Windows.Forms.ToolStripMenuItem DebugMenuItem;
    }
}