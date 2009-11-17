namespace XBuilder
{
    partial class TestForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.NodesBox = new System.Windows.Forms.TextBox();
            this.SetButton = new System.Windows.Forms.Button();
            this.EdgesBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.testPanel1 = new XBuilder.TestPanel();
            this.LayoutButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Nodes";
            // 
            // NodesBox
            // 
            this.NodesBox.Location = new System.Drawing.Point(59, 15);
            this.NodesBox.Name = "NodesBox";
            this.NodesBox.Size = new System.Drawing.Size(68, 20);
            this.NodesBox.TabIndex = 3;
            // 
            // SetButton
            // 
            this.SetButton.Location = new System.Drawing.Point(263, 13);
            this.SetButton.Name = "SetButton";
            this.SetButton.Size = new System.Drawing.Size(75, 23);
            this.SetButton.TabIndex = 4;
            this.SetButton.Text = "Set";
            this.SetButton.UseVisualStyleBackColor = true;
            this.SetButton.Click += new System.EventHandler(this.SetButton_Click);
            // 
            // EdgesBox
            // 
            this.EdgesBox.Location = new System.Drawing.Point(176, 15);
            this.EdgesBox.Name = "EdgesBox";
            this.EdgesBox.Size = new System.Drawing.Size(68, 20);
            this.EdgesBox.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(133, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Edges";
            // 
            // testPanel1
            // 
            this.testPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.testPanel1.Location = new System.Drawing.Point(12, 41);
            this.testPanel1.Name = "testPanel1";
            this.testPanel1.Size = new System.Drawing.Size(442, 296);
            this.testPanel1.TabIndex = 7;
            // 
            // LayoutButton
            // 
            this.LayoutButton.Location = new System.Drawing.Point(344, 13);
            this.LayoutButton.Name = "LayoutButton";
            this.LayoutButton.Size = new System.Drawing.Size(75, 23);
            this.LayoutButton.TabIndex = 8;
            this.LayoutButton.Text = "Layout";
            this.LayoutButton.UseVisualStyleBackColor = true;
            this.LayoutButton.Click += new System.EventHandler(this.LayoutButton_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 349);
            this.Controls.Add(this.LayoutButton);
            this.Controls.Add(this.testPanel1);
            this.Controls.Add(this.EdgesBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SetButton);
            this.Controls.Add(this.NodesBox);
            this.Controls.Add(this.label1);
            this.Name = "TestForm";
            this.Text = "Test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox NodesBox;
        private System.Windows.Forms.Button SetButton;
        private System.Windows.Forms.TextBox EdgesBox;
        private System.Windows.Forms.Label label2;
        private TestPanel testPanel1;
        private System.Windows.Forms.Button LayoutButton;

    }
}