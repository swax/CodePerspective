namespace XLibrary
{
    partial class TreeForm
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
            this.AppTreePanel = new XLibrary.TreePanel();
            this.BottomStrip.SuspendLayout();
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
            this.SelectedLabel.Size = new System.Drawing.Size(83, 17);
            this.SelectedLabel.Text = "Selected Object";
            // 
            // ResetTimer
            // 
            this.ResetTimer.Interval = 500;
            this.ResetTimer.Tick += new System.EventHandler(this.ResetTimer_Tick);
            // 
            // AppTreePanel
            // 
            this.AppTreePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AppTreePanel.BackColor = System.Drawing.Color.White;
            this.AppTreePanel.Location = new System.Drawing.Point(0, 0);
            this.AppTreePanel.Name = "AppTreePanel";
            this.AppTreePanel.Size = new System.Drawing.Size(292, 246);
            this.AppTreePanel.TabIndex = 0;
            // 
            // TreeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.Controls.Add(this.BottomStrip);
            this.Controls.Add(this.AppTreePanel);
            this.Name = "TreeForm";
            this.Text = "MainForm";
            this.BottomStrip.ResumeLayout(false);
            this.BottomStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public TreePanel AppTreePanel;
        public System.Windows.Forms.ToolStripStatusLabel SelectedLabel;
        private System.Windows.Forms.StatusStrip BottomStrip;
        private System.Windows.Forms.Timer ResetTimer;
    }
}