using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace SharpHelper
{
    /// <summary>
    /// Output buffer manager
    /// </summary>
    public class SharpOutputBuffer : IDisposable
    {
        /// <summary>
        /// Buffer pointer
        /// </summary>
        public Buffer11 Buffer { get; private set; }

        /// <summary>
        /// Device pointer
        /// </summary>
        public SharpDevice Device { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="size">Buffer size</param>
        public SharpOutputBuffer(SharpDevice device, int size)
        {
            Device = device;
            BufferDescription desc = new BufferDescription()
            {
                SizeInBytes = size,
                BindFlags = BindFlags.VertexBuffer | BindFlags.StreamOutput,
                Usage = ResourceUsage.Default,
            };
            this.Buffer = new Buffer11(Device.Device, desc);
        }

        /// <summary>
        /// Begin buffer
        /// </summary>
        public void Begin()
        {
            Device.DeviceContext.StreamOutput.SetTarget(this.Buffer, 0);
        }

        /// <summary>
        /// End rendering on this buffer
        /// </summary>
        public void End()
        {
            Device.DeviceContext.StreamOutput.SetTargets(new StreamOutputBufferBinding[] { new StreamOutputBufferBinding() });
        }


        /// <summary>
        /// Release resource
        /// </summary>
        public void Dispose()
        {
            if (this.Buffer != null)
                this.Buffer.Dispose();
        }

        /// <summary>
        /// Draw buffer
        /// </summary>
        /// <param name="vertexSize">Size of vertex in the buffer</param>
        public void Draw(int vertexSize)
        {
            Device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Buffer, vertexSize, 0));
            Device.DeviceContext.DrawAuto();
        }
    }
}
