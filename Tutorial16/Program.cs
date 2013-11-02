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

namespace Tutorial16
{
    static class Program
    {
        //Data
        struct Data
        {
            public Matrix world;
            public Matrix worldViewProjection;
            public Vector4 lightDirection;
            public Vector4 cameraPosition;

            public Matrix mat1;
            public Matrix mat2;
            public Matrix mat3;
            public Matrix mat4;
            public Matrix mat5;
            public Matrix mat6;
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
            form.Text = "Tutorial 16: Environment Mapping";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //load font
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                //load model from wavefront obj file
                SharpMesh teapot = SharpMesh.CreateFromObj(device, "../../../Models/teapot.obj");

                //init shader
                SharpShader cubeMapPass = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", GeometryShaderFunction = "GS", PixelShaderFunction = "PS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                    });

                //second pass
                SharpShader standard = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS_SECOND", PixelShaderFunction = "PS_SECOND" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                    });


                //second pass
                SharpShader reflection = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS_SECOND", PixelShaderFunction = "PS_REFLECTION" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
                    });

                //render target
                SharpCubeTarget cubeTarget = new SharpCubeTarget(device, 512, Format.R8G8B8A8_UNorm);

                //init constant buffer
                Buffer11 dataConstantBuffer = cubeMapPass.CreateBuffer<Data>();

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


                    //MATRICES

                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    //90° degree with 1 ratio
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 2.0F, 1, 1F, 10000.0F);

                    //camera
                    Vector3 from = new Vector3(0, 30, 70);
                    Vector3 to = new Vector3(0, 0, 0);

                    Matrix view = Matrix.LookAtLH(from, to, Vector3.UnitY);
                    Matrix world = Matrix.Translation(0, 0, 50) * Matrix.RotationY(Environment.TickCount / 1000.0F);

                    //light direction
                    Vector3 lightDirection = new Vector3(0.5f, 0, -1);
                    lightDirection.Normalize();

                    //six axis 
                    Matrix view1 = Matrix.LookAtLH(new Vector3(), new Vector3(1, 0, 0), Vector3.UnitY);
                    Matrix view2 = Matrix.LookAtLH(new Vector3(), new Vector3(-1, 0, 0), Vector3.UnitY);
                    Matrix view3 = Matrix.LookAtLH(new Vector3(), new Vector3(0, 1, 0), -Vector3.UnitZ);
                    Matrix view4 = Matrix.LookAtLH(new Vector3(), new Vector3(0, -1, 0), Vector3.UnitZ);
                    Matrix view5 = Matrix.LookAtLH(new Vector3(), new Vector3(0, 0, 1), Vector3.UnitY);
                    Matrix view6 = Matrix.LookAtLH(new Vector3(), new Vector3(0, 0, -1), Vector3.UnitY);



                    //BEGIN RENDERING TO CUBE TEXTURE 

                    cubeTarget.Apply();
                    cubeTarget.Clear(Color.CornflowerBlue);


                    Data sceneInformation = new Data()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1),
                        cameraPosition = new Vector4(from, 1),
                        mat1 = world * view1 * projection,
                        mat2 = world * view2 * projection,
                        mat3 = world * view3 * projection,
                        mat4 = world * view4 * projection,
                        mat5 = world * view5 * projection,
                        mat6 = world * view6 * projection
                    };
                    //write data inside constant buffer
                    device.UpdateData<Data>(dataConstantBuffer, sceneInformation);


                    //apply shader
                    cubeMapPass.Apply();

                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, dataConstantBuffer);
                    device.DeviceContext.GeometryShader.SetConstantBuffer(0, dataConstantBuffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, dataConstantBuffer);

                    //draw mesh
                    teapot.Begin();
                    for (int i = 0; i < teapot.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
                        teapot.Draw(i);
                    }


                    //RENDERING TO DEVICE

                    //Set original targets
                    device.SetDefaultTargets();

                    //clear color
                    device.Clear(Color.Blue);

                    //apply shader
                    standard.Apply();


                    //set target
                    device.DeviceContext.PixelShader.SetShaderResource(1, cubeTarget.Resource);

                    
                    projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 10000.0F);
                    sceneInformation = new Data()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1),
                        cameraPosition = new Vector4(from, 1),
                        mat1 = world * view1 * projection,
                        mat2 = world * view2 * projection,
                        mat3 = world * view3 * projection,
                        mat4 = world * view4 * projection,
                        mat5 = world * view5 * projection,
                        mat6 = world * view6 * projection
                    };
                    //write data inside constant buffer
                    device.UpdateData<Data>(dataConstantBuffer, sceneInformation);

                    //room
                    teapot.Begin();
                    for (int i = 0; i < teapot.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
                        teapot.Draw(i);
                    }



                    //apply shader
                    reflection.Apply();


                    sceneInformation = new Data()
                    {
                        world = Matrix.Identity,
                        worldViewProjection = view * projection,
                        lightDirection = new Vector4(lightDirection, 1),
                        cameraPosition = new Vector4(from, 1),
                        mat1 = Matrix.Identity,
                        mat2 = Matrix.Identity,
                        mat3 = Matrix.Identity,
                        mat4 = Matrix.Identity,
                        mat5 = Matrix.Identity,
                        mat6 = Matrix.Identity
                    };
                    //write data inside constant buffer
                    device.UpdateData<Data>(dataConstantBuffer, sceneInformation);

                    //draw mesh
                    teapot.Begin();
                    for (int i = 0; i < teapot.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, teapot.SubSets[i].DiffuseMap);
                        teapot.Draw(i);
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
                teapot.Dispose();
                dataConstantBuffer.Dispose();
                
                cubeMapPass.Dispose();
                standard.Dispose();
                reflection.Dispose();

                cubeTarget.Dispose();
                


            }
        }
    }
}
