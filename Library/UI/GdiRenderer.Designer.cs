namespace XLibrary
{
    partial class GdiRenderer
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
            this.SuspendLayout();
            // 
            // TreePanelGdiPlus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "TreePanelGdiPlus";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GdiRenderer_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GdiRenderer_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GdiRenderer_KeyUp);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.GdiRenderer_MouseDoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GdiRenderer_MouseDown);
            this.MouseLeave += new System.EventHandler(this.GdiRenderer_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GdiRenderer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GdiRenderer_MouseUp);
            this.Resize += new System.EventHandler(this.GdiRenderer_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
