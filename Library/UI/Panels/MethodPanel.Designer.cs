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
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.CSharpRadioButton = new System.Windows.Forms.RadioButton();
            this.MsilRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ProfileView = new XLibrary.ProfilePanel();
            this.CSharpView = new DeOps.Interface.Views.WebBrowserEx();
            this.ProfileRadioButton = new System.Windows.Forms.RadioButton();
            this.NavButtons = new XLibrary.UI.DetailsNav();
            this.DetailsLabel = new System.Windows.Forms.Label();
            this.SummaryLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
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
            this.MsilView.Location = new System.Drawing.Point(4, 5);
            this.MsilView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MsilView.MultiSelect = false;
            this.MsilView.Name = "MsilView";
            this.MsilView.Size = new System.Drawing.Size(319, 430);
            this.MsilView.TabIndex = 15;
            this.MsilView.UseCompatibleStateImageBehavior = false;
            this.MsilView.View = System.Windows.Forms.View.Details;
            this.MsilView.Visible = false;
            this.MsilView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MsilView_MouseDoubleClick);
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
            // CSharpRadioButton
            // 
            this.CSharpRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CSharpRadioButton.AutoSize = true;
            this.CSharpRadioButton.Checked = true;
            this.CSharpRadioButton.Location = new System.Drawing.Point(984, 9);
            this.CSharpRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CSharpRadioButton.Name = "CSharpRadioButton";
            this.CSharpRadioButton.Size = new System.Drawing.Size(48, 24);
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
            this.MsilRadioButton.Location = new System.Drawing.Point(1045, 9);
            this.MsilRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MsilRadioButton.Name = "MsilRadioButton";
            this.MsilRadioButton.Size = new System.Drawing.Size(62, 24);
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
            this.panel1.Location = new System.Drawing.Point(4, 42);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1187, 454);
            this.panel1.TabIndex = 20;
            // 
            // ProfileView
            // 
            this.ProfileView.Location = new System.Drawing.Point(687, 9);
            this.ProfileView.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.ProfileView.Name = "ProfileView";
            this.ProfileView.Size = new System.Drawing.Size(493, 440);
            this.ProfileView.TabIndex = 17;
            this.ProfileView.Visible = false;
            // 
            // CSharpView
            // 
            this.CSharpView.Location = new System.Drawing.Point(365, 9);
            this.CSharpView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CSharpView.MinimumSize = new System.Drawing.Size(27, 31);
            this.CSharpView.Name = "CSharpView";
            this.CSharpView.Size = new System.Drawing.Size(289, 428);
            this.CSharpView.TabIndex = 16;
            this.CSharpView.Visible = false;
            this.CSharpView.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.CSharpView_Navigating);
            // 
            // ProfileRadioButton
            // 
            this.ProfileRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProfileRadioButton.AutoSize = true;
            this.ProfileRadioButton.Location = new System.Drawing.Point(1114, 9);
            this.ProfileRadioButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ProfileRadioButton.Name = "ProfileRadioButton";
            this.ProfileRadioButton.Size = new System.Drawing.Size(73, 24);
            this.ProfileRadioButton.TabIndex = 21;
            this.ProfileRadioButton.Text = "Profile";
            this.ProfileRadioButton.UseVisualStyleBackColor = true;
            this.ProfileRadioButton.CheckedChanged += new System.EventHandler(this.ProfileRadioButton_CheckedChanged);
            // 
            // NavButtons
            // 
            this.NavButtons.Location = new System.Drawing.Point(4, 12);
            this.NavButtons.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.NavButtons.Name = "NavButtons";
            this.NavButtons.Size = new System.Drawing.Size(75, 25);
            this.NavButtons.TabIndex = 32;
            // 
            // DetailsLabel
            // 
            this.DetailsLabel.AutoSize = true;
            this.DetailsLabel.Location = new System.Drawing.Point(73, 0);
            this.DetailsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DetailsLabel.Name = "DetailsLabel";
            this.DetailsLabel.Size = new System.Drawing.Size(53, 20);
            this.DetailsLabel.TabIndex = 1;
            this.DetailsLabel.Text = "details";
            // 
            // SummaryLabel
            // 
            this.SummaryLabel.AutoSize = true;
            this.SummaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.SummaryLabel.Location = new System.Drawing.Point(4, 0);
            this.SummaryLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SummaryLabel.Name = "SummaryLabel";
            this.SummaryLabel.Size = new System.Drawing.Size(61, 17);
            this.SummaryLabel.TabIndex = 0;
            this.SummaryLabel.Text = "Method";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.SummaryLabel);
            this.flowLayoutPanel1.Controls.Add(this.DetailsLabel);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(87, 12);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(885, 23);
            this.flowLayoutPanel1.TabIndex = 33;
            // 
            // CodePanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.ProfileRadioButton);
            this.Controls.Add(this.MsilRadioButton);
            this.Controls.Add(this.CSharpRadioButton);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.NavButtons);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "CodePanel";
            this.Size = new System.Drawing.Size(1195, 495);
            this.Load += new System.EventHandler(this.CodePanel_Load);
            this.VisibleChanged += new System.EventHandler(this.CodePanel_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView MsilView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.RadioButton CSharpRadioButton;
        private System.Windows.Forms.RadioButton MsilRadioButton;
        private System.Windows.Forms.Panel panel1;
        private DeOps.Interface.Views.WebBrowserEx CSharpView;
        private ProfilePanel ProfileView;
        private System.Windows.Forms.RadioButton ProfileRadioButton;
        private UI.DetailsNav NavButtons;
        private System.Windows.Forms.Label DetailsLabel;
        private System.Windows.Forms.Label SummaryLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
