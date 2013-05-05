using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpHelper;

using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Tutorial8
{

    struct Data
    {
        public Matrix World;
        public Matrix ViewProj;
    }

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

            int[] indices = Enumerable.Range(0, 1000).ToArray();
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    for (int z = 0; z < 10; z++)
                    {
                        vertices.Add(new Vector3(i - 5, j - 5, 5 - z) * 12);
                    }
                }
            }


            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 8: Geometry Shader";
            SharpFPS fpsCounter = new SharpFPS();

            //number of cube
            int count = 1000;

            using (SharpDevice device = new SharpDevice(form))
            {
                SharpBatch font = new SharpBatch(device, "textfont.dds");
                SharpMesh mesh = SharpMesh.Create<Vector3>(device, vertices.ToArray(), indices);
                SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS", GeometryShaderFunction = "GS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0)
                    });

                Buffer11 buffer = shader.CreateBuffer<Data>();
                ShaderResourceView texture = ShaderResourceView.FromFile(device.Device, "../../texture.dds");

                fpsCounter.Reset();

                form.KeyDown += (sender, e) =>
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Up:
                            if (count < 1000)
                                count++;
                            break;
                        case Keys.Down:
                            if (count > 0)
                                count--;
                            break;
                    }
                };

                //main loop
                RenderLoop.Run(form, () =>
                {
                    //Resizing
                    if (device.MustResize)
                    {
                        device.Resize();
                        font.Resize();
                    }


                    //apply state
                    device.UpdateAllStates();

                    //clear color
                    device.Clear(Color.CornflowerBlue);

                    //apply shader
                    shader.Apply();



                    //apply constant buffer to shader
                    device.DeviceContext.GeometryShader.SetConstantBuffer(0, buffer);

                    //set texture
                    device.DeviceContext.PixelShader.SetShaderResource(0, texture);

                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 10000);
                    Matrix view = Matrix.LookAtLH(new Vector3(10, 20, -100), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.RotationY(Environment.TickCount / 1000.0F);

                    Data matrices = new Data() { World = world, ViewProj = view * projection };
                    device.UpdateData<Data>(buffer, matrices);

                    //draw mesh
                    mesh.DrawPoints(count);


                    //begin drawing text
                    device.DeviceContext.GeometryShader.Set(null);
                    font.Begin();

                    //draw string
                    fpsCounter.Update();
                    font.DrawString("FPS: " + fpsCounter.FPS, 0, 0, Color.White);
                    font.DrawString("Cube Count: " + count, 0, 30, Color.White);
                    font.DrawString("Press Up and Down to change number", 0, 60, Color.White);

                    //flush text to view
                    font.End();
                    //present
                    device.Present();


                });

                //release resource
                font.Dispose();
                mesh.Dispose();
                buffer.Dispose();
            }
        }
    }
}
