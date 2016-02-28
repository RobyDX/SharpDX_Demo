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


namespace Tutorial5
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

            //Init textured cube
            int[] indices = new int[]
            {
                0,1,2,0,2,3,
                4,6,5,4,7,6,
                8,9,10,8,10,11,
                12,14,13,12,15,14,
                16,18,17,16,19,18,
                20,21,22,20,22,23
            };

            TexturedVertex[] vertices = new[]
            {
                ////TOP
                new TexturedVertex(new Vector3(-5,5,5),new Vector2(1,1)),
                new TexturedVertex(new Vector3(5,5,5),new Vector2(0,1)),
                new TexturedVertex(new Vector3(5,5,-5),new Vector2(0,0)),
                new TexturedVertex(new Vector3(-5,5,-5),new Vector2(1,0)),
                //BOTTOM
                new TexturedVertex(new Vector3(-5,-5,5),new Vector2(1,1)),
                new TexturedVertex(new Vector3(5,-5,5),new Vector2(0,1)),
                new TexturedVertex(new Vector3(5,-5,-5),new Vector2(0,0)),
                new TexturedVertex(new Vector3(-5,-5,-5),new Vector2(1,0)),
                //LEFT
                new TexturedVertex(new Vector3(-5,-5,5),new Vector2(0,1)),
                new TexturedVertex(new Vector3(-5,5,5),new Vector2(0,0)),
                new TexturedVertex(new Vector3(-5,5,-5),new Vector2(1,0)),
                new TexturedVertex(new Vector3(-5,-5,-5),new Vector2(1,1)),
                //RIGHT
                new TexturedVertex(new Vector3(5,-5,5),new Vector2(1,1)),
                new TexturedVertex(new Vector3(5,5,5),new Vector2(1,0)),
                new TexturedVertex(new Vector3(5,5,-5),new Vector2(0,0)),
                new TexturedVertex(new Vector3(5,-5,-5),new Vector2(0,1)),
                //FRONT
                new TexturedVertex(new Vector3(-5,5,5),new Vector2(1,0)),
                new TexturedVertex(new Vector3(5,5,5),new Vector2(0,0)),
                new TexturedVertex(new Vector3(5,-5,5),new Vector2(0,1)),
                new TexturedVertex(new Vector3(-5,-5,5),new Vector2(1,1)),
                //BACK
                new TexturedVertex(new Vector3(-5,5,-5),new Vector2(0,0)),
                new TexturedVertex(new Vector3(5,5,-5),new Vector2(1,0)),
                new TexturedVertex(new Vector3(5,-5,-5),new Vector2(1,1)),
                new TexturedVertex(new Vector3(-5,-5,-5),new Vector2(0,1))
            };





            //render form
            RenderForm form = new RenderForm();
            form.Text = "Tutorial 5: Texture";
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {
                //Init Mesh
                SharpMesh mesh = SharpMesh.Create<TexturedVertex>(device, vertices, indices);

                //Init shader from file
                SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
                    });

                //Create constant buffer
                Buffer11 buffer = shader.CreateBuffer<Matrix>();
                //Create texture from file
                ShaderResourceView texture = device.LoadTextureFromFile("../../texture.bmp");

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


                    //apply shader
                    shader.Apply();

                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                    //set texture
                    device.DeviceContext.PixelShader.SetShaderResource(0, texture);

                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 1000);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 10, -30), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.RotationY(Environment.TickCount / 1000.0F);
                    Matrix WVP = world * view * projection;
                    device.UpdateData<Matrix>(buffer, WVP);

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

                //release resource
                mesh.Dispose();
                buffer.Dispose();
                texture.Dispose();
            }
        }
    }
}