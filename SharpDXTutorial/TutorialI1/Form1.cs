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

namespace TutorialI1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //init controller
        Controller controller1 = new Controller(UserIndex.One);

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void timerCounter_Tick(object sender, EventArgs e)
        {
            lblStatus.Text = "Connect an XInput Pad";

            if (controller1.IsConnected)
            {
                lblStatus.Text = "XInput Pad Connected";

                Gamepad pad = controller1.GetState().Gamepad;

                ShowLeftAxis(pad);
                ShowRightAxis(pad);
                ShowButtonStatus(pad);

                ShowDPad(pad);


                lblBattery.Text = "Battery Level: " + controller1.GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryLevel.ToString();
            }

        }



        private void ShowButtonStatus(Gamepad pad)
        {

            pButtonA.BackColor = (pad.Buttons & GamepadButtonFlags.A) != 0 ? Color.Green : Color.White;
            pButtonB.BackColor = (pad.Buttons & GamepadButtonFlags.B) != 0 ? Color.Red : Color.White;
            pButtonX.BackColor = (pad.Buttons & GamepadButtonFlags.X) != 0 ? Color.Blue : Color.White;
            pButtonY.BackColor = (pad.Buttons & GamepadButtonFlags.Y) != 0 ? Color.Yellow : Color.White;

            pTriggerL.BackColor = Color.FromArgb(0, pad.LeftTrigger, 0);
            pTriggerR.BackColor = Color.FromArgb(0, pad.RightTrigger, 0);
        }

        private void ShowLeftAxis(Gamepad pad)
        {
            Graphics g = pLeft.CreateGraphics();
            g.Clear(System.Drawing.Color.White);

            int leftL = (pad.LeftThumbX * 50) / 32768;
            int topL = (pad.LeftThumbY * 50) / -32768;
            g.FillEllipse(Brushes.Red, leftL + 45, topL + 45, 10, 10);
        }

        private void ShowRightAxis(Gamepad pad)
        {
            Graphics g = pRight.CreateGraphics();
            g.Clear(System.Drawing.Color.White);

            int leftR = (pad.RightThumbX * 50) / 32768;
            int topR = (pad.RightThumbY * 50) / -32768;
            g.FillEllipse(Brushes.Red, leftR + 45, topR + 45, 10, 10);
        }


        private void ShowDPad(Gamepad pad)
        {
            Graphics g = pDPad.CreateGraphics();
            g.Clear(Color.White);
            int angle = 0;
            int offset = 0;

            if ((pad.Buttons & GamepadButtonFlags.DPadUp) != 0)
            {
                angle = 270;
                offset = 45;
            }

            if ((pad.Buttons & GamepadButtonFlags.DPadDown) != 0)
            {
                angle = 90;
                offset = 45;
            }

            if ((pad.Buttons & GamepadButtonFlags.DPadLeft) != 0)
            {
                if (angle == 270)
                    angle = 225;
                else if (angle == 90)
                    angle = 135;
                else
                    angle = 180;

                offset = 45;
            }

            if ((pad.Buttons & GamepadButtonFlags.DPadRight) != 0)
            {

                if (angle == 270)
                    angle = 315;
                else if (angle == 90)
                    angle = 45;
                else
                    angle = 0;

                offset = 45;
            }


            g.FillPie(Brushes.Red, 0, 0, 100, 100, angle - 22.5F, offset);
        }
    }
}
