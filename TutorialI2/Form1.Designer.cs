namespace TutorialI2
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
            this.lblText = new System.Windows.Forms.Label();
            this.timerEvent = new System.Windows.Forms.Timer(this.components);
            this.lblRightEngine = new System.Windows.Forms.Label();
            this.lblLeftEngine = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblText.Location = new System.Drawing.Point(12, 26);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(374, 24);
            this.lblText.TabIndex = 0;
            this.lblText.Text = "User Left and Right Trigger to Start Vibration";
            // 
            // timerEvent
            // 
            this.timerEvent.Enabled = true;
            this.timerEvent.Tick += new System.EventHandler(this.timerEvent_Tick);
            // 
            // lblRightEngine
            // 
            this.lblRightEngine.AutoSize = true;
            this.lblRightEngine.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRightEngine.Location = new System.Drawing.Point(214, 74);
            this.lblRightEngine.Name = "lblRightEngine";
            this.lblRightEngine.Size = new System.Drawing.Size(58, 24);
            this.lblRightEngine.TabIndex = 1;
            this.lblRightEngine.Text = "Right:";
            // 
            // lblLeftEngine
            // 
            this.lblLeftEngine.AutoSize = true;
            this.lblLeftEngine.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLeftEngine.Location = new System.Drawing.Point(12, 74);
            this.lblLeftEngine.Name = "lblLeftEngine";
            this.lblLeftEngine.Size = new System.Drawing.Size(44, 24);
            this.lblLeftEngine.TabIndex = 2;
            this.lblLeftEngine.Text = "Left:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 195);
            this.Controls.Add(this.lblLeftEngine);
            this.Controls.Add(this.lblRightEngine);
            this.Controls.Add(this.lblText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Input - Vibration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Timer timerEvent;
        private System.Windows.Forms.Label lblRightEngine;
        private System.Windows.Forms.Label lblLeftEngine;
    }
}

