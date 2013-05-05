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

namespace Tutorial7
{
    static class Program
    {
        //struct used to set shader constant buffer
        struct Data
        {
            public Matrix world;
            public Matrix worldViewProjection;
            public Vector4 lightDirection;
        }

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
            form.Text = "Tutorial 7: Loading Obj File";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //load font
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                //load model from wavefront obj file
                SharpMesh mesh = SharpMesh.CreateFromObj(device, "../../../Models/car/car.obj");

                //init shader
                SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                    });

                //init constant buffer
                Buffer11 buffer = shader.CreateBuffer<Data>();

                //init frame counter
                fpsCounter.Reset();

                //main loop
                RenderLoop.Run(form, () =>
                {
                    //Resizing
                    if (device.MustResize)
                    {
                        device.Resize();
                        font.Resize();
                    }

                    //apply states
                    device.UpdateAllStates();

                    //clear color
                    device.Clear(Color.CornflowerBlue);

                    //apply shader
                    shader.Apply();

                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1F, 1000.0F);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 10, -30), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.RotationY(Environment.TickCount / 1000.0F);

                    //light direction
                    Vector3 lightDirection = new Vector3(0.5f, 0, -1);
                    lightDirection.Normalize();


                    Data sceneInformation = new Data()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1)
                    };

                    //write data inside constant buffer
                    device.UpdateData<Data>(buffer, sceneInformation);

                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, buffer);

                    //draw mesh
                    mesh.Begin();
                    for (int i = 0; i < mesh.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, mesh.SubSets[i].DiffuseMap);
                        mesh.Draw(i);
                    }


                    //begin drawing text
                    font.Begin();

                    //draw string
                    fpsCounter.Update();
                    font.DrawString("FPS: " + fpsCounter.FPS, 0, 0, Color.White);

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
