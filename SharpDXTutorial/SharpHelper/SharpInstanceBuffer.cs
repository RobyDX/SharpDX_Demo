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

namespace SharpHelper
{
    /// <summary>
    /// Helper for drawing Instance Buffer
    /// </summary>
    /// <typeparam name="T">Data Type</typeparam>
    public class SharpInstanceBuffer<T> where T : struct
    {
        /// <summary>
        /// Device Pointer
        /// </summary>
        public SharpDevice Device { get; private set; }

        /// <summary>
        /// Instance Buffer
        /// </summary>
        public Buffer11 InstanceBuffer
        {
            get { return _instanceBuffer; }
        }

        /// <summary>
        /// Number of instances inside buffer
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Size of each instance data
        /// </summary>
        public int Stride { get; private set; }


        private Buffer11 _instanceBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="data">Data to load inside</param>
        public SharpInstanceBuffer(SharpDevice device, T[] data)
        {
            _instanceBuffer = Buffer11.Create(device.Device, BindFlags.VertexBuffer, data);
            Device = device;
            Stride = Utilities.SizeOf<T>();
            Count = data.Length;
        }

        /// <summary>
        /// Draw instance Data in Vertex Buffer 0 and Index Buffer must be ready inside device context
        /// </summary>
        /// <param name="count">Number of instance to draw</param>
        /// <param name="indexCountPerInstance">Index count of each instance</param>
        /// <param name="startIndexLocation">Starting index of the current instance</param>
        public void DrawInstance(int count, int indexCountPerInstance, int startIndexLocation)
        {
            int c = Math.Min(count, Count);
            Device.DeviceContext.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(_instanceBuffer, Stride, 0));
            Device.DeviceContext.DrawIndexedInstanced(indexCountPerInstance, count, startIndexLocation, 0, 0);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _instanceBuffer.Dispose();
        }
    }
}
