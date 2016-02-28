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

namespace Tutorial4
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

            //Indices
            int[] indices = new int[]
            {
                0,1,2,0,2,3,
                4,6,5,4,7,6,
                8,9,10,8,10,11,
                12,14,13,12,15,14,
                16,18,17,16,19,18,
                20,21,22,20,22,23
            };


            //Vertices
            ColoredVertex[] vertices = new[]
            {
                ////TOP
                new ColoredVertex(new Vector3(-5,5,5),new Vector4(0,1,0,0)),
                new ColoredVertex(new Vector3(5,5,5),new Vector4(0,1,0,0)),
                new ColoredVertex(new Vector3(5,5,-5),new Vector4(0,1,0,0)),
                new ColoredVertex(new Vector3(-5,5,-5),new Vector4(0,1,0,0)),
                //BOTTOM
                new ColoredVertex(new Vector3(-5,-5,5),new Vector4(1,0,1,1)),
                new ColoredVertex(new Vector3(5,-5,5),new Vector4(1,0,1,1)),
                new ColoredVertex(new Vector3(5,-5,-5),new Vector4(1,0,1,1)),
                new ColoredVertex(new Vector3(-5,-5,-5),new Vector4(1,0,1,1)),
                //LEFT
                new ColoredVertex(new Vector3(-5,-5,5),new Vector4(1,0,0,1)),
                new ColoredVertex(new Vector3(-5,5,5),new Vector4(1,0,0,1)),
                new ColoredVertex(new Vector3(-5,5,-5),new Vector4(1,0,0,1)),
                new ColoredVertex(new Vector3(-5,-5,-5),new Vector4(1,0,0,1)),
                //RIGHT
                new ColoredVertex(new Vector3(5,-5,5),new Vector4(1,1,0,1)),
                new ColoredVertex(new Vector3(5,5,5),new Vector4(1,1,0,1)),
                new ColoredVertex(new Vector3(5,5,-5),new Vector4(1,1,0,1)),
                new ColoredVertex(new Vector3(5,-5,-5),new Vector4(1,1,0,1)),
                //FRONT
                new ColoredVertex(new Vector3(-5,5,5),new Vector4(0,1,1,1)),
                new ColoredVertex(new Vector3(5,5,5),new Vector4(0,1,1,1)),
                new ColoredVertex(new Vector3(5,-5,5),new Vector4(0,1,1,1)),
                new ColoredVertex(new Vector3(-5,-5,5),new Vector4(0,1,1,1)),
                //BACK
                new ColoredVertex(new Vector3(-5,5,-5),new Vector4(0,0,1,1)),
                new ColoredVertex(new Vector3(5,5,-5),new Vector4(0,0,1,1)),
                new ColoredVertex(new Vector3(5,-5,-5),new Vector4(0,0,1,1)),
                new ColoredVertex(new Vector3(-5,-5,-5),new Vector4(0,0,1,1))
            };



            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 4: Primitives";

            //Help to count Frame Per Seconds
            SharpFPS fpsCounter = new SharpFPS();

            using (SharpDevice device = new SharpDevice(form))
            {
                //Init Mesh
                SharpMesh mesh = SharpMesh.Create<ColoredVertex>(device, vertices, indices);

                //Create Shader From File and Create Input Layout
                SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
                    });

                //create constant buffer
                Buffer11 buffer = shader.CreateBuffer<Matrix>();

                fpsCounter.Reset();

                //main loop
                RenderLoop.Run(form, () =>
                {
                    //Resizing
                    if (device.MustResize)
                    {
                        device.Resize();
                    }

                    //apply states
                    device.UpdateAllStates();

                    //clear color
                    device.Clear(Color.CornflowerBlue);

                    //Set matrices
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 1000);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 10, -50), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.RotationY(Environment.TickCount / 1000.0F);
                    Matrix WVP = world * view * projection;

                    //update constant buffer
                    device.UpdateData<Matrix>(buffer, WVP);

                    //pass constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                    //apply shader
                    shader.Apply();

                    //draw mesh
                    mesh.Draw();

                    //begin drawing text
                    device.Font.Begin();

                    //draw string
                    fpsCounter.Update();
                    device.Font.DrawString("FPS: " + fpsCounter.FPS, 0, 0);

                    //flush text to view
                    device.Font.End();
                    //present
                    device.Present();
                });

                //release resources
                mesh.Dispose();
                buffer.Dispose();
            }

        }
    }
}
