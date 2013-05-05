namespace TutorialI1
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
            this.timerCounter = new System.Windows.Forms.Timer(this.components);
            this.pLeft = new System.Windows.Forms.PictureBox();
            this.pRight = new System.Windows.Forms.PictureBox();
            this.pButtonY = new System.Windows.Forms.PictureBox();
            this.pButtonX = new System.Windows.Forms.PictureBox();
            this.pButtonA = new System.Windows.Forms.PictureBox();
            this.pButtonB = new System.Windows.Forms.PictureBox();
            this.pTriggerL = new System.Windows.Forms.PictureBox();
            this.pTriggerR = new System.Windows.Forms.PictureBox();
            this.pDPad = new System.Windows.Forms.PictureBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblBattery = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblHelp = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pTriggerL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pTriggerR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pDPad)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerCounter
            // 
            this.timerCounter.Enabled = true;
            this.timerCounter.Tick += new System.EventHandler(this.timerCounter_Tick);
            // 
            // pLeft
            // 
            this.pLeft.Location = new System.Drawing.Point(83, 57);
            this.pLeft.Name = "pLeft";
            this.pLeft.Size = new System.Drawing.Size(100, 100);
            this.pLeft.TabIndex = 0;
            this.pLeft.TabStop = false;
            // 
            // pRight
            // 
            this.pRight.Location = new System.Drawing.Point(400, 57);
            this.pRight.Name = "pRight";
            this.pRight.Size = new System.Drawing.Size(100, 100);
            this.pRight.TabIndex = 1;
            this.pRight.TabStop = false;
            // 
            // pButtonY
            // 
            this.pButtonY.Location = new System.Drawing.Point(456, 182);
            this.pButtonY.Name = "pButtonY";
            this.pButtonY.Size = new System.Drawing.Size(50, 50);
            this.pButtonY.TabIndex = 2;
            this.pButtonY.TabStop = false;
            // 
            // pButtonX
            // 
            this.pButtonX.Location = new System.Drawing.Point(400, 182);
            this.pButtonX.Name = "pButtonX";
            this.pButtonX.Size = new System.Drawing.Size(50, 50);
            this.pButtonX.TabIndex = 3;
            this.pButtonX.TabStop = false;
            // 
            // pButtonA
            // 
            this.pButtonA.Location = new System.Drawing.Point(400, 238);
            this.pButtonA.Name = "pButtonA";
            this.pButtonA.Size = new System.Drawing.Size(50, 50);
            this.pButtonA.TabIndex = 4;
            this.pButtonA.TabStop = false;
            // 
            // pButtonB
            // 
            this.pButtonB.Location = new System.Drawing.Point(456, 238);
            this.pButtonB.Name = "pButtonB";
            this.pButtonB.Size = new System.Drawing.Size(50, 50);
            this.pButtonB.TabIndex = 5;
            this.pButtonB.TabStop = false;
            // 
            // pTriggerL
            // 
            this.pTriggerL.Location = new System.Drawing.Point(27, 57);
            this.pTriggerL.Name = "pTriggerL";
            this.pTriggerL.Size = new System.Drawing.Size(50, 100);
            this.pTriggerL.TabIndex = 6;
            this.pTriggerL.TabStop = false;
            // 
            // pTriggerR
            // 
            this.pTriggerR.Location = new System.Drawing.Point(506, 57);
            this.pTriggerR.Name = "pTriggerR";
            this.pTriggerR.Size = new System.Drawing.Size(50, 100);
            this.pTriggerR.TabIndex = 7;
            this.pTriggerR.TabStop = false;
            // 
            // pDPad
            // 
            this.pDPad.Location = new System.Drawing.Point(83, 188);
            this.pDPad.Name = "pDPad";
            this.pDPad.Size = new System.Drawing.Size(100, 100);
            this.pDPad.TabIndex = 8;
            this.pDPad.TabStop = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(216, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 20);
            this.lblStatus.TabIndex = 9;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblBattery});
            this.statusStrip1.Location = new System.Drawing.Point(0, 355);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(597, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblBattery
            // 
            this.lblBattery.Name = "lblBattery";
            this.lblBattery.Size = new System.Drawing.Size(74, 17);
            this.lblBattery.Text = "Battery Level";
            // 
            // lblHelp
            // 
            this.lblHelp.AutoSize = true;
            this.lblHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHelp.Location = new System.Drawing.Point(159, 315);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(252, 20);
            this.lblHelp.TabIndex = 12;
            this.lblHelp.Text = "Press Analogic Sticks and Buttons";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 377);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pDPad);
            this.Controls.Add(this.pTriggerR);
            this.Controls.Add(this.pTriggerL);
            this.Controls.Add(this.pButtonB);
            this.Controls.Add(this.pButtonA);
            this.Controls.Add(this.pButtonX);
            this.Controls.Add(this.pButtonY);
            this.Controls.Add(this.pRight);
            this.Controls.Add(this.pLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Input Game-Pad";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pButtonB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pTriggerL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pTriggerR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pDPad)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timerCounter;
        private System.Windows.Forms.PictureBox pLeft;
        private System.Windows.Forms.PictureBox pRight;
        private System.Windows.Forms.PictureBox pButtonY;
        private System.Windows.Forms.PictureBox pButtonX;
        private System.Windows.Forms.PictureBox pButtonA;
        private System.Windows.Forms.PictureBox pButtonB;
        private System.Windows.Forms.PictureBox pTriggerL;
        private System.Windows.Forms.PictureBox pTriggerR;
        private System.Windows.Forms.PictureBox pDPad;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblBattery;
        private System.Windows.Forms.Label lblHelp;
    }
}

