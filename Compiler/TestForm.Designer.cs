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
            this.LayoutButton = new System.Windows.Forms.Button();
            this.UncrossButton = new System.Windows.Forms.Button();
            this.MinDistButton = new System.Windows.Forms.Button();
            this.AutoButton = new System.Windows.Forms.Button();
            this.testPanel1 = new XBuilder.TestPanel();
            this.WeightBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
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
            this.SetButton.Text = "Reset";
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
            // LayoutButton
            // 
            this.LayoutButton.Location = new System.Drawing.Point(263, 42);
            this.LayoutButton.Name = "LayoutButton";
            this.LayoutButton.Size = new System.Drawing.Size(75, 23);
            this.LayoutButton.TabIndex = 8;
            this.LayoutButton.Text = "Layout";
            this.LayoutButton.UseVisualStyleBackColor = true;
            this.LayoutButton.Click += new System.EventHandler(this.LayoutButton_Click);
            // 
            // UncrossButton
            // 
            this.UncrossButton.Location = new System.Drawing.Point(344, 41);
            this.UncrossButton.Name = "UncrossButton";
            this.UncrossButton.Size = new System.Drawing.Size(75, 23);
            this.UncrossButton.TabIndex = 9;
            this.UncrossButton.Text = "Uncross";
            this.UncrossButton.UseVisualStyleBackColor = true;
            this.UncrossButton.Click += new System.EventHandler(this.UncrossButton_Click);
            // 
            // MinDistButton
            // 
            this.MinDistButton.Location = new System.Drawing.Point(425, 41);
            this.MinDistButton.Name = "MinDistButton";
            this.MinDistButton.Size = new System.Drawing.Size(75, 23);
            this.MinDistButton.TabIndex = 10;
            this.MinDistButton.Text = "Min Dist";
            this.MinDistButton.UseVisualStyleBackColor = true;
            this.MinDistButton.Click += new System.EventHandler(this.MinDistButton_Click);
            // 
            // AutoButton
            // 
            this.AutoButton.Location = new System.Drawing.Point(344, 13);
            this.AutoButton.Name = "AutoButton";
            this.AutoButton.Size = new System.Drawing.Size(75, 23);
            this.AutoButton.TabIndex = 11;
            this.AutoButton.Text = "Auto";
            this.AutoButton.UseVisualStyleBackColor = true;
            this.AutoButton.Click += new System.EventHandler(this.AutoButton_Click);
            // 
            // testPanel1
            // 
            this.testPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.testPanel1.Location = new System.Drawing.Point(12, 86);
            this.testPanel1.Name = "testPanel1";
            this.testPanel1.Size = new System.Drawing.Size(775, 375);
            this.testPanel1.TabIndex = 7;
            // 
            // WeightBox
            // 
            this.WeightBox.Location = new System.Drawing.Point(176, 43);
            this.WeightBox.Name = "WeightBox";
            this.WeightBox.Size = new System.Drawing.Size(68, 20);
            this.WeightBox.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(106, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Weight Max";
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 473);
            this.Controls.Add(this.WeightBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AutoButton);
            this.Controls.Add(this.MinDistButton);
            this.Controls.Add(this.UncrossButton);
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
        private System.Windows.Forms.Button UncrossButton;
        private System.Windows.Forms.Button MinDistButton;
        private System.Windows.Forms.Button AutoButton;
        private System.Windows.Forms.TextBox WeightBox;
        private System.Windows.Forms.Label label3;

    }
}