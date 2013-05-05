namespace Tutorial1
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
            this.cboDevice = new System.Windows.Forms.ComboBox();
            this.lblDevice = new System.Windows.Forms.Label();
            this.lblOutput = new System.Windows.Forms.Label();
            this.cboOutput = new System.Windows.Forms.ComboBox();
            this.lstMode = new System.Windows.Forms.ListBox();
            this.lblModes = new System.Windows.Forms.Label();
            this.lblFeatureLevel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboDevice
            // 
            this.cboDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDevice.FormattingEnabled = true;
            this.cboDevice.Location = new System.Drawing.Point(12, 32);
            this.cboDevice.Name = "cboDevice";
            this.cboDevice.Size = new System.Drawing.Size(712, 28);
            this.cboDevice.TabIndex = 0;
            this.cboDevice.SelectedIndexChanged += new System.EventHandler(this.cboDevice_SelectedIndexChanged);
            // 
            // lblDevice
            // 
            this.lblDevice.AutoSize = true;
            this.lblDevice.Location = new System.Drawing.Point(12, 9);
            this.lblDevice.Name = "lblDevice";
            this.lblDevice.Size = new System.Drawing.Size(57, 20);
            this.lblDevice.TabIndex = 1;
            this.lblDevice.Text = "Device";
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(12, 63);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(58, 20);
            this.lblOutput.TabIndex = 3;
            this.lblOutput.Text = "Output";
            // 
            // cboOutput
            // 
            this.cboOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOutput.FormattingEnabled = true;
            this.cboOutput.Location = new System.Drawing.Point(12, 86);
            this.cboOutput.Name = "cboOutput";
            this.cboOutput.Size = new System.Drawing.Size(712, 28);
            this.cboOutput.TabIndex = 2;
            this.cboOutput.SelectedIndexChanged += new System.EventHandler(this.cboOutput_SelectedIndexChanged);
            // 
            // lstMode
            // 
            this.lstMode.FormattingEnabled = true;
            this.lstMode.ItemHeight = 20;
            this.lstMode.Location = new System.Drawing.Point(12, 141);
            this.lstMode.Name = "lstMode";
            this.lstMode.Size = new System.Drawing.Size(712, 404);
            this.lstMode.TabIndex = 4;
            // 
            // lblModes
            // 
            this.lblModes.AutoSize = true;
            this.lblModes.Location = new System.Drawing.Point(12, 117);
            this.lblModes.Name = "lblModes";
            this.lblModes.Size = new System.Drawing.Size(57, 20);
            this.lblModes.TabIndex = 5;
            this.lblModes.Text = "Modes";
            // 
            // lblFeatureLevel
            // 
            this.lblFeatureLevel.AutoSize = true;
            this.lblFeatureLevel.Location = new System.Drawing.Point(458, 9);
            this.lblFeatureLevel.Name = "lblFeatureLevel";
            this.lblFeatureLevel.Size = new System.Drawing.Size(0, 20);
            this.lblFeatureLevel.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 562);
            this.Controls.Add(this.lblFeatureLevel);
            this.Controls.Add(this.lblModes);
            this.Controls.Add(this.lstMode);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.cboOutput);
            this.Controls.Add(this.lblDevice);
            this.Controls.Add(this.cboDevice);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Tutorial 1: Enumeration";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboDevice;
        private System.Windows.Forms.Label lblDevice;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.ComboBox cboOutput;
        private System.Windows.Forms.ListBox lstMode;
        private System.Windows.Forms.Label lblModes;
        private System.Windows.Forms.Label lblFeatureLevel;
    }
}

