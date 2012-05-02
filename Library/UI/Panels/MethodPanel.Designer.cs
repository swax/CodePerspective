namespace XLibrary.Panels
{
    partial class CodePanel
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
            this.MsilView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MethodNameLabel = new System.Windows.Forms.Label();
            this.CSharpRadioButton = new System.Windows.Forms.RadioButton();
            this.MsilRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ProfileRadioButton = new System.Windows.Forms.RadioButton();
            this.ProfileView = new XLibrary.ProfilePanel();
            this.CSharpView = new DeOps.Interface.Views.WebBrowserEx();
            this.NavButtons = new XLibrary.UI.DetailsNav();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MsilView
            // 
            this.MsilView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.MsilView.FullRowSelect = true;
            this.MsilView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.MsilView.HideSelection = false;
            this.MsilView.Location = new System.Drawing.Point(3, 3);
            this.MsilView.MultiSelect = false;
            this.MsilView.Name = "MsilView";
            this.MsilView.Size = new System.Drawing.Size(240, 281);
            this.MsilView.TabIndex = 15;
            this.MsilView.UseCompatibleStateImageBehavior = false;
            this.MsilView.View = System.Windows.Forms.View.Details;
            this.MsilView.Visible = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Offset";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "OpCode";
            this.columnHeader2.Width = 82;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Operand";
            this.columnHeader3.Width = 174;
            // 
            // MethodNameLabel
            // 
            this.MethodNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MethodNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MethodNameLabel.Location = new System.Drawing.Point(65, 8);
            this.MethodNameLabel.Name = "MethodNameLabel";
            this.MethodNameLabel.Size = new System.Drawing.Size(662, 13);
            this.MethodNameLabel.TabIndex = 17;
            this.MethodNameLabel.Text = "Method";
            // 
            // CSharpRadioButton
            // 
            this.CSharpRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CSharpRadioButton.AutoSize = true;
            this.CSharpRadioButton.Checked = true;
            this.CSharpRadioButton.Location = new System.Drawing.Point(733, 6);
            this.CSharpRadioButton.Name = "CSharpRadioButton";
            this.CSharpRadioButton.Size = new System.Drawing.Size(39, 17);
            this.CSharpRadioButton.TabIndex = 18;
            this.CSharpRadioButton.TabStop = true;
            this.CSharpRadioButton.Text = "C#";
            this.CSharpRadioButton.UseVisualStyleBackColor = true;
            this.CSharpRadioButton.CheckedChanged += new System.EventHandler(this.CSharpRadioButton_CheckedChanged);
            // 
            // MsilRadioButton
            // 
            this.MsilRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MsilRadioButton.AutoSize = true;
            this.MsilRadioButton.Location = new System.Drawing.Point(778, 6);
            this.MsilRadioButton.Name = "MsilRadioButton";
            this.MsilRadioButton.Size = new System.Drawing.Size(50, 17);
            this.MsilRadioButton.TabIndex = 19;
            this.MsilRadioButton.Text = "MSIL";
            this.MsilRadioButton.UseVisualStyleBackColor = true;
            this.MsilRadioButton.CheckedChanged += new System.EventHandler(this.MsilRadioButton_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.ProfileView);
            this.panel1.Controls.Add(this.CSharpView);
            this.panel1.Controls.Add(this.MsilView);
            this.panel1.Location = new System.Drawing.Point(3, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(888, 295);
            this.panel1.TabIndex = 20;
            // 
            // ProfileRadioButton
            // 
            this.ProfileRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProfileRadioButton.AutoSize = true;
            this.ProfileRadioButton.Location = new System.Drawing.Point(834, 6);
            this.ProfileRadioButton.Name = "ProfileRadioButton";
            this.ProfileRadioButton.Size = new System.Drawing.Size(54, 17);
            this.ProfileRadioButton.TabIndex = 21;
            this.ProfileRadioButton.Text = "Profile";
            this.ProfileRadioButton.UseVisualStyleBackColor = true;
            this.ProfileRadioButton.CheckedChanged += new System.EventHandler(this.ProfileRadioButton_CheckedChanged);
            // 
            // ProfileView
            // 
            this.ProfileView.Location = new System.Drawing.Point(515, 6);
            this.ProfileView.Name = "ProfileView";
            this.ProfileView.Size = new System.Drawing.Size(370, 286);
            this.ProfileView.TabIndex = 17;
            this.ProfileView.Visible = false;
            // 
            // CSharpView
            // 
            this.CSharpView.Location = new System.Drawing.Point(274, 6);
            this.CSharpView.MinimumSize = new System.Drawing.Size(20, 20);
            this.CSharpView.Name = "CSharpView";
            this.CSharpView.Size = new System.Drawing.Size(217, 278);
            this.CSharpView.TabIndex = 16;
            this.CSharpView.Visible = false;
            this.CSharpView.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.CSharpView_Navigating);
            // 
            // NavButtons
            // 
            this.NavButtons.Location = new System.Drawing.Point(3, 8);
            this.NavButtons.Name = "NavButtons";
            this.NavButtons.Size = new System.Drawing.Size(56, 16);
            this.NavButtons.TabIndex = 32;
            // 
            // CodePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.NavButtons);
            this.Controls.Add(this.ProfileRadioButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MsilRadioButton);
            this.Controls.Add(this.CSharpRadioButton);
            this.Controls.Add(this.MethodNameLabel);
            this.Name = "CodePanel";
            this.Size = new System.Drawing.Size(894, 322);
            this.VisibleChanged += new System.EventHandler(this.CodePanel_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView MsilView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label MethodNameLabel;
        private System.Windows.Forms.RadioButton CSharpRadioButton;
        private System.Windows.Forms.RadioButton MsilRadioButton;
        private System.Windows.Forms.Panel panel1;
        private DeOps.Interface.Views.WebBrowserEx CSharpView;
        private ProfilePanel ProfileView;
        private System.Windows.Forms.RadioButton ProfileRadioButton;
        private UI.DetailsNav NavButtons;
    }
}
