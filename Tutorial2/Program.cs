using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpHelper;

namespace Tutorial2
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!SharpDevice.IsDirectX11Supported())
            {
                System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
                return;
            }

            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 2: Init Device (press key from 1 to 8)";

            //background color
            Color4 color = Color.CornflowerBlue;

            //keydown event
            form.KeyDown += (sender, e) =>
            {
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.D1:
                        color = Color.CornflowerBlue;
                        break;
                    case System.Windows.Forms.Keys.D2:
                        color = Color.Red;
                        break;
                    case System.Windows.Forms.Keys.D3:
                        color = Color.Blue;
                        break;
                    case System.Windows.Forms.Keys.D4:
                        color = Color.Orange;
                        break;
                    case System.Windows.Forms.Keys.D5:
                        color = Color.Yellow;
                        break;
                    case System.Windows.Forms.Keys.D6:
                        color = Color.Olive;
                        break;
                    case System.Windows.Forms.Keys.D7:
                        color = Color.Orchid;
                        break;
                    case System.Windows.Forms.Keys.D8:
                        color = Color.Black;
                        break;
                }
            };

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
                    device.Clear(color);

                    //present
                    device.Present();
                });
            }
        }
    }
}
