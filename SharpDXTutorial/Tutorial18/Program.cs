using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpHelper;
using SharpHelper.Skinning;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Tutorial18
{
    static class Program
    {

        public struct Data
        {

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
            form.Text = "Tutorial 18: Skin Animation";
            SharpFPS fpsCounter = new SharpFPS();

            //number of cube
            int count = 1000;

            using (SharpDevice device = new SharpDevice(form))
            {
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                //Input layout for Skinning Mesh
                InputElement[] description = new InputElement[]
                {
                    new InputElement("POSITION",0,SharpDX.DXGI.Format.R32G32B32_Float,0,0),
                    new InputElement("NORMAL",0,SharpDX.DXGI.Format.R32G32B32_Float,12,0),
                    new InputElement("TEXCOORD",0,SharpDX.DXGI.Format.R32G32_Float,24,0),
                    new InputElement("BINORMAL",0,SharpDX.DXGI.Format.R32G32B32_Float,32,0),
                    new InputElement("TANGENT",0,SharpDX.DXGI.Format.R32G32B32_Float,44,0),
                    new InputElement("JOINT",0,SharpDX.DXGI.Format.R32G32B32A32_Float,56,0),
                    new InputElement("WEIGHT",0,SharpDX.DXGI.Format.R32G32B32A32_Float,72,0),
                };

                SharpShader staticShader = new SharpShader(device, "../../Basic.hlsl",
                    new SharpShaderDescription()
                    {
                        VertexShaderFunction = "VSMain",
                        PixelShaderFunction = "PSMain"
                    }, description);

                SharpShader skinShader = new SharpShader(device, "../../BasicSkin.hlsl",
                    new SharpShaderDescription()
                    {
                        VertexShaderFunction = "VSMain",
                        PixelShaderFunction = "PSMain"
                    }, description);


                Buffer11 lightBuffer = skinShader.CreateBuffer<Vector4>();

                string path = @"../../../Models/Troll/";

                SharpModel model = new SharpModel(device,
                    ColladaImporter.Import(path + "troll.dae"));

                foreach (Geometry g in model.Geometries)
                {
                    if (g.IsAnimated)
                        g.Shader = skinShader;
                    else
                        g.Shader = staticShader;

                    if (!string.IsNullOrEmpty(g.Material.DiffuseTextureName))
                    {
                        g.Material.DiffuseTexture = ShaderResourceView.FromFile(device.Device, path + g.Material.DiffuseTextureName);

                        g.Material.NormalTextureName = Path.GetFileNameWithoutExtension(g.Material.DiffuseTextureName) + "N.dds";

                        g.Material.NormalTexture = ShaderResourceView.FromFile(device.Device, path + g.Material.NormalTextureName);
                    }
                }

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

                int lastTick = Environment.TickCount;

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



                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 10000);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, -100, 50), new Vector3(0, 0, 50), Vector3.UnitZ);
                    Matrix world = Matrix.Identity;

                    float angle = Environment.TickCount / 2000.0F;
                    Vector3 light = new Vector3((float)Math.Sin(angle), (float)Math.Cos(angle), 0);
                    light.Normalize();
                    device.UpdateData<Vector4>(lightBuffer, new Vector4(light, 1));
                    device.DeviceContext.VertexShader.SetConstantBuffer(2, lightBuffer);



                    float animationTime = (Environment.TickCount - lastTick) / 1000.0F;

                    if (animationTime >= model.Animations.First().Duration)
                    {
                        lastTick = Environment.TickCount;
                        animationTime = 0;
                    }

                    model.SetTime(animationTime);

                    model.Draw(device, new SkinShaderInformation()
                    {
                        Trasform = world * view * projection,
                        World = world
                    });

                    font.Begin();

                    //draw string
                    fpsCounter.Update();
                    font.DrawString("FPS: " + fpsCounter.FPS, 0, 0, Color.White);
                    font.DrawString("Skinning Animation With Collada", 0, 30, Color.White);

                    //flush text to view
                    font.End();
                    //present
                    device.Present();


                });

                //release resource
                font.Dispose();
            }
        }
    }
}
