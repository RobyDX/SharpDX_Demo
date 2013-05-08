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

namespace Tutorial14
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
            form.Text = "Tutorial 14: Output Stream";
            SharpFPS fpsCounter = new SharpFPS();


            using (SharpDevice device = new SharpDevice(form))
            {


                //Init font
                SharpBatch font = new SharpBatch(device, "textfont.dds");

                //Init Mesh
                SharpMesh mesh = SharpMesh.Create<TexturedVertex>(device, vertices, indices);

                //Init shader from file
                SharpShader shader = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS", PixelShaderFunction = "PS" },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
                    });

                //output stream element declaration
                StreamOutputElement[] soDeclaration = new StreamOutputElement[]
                {
                    new StreamOutputElement(){SemanticName="POSITION",ComponentCount=3},
                    new StreamOutputElement(){SemanticName="TEXCOORD",ComponentCount=2}
                };
                const int streamOutputVertexSize = 20; //bytes (3 floats for position, 2 floats for texcoord)


                //output shader
                SharpShader shaderOutput = new SharpShader(device, "../../HLSL.txt",
                    new SharpShaderDescription() { VertexShaderFunction = "VS_O", GeometryShaderFunction = "GS_O", GeometrySO = soDeclaration },
                    new InputElement[] {  
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
                    });

                //declare 2 output buffer to switch between them
                //one will contain the source and the other will be the target of the rendeinrg
                SharpOutputBuffer outputBufferA = new SharpOutputBuffer(device, 100000);
                SharpOutputBuffer outputBufferB = new SharpOutputBuffer(device, 100000);


                //Create constant buffer
                Buffer11 buffer = shader.CreateBuffer<Matrix>();

                //Create texture from file
                ShaderResourceView texture = ShaderResourceView.FromFile(device.Device, "../../texture.bmp");


                fpsCounter.Reset();

                //for updating
                bool update = true;
                Vector3 nextPosition = new Vector3();


                form.KeyUp += (sender, e) =>
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Up:
                            update = true;
                            nextPosition += new Vector3(0, 10, 0);
                            break;
                        case Keys.Down:
                            update = true;
                            nextPosition += new Vector3(0, -10, 0);
                            break;
                        case Keys.A:
                            update = true;
                            nextPosition += new Vector3(-10, 0, 0);
                            break;
                        case Keys.D:
                            update = true;
                            nextPosition += new Vector3(10, 0, 0);
                            break;
                        case Keys.W:
                            update = true;
                            nextPosition += new Vector3(0, 0, 10);
                            break;
                        case Keys.S:
                            update = true;
                            nextPosition += new Vector3(0, 0, -10);
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




                    //apply constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);

                    //set texture
                    device.DeviceContext.PixelShader.SetShaderResource(0, texture);

                    //update output buffer only when needed
                    if (update)
                    {
                        //switch buffer
                        var t = outputBufferA;
                        outputBufferA = outputBufferB;
                        outputBufferB = t;

                        //apply shader
                        shaderOutput.Apply();

                        //start drawing on output buffer
                        outputBufferA.Begin();

                        //draw the other output buffer as source
                        device.UpdateData<Matrix>(buffer, Matrix.Identity);
                        outputBufferB.Draw(streamOutputVertexSize);


                        //draw the mesh to add it to buffer
                        device.UpdateData<Matrix>(buffer, Matrix.Translation(nextPosition));
                        //draw mesh
                        mesh.Draw();

                        //end rendering on buffer
                        outputBufferA.End();

                        //stop updating
                        update = false;
                    }




                    //set transformation matrix
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;
                    Matrix projection = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1, 1000);
                    Matrix view = Matrix.LookAtLH(new Vector3(0, 50, -100), new Vector3(), Vector3.UnitY);
                    Matrix world = Matrix.Identity;
                    Matrix WVP = world * view * projection;
                    device.UpdateData<Matrix>(buffer, WVP);

                    //stream
                    shader.Apply();

                    //draw all buffer every frame
                    outputBufferA.Draw(streamOutputVertexSize);

                    //begin drawing text
                    font.Begin();

                    //draw string
                    fpsCounter.Update();
                    font.DrawString("FPS: " + fpsCounter.FPS, 0, 0, Color.White);
                    font.DrawString("Press WASD, Up, Down to move cube", 0, 30, Color.White);

                    //flush text to view
                    font.End();
                    //present
                    device.Present();
                });

                //release resource
                font.Dispose();
                mesh.Dispose();
                buffer.Dispose();
                texture.Dispose();
            }
        }
    }
}
