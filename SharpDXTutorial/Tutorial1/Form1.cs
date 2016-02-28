using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;

namespace Tutorial1
{
    public partial class Form1 : Form
    {
        //allow to check Hardware
        Factory factory = new Factory1();
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //Enumerate adapters inside PC
            foreach (Adapter adapter in factory.Adapters)
            {
                cboDevice.Items.Add(string.Format("Name: {0} Id: {1}",
                    adapter.Description.Description, adapter.Description.DeviceId));
            }


            if (cboDevice.Items.Count > 0)
                cboDevice.SelectedIndex = 0;
        }

        private void cboDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetOutput();
        }

        private void cboOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetModes();
        }

        private void GetOutput()
        {
            cboOutput.Items.Clear();
            lstMode.Items.Clear();

            if (cboDevice.SelectedIndex < 0)
                return;

            //Check The features level of the current Graphics Adapter 
            //11 mean DirectX11, 10_1 mean DirectX10.1 etc
            lblFeatureLevel.Text = "Feature Level: " + SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(factory.Adapters[cboDevice.SelectedIndex]);

            cboOutput.Items.Clear();

            //get the outputs (Monitors,TV) connected to Adapters
            foreach (Output o in factory.Adapters[cboDevice.SelectedIndex].Outputs)
            {
                cboOutput.Items.Add(string.Format("Name: {0} Size: {1}x{2} {3}",
                    o.Description.DeviceName,
                    o.Description.DesktopBounds.Right,
                    o.Description.DesktopBounds.Bottom,
                    (o.Description.IsAttachedToDesktop ? " Connected" : "Disconnected")
                    ));
            }

            if (cboOutput.Items.Count > 0)
                cboOutput.SelectedIndex = 0;

            

        }

        private void GetModes()
        {
            lstMode.Items.Clear();
            if (cboDevice.SelectedIndex < 0)
                return;

            if (cboOutput.SelectedIndex < 0)
                return;

            //get the resolutions supported by the output connected to selected Graphics Adapter
            ModeDescription[] list = factory.Adapters[cboDevice.SelectedIndex].Outputs[cboOutput.SelectedIndex].GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Scaling);
            lstMode.Items.Clear();

            foreach (ModeDescription desc in list)
            {
                lstMode.Items.Add(string.Format("{0}x{1}   Format: {2}   Hz: {3}",
                    desc.Width,
                    desc.Height,
                    desc.Format,
                    desc.RefreshRate.Numerator / desc.RefreshRate.Denominator));//current Refresh Rate in Hz (refreshes per second)
            }
        }


    }
}
