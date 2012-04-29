namespace XTestApp
{
    partial class Form1
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
            this.throwButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.TestTimer = new System.Windows.Forms.Timer(this.components);
            this.StartTimerButton = new System.Windows.Forms.Button();
            this.ProfileTestButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // throwButton
            // 
            this.throwButton.Location = new System.Drawing.Point(111, 47);
            this.throwButton.Name = "throwButton";
            this.throwButton.Size = new System.Drawing.Size(75, 23);
            this.throwButton.TabIndex = 0;
            this.throwButton.Text = "throw";
            this.throwButton.UseVisualStyleBackColor = true;
            this.throwButton.Click += new System.EventHandler(this.throwButton_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(111, 76);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "gc collect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TestTimer
            // 
            this.TestTimer.Interval = 1000;
            this.TestTimer.Tick += new System.EventHandler(this.TestTimer_Tick);
            // 
            // StartTimerButton
            // 
            this.StartTimerButton.Location = new System.Drawing.Point(111, 206);
            this.StartTimerButton.Name = "StartTimerButton";
            this.StartTimerButton.Size = new System.Drawing.Size(75, 23);
            this.StartTimerButton.TabIndex = 2;
            this.StartTimerButton.Text = "start timer";
            this.StartTimerButton.UseVisualStyleBackColor = true;
            this.StartTimerButton.Click += new System.EventHandler(this.StartTimerButton_Click);
            // 
            // ProfileTestButton
            // 
            this.ProfileTestButton.Location = new System.Drawing.Point(111, 177);
            this.ProfileTestButton.Name = "ProfileTestButton";
            this.ProfileTestButton.Size = new System.Drawing.Size(75, 23);
            this.ProfileTestButton.TabIndex = 3;
            this.ProfileTestButton.Text = "profile test";
            this.ProfileTestButton.UseVisualStyleBackColor = true;
            this.ProfileTestButton.Click += new System.EventHandler(this.ProfileTestButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 266);
            this.Controls.Add(this.ProfileTestButton);
            this.Controls.Add(this.StartTimerButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.throwButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button throwButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer TestTimer;
        private System.Windows.Forms.Button StartTimerButton;
        private System.Windows.Forms.Button ProfileTestButton;
    }
}

