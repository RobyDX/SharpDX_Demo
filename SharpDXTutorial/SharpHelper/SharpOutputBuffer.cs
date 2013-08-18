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
    /// Output buffer manager
    /// </summary>
    public class SharpOutputBuffer : IDisposable
    {
        //buffer
        private Buffer11 _buffer;

        /// <summary>
        /// Buffer pointer
        /// </summary>
        public Buffer11 Buffer
        {
            get { return _buffer; }
        }


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
            _buffer = new Buffer11(Device.Device, desc);
        }

        /// <summary>
        /// Begin buffer
        /// </summary>
        public void Begin()
        {
            Device.DeviceContext.StreamOutput.SetTarget(_buffer, 0);
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
            _buffer.Dispose();
        }

        /// <summary>
        /// Draw buffer
        /// </summary>
        /// <param name="vertexSize">Size of vertex in the buffer</param>
        public void Draw(int vertexSize)
        {
            Device.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_buffer, vertexSize, 0));
            Device.DeviceContext.DrawAuto();
        }


    }
}
