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

namespace Tutorial9
{
    static class Program
    {

        struct Data
        {
            public Matrix world;
            public Matrix worldViewProjection;
            public Vector4 lightDirection;
            public Vector4 viewDirection;
            public Vector4 bias;
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
            form.Text = "Tutorial 9: Normal Mapping";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //load model from wavefront obj file
                SharpMesh mesh = SharpMesh.CreateNormalMappedFromObj(device, "../../../Models/dog/dog.obj");

                //init shader with normal map illumination
                SharpShader shaderNormal = new SharpShader(device, "../../HLSL_normal.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
                        new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 36, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0)
                    });

                //Init shader with standard illumination
                SharpShader shaderStandard = new SharpShader(device, "../../HLSL_standard.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TANGENT", 0, Format.R32G32B32_Float, 24, 0),
                        new InputElement("BINORMAL", 0, Format.R32G32B32_Float, 36, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0)
                    });

                //init constant buffer
                Buffer11 buffer = shaderNormal.CreateBuffer<Data>();

                //init frame counter
                fpsCounter.Reset();

                //Used for parallax mapping
                float bias = 0.005f;
                //to active normal mapping
                bool normalMap = true;


                form.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.A)
                        bias += 0.005f;
                    else if (e.KeyCode == Keys.S)
                        bias -= 0.005f;

                    if (e.KeyCode == Keys.N)
                        normalMap = true;
                    if (e.KeyCode == Keys.D)
                        normalMap = false;
                };

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



                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1F, 1000.0F);
                    //set camera position and target
                    Vector3 from = new Vector3(0, 70, -150);
                    Vector3 to = new Vector3(0, 50, 0);

                    Matrix view = Matrix.LookAtLH(from, to, Vector3.UnitY);

                    //set world matrix
                    Matrix world = Matrix.RotationY(Environment.TickCount / 2000.0F);

                    //light direction
                    Vector3 lightDirection = new Vector3(0.5f, 0, -1);
                    lightDirection.Normalize();


                    Data sceneInformation = new Data()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1),
                        viewDirection = new Vector4(Vector3.Normalize(from - to), 1),
                        bias = new Vector4(bias, 0, 0, 0)
                    };


                    //apply shader
                    if (normalMap)
                        shaderNormal.Apply();
                    else
                        shaderStandard.Apply();

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
                        mesh.Draw(i);
                    }


                    //begin drawing text
                    device.Font.Begin();

                    //draw string
                    fpsCounter.Update();
                    device.Font.DrawString("FPS: " + fpsCounter.FPS, 0, 0);
                    device.Font.DrawString("Press N or D to switch mode: ", 0, 20);
                    device.Font.DrawString("Press A or S to change bias: " + bias, 0, 40);
                    //flush text to view
                    device.Font.End();
                    //present
                    device.Present();

                });

                //release resource
                mesh.Dispose();
                buffer.Dispose();
                shaderNormal.Dispose();
                shaderStandard.Dispose();
            }
        }
    }
}
