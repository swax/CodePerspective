namespace XBuilder
{
    partial class GraphForm
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
            this.graphPanel1 = new XBuilder.GraphPanel();
            this.ResetButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ChargeBox = new System.Windows.Forms.TextBox();
            this.SetChargeLink = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // graphPanel1
            // 
            this.graphPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.graphPanel1.Location = new System.Drawing.Point(12, 113);
            this.graphPanel1.Name = "graphPanel1";
            this.graphPanel1.Size = new System.Drawing.Size(365, 186);
            this.graphPanel1.TabIndex = 0;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(12, 12);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 23);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Charge";
            // 
            // ChargeBox
            // 
            this.ChargeBox.Location = new System.Drawing.Point(140, 14);
            this.ChargeBox.Name = "ChargeBox";
            this.ChargeBox.Size = new System.Drawing.Size(68, 20);
            this.ChargeBox.TabIndex = 3;
            // 
            // SetChargeLink
            // 
            this.SetChargeLink.AutoSize = true;
            this.SetChargeLink.Location = new System.Drawing.Point(214, 17);
            this.SetChargeLink.Name = "SetChargeLink";
            this.SetChargeLink.Size = new System.Drawing.Size(23, 13);
            this.SetChargeLink.TabIndex = 4;
            this.SetChargeLink.TabStop = true;
            this.SetChargeLink.Text = "Set";
            this.SetChargeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetChargeLink_LinkClicked);
            // 
            // GraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 311);
            this.Controls.Add(this.SetChargeLink);
            this.Controls.Add(this.ChargeBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.graphPanel1);
            this.Name = "GraphForm";
            this.Text = "GraphForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GraphPanel graphPanel1;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ChargeBox;
        private System.Windows.Forms.LinkLabel SetChargeLink;
    }
}