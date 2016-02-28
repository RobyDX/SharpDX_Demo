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

namespace Tutorial17
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
            form.Text = "Tutorial 17: Query";
            //frame rate counter
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {

                //load model from wavefront obj file
                SharpMesh earth = SharpMesh.CreateFromObj(device, "../../../Models/planets/earth.obj");
                SharpMesh moon = SharpMesh.CreateFromObj(device, "../../../Models/planets/moon.obj");

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

                Query pipelineQuery = new Query(device.Device, new QueryDescription() { Flags = QueryFlags.None, Type = QueryType.PipelineStatistics });

                QueryDataPipelineStatistics stats = new QueryDataPipelineStatistics();


                //init frame counter
                fpsCounter.Reset();

                int lastX = 0;
                float currentAngle = 50;
                form.MouseMove += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        currentAngle += (lastX - e.X);
                    }
                    lastX = e.X;
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

                    //apply shader
                    shader.Apply();


                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, buffer);

                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1F, 1000.0F);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -250), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.Translation(0, 0, 200) * Matrix.RotationY(MathUtil.DegreesToRadians(currentAngle));

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

                    //draw mesh
                    moon.Begin();
                    for (int i = 0; i < moon.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, moon.SubSets[i].DiffuseMap);
                        moon.Draw(i);
                    }

                    //begin analizing
                    device.DeviceContext.Begin(pipelineQuery);

                    world = Matrix.RotationY(Environment.TickCount / 2000.0F);
                    sceneInformation = new Data()
                    {
                        world = world,
                        worldViewProjection = world * view * projection,
                        lightDirection = new Vector4(lightDirection, 1)
                    };

                    //write data inside constant buffer
                    device.UpdateData<Data>(buffer, sceneInformation);

                    //draw mesh
                    earth.Begin();
                    for (int i = 0; i < earth.SubSets.Count; i++)
                    {
                        device.DeviceContext.PixelShader.SetShaderResource(0, earth.SubSets[i].DiffuseMap);
                        earth.Draw(i);
                    }
                    //end analizing
                    device.DeviceContext.End(pipelineQuery);

                    //get result

                    while (!device.DeviceContext.GetData<QueryDataPipelineStatistics>(pipelineQuery, AsynchronousFlags.None, out stats))
                    {
                    }

                    //begin drawing text
                    device.Font.Begin();

                    //draw string
                    fpsCounter.Update();
                    device.Font.DrawString("Earth Stats : FPS: " + fpsCounter.FPS, 0, 0);

                    //print earth stats
                    device.Font.DrawString("Earth Stats : Use Mouse to Rotate Moon To Cover Earth ", 0, 30);
                    device.Font.DrawString(string.Format("Primitive Count: {0}", stats.IAPrimitiveCount), 0, 60);
                    device.Font.DrawString(string.Format("Vertex Count Count: {0}", stats.IAVerticeCount), 0, 90);
                    device.Font.DrawString(string.Format("Vertex Shader Execution: {0}", stats.VSInvocationCount), 0, 120);
                    device.Font.DrawString(string.Format("Pixel Shader Execution: {0}", stats.PSInvocationCount), 0, 150);


                    //flush text to view
                    device.Font.End();
                    //present
                    device.Present();




                });

                //release resource
                earth.Dispose();
                moon.Dispose();
                buffer.Dispose();
                pipelineQuery.Dispose();
            }
        }
    }
}
