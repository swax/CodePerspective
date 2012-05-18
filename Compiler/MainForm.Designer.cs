namespace XBuilder
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buildPanel1 = new XBuilder.BuildPanel();
            this.monitorPanel1 = new XBuilder.MonitorPanel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.scannerPanel1 = new XBuilder.ScannerPanel();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(385, 393);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buildPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(377, 367);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Build";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.monitorPanel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(377, 367);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Monitor";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buildPanel1
            // 
            this.buildPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buildPanel1.Location = new System.Drawing.Point(3, 3);
            this.buildPanel1.Name = "buildPanel1";
            this.buildPanel1.Size = new System.Drawing.Size(371, 361);
            this.buildPanel1.TabIndex = 0;
            // 
            // monitorPanel1
            // 
            this.monitorPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.monitorPanel1.Location = new System.Drawing.Point(3, 3);
            this.monitorPanel1.Name = "monitorPanel1";
            this.monitorPanel1.Size = new System.Drawing.Size(371, 361);
            this.monitorPanel1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.scannerPanel1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(377, 367);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Scanner";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // scannerPanel1
            // 
            this.scannerPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scannerPanel1.Location = new System.Drawing.Point(3, 3);
            this.scannerPanel1.Name = "scannerPanel1";
            this.scannerPanel1.Size = new System.Drawing.Size(371, 361);
            this.scannerPanel1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(385, 393);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "Ghost in the Code";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private BuildPanel buildPanel1;
        private MonitorPanel monitorPanel1;
        private System.Windows.Forms.TabPage tabPage3;
        private ScannerPanel scannerPanel1;


    }
}

