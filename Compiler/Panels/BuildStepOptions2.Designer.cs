namespace XBuilder.Panels
{
    partial class BuildStepOptions2
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
            this.label1 = new System.Windows.Forms.Label();
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.EnableLocalViewer = new System.Windows.Forms.CheckBox();
            this.ShowOnStartCheckBox = new System.Windows.Forms.CheckBox();
            this.EnableIpcServer = new System.Windows.Forms.CheckBox();
            this.EnableRemoteViewer = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ListenPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.EncryptionKey = new System.Windows.Forms.TextBox();
            this.GenerateKey = new System.Windows.Forms.Button();
            this.GeneratePort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 20);
            this.label1.TabIndex = 24;
            this.label1.Text = "Viewer Options";
            // 
            // BackButton
            // 
            this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackButton.Location = new System.Drawing.Point(232, 300);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(75, 23);
            this.BackButton.TabIndex = 37;
            this.BackButton.Text = "< Back";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.Location = new System.Drawing.Point(313, 300);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 36;
            this.NextButton.Text = "Next >";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // EnableLocalViewer
            // 
            this.EnableLocalViewer.AutoSize = true;
            this.EnableLocalViewer.Location = new System.Drawing.Point(7, 53);
            this.EnableLocalViewer.Name = "EnableLocalViewer";
            this.EnableLocalViewer.Size = new System.Drawing.Size(328, 17);
            this.EnableLocalViewer.TabIndex = 38;
            this.EnableLocalViewer.Text = "Enable viewer (requires environment to support Windows.Forms)";
            this.EnableLocalViewer.UseVisualStyleBackColor = true;
            this.EnableLocalViewer.CheckedChanged += new System.EventHandler(this.EnableLocalViewer_CheckedChanged);
            // 
            // ShowOnStartCheckBox
            // 
            this.ShowOnStartCheckBox.AutoSize = true;
            this.ShowOnStartCheckBox.Location = new System.Drawing.Point(26, 76);
            this.ShowOnStartCheckBox.Name = "ShowOnStartCheckBox";
            this.ShowOnStartCheckBox.Size = new System.Drawing.Size(165, 17);
            this.ShowOnStartCheckBox.TabIndex = 39;
            this.ShowOnStartCheckBox.Text = "Show viewer when app starts";
            this.ShowOnStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // EnableIpcServer
            // 
            this.EnableIpcServer.AutoSize = true;
            this.EnableIpcServer.Location = new System.Drawing.Point(26, 99);
            this.EnableIpcServer.Name = "EnableIpcServer";
            this.EnableIpcServer.Size = new System.Drawing.Size(341, 17);
            this.EnableIpcServer.TabIndex = 40;
            this.EnableIpcServer.Text = "Embed IPC server (allows viewer to be started from the monitor tab)";
            this.EnableIpcServer.UseVisualStyleBackColor = true;
            // 
            // EnableRemoteViewer
            // 
            this.EnableRemoteViewer.AutoSize = true;
            this.EnableRemoteViewer.Location = new System.Drawing.Point(7, 143);
            this.EnableRemoteViewer.Name = "EnableRemoteViewer";
            this.EnableRemoteViewer.Size = new System.Drawing.Size(353, 17);
            this.EnableRemoteViewer.TabIndex = 41;
            this.EnableRemoteViewer.Text = "Embed TCP/IP server (allows you to view instances running remotely)";
            this.EnableRemoteViewer.UseVisualStyleBackColor = true;
            this.EnableRemoteViewer.CheckedChanged += new System.EventHandler(this.EnableRemoteViewer_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 198);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "Listen Port";
            // 
            // ListenPort
            // 
            this.ListenPort.Location = new System.Drawing.Point(107, 195);
            this.ListenPort.Name = "ListenPort";
            this.ListenPort.Size = new System.Drawing.Size(102, 20);
            this.ListenPort.TabIndex = 43;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 172);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 44;
            this.label3.Text = "Encryption Key";
            // 
            // EncryptionKey
            // 
            this.EncryptionKey.Location = new System.Drawing.Point(107, 169);
            this.EncryptionKey.Name = "EncryptionKey";
            this.EncryptionKey.Size = new System.Drawing.Size(102, 20);
            this.EncryptionKey.TabIndex = 45;
            // 
            // GenerateKey
            // 
            this.GenerateKey.Location = new System.Drawing.Point(215, 167);
            this.GenerateKey.Name = "GenerateKey";
            this.GenerateKey.Size = new System.Drawing.Size(75, 23);
            this.GenerateKey.TabIndex = 46;
            this.GenerateKey.Text = "Generate";
            this.GenerateKey.UseVisualStyleBackColor = true;
            this.GenerateKey.Click += new System.EventHandler(this.GenerateKey_Click);
            // 
            // GeneratePort
            // 
            this.GeneratePort.Location = new System.Drawing.Point(215, 193);
            this.GeneratePort.Name = "GeneratePort";
            this.GeneratePort.Size = new System.Drawing.Size(75, 23);
            this.GeneratePort.TabIndex = 47;
            this.GeneratePort.Text = "Random";
            this.GeneratePort.UseVisualStyleBackColor = true;
            this.GeneratePort.Click += new System.EventHandler(this.GeneratePort_Click);
            // 
            // BuildStepOptions2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GeneratePort);
            this.Controls.Add(this.GenerateKey);
            this.Controls.Add(this.EncryptionKey);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ListenPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.EnableRemoteViewer);
            this.Controls.Add(this.EnableIpcServer);
            this.Controls.Add(this.ShowOnStartCheckBox);
            this.Controls.Add(this.EnableLocalViewer);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.label1);
            this.Name = "BuildStepOptions2";
            this.Size = new System.Drawing.Size(391, 326);
            this.Load += new System.EventHandler(this.BuildStepOptions2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.CheckBox EnableLocalViewer;
        private System.Windows.Forms.CheckBox ShowOnStartCheckBox;
        private System.Windows.Forms.CheckBox EnableIpcServer;
        private System.Windows.Forms.CheckBox EnableRemoteViewer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ListenPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox EncryptionKey;
        private System.Windows.Forms.Button GenerateKey;
        private System.Windows.Forms.Button GeneratePort;
    }
}
