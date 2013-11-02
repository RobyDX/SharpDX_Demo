using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace SharpHelper
{
    /// <summary>
    /// Contain Render Targets
    /// </summary>
    public class SharpRenderTarget : IDisposable
    {
        /// <summary>
        /// Device pointer
        /// </summary>
        public SharpDevice Device { get; private set; }

        /// <summary>
        /// Render Target
        /// </summary>
        public RenderTargetView Target { get; private set; }

        /// <summary>
        /// Depth Buffer for Render Target
        /// </summary>
        public DepthStencilView Zbuffer { get; private set; }

        /// <summary>
        /// Resource connected to Render Target
        /// </summary>
        public ShaderResourceView Resource { get; private set; }

        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        public SharpRenderTarget(SharpDevice device, int width, int height, Format format)
        {
            Device = device;
            Height = height;
            Width = width;

            using (Texture2D target = new Texture2D(device.Device, new Texture2DDescription()
            {
                Format = format,
                Width = width,
                Height = height,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            }))
            {

                this.Target = new RenderTargetView(device.Device, target);
                this.Resource = new ShaderResourceView(device.Device, target);
            }

            using (Texture2D zBufferTexture = new Texture2D(Device.Device, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            }))
            {
                // Create the depth buffer view
                this.Zbuffer = new DepthStencilView(Device.Device, zBufferTexture);
            }
        }

        /// <summary>
        /// Apply Render Target To Device Context
        /// </summary>
        public void Apply()
        {
            Device.DeviceContext.Rasterizer.SetViewport(0, 0, Width, Height);
            Device.DeviceContext.OutputMerger.SetTargets(this.Zbuffer, this.Target);
        }

        /// <summary>
        /// Dispose resource
        /// </summary>
        public void Dispose()
        {
            if (this.Resource != null)
                this.Resource.Dispose();
            if (this.Target != null)
                this.Target.Dispose();
            if (this.Zbuffer != null)
                this.Zbuffer.Dispose();
        }

        /// <summary>
        /// Clear backbuffer and zbuffer
        /// </summary>
        /// <param name="color">background color</param>
        public void Clear(Color4 color)
        {
            Device.DeviceContext.ClearRenderTargetView(this.Target, color);
            Device.DeviceContext.ClearDepthStencilView(this.Zbuffer, DepthStencilClearFlags.Depth, 1.0F, 0);
        }
    }
}
