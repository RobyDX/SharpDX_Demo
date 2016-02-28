using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;
using SharpHelper;

namespace Tutorial3
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!SharpDevice.IsDirectX11Supported())
            {
                System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
                return;
            }

            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 3: Font";


            //main loop
            using (SharpDevice device = new SharpDevice(form))
            {

                RenderLoop.Run(form, () =>
                {
                    //resize if form was resized
                    if (device.MustResize)
                    {
                        device.Resize();
                    }

                    //clear color
                    device.Clear(Color.CornflowerBlue);

                    //begin drawing text
                    device.Font.Begin();

                    //draw string
                    device.Font.DrawString("Hello SharpDX", 0, 0);

                    device.Font.DrawString("Current Time " + DateTime.Now.ToString(), 0, 32);

                    //flush text to view
                    device.Font.End();

                    //present
                    device.Present();
                });

            }
        }
    }
}
