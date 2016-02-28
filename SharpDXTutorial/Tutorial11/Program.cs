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


namespace Tutorial11
{
    static class Program
    {

        struct Data
        {
            public Matrix world;
            public Matrix viewProj;
            public Vector4 factor;
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

            int[] indices = new int[] 
            { 
                0,1,2,3,4,5
            };

            Vector3[] vertices = new Vector3[]{
                new Vector3(-50, 0, 50),
                new Vector3(50, 0, 50),
                new Vector3(50, 0, -50),
                new Vector3(-50, 0, 50),
                new Vector3(50, 0, -50),
                new Vector3(-50, 0, -50)
            };


            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 11: Tesselation";
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                
                //Init mesh
                SharpMesh mesh = SharpMesh.Create<Vector3>(device, vertices, indices);

                //Init shader
                SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription()
                    {
                        VertexShaderFunction = "VS",
                        PixelShaderFunction = "PS",
                        GeometryShaderFunction = "GS",
                        HullShaderFunction = "HS",
                        DomainShaderFunction = "DS"
                    },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    });

                //create constant buffer
                Buffer11 buffer = shader.CreateBuffer<Data>();

                fpsCounter.Reset();

                //tessellation value
                int nFactor = 1;
                form.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Up)
                        nFactor++;
                    if (e.KeyCode == Keys.Down && nFactor > 1)
                        nFactor--;
                    if (e.KeyCode == Keys.W)
                        device.SetWireframeRasterState();
                    if (e.KeyCode == Keys.S)
                        device.SetDefaultRasterState();
                };

                //main loop
                RenderLoop.Run(form, () =>
                {
                    //Resizing
                    if (device.MustResize)
                    {
                        device.Resize();
                    }

                    //clear color
                    device.Clear(Color.CornflowerBlue);
                    device.UpdateAllStates();

                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(MathUtil.Pi / 3.0F, ratio, 1, 1000);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 30, -80), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.RotationY(Environment.TickCount / 2000.0F);
                    Matrix WVP = world * view * projection;

                    device.UpdateData<Data>(buffer, new Data()
                    {
                        world = world,
                        viewProj = view * projection,
                        factor = new Vector4(nFactor, nFactor, 0, 0)
                    });
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.GeometryShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.HullShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.DomainShader.SetConstantBuffer(0, buffer);

                    shader.Apply();

                    //draw mesh as patch
                    mesh.DrawPatch(SharpDX.Direct3D.PrimitiveTopology.PatchListWith3ControlPoints);

                    //remove unused shader
                    shader.Clear();

                    //begin drawing text
                    device.Font.Begin();

                    //draw string
                    fpsCounter.Update();
                    device.Font.DrawString("FPS: " + fpsCounter.FPS, 0, 0);
                    device.Font.DrawString("Tessellation Factor: " + nFactor, 0, 30);
                    device.Font.DrawString("Press Up And Down to change Tessellation Factor,W and S to switch to wireframe ", 0, 60);

                    //flush text to view
                    device.Font.End();
                    //present
                    device.Present();
                });


                //release resource
                mesh.Dispose();
                buffer.Dispose();
                shader.Dispose();
            }

        }
    }
}
