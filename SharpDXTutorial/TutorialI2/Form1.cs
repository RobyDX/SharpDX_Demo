using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.XInput;

namespace TutorialI2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Controller controller1 = new Controller(UserIndex.One);

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timerEvent_Tick(object sender, EventArgs e)
        {
            if (controller1 != null && controller1.IsConnected)
            {
                Vibration v = new Vibration();
                v.LeftMotorSpeed = (ushort)(controller1.GetState().Gamepad.LeftTrigger * 255);
                v.RightMotorSpeed = (ushort)(controller1.GetState().Gamepad.RightTrigger * 255);
                lblLeftEngine.Text = "Left: " + v.LeftMotorSpeed;
                lblRightEngine.Text = "Right: " + v.RightMotorSpeed;
                controller1.SetVibration(v);

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (controller1 != null && controller1.IsConnected)
            {
                Vibration v = new Vibration();
                v.LeftMotorSpeed = 0;
                v.RightMotorSpeed = 0;
                controller1.SetVibration(v);
            }
        }
    }
}
