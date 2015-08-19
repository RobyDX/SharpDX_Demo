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


namespace Tutorial10
{
    static class Program
    {

        struct PhongData
        {
            public Matrix world;
            public Matrix worldViewProjection;
            public Vector4 lightDirection;
        }

        struct RenderTargetData
        {
            public Matrix worldViewProjection;
            public Vector4 data;
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
            form.Text = "Tutorial 10: Render Target";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //load font
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                //load model from wavefront obj file
                SharpMesh dogMesh = SharpMesh.CreateNormalMappedFromObj(device, "../../../Models/dog/dog.obj");
                SharpMesh cubeMesh = SharpMesh.CreateFromObj(device, "../../../Models/cube.obj");

                //init shader
                SharpShader phongShader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
                        new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 36, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0)
                    });

                SharpShader renderTargetShader = new SharpShader(device, "../../HLSL_RT.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                    });

                //render target
                SharpRenderTarget target = new SharpRenderTarget(device, 512, 512, Format.R8G8B8A8_UNorm);

                //init constant buffer
                Buffer11 phongConstantBuffer = phongShader.CreateBuffer<PhongData>();
                Buffer11 renderTargetConstantBuffer = phongShader.CreateBuffer<RenderTargetData>();

                //init frame counter
                fpsCounter.Reset();

                //effect inside shader
                int mode = 0;
                form.KeyDown += (sender, e) =>
                {
                    switch (e.KeyCode)
                    {
                        case Keys.D1:
                            mode = 0;
                            break;
                        case Keys.D2:
                            mode = 1;
                            break;
                        case Keys.D3:
                            mode = 2;
                            break;
                        case Keys.D4:
                            mode = 3;
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

                    //apply states
                    device.UpdateAllStates();


                    //BEGIN RENDERING TO TEXTURE
                    target.Apply();
                    target.Clear(Color.CornflowerBlue);

                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1F, 1000.0F);

                    Vector3 from = new Vector3(0, 70, -150);
                    Vector3 to = new Vector3(0, 50, 0);

                    Matrix view = Matrix.LookAtLH(from, to, Vector3.UnitY);
                    Matrix world = Matrix.RotationY(Environment.TickCount / 2000.0F);

                    //light direction
                    Vector3 lightDirection = new Vector3(0.5f, 0, -1);
                    lightDirection.Normalize();


                    PhongData sceneInformation = new PhongData()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1),
                    };


                    //apply shader
                    phongShader.Apply();

                    //write data inside constant buffer
                    device.UpdateData<PhongData>(phongConstantBuffer, sceneInformation);

                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, phongConstantBuffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, phongConstantBuffer);

                    //draw mesh
                    dogMesh.Begin();
                    for (int i = 0; i < dogMesh.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, dogMesh.SubSets[i].DiffuseMap);
                        device.DeviceContext.PixelShader.SetShaderResource(1, dogMesh.SubSets[i].NormalMap);
                        dogMesh.Draw(i);
                    }


                    //RENDERING TO DEVICE

                    //Set original targets
                    device.SetDefaultTargers();

                    //apply shader
                    renderTargetShader.Apply();

                    Matrix WVP =
                        Matrix.RotationY(Environment.TickCount / 2000.0F) *
                        Matrix.LookAtLH(new Vector3(7, 10, -13), new Vector3(), Vector3.UnitY) *
                        projection;
                    device.UpdateData<RenderTargetData>(renderTargetConstantBuffer, new RenderTargetData() { worldViewProjection = WVP, data = new Vector4(mode, 0, 0, 0) });


                    //clear color
                    device.Clear(Color.Black);

                    renderTargetShader.Apply();

                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, renderTargetConstantBuffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, renderTargetConstantBuffer);

                    //set target
                    device.DeviceContext.PixelShader.SetShaderResource(0, target.Resource);

                    cubeMesh.Draw();


                    //begin drawing text
                    font.Begin();

                    //draw string
                    fpsCounter.Update();
                    font.DrawString("FPS: " + fpsCounter.FPS, 0, 0, Color.White);
                    font.DrawString("Press 1 To 4 to change Effect", 0, 30, Color.White);

                    //flush text to view
                    font.End();
                    //present
                    device.Present();

                });


                //release resource
                font.Dispose();
                dogMesh.Dispose();
                cubeMesh.Dispose();
                phongConstantBuffer.Dispose();
                renderTargetConstantBuffer.Dispose();
                phongShader.Dispose();
                renderTargetShader.Dispose();

            }
        }
    }
}
