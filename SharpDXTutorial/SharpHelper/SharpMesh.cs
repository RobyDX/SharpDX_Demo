using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;

using Buffer11 = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.IO;


namespace SharpHelper
{
    /// <summary>
    /// To Render Static Object
    /// </summary>
    public class SharpMesh : IDisposable
    {

        /// <summary>
        /// Device pointer
        /// </summary>
        public SharpDevice Device { get; private set; }

        /// <summary>
        /// Vertex Buffer
        /// </summary>
        public Buffer11 VertexBuffer { get; private set; }

        /// <summary>
        /// Index Buffer
        /// </summary>
        public Buffer11 IndexBuffer { get; private set; }

        /// <summary>
        /// Vertex Size
        /// </summary>
        public int VertexSize { get; private set; }

        /// <summary>
        /// Mesh Parts
        /// </summary>
        public List<SharpSubSet> SubSets { get; private set; }

        private SharpMesh(SharpDevice device)
        {
            Device = device;
            SubSets = new List<SharpSubSet>();
        }


        /// <summary>
        /// Draw Mesh
        /// </summary>
        public void Draw()
        {
            Device.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            Device.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            Device.DeviceContext.DrawIndexed(SubSets[0].IndexCount, 0, 0);
        }


        /// <summary>
        /// Draw Mesh
        /// </summary>
        /// <param name="subset">Subsets</param>
        public void Draw(int subset)
        {
            Device.DeviceContext.DrawIndexed(SubSets[subset].IndexCount, SubSets[subset].StartIndex, 0);
        }

        /// <summary>
        /// Create From Vertices and Indices array
        /// </summary>
        /// <typeparam name="VType">Vertex Type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Mesh</returns>
        public static SharpMesh Create<VType>(SharpDevice device, VType[] vertices, int[] indices) where VType : struct
        {
            SharpMesh mesh = new SharpMesh(device);
            mesh.VertexBuffer = Buffer11.Create<VType>(device.Device, BindFlags.VertexBuffer, vertices);
            mesh.IndexBuffer = Buffer11.Create(device.Device, BindFlags.IndexBuffer, indices);
            mesh.VertexSize = SharpDX.Utilities.SizeOf<VType>();
            mesh.SubSets.Add(new SharpSubSet()
            {
                DiffuseColor = new Vector4(1, 1, 1, 1),
                IndexCount = indices.Count()
            });
            return mesh;
        }

        /// <summary>
        /// Create a mesh from wavefront obj file format
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Filename</param>
        /// <returns>Mesh</returns>
        public static SharpMesh CreateFromObj(SharpDevice device, string filename)
        {
            SharpMesh mesh = new SharpMesh(device);

            WaveFrontModel[] modelParts = WaveFrontModel.CreateFromObj(filename);
            mesh.Device = device;
            mesh.SubSets = new List<SharpSubSet>();

            List<StaticVertex> vertices = new List<StaticVertex>();
            List<int> indices = new List<int>();

            int vcount = 0;
            int icount = 0;
            foreach (WaveFrontModel model in modelParts)
            {
                vertices.AddRange(model.VertexData);
                indices.AddRange(model.IndexData.Select(i => i + vcount));

                var mate = model.MeshMaterial.First();


                ShaderResourceView tex = null;
                if (!string.IsNullOrEmpty(mate.DiffuseMap))
                {
                    string textureFile = System.IO.Path.GetDirectoryName(filename) + "\\" + System.IO.Path.GetFileName(mate.DiffuseMap);
                    tex = ShaderResourceView.FromFile(device.Device, textureFile);
                }

                mesh.SubSets.Add(new SharpSubSet()
                {
                    IndexCount = model.IndexData.Count,
                    StartIndex = icount,
                    DiffuseMap = tex
                });



                vcount += model.VertexData.Count;
                icount += model.IndexData.Count;
            }

            mesh.VertexBuffer = Buffer11.Create<StaticVertex>(device.Device, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer11.Create(device.Device, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = SharpDX.Utilities.SizeOf<StaticVertex>();

            return mesh;
        }


        /// <summary>
        /// Create a mesh from wavefront obj file format using Tangent and Binormal vertex format
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Filename</param>
        /// <returns>Mesh</returns>
        public static SharpMesh CreateNormalMappedFromObj(SharpDevice device, string filename)
        {
            SharpMesh mesh = new SharpMesh(device);

            WaveFrontModel[] modelParts = WaveFrontModel.CreateFromObj(filename);
            mesh.Device = device;
            mesh.SubSets = new List<SharpSubSet>();

            List<TangentVertex> vertices = new List<TangentVertex>();
            List<int> indices = new List<int>();

            int vcount = 0;
            int icount = 0;
            foreach (WaveFrontModel model in modelParts)
            {
                vertices.AddRange(model.TangentData);
                indices.AddRange(model.IndexData.Select(i => i + vcount));

                var mate = model.MeshMaterial.First();


                ShaderResourceView tex = null;
                ShaderResourceView ntex = null;

                if (!string.IsNullOrEmpty(mate.DiffuseMap))
                {
                    string textureFile = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(mate.DiffuseMap);
                    tex = ShaderResourceView.FromFile(device.Device, textureFile);

                    string normalMap = Path.GetDirectoryName(textureFile) + "\\" + Path.GetFileNameWithoutExtension(textureFile) + "N" + Path.GetExtension(textureFile);
                    ntex = ShaderResourceView.FromFile(device.Device, normalMap);
                }

                mesh.SubSets.Add(new SharpSubSet()
                {
                    IndexCount = model.IndexData.Count,
                    StartIndex = icount,
                    DiffuseMap = tex,
                    NormalMap = ntex
                });

                vcount += model.VertexData.Count;
                icount += model.IndexData.Count;
            }

            mesh.VertexBuffer = Buffer11.Create<TangentVertex>(device.Device, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer11.Create(device.Device, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = SharpDX.Utilities.SizeOf<TangentVertex>();

            return mesh;
        }

        /// <summary>
        /// Create a quad for Multiple Render Target
        /// </summary>
        /// <param name="device">Device</param>
        /// <returns>Mesh</returns>
        public static SharpMesh CreateQuad(SharpDevice device)
        {
            Vector3[] vertices = new Vector3[] 
            { 
                new Vector3(-1, 1, 0), 
                new Vector3(-1, -1, 0), 
                new Vector3(1, 1, 0), 
                new Vector3(1, -1, 0) 
            };

            int[] indices = new int[] { 0, 2, 1, 2, 3, 1 };
            SharpMesh mesh = new SharpMesh(device);
            mesh.VertexBuffer = Buffer11.Create<Vector3>(device.Device, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer11.Create(device.Device, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = SharpDX.Utilities.SizeOf<Vector3>();

            mesh.SubSets.Add(new SharpSubSet()
            {
                DiffuseColor = new Vector4(1, 1, 1, 1),
                IndexCount = indices.Count()
            });

            return mesh;
        }

        /// <summary>
        /// Set all buffer and topology property to speed up rendering
        /// </summary>
        public void Begin()
        {
            Device.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            Device.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
        }

        /// <summary>
        /// Draw all vertices as points
        /// </summary>
        /// <param name="count"></param>
        public void DrawPoints(int count = int.MaxValue)
        {
            Device.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            Device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            Device.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            Device.DeviceContext.DrawIndexed(Math.Min(count, SubSets[0].IndexCount), 0, 0);
        }

        /// <summary>
        /// Draw patch
        /// </summary>
        /// <param name="topology">Patch Topology type</param>
        public void DrawPatch(PrimitiveTopology topology)
        {
            Device.DeviceContext.InputAssembler.PrimitiveTopology = topology;
            Device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            Device.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            Device.DeviceContext.DrawIndexed(SubSets[0].IndexCount, 0, 0);
        }

        /// <summary>
        /// Release resource
        /// </summary>
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            foreach (var s in SubSets)
            {
                if (s.DiffuseMap != null)
                    s.DiffuseMap.Dispose();

                if (s.NormalMap != null)
                    s.NormalMap.Dispose();
            }
        }
    }
}
