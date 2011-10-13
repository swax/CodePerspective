namespace XLibrary.Panels
{
    partial class ViewPanel
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
            this.LayoutTreeMapButton = new System.Windows.Forms.RadioButton();
            this.LayoutCallGraphButton = new System.Windows.Forms.RadioButton();
            this.SizeLinesButton = new System.Windows.Forms.RadioButton();
            this.SizeConstantButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.SizeCallsButton = new System.Windows.Forms.RadioButton();
            this.SizeTimeInMethodButton = new System.Windows.Forms.RadioButton();
            this.SizeTimePerCallButton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.IncludeOutsideZoomButton = new System.Windows.Forms.CheckBox();
            this.IncludeNotXRayedButton = new System.Windows.Forms.CheckBox();
            this.ShowNotHitButton = new System.Windows.Forms.RadioButton();
            this.ShowHitButton = new System.Windows.Forms.RadioButton();
            this.ShowAllButton = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.ShowInstancesButton = new System.Windows.Forms.RadioButton();
            this.ResetHitButton = new System.Windows.Forms.Button();
            this.CallsRealTimeButton = new System.Windows.Forms.CheckBox();
            this.CallsAllButton = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.LayoutDependencyButton = new System.Windows.Forms.RadioButton();
            this.LayoutInOrder = new System.Windows.Forms.CheckBox();
            this.LayoutClassCallsButton = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.TrackingInstances = new System.Windows.Forms.CheckBox();
            this.TrackingMethodCalls = new System.Windows.Forms.CheckBox();
            this.TrackingClassCalls = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.IncludeFields = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Layout";
            // 
            // LayoutTreeMapButton
            // 
            this.LayoutTreeMapButton.AutoSize = true;
            this.LayoutTreeMapButton.Location = new System.Drawing.Point(3, 3);
            this.LayoutTreeMapButton.Name = "LayoutTreeMapButton";
            this.LayoutTreeMapButton.Size = new System.Drawing.Size(71, 17);
            this.LayoutTreeMapButton.TabIndex = 1;
            this.LayoutTreeMapButton.TabStop = true;
            this.LayoutTreeMapButton.Text = "Tree Map";
            this.LayoutTreeMapButton.UseVisualStyleBackColor = true;
            this.LayoutTreeMapButton.CheckedChanged += new System.EventHandler(this.LayoutTreeMapButton_CheckedChanged);
            // 
            // LayoutCallGraphButton
            // 
            this.LayoutCallGraphButton.AutoSize = true;
            this.LayoutCallGraphButton.Location = new System.Drawing.Point(3, 26);
            this.LayoutCallGraphButton.Name = "LayoutCallGraphButton";
            this.LayoutCallGraphButton.Size = new System.Drawing.Size(86, 17);
            this.LayoutCallGraphButton.TabIndex = 2;
            this.LayoutCallGraphButton.TabStop = true;
            this.LayoutCallGraphButton.Text = "Method Calls";
            this.LayoutCallGraphButton.UseVisualStyleBackColor = true;
            this.LayoutCallGraphButton.CheckedChanged += new System.EventHandler(this.LayoutCallGraphButton_CheckedChanged);
            // 
            // SizeLinesButton
            // 
            this.SizeLinesButton.AutoSize = true;
            this.SizeLinesButton.Location = new System.Drawing.Point(3, 26);
            this.SizeLinesButton.Name = "SizeLinesButton";
            this.SizeLinesButton.Size = new System.Drawing.Size(50, 17);
            this.SizeLinesButton.TabIndex = 5;
            this.SizeLinesButton.TabStop = true;
            this.SizeLinesButton.Text = "Lines";
            this.SizeLinesButton.UseVisualStyleBackColor = true;
            this.SizeLinesButton.CheckedChanged += new System.EventHandler(this.SizeLinesButton_CheckedChanged);
            // 
            // SizeConstantButton
            // 
            this.SizeConstantButton.AutoSize = true;
            this.SizeConstantButton.Location = new System.Drawing.Point(3, 3);
            this.SizeConstantButton.Name = "SizeConstantButton";
            this.SizeConstantButton.Size = new System.Drawing.Size(67, 17);
            this.SizeConstantButton.TabIndex = 4;
            this.SizeConstantButton.TabStop = true;
            this.SizeConstantButton.Text = "Constant";
            this.SizeConstantButton.UseVisualStyleBackColor = true;
            this.SizeConstantButton.CheckedChanged += new System.EventHandler(this.SizeConstantButton_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(388, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Size";
            // 
            // SizeCallsButton
            // 
            this.SizeCallsButton.AutoSize = true;
            this.SizeCallsButton.Location = new System.Drawing.Point(3, 72);
            this.SizeCallsButton.Name = "SizeCallsButton";
            this.SizeCallsButton.Size = new System.Drawing.Size(47, 17);
            this.SizeCallsButton.TabIndex = 7;
            this.SizeCallsButton.TabStop = true;
            this.SizeCallsButton.Text = "Calls";
            this.SizeCallsButton.UseVisualStyleBackColor = true;
            this.SizeCallsButton.CheckedChanged += new System.EventHandler(this.SizeCallsButton_CheckedChanged);
            // 
            // SizeTimeInMethodButton
            // 
            this.SizeTimeInMethodButton.AutoSize = true;
            this.SizeTimeInMethodButton.Location = new System.Drawing.Point(3, 49);
            this.SizeTimeInMethodButton.Name = "SizeTimeInMethodButton";
            this.SizeTimeInMethodButton.Size = new System.Drawing.Size(98, 17);
            this.SizeTimeInMethodButton.TabIndex = 6;
            this.SizeTimeInMethodButton.TabStop = true;
            this.SizeTimeInMethodButton.Text = "Time in Method";
            this.SizeTimeInMethodButton.UseVisualStyleBackColor = true;
            this.SizeTimeInMethodButton.CheckedChanged += new System.EventHandler(this.SizeTimeInMethodButton_CheckedChanged);
            // 
            // SizeTimePerCallButton
            // 
            this.SizeTimePerCallButton.AutoSize = true;
            this.SizeTimePerCallButton.Location = new System.Drawing.Point(3, 95);
            this.SizeTimePerCallButton.Name = "SizeTimePerCallButton";
            this.SizeTimePerCallButton.Size = new System.Drawing.Size(86, 17);
            this.SizeTimePerCallButton.TabIndex = 8;
            this.SizeTimePerCallButton.TabStop = true;
            this.SizeTimePerCallButton.Text = "Time per Call";
            this.SizeTimePerCallButton.UseVisualStyleBackColor = true;
            this.SizeTimePerCallButton.CheckedChanged += new System.EventHandler(this.SizeTimePerCallButton_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(281, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Include";
            // 
            // IncludeOutsideZoomButton
            // 
            this.IncludeOutsideZoomButton.AutoSize = true;
            this.IncludeOutsideZoomButton.Location = new System.Drawing.Point(3, 3);
            this.IncludeOutsideZoomButton.Name = "IncludeOutsideZoomButton";
            this.IncludeOutsideZoomButton.Size = new System.Drawing.Size(92, 17);
            this.IncludeOutsideZoomButton.TabIndex = 10;
            this.IncludeOutsideZoomButton.Text = "Outside Zoom";
            this.IncludeOutsideZoomButton.UseVisualStyleBackColor = true;
            this.IncludeOutsideZoomButton.CheckedChanged += new System.EventHandler(this.IncludeOutsideZoomButton_CheckedChanged);
            // 
            // IncludeNotXRayedButton
            // 
            this.IncludeNotXRayedButton.AutoSize = true;
            this.IncludeNotXRayedButton.Location = new System.Drawing.Point(3, 26);
            this.IncludeNotXRayedButton.Name = "IncludeNotXRayedButton";
            this.IncludeNotXRayedButton.Size = new System.Drawing.Size(84, 17);
            this.IncludeNotXRayedButton.TabIndex = 11;
            this.IncludeNotXRayedButton.Text = "Not XRayed";
            this.IncludeNotXRayedButton.UseVisualStyleBackColor = true;
            this.IncludeNotXRayedButton.CheckedChanged += new System.EventHandler(this.IncludeNotXRayedButton_CheckedChanged);
            // 
            // ShowNotHitButton
            // 
            this.ShowNotHitButton.AutoSize = true;
            this.ShowNotHitButton.Location = new System.Drawing.Point(3, 49);
            this.ShowNotHitButton.Name = "ShowNotHitButton";
            this.ShowNotHitButton.Size = new System.Drawing.Size(74, 17);
            this.ShowNotHitButton.TabIndex = 15;
            this.ShowNotHitButton.TabStop = true;
            this.ShowNotHitButton.Text = "Not Called";
            this.ShowNotHitButton.UseVisualStyleBackColor = true;
            this.ShowNotHitButton.CheckedChanged += new System.EventHandler(this.ShowNotHitButton_CheckedChanged);
            // 
            // ShowHitButton
            // 
            this.ShowHitButton.AutoSize = true;
            this.ShowHitButton.Location = new System.Drawing.Point(3, 26);
            this.ShowHitButton.Name = "ShowHitButton";
            this.ShowHitButton.Size = new System.Drawing.Size(54, 17);
            this.ShowHitButton.TabIndex = 14;
            this.ShowHitButton.TabStop = true;
            this.ShowHitButton.Text = "Called";
            this.ShowHitButton.UseVisualStyleBackColor = true;
            this.ShowHitButton.CheckedChanged += new System.EventHandler(this.ShowHitButton_CheckedChanged);
            // 
            // ShowAllButton
            // 
            this.ShowAllButton.AutoSize = true;
            this.ShowAllButton.Location = new System.Drawing.Point(3, 3);
            this.ShowAllButton.Name = "ShowAllButton";
            this.ShowAllButton.Size = new System.Drawing.Size(36, 17);
            this.ShowAllButton.TabIndex = 13;
            this.ShowAllButton.TabStop = true;
            this.ShowAllButton.Text = "All";
            this.ShowAllButton.UseVisualStyleBackColor = true;
            this.ShowAllButton.CheckedChanged += new System.EventHandler(this.ShowAllButton_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(103, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Methods";
            // 
            // ShowInstancesButton
            // 
            this.ShowInstancesButton.AutoSize = true;
            this.ShowInstancesButton.Location = new System.Drawing.Point(3, 72);
            this.ShowInstancesButton.Name = "ShowInstancesButton";
            this.ShowInstancesButton.Size = new System.Drawing.Size(45, 17);
            this.ShowInstancesButton.TabIndex = 16;
            this.ShowInstancesButton.TabStop = true;
            this.ShowInstancesButton.Text = "Initd";
            this.ShowInstancesButton.UseVisualStyleBackColor = true;
            this.ShowInstancesButton.CheckedChanged += new System.EventHandler(this.ShowInstancesButton_CheckedChanged);
            // 
            // ResetHitButton
            // 
            this.ResetHitButton.Location = new System.Drawing.Point(3, 95);
            this.ResetHitButton.Name = "ResetHitButton";
            this.ResetHitButton.Size = new System.Drawing.Size(75, 23);
            this.ResetHitButton.TabIndex = 17;
            this.ResetHitButton.Text = "Reset Called";
            this.ResetHitButton.UseVisualStyleBackColor = true;
            this.ResetHitButton.Click += new System.EventHandler(this.ResetHitButton_Click);
            // 
            // CallsRealTimeButton
            // 
            this.CallsRealTimeButton.AutoSize = true;
            this.CallsRealTimeButton.Location = new System.Drawing.Point(3, 26);
            this.CallsRealTimeButton.Name = "CallsRealTimeButton";
            this.CallsRealTimeButton.Size = new System.Drawing.Size(70, 17);
            this.CallsRealTimeButton.TabIndex = 20;
            this.CallsRealTimeButton.Text = "Real-time";
            this.CallsRealTimeButton.UseVisualStyleBackColor = true;
            this.CallsRealTimeButton.CheckedChanged += new System.EventHandler(this.CallsRealTimeButton_CheckedChanged);
            // 
            // CallsAllButton
            // 
            this.CallsAllButton.AutoSize = true;
            this.CallsAllButton.Location = new System.Drawing.Point(3, 3);
            this.CallsAllButton.Name = "CallsAllButton";
            this.CallsAllButton.Size = new System.Drawing.Size(37, 17);
            this.CallsAllButton.TabIndex = 19;
            this.CallsAllButton.Text = "All";
            this.CallsAllButton.UseVisualStyleBackColor = true;
            this.CallsAllButton.CheckedChanged += new System.EventHandler(this.CallsAllButton_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(198, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Calls";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.LayoutDependencyButton);
            this.panel1.Controls.Add(this.LayoutInOrder);
            this.panel1.Controls.Add(this.LayoutClassCallsButton);
            this.panel1.Controls.Add(this.LayoutTreeMapButton);
            this.panel1.Controls.Add(this.LayoutCallGraphButton);
            this.panel1.Location = new System.Drawing.Point(6, 17);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(94, 137);
            this.panel1.TabIndex = 21;
            // 
            // LayoutDependencyButton
            // 
            this.LayoutDependencyButton.AutoSize = true;
            this.LayoutDependencyButton.Location = new System.Drawing.Point(3, 94);
            this.LayoutDependencyButton.Name = "LayoutDependencyButton";
            this.LayoutDependencyButton.Size = new System.Drawing.Size(94, 17);
            this.LayoutDependencyButton.TabIndex = 22;
            this.LayoutDependencyButton.TabStop = true;
            this.LayoutDependencyButton.Text = "Dependencies";
            this.LayoutDependencyButton.UseVisualStyleBackColor = true;
            this.LayoutDependencyButton.CheckedChanged += new System.EventHandler(this.LayoutDependencyButton_CheckedChanged);
            // 
            // LayoutInOrder
            // 
            this.LayoutInOrder.AutoSize = true;
            this.LayoutInOrder.Location = new System.Drawing.Point(25, 48);
            this.LayoutInOrder.Name = "LayoutInOrder";
            this.LayoutInOrder.Size = new System.Drawing.Size(64, 17);
            this.LayoutInOrder.TabIndex = 21;
            this.LayoutInOrder.Text = "In Order";
            this.LayoutInOrder.UseVisualStyleBackColor = true;
            this.LayoutInOrder.CheckedChanged += new System.EventHandler(this.LayoutInOrder_CheckedChanged);
            // 
            // LayoutClassCallsButton
            // 
            this.LayoutClassCallsButton.AutoSize = true;
            this.LayoutClassCallsButton.Location = new System.Drawing.Point(3, 71);
            this.LayoutClassCallsButton.Name = "LayoutClassCallsButton";
            this.LayoutClassCallsButton.Size = new System.Drawing.Size(75, 17);
            this.LayoutClassCallsButton.TabIndex = 3;
            this.LayoutClassCallsButton.TabStop = true;
            this.LayoutClassCallsButton.Text = "Class Calls";
            this.LayoutClassCallsButton.UseVisualStyleBackColor = true;
            this.LayoutClassCallsButton.CheckedChanged += new System.EventHandler(this.LayoutClassCallsButton_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.ShowAllButton);
            this.panel2.Controls.Add(this.ShowHitButton);
            this.panel2.Controls.Add(this.ShowNotHitButton);
            this.panel2.Controls.Add(this.ShowInstancesButton);
            this.panel2.Controls.Add(this.ResetHitButton);
            this.panel2.Location = new System.Drawing.Point(106, 17);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(89, 137);
            this.panel2.TabIndex = 22;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.CallsAllButton);
            this.panel3.Controls.Add(this.CallsRealTimeButton);
            this.panel3.Location = new System.Drawing.Point(201, 17);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(77, 137);
            this.panel3.TabIndex = 23;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.IncludeFields);
            this.panel4.Controls.Add(this.IncludeOutsideZoomButton);
            this.panel4.Controls.Add(this.IncludeNotXRayedButton);
            this.panel4.Location = new System.Drawing.Point(284, 17);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(101, 137);
            this.panel4.TabIndex = 24;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.SizeConstantButton);
            this.panel5.Controls.Add(this.SizeLinesButton);
            this.panel5.Controls.Add(this.SizeTimeInMethodButton);
            this.panel5.Controls.Add(this.SizeCallsButton);
            this.panel5.Controls.Add(this.SizeTimePerCallButton);
            this.panel5.Location = new System.Drawing.Point(391, 16);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(107, 138);
            this.panel5.TabIndex = 25;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.TrackingInstances);
            this.panel6.Controls.Add(this.TrackingMethodCalls);
            this.panel6.Controls.Add(this.TrackingClassCalls);
            this.panel6.Location = new System.Drawing.Point(504, 16);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(107, 138);
            this.panel6.TabIndex = 27;
            // 
            // TrackingInstances
            // 
            this.TrackingInstances.AutoSize = true;
            this.TrackingInstances.Location = new System.Drawing.Point(3, 49);
            this.TrackingInstances.Name = "TrackingInstances";
            this.TrackingInstances.Size = new System.Drawing.Size(72, 17);
            this.TrackingInstances.TabIndex = 14;
            this.TrackingInstances.Text = "Instances";
            this.TrackingInstances.UseVisualStyleBackColor = true;
            this.TrackingInstances.CheckedChanged += new System.EventHandler(this.TrackingInstances_CheckedChanged);
            // 
            // TrackingMethodCalls
            // 
            this.TrackingMethodCalls.AutoSize = true;
            this.TrackingMethodCalls.Location = new System.Drawing.Point(3, 5);
            this.TrackingMethodCalls.Name = "TrackingMethodCalls";
            this.TrackingMethodCalls.Size = new System.Drawing.Size(87, 17);
            this.TrackingMethodCalls.TabIndex = 12;
            this.TrackingMethodCalls.Text = "Method Calls";
            this.TrackingMethodCalls.UseVisualStyleBackColor = true;
            this.TrackingMethodCalls.CheckedChanged += new System.EventHandler(this.TrackingMethodCalls_CheckedChanged);
            // 
            // TrackingClassCalls
            // 
            this.TrackingClassCalls.AutoSize = true;
            this.TrackingClassCalls.Location = new System.Drawing.Point(3, 28);
            this.TrackingClassCalls.Name = "TrackingClassCalls";
            this.TrackingClassCalls.Size = new System.Drawing.Size(76, 17);
            this.TrackingClassCalls.TabIndex = 13;
            this.TrackingClassCalls.Text = "Class Calls";
            this.TrackingClassCalls.UseVisualStyleBackColor = true;
            this.TrackingClassCalls.CheckedChanged += new System.EventHandler(this.TrackingClassCalls_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(501, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Tracking";
            // 
            // IncludeFields
            // 
            this.IncludeFields.AutoSize = true;
            this.IncludeFields.Location = new System.Drawing.Point(3, 48);
            this.IncludeFields.Name = "IncludeFields";
            this.IncludeFields.Size = new System.Drawing.Size(53, 17);
            this.IncludeFields.TabIndex = 12;
            this.IncludeFields.Text = "Fields";
            this.IncludeFields.UseVisualStyleBackColor = true;
            this.IncludeFields.CheckedChanged += new System.EventHandler(this.IncludeFields_CheckedChanged);
            // 
            // ViewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ViewPanel";
            this.Size = new System.Drawing.Size(662, 157);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        public System.Windows.Forms.RadioButton LayoutTreeMapButton;
        public System.Windows.Forms.RadioButton LayoutCallGraphButton;
        public System.Windows.Forms.RadioButton SizeLinesButton;
        public System.Windows.Forms.RadioButton SizeConstantButton;
        public System.Windows.Forms.RadioButton SizeCallsButton;
        public System.Windows.Forms.RadioButton SizeTimeInMethodButton;
        public System.Windows.Forms.RadioButton SizeTimePerCallButton;
        public System.Windows.Forms.CheckBox IncludeOutsideZoomButton;
        public System.Windows.Forms.CheckBox IncludeNotXRayedButton;
        public System.Windows.Forms.RadioButton ShowNotHitButton;
        public System.Windows.Forms.RadioButton ShowHitButton;
        public System.Windows.Forms.RadioButton ShowAllButton;
        public System.Windows.Forms.RadioButton ShowInstancesButton;
        public System.Windows.Forms.Button ResetHitButton;
        public System.Windows.Forms.CheckBox CallsRealTimeButton;
        public System.Windows.Forms.CheckBox CallsAllButton;
        public System.Windows.Forms.RadioButton LayoutClassCallsButton;
        private System.Windows.Forms.Panel panel6;
        public System.Windows.Forms.CheckBox TrackingInstances;
        public System.Windows.Forms.CheckBox TrackingMethodCalls;
        public System.Windows.Forms.CheckBox TrackingClassCalls;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.CheckBox LayoutInOrder;
        public System.Windows.Forms.RadioButton LayoutDependencyButton;
        public System.Windows.Forms.CheckBox IncludeFields;
    }
}
