using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace SharpHelper
{
    /// <summary>
    /// Shader description
    /// </summary>
    public class SharpShaderDescription
    {
        /// <summary>
        /// Vertex Shader Function Name
        /// </summary>
        public string VertexShaderFunction { get; set; }

        /// <summary>
        /// Pixel Shader Function Name
        /// </summary>
        public string PixelShaderFunction { get; set; }

        /// <summary>
        /// Geometry Shader Function Name
        /// </summary>
        public string GeometryShaderFunction { get; set; }

        /// <summary>
        /// Hull Shader Function Name
        /// </summary>
        public string HullShaderFunction { get; set; }

        /// <summary>
        /// Domain Shader Function Name
        /// </summary>
        public string DomainShaderFunction { get; set; }

        /// <summary>
        /// Stream output elements
        /// </summary>
        public StreamOutputElement[] GeometrySO { get; set; }

    }

    /// <summary>
    /// Shader Helper Class
    /// </summary>
    public class SharpShader : IDisposable
    {
        /// <summary>
        /// Vertex Shader
        /// </summary>
        public VertexShader VertexShader { get; private set; }

        /// <summary>
        /// Pixel Shader
        /// </summary>
        public PixelShader PixelShader { get; private set; }

        /// <summary>
        /// Geometry Shader
        /// </summary>
        public GeometryShader GeometryShader { get; private set; }

        /// <summary>
        /// Domain Shader
        /// </summary>
        public DomainShader DomainShader { get; private set; }

        /// <summary>
        /// Hull Shader
        /// </summary>
        public HullShader HullShader { get; private set; }

        /// <summary>
        /// Input Layout
        /// </summary>
        public InputLayout Layout { get; private set; }

        /// <summary>
        /// Pointer to current device
        /// </summary>
        public SharpDevice Device { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Path of shader file</param>
        /// <param name="description">Description structure</param>
        /// <param name="elements">Input Layout Elements</param>
        public SharpShader(SharpDevice device, string filename, SharpShaderDescription description, InputElement[] elements)
        {
            Device = device;
            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(filename, description.VertexShaderFunction, "vs_5_0");
            VertexShader = new VertexShader(Device.Device, vertexShaderByteCode);

            //create pixel shader
            if (!string.IsNullOrEmpty(description.PixelShaderFunction))
            {
                var pixelShaderByteCode = ShaderBytecode.CompileFromFile(filename, description.PixelShaderFunction, "ps_5_0");
                PixelShader = new PixelShader(Device.Device, pixelShaderByteCode);
            }

            if (!string.IsNullOrEmpty(description.GeometryShaderFunction))
            {
                var geometryShaderByteCode = ShaderBytecode.CompileFromFile(filename, description.GeometryShaderFunction, "gs_5_0");

                if (description.GeometrySO == null)
                    GeometryShader = new GeometryShader(Device.Device, geometryShaderByteCode);
                else
                {
                    int[] size = new int[] { description.GeometrySO.Select(e => e.ComponentCount * 4).Sum() };
                    
                    GeometryShader = new GeometryShader(Device.Device, geometryShaderByteCode, description.GeometrySO, size, -1);
                }
            }

            if (!string.IsNullOrEmpty(description.DomainShaderFunction))
            {
                var domainShaderByteCode = ShaderBytecode.CompileFromFile(filename, description.DomainShaderFunction, "ds_5_0");
                DomainShader = new DomainShader(Device.Device, domainShaderByteCode);
            }

            if (!string.IsNullOrEmpty(description.HullShaderFunction))
            {
                var hullShaderByteCode = ShaderBytecode.CompileFromFile(filename, description.HullShaderFunction, "hs_5_0");
                HullShader = new HullShader(Device.Device, hullShaderByteCode);
            }

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            // Layout from VertexShader input signature
            Layout = new InputLayout(Device.Device, signature, elements);

        }

        /// <summary>
        /// Create a constant buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Buffer CreateBuffer<T>() where T : struct
        {
            return new Buffer(Device.Device, Utilities.SizeOf<T>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        /// <summary>
        /// Applica lo shader
        /// </summary>
        public void Apply()
        {
            Device.DeviceContext.InputAssembler.InputLayout = Layout;
            Device.DeviceContext.VertexShader.Set(VertexShader);
            Device.DeviceContext.PixelShader.Set(PixelShader);
            Device.DeviceContext.GeometryShader.Set(GeometryShader);
            Device.DeviceContext.DomainShader.Set(DomainShader);
            Device.DeviceContext.HullShader.Set(HullShader);
        }

        /// <summary>
        /// Remove Shader from
        /// </summary>
        public void Clear()
        {
            Device.DeviceContext.VertexShader.Set(null);
            Device.DeviceContext.PixelShader.Set(null);
            Device.DeviceContext.GeometryShader.Set(null);
            Device.DeviceContext.DomainShader.Set(null);
            Device.DeviceContext.HullShader.Set(null);
        }

        /// <summary>
        /// Release Elements
        /// </summary>
        public void Dispose()
        {
            if (VertexShader != null)
                VertexShader.Dispose();

            if (PixelShader != null)
                PixelShader.Dispose();

            if (GeometryShader != null)
                GeometryShader.Dispose();

            if (DomainShader != null)
                DomainShader.Dispose();

            if (HullShader != null)
                HullShader.Dispose();

            if (Layout != null)
                Layout.Dispose();
        }
    }
}
