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
using SharpHelper.Skinning;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Tutorial19
{

    struct TessellationVertex
    {
        public Vector3 Position;
        public Vector2 Texture;

        public TessellationVertex(float x, float y, float z, float tu, float tv)
        {
            Position = new Vector3(x, y, z);
            Texture = new Vector2(tu, tv);
        }
    }

    struct SceneData
    {
        public Matrix Transform;
        public Vector4 ViewAt;
        public Vector4 LightDirection;
    };

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

            RenderForm form = new RenderForm();
            form.Text = "Tutorial 19: Adaptive Tesselation";
            SharpFPS fpsCounter = new SharpFPS();

            using (SharpDevice device = new SharpDevice(form))
            {
                InputElement[] description = new InputElement[]
                {
                    new InputElement("POSITION",0,SharpDX.DXGI.Format.R32G32B32_Float,0,0),
                    new InputElement("TEXCOORD",0,SharpDX.DXGI.Format.R32G32_Float,12,0),
                };

                SharpShader shader = new SharpShader(device, "../../Shader.hlsl",
                    new SharpShaderDescription()
                    {
                        VertexShaderFunction = "VSMain",
                        PixelShaderFunction = "PSMain",
                        DomainShaderFunction = "DSMain",
                        HullShaderFunction = "HSMain"
                    }, description);



                int seqX = 64;
                int seqY = 64;
                float size = 100;
                float sizeW = seqX * size;
                float sizeH = seqY * size;

                TessellationVertex[] vertices = new TessellationVertex[4356];
                int[] indices = new int[63504];
                int k = 0;
                for (int y = -1; y < seqY + 1; y++)
                {
                    for (int x = -1; x < seqX + 1; x++)
                    {
                        float vX = x * size - (seqX / 2.0F) * size;
                        float vY = y * size - (seqY / 2.0F) * size;
                        float vZ = 0;

                        TessellationVertex v = new TessellationVertex(vX, vY, vZ, x / 4.0F, y / 4.0F);
                        vertices[k] = v;
                        k++;
                    }
                }

                //indici
                k = 0;
                for (int y = 0; y < seqY - 1; y++)
                {
                    for (int x = 0; x < seqX - 1; x++)
                    {
                        int startX = x + 1;
                        int startY = y + 1;

                        for (int j = -1; j < 3; j++)
                        {
                            for (int i = -1; i < 3; i++)
                            {
                                indices[k] = (i + startX + (seqX + 2) * (j + startY));
                                k++;
                            }
                        }
                    }
                }

                SharpMesh mesh = SharpMesh.Create<TessellationVertex>(device, vertices, indices);

                string path = @"../../../Models/adaptive_tess/";

                ShaderResourceView diffuseMap = device.LoadTextureFromFile(path + "D.dds");
                ShaderResourceView normalMap = device.LoadTextureFromFile(path + "N.dds");
                ShaderResourceView heightMap = device.LoadTextureFromFile(path + "H.dds");

                Buffer11 buffer = shader.CreateBuffer<SceneData>();

                fpsCounter.Reset();

                float angle = 3.14F;
                float distance = 1200;
                float heightPos = 500;


                form.KeyDown += (sender, e) =>
                {
                    switch (e.KeyCode)
                    {
                        case Keys.A:
                            distance -= 5;
                            if (distance < 100) distance = 100;
                            break;
                        case Keys.Z:
                            distance += 5;
                            if (distance > 2000) distance = 2000;
                            break;
                        case Keys.Up:
                            heightPos--;
                            if (heightPos < 50) heightPos = 50;
                            break;
                        case Keys.Down:
                            heightPos++;
                            if (heightPos > 800) heightPos = 800;
                            break;
                        case Keys.Left:
                            angle -= 0.01F;
                            break;
                        case Keys.Right:
                            angle += 0.01F;
                            break;
                        case Keys.W:
                            device.SetWireframeRasterState();
                            break;
                        case Keys.S:
                            device.SetDefaultRasterState();
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
                    }

                    //apply states
                    device.UpdateAllStates();

                    //clear color
                    device.Clear(Color.CornflowerBlue);

                    //Set matrices
                    float ratio = (float)form.ClientRectangle.Width / (float)form.ClientRectangle.Height;

                    Vector3 vAt = new Vector3((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance, heightPos);
                    Vector3 vTo = new Vector3(0, 0, 0);
                    Vector3 vUp = new Vector3(0, 0, 1);

                    Matrix view = Matrix.LookAtLH(vAt, vTo, vUp);
                    Matrix proj = Matrix.PerspectiveFovLH(3.14F / 3.0F, ratio, 1.0F, 50000);

                    Vector3 lightDir = new Vector3(1, 0, -2);
                    lightDir.Normalize();

                    Matrix WVP = view * proj;
                    WVP.Transpose();

                    //update constant buffer
                    SceneData data = new SceneData()
                    {
                        Transform = WVP,
                        LightDirection = new Vector4(lightDir, 0),
                        ViewAt = new Vector4(vAt, 0)
                    };
                    device.UpdateData<SceneData>(buffer, data);

                    //pass constant buffer to shader
                    device.DeviceContext.VertexShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.PixelShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.HullShader.SetConstantBuffer(0, buffer);
                    device.DeviceContext.DomainShader.SetConstantBuffer(0, buffer);

                    //set map to shader
                    device.DeviceContext.DomainShader.SetShaderResource(0, heightMap);
                    device.DeviceContext.PixelShader.SetShaderResource(0, diffuseMap);
                    device.DeviceContext.PixelShader.SetShaderResource(1, normalMap);

                    //apply shader
                    shader.Apply();

                    //draw mesh
                    mesh.DrawPatch(SharpDX.Direct3D.PrimitiveTopology.PatchListWith16ControlPoints);

                    //begin drawing text
                    device.Font.Begin();

                    //draw string
                    fpsCounter.Update();
                    device.Font.DrawString("FPS: " + fpsCounter.FPS, 0, 0);

                    device.Font.DrawString("Presso Up,Down,Left,Right,A,Z to move camera", 0, 20);
                    device.Font.DrawString("Presso W and S to Switch to Wireframe", 0, 40);


                    //flush text to view
                    device.Font.End();
                    //present
                    device.Present();

                });
            }
        }


    }
}
