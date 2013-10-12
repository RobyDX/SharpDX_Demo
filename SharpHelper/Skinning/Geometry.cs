using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace SharpHelper.Skinning
{
    /// <summary>
    /// Skinning data for Shader
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SkinShaderInformation
    {
        /// <summary>
        /// Transform matrix
        /// </summary>
        public Matrix Trasform;

        /// <summary>
        /// World Matrix
        /// </summary>
        public Matrix World;

    }

    /// <summary>
    /// Mesh for Skinning Rendering
    /// </summary>
    public class Geometry : IDisposable
    {
        private Buffer11 transformBuffer;
        private Buffer11 paletteBuffer;

        /// <summary>
        /// Device
        /// </summary>
        public SharpDevice Device { get; protected set; }

        /// <summary>
        /// Shader
        /// </summary>
        public SharpShader Shader { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Vertex Buffer
        /// </summary>
        public SharpDX.Direct3D11.Buffer VertexBuffer { get; private set; }

        /// <summary>
        /// Vertex Count
        /// </summary>
        public int VertexCount { get; private set; }

        /// <summary>
        /// Index Count
        /// </summary>
        public int IndexCount { get; private set; }

        /// <summary>
        /// Index Buffer
        /// </summary>
        public SharpDX.Direct3D11.Buffer IndexBuffer { get; private set; }

        /// <summary>
        /// Node
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// True for animated geometry
        /// </summary>
        public bool IsAnimated { get; private set; }

        /// <summary>
        /// Material
        /// </summary>
        public Material Material { get; private set; }
        
        //Constructor
        internal Geometry(SharpDevice device, ModelGeometry data, bool animated)
        {
            this.Name = data.Name;
            this.Device = device;

            //Data
            List<VertexFormat> vertices = new List<VertexFormat>(data.Vertices);

            List<int> indices = new List<int>(data.Indices);

            VertexCount = vertices.Count;
            IndexCount = indices.Count;

            //Vertex Buffer
            VertexBuffer = SharpDX.Direct3D11.Buffer.Create<VertexFormat>(Device.Device, BindFlags.VertexBuffer, vertices.ToArray());

            //Index Buffer
            IndexBuffer = SharpDX.Direct3D11.Buffer.Create<int>(Device.Device, BindFlags.IndexBuffer, indices.ToArray());


            Material = new Material(data.Material);

            IsAnimated = animated;


            transformBuffer = new Buffer11(Device.Device, Utilities.SizeOf<SkinShaderInformation>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            paletteBuffer = new Buffer11(Device.Device, Utilities.SizeOf<Matrix>() * 256, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        /// <summary>
        /// Prepare for rendering
        /// </summary>
        /// <param name="information">Rendering Information</param>
        public void Apply(SkinShaderInformation information)
        {
            Shader.Apply();
            if (IsAnimated)
            {
                SkinShaderInformation info = new SkinShaderInformation();
                info.World = Node.PreComputed * information.World;
                info.Trasform = information.Trasform;

                Device.DeviceContext.UpdateSubresource(ref info, transformBuffer);
                Device.DeviceContext.VertexShader.SetConstantBuffer(0, transformBuffer);


                Matrix[] m = Node.Skinning.GetPalette();

                Device.DeviceContext.UpdateSubresource(m, paletteBuffer);
                Device.DeviceContext.VertexShader.SetConstantBuffer(1, paletteBuffer);
            }
            else
            {
                Matrix currentMat = Node.GetNodeMatrix();

                SkinShaderInformation info = new SkinShaderInformation();

                info.World = currentMat * information.World;
                info.Trasform = currentMat * information.Trasform;

                Device.DeviceContext.UpdateSubresource(ref info, transformBuffer);
                Device.DeviceContext.VertexShader.SetConstantBuffer(0, transformBuffer);
            }


            Device.DeviceContext.PixelShader.SetShaderResource(0, Material.DiffuseTexture);
            Device.DeviceContext.PixelShader.SetShaderResource(1, Material.NormalTexture);


        }

        //Draw
        internal void Draw(DeviceContext context)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexFormat.Size, 0));
            context.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
            context.DrawIndexed(IndexCount, 0, 0);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
