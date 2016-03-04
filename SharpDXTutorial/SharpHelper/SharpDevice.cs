using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

//
using Device11 = SharpDX.Direct3D11.Device;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace SharpHelper
{

    /// <summary>
    /// Encapsulate All DirectX Elements
    /// </summary>
    public class SharpDevice : IDisposable
    {
        //private 
        private Device11 _device;
        private DeviceContext _deviceContext;
        private SwapChain _swapchain;

        private RenderTargetView _backbufferView;
        private DepthStencilView _zbufferView;

        private RasterizerState _rasterState;
        private BlendState _blendState;
        private DepthStencilState _depthState;
        private SamplerState _samplerState;

        /// <summary>
        /// Device
        /// </summary>
        public Device11 Device { get { return _device; } }

        /// <summary>
        /// Device Context
        /// </summary>
        public DeviceContext DeviceContext { get { return _deviceContext; } }

        /// <summary>
        /// Swapchain
        /// </summary>
        public SwapChain SwapChain { get { return _swapchain; } }

        /// <summary>
        /// Rendering Form
        /// </summary>
        public RenderForm View { get; private set; }

        /// <summary>
        /// Indicate that device must be resized
        /// </summary>
        public bool MustResize { get; private set; }

        /// <summary>
        /// View to BackBuffer
        /// </summary>
        public RenderTargetView BackBufferView { get { return _backbufferView; } }

        /// <summary>
        /// View to Depth Buffer
        /// </summary>
        public DepthStencilView ZBufferView { get { return _zbufferView; } }

        /// <summary>
        /// Font Batch
        /// </summary>
        public Sharp2D Font { get; private set; }

        /// <summary>
        /// Init all object to start rendering
        /// </summary>
        /// <param name="form">Rendering form</param>
        /// <param name="debug">Active the debug mode</param>
        public SharpDevice(RenderForm form, bool debug = false)
        {
            View = form;
            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,//buffer count
                ModeDescription = new ModeDescription(View.ClientSize.Width, View.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),//sview
                IsWindowed = true,
                OutputHandle = View.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            FeatureLevel[] levels = new FeatureLevel[] { FeatureLevel.Level_11_1 };

            //create device and swapchain
            DeviceCreationFlags flag = DeviceCreationFlags.None | DeviceCreationFlags.BgraSupport;
            if (debug)
                flag = DeviceCreationFlags.Debug;

            Device11.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, flag, levels, desc, out _device, out _swapchain);

            //get context to device
            _deviceContext = Device.ImmediateContext;


            //Ignore all windows events
            var factory = SwapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(View.Handle, WindowAssociationFlags.IgnoreAll);

            //Setup handler on resize form
            View.UserResized += (sender, args) => MustResize = true;

            //Set Default State
            SetDefaultRasterState();
            SetDefaultDepthState();
            SetDefaultBlendState();
            SetDefaultSamplerState();

            Font = new Sharp2D(this);

            //Resize all items
            Resize();
        }





        /// <summary>
        /// Create and Resize all items
        /// </summary>
        public void Resize()
        {
            // Dispose all previous allocated resources
            Font.Release();
            Utilities.Dispose(ref _backbufferView);
            Utilities.Dispose(ref _zbufferView);
            

            if (View.ClientSize.Width == 0 || View.ClientSize.Height == 0)
                return;

            // Resize the backbuffer
            SwapChain.ResizeBuffers(1, View.ClientSize.Width, View.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            var _backBufferTexture = SwapChain.GetBackBuffer<Texture2D>(0);

            //update font
            Font.UpdateResources(_backBufferTexture);

            // Backbuffer
            _backbufferView = new RenderTargetView(Device, _backBufferTexture);
            _backBufferTexture.Dispose();

            // Depth buffer

            var _zbufferTexture = new Texture2D(Device, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = View.ClientSize.Width,
                Height = View.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });


            // Create the depth buffer view
            _zbufferView = new DepthStencilView(Device, _zbufferTexture);
            _zbufferTexture.Dispose();

            SetDefaultTargers();
            
            // End resize
            MustResize = false;
        }

        /// <summary>
        /// Set default render and depth buffer inside device context
        /// </summary>
        public void SetDefaultTargers()
        {
            // Setup targets and viewport for rendering
            DeviceContext.Rasterizer.SetViewport(0, 0, View.ClientSize.Width, View.ClientSize.Height);
            DeviceContext.OutputMerger.SetTargets(_zbufferView, _backbufferView);
        }

        /// <summary>
        /// Dispose element
        /// </summary>
        public void Dispose()
        {
            Font.Dispose();
            _rasterState.Dispose();
            _blendState.Dispose();
            _depthState.Dispose();
            _samplerState.Dispose();

            _backbufferView.Dispose();
            _zbufferView.Dispose();
            _swapchain.Dispose();
            _deviceContext.Dispose();
            _device.Dispose();
        }

        /// <summary>
        /// Clear backbuffer and zbuffer
        /// </summary>
        /// <param name="color">background color</param>
        public void Clear(Color4 color)
        {
            DeviceContext.ClearRenderTargetView(_backbufferView, color);
            DeviceContext.ClearDepthStencilView(_zbufferView, DepthStencilClearFlags.Depth, 1.0F, 0);
        }

        /// <summary>
        /// Present scene to screen
        /// </summary>
        public void Present()
        {
            SwapChain.Present(0, PresentFlags.None);
        }




        /// <summary>
        /// Set current rasterizer state to default
        /// </summary>
        public void SetDefaultRasterState()
        {
            Utilities.Dispose(ref _rasterState);
            //Rasterize state
            RasterizerStateDescription rasterDescription = RasterizerStateDescription.Default();
            _rasterState = new RasterizerState(Device, rasterDescription);
            DeviceContext.Rasterizer.State = _rasterState;
        }

        /// <summary>
        /// Set current rasterizer state to wireframe
        /// </summary>
        public void SetWireframeRasterState()
        {
            _rasterState.Dispose();
            //Rasterize state
            RasterizerStateDescription rasterDescription = RasterizerStateDescription.Default();
            rasterDescription.FillMode = FillMode.Wireframe;
            _rasterState = new RasterizerState(Device, rasterDescription);

            DeviceContext.Rasterizer.State = _rasterState;
        }

        /// <summary>
        /// Set current blending state to default
        /// </summary>
        public void SetDefaultBlendState()
        {
            Utilities.Dispose(ref _blendState);
            BlendStateDescription description = BlendStateDescription.Default();
            _blendState = new BlendState(Device, description);
        }

        /// <summary>
        /// Set current blending state
        /// </summary>
        /// <param name="operation">Blend Operation</param>
        /// <param name="source">Source Option</param>
        /// <param name="destination">Destination Option</param>
        public void SetBlend(BlendOperation operation, BlendOption source, BlendOption destination)
        {
            Utilities.Dispose(ref _blendState);
            BlendStateDescription description = BlendStateDescription.Default();

            description.RenderTarget[0].BlendOperation = operation;
            description.RenderTarget[0].SourceBlend = source;
            description.RenderTarget[0].DestinationBlend = destination;
            description.RenderTarget[0].IsBlendEnabled = true;
            _blendState = new BlendState(Device, description);
        }

        /// <summary>
        /// Set current depth state to default
        /// </summary>
        public void SetDefaultDepthState()
        {
            Utilities.Dispose(ref _depthState);
            DepthStencilStateDescription description = DepthStencilStateDescription.Default();
            description.DepthComparison = Comparison.LessEqual;
            description.IsDepthEnabled = true;

            _depthState = new DepthStencilState(Device, description);
        }

        /// <summary>
        /// Set current sampler state to default
        /// </summary>
        public void SetDefaultSamplerState()
        {
            Utilities.Dispose(ref _samplerState);
            SamplerStateDescription description = SamplerStateDescription.Default();
            description.Filter = Filter.MinMagMipLinear;
            description.AddressU = TextureAddressMode.Wrap;
            description.AddressV = TextureAddressMode.Wrap;
            _samplerState = new SamplerState(Device, description);
        }

        /// <summary>
        /// Set current states inside context
        /// </summary>
        public void UpdateAllStates()
        {
            DeviceContext.Rasterizer.State = _rasterState;
            DeviceContext.OutputMerger.SetBlendState(_blendState);
            DeviceContext.OutputMerger.SetDepthStencilState(_depthState);
            DeviceContext.PixelShader.SetSampler(0, _samplerState);
        }

        /// <summary>
        /// Update constant buffer
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="buffer">Buffer to update</param>
        /// <param name="data">Data to write inside buffer</param>
        public void UpdateData<T>(Buffer11 buffer, T data) where T : struct
        {
            DeviceContext.UpdateSubresource(ref data, buffer);
        }

        /// <summary>
        /// Apply multiple targets to device
        /// </summary>
        /// <param name="targets">List of targets. Depth Buffer is taken from first one</param>
        public void ApplyMultipleRenderTarget(params SharpRenderTarget[] targets)
        {
            var targetsView = targets.Select(t => t.Target).ToArray();
            DeviceContext.OutputMerger.SetTargets(targets[0].Zbuffer, targetsView);
            DeviceContext.Rasterizer.SetViewport(0, 0, targets[0].Width, targets[0].Height);
        }


        /// <summary>
        /// DirectX11 Support Avaiable
        /// </summary>
        /// <returns>Supported</returns>
        public static bool IsDirectX11Supported()
        {
            return SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == FeatureLevel.Level_11_0;
        }


    }
}
