namespace XLibrary
{
    partial class GLPanel
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
            this.GLView = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // GLView
            // 
            this.GLView.BackColor = System.Drawing.Color.Black;
            this.GLView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GLView.Location = new System.Drawing.Point(0, 0);
            this.GLView.Name = "GLView";
            this.GLView.Size = new System.Drawing.Size(150, 150);
            this.GLView.TabIndex = 0;
            this.GLView.VSync = false;
            this.GLView.Load += new System.EventHandler(this.GLView_Load);
            this.GLView.Paint += new System.Windows.Forms.PaintEventHandler(this.GLView_Paint);
            this.GLView.Resize += new System.EventHandler(this.GLView_Resize);
            // 
            // GLPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GLView);
            this.Name = "GLPanel";
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl GLView;
    }
}
