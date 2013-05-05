using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpHelper;

using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Tutorial13
{
    static class Program
    {

        struct Data
        {
            public Matrix world;
            public Matrix viewProjection;
            public Vector4 lightDirection;
            public Vector4 viewDirection;
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
            form.Text = "Tutorial 13: Geometry Instancing";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //load font
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                //load model from wavefront obj file
                SharpMesh mesh = SharpMesh.CreateNormalMappedFromObj(device, "../../../Models/dog/dog.obj");

                //init shader with normal map illumination
                SharpShader shader = new SharpShader(device, "../../HLSL_instancing.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
                        new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 36, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0),
                        new InputElement("INSTANCEPOSITION",0,Format.R32G32B32_Float,0,1,InputClassification.PerInstanceData,1)
                    });

                SharpShader shaderStandard = new SharpShader(device, "../../HLSL_standard.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
                        new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 36, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0)
                    });

                //create instance buffer data
                List<Vector3> positions = new List<Vector3>();

                for (int z = 0; z < 100; z++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            positions.Add(new Vector3(i * 80 - 400, j * 80 - 400, z * 80 - 400));
                        }
                    }
                }

                SharpInstanceBuffer<Vector3> instanceBuffer = new SharpInstanceBuffer<Vector3>(device, positions.ToArray());

                //init constant buffer
                Buffer11 buffer = shader.CreateBuffer<Data>();



                //init frame counter
                fpsCounter.Reset();

                //to active normal mapping
                bool instancing = true;
                int instanceCount = 5000;

                form.KeyDown += (sender, e) =>
                {
                    switch (e.KeyCode)
                    {
                        case Keys.I:
                            instancing = true;
                            break;
                        case Keys.D:
                            instancing = false;
                            break;
                        case Keys.Up:

                            instanceCount += 100;

                            if (instanceCount >= 10000)
                                instanceCount = 10000;
                            break;
                        case Keys.Down:

                            instanceCount -= 100;
                            if (instanceCount < 0)
                                instanceCount = 0;
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

                    //clear color
                    device.Clear(Color.CornflowerBlue);



                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1F, 50000.0F);

                    //set camera position and target
                    Vector3 from = new Vector3(0, 70, -1000);
                    Vector3 to = new Vector3(0, 50, 0);
                    Matrix view = Matrix.LookAtLH(from, to, Vector3.UnitY);

                    //light direction
                    Vector3 lightDirection = new Vector3(0.5f, 0, -1);
                    lightDirection.Normalize();

                    if (instancing)
                    {
                        //Instancing rendering loop

                        //set world matrix
                        Matrix world = Matrix.RotationY(Environment.TickCount / 2000.0F);


                        Data sceneInformation = new Data()
                        {
                            world = world,
                            viewProjection = view * projection,
                            lightDirection = new Vector4(lightDirection, 1),
                            viewDirection = new Vector4(Vector3.Normalize(from - to), 1)
                        };


                        //apply shader
                        shader.Apply();

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
                            device.DeviceContext.PixelShader.SetShaderResource(1, mesh.SubSets[i].NormalMap);
                            instanceBuffer.DrawInstance(instanceCount, mesh.SubSets[i].IndexCount, mesh.SubSets[i].StartIndex);
                        }
                    }
                    else
                    {
                        //non instancing
                        //apply shader
                        shaderStandard.Apply();

                        //apply constant buffer to shader
                        device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);
                        device.DeviceContext.PixelShader.SetConstantBuffer(0, buffer);

                        //prepare mesh
                        mesh.Begin();

                        for (int j = 0; j < instanceCount; j++)
                        {
                            //set world matrix
                            Matrix world = Matrix.RotationY(Environment.TickCount / 2000.0F) * Matrix.Translation(positions[j]);


                            Data sceneInformation = new Data()
                            {
                                world = world,
                                viewProjection = view * projection,
                                lightDirection = new Vector4(lightDirection, 1),
                                viewDirection = new Vector4(Vector3.Normalize(from - to), 1)
                            };

                            //write data inside constant buffer
                            device.UpdateData<Data>(buffer, sceneInformation);


                            //draw mesh
                            for (int i = 0; i < mesh.SubSets.Count; i++)
                            {
                                device.DeviceContext.PixelShader.SetShaderResource(0, mesh.SubSets[i].DiffuseMap);
                                device.DeviceContext.PixelShader.SetShaderResource(1, mesh.SubSets[i].NormalMap);
                                mesh.Draw(i);
                            }
                        }

                    }

                    //begin drawing text
                    font.Begin();

                    //draw string
                    fpsCounter.Update();
                    font.DrawString("FPS: " + fpsCounter.FPS, 0, 0, Color.White);
                    if (instancing)
                        font.DrawString("Instancing On: Press D to use standard rendering. See FPS", 0, 30, Color.White);
                    else
                        font.DrawString("Instancing Off: Press I to use Instancing. See FPS", 0, 30, Color.White);

                    font.DrawString("Press up and down to change count ", 0, 60, Color.White);
                    font.DrawString("Count: " + instanceCount, 0, 90, Color.White);

                    //flush text to view
                    font.End();
                    //present
                    device.Present();

                });

                //release resource
                font.Dispose();
                mesh.Dispose();
                buffer.Dispose();
                shader.Dispose();
                shaderStandard.Dispose();
                instanceBuffer.Dispose();
            }
        }
    }
}
