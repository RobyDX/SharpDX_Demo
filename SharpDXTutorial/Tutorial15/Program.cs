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

namespace Tutorial15
{
    static class Program
    {

        struct ToonData
        {
            public Matrix world;
            public Matrix worldViewProjection;
            public Vector4 lightDirection;
            public Vector4 cameraPosition;
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
            form.Text = "Tutorial 15: Toon Shading With Multiple Render Target";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //load model from wavefront obj file
                SharpMesh dogMesh = SharpMesh.CreateFromObj(device, "../../../Models/dog/dog.obj");

                //quad
                SharpMesh quad = SharpMesh.CreateQuad(device);

                //init shader
                SharpShader firstPass = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                    });

                //second pass
                SharpShader secondPass = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS_QUAD", PixelShaderFunction = "PS_QUAD" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    });

                //render target
                SharpRenderTarget target = new SharpRenderTarget(device, form.ClientSize.Width, form.ClientSize.Height, Format.R8G8B8A8_UNorm);
                //render target
                SharpRenderTarget target2 = new SharpRenderTarget(device, form.ClientSize.Width, form.ClientSize.Height, Format.R8G8B8A8_UNorm);


                //toon texture
                ShaderResourceView bandTexture = device.LoadTextureFromFile("../../../Models/band.bmp");

                //init constant buffer
                Buffer11 toonConstantBuffer = firstPass.CreateBuffer<ToonData>();

                //init frame counter
                fpsCounter.Reset();


                //main loop
                RenderLoop.Run(form, () =>
                {
                    //Resizing
                    if (device.MustResize)
                    {
                        device.Resize();

                        //render target
                        target.Dispose();
                        target = new SharpRenderTarget(device, form.ClientSize.Width, form.ClientSize.Height, Format.R8G8B8A8_UNorm);
                        

                        //render target
                        target2.Dispose();
                        target2 = new SharpRenderTarget(device, form.ClientSize.Width, form.ClientSize.Height, Format.R8G8B8A8_UNorm);

                    }

                    //apply states
                    device.UpdateAllStates();


                    //BEGIN RENDERING TO TEXTURE (MULTIPLE)

                    device.ApplyMultipleRenderTarget(target, target2);


                    target.Clear(Color.CornflowerBlue);
                    target2.Clear(Color.Black);


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


                    ToonData sceneInformation = new ToonData()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1),
                        cameraPosition = new Vector4(from, 1)
                    };


                    //apply shader
                    firstPass.Apply();

                    //set toon band texture
                    device.DeviceContext.PixelShader.SetShaderResource(1, bandTexture);

                    //write data inside constant buffer
                    device.UpdateData<ToonData>(toonConstantBuffer, sceneInformation);

                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, toonConstantBuffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, toonConstantBuffer);

                    //draw mesh
                    dogMesh.Begin();
                    for (int i = 0; i < dogMesh.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, dogMesh.SubSets[i].DiffuseMap);
                        dogMesh.Draw(i);
                    }



                    //RENDERING TO DEVICE

                    //Set original targets
                    device.SetDefaultTargers();

                    //apply shader
                    secondPass.Apply();


                    //clear color
                    device.Clear(Color.Black);

                    secondPass.Apply();

                    //set target
                    device.DeviceContext.PixelShader.SetShaderResource(0, target.Resource);
                    device.DeviceContext.PixelShader.SetShaderResource(1, target2.Resource);

                    quad.Draw();


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


                //release resource
                dogMesh.Dispose();
                quad.Dispose();
                toonConstantBuffer.Dispose();
                firstPass.Dispose();
                secondPass.Dispose();
                bandTexture.Dispose();
                target.Dispose();
                target2.Dispose();

            }
        }
    }
}
