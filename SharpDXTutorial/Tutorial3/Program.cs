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
                //create font from file (generated with tkfont.exe)
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                RenderLoop.Run(form, () =>
                {
                    //resize if form was resized
                    if (device.MustResize)
                    {
                        device.Resize();
                        font.Resize();
                    }

                    //clear color
                    device.Clear(Color.CornflowerBlue);

                    //begin drawing text
                    font.Begin();
                    
                    //draw string
                    font.DrawString("Hello SharpDX", 0, 0, Color.White);
                    
                    font.DrawString("Current Time " + DateTime.Now.ToString(), 0, 32, Color.White);
                    
                    //flush text to view
                    font.End();

                    //present
                    device.Present();
                });

                font.Dispose();
            }
        }
    }
}
