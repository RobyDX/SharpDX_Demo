using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace SharpHelper
{

    /// <summary>
    /// Describe a mesh subset
    /// </summary>
    public class SharpSubSet
    {
        /// <summary>
        /// Diffuse map
        /// </summary>
        public ShaderResourceView DiffuseMap { get; set; }

        /// <summary>
        /// Normal Map
        /// </summary>
        public ShaderResourceView NormalMap { get; set; }

        /// <summary>
        /// Ambient Color (RGBA)
        /// </summary>
        public Vector4 AmbientColor { get; set; }

        /// <summary>
        /// Diffuse Color (RGBA)
        /// </summary>
        public Vector4 DiffuseColor { get; set; }

        /// <summary>
        /// Specular Color (RGBA)
        /// </summary>
        public Vector4 SpecularColor { get; set; }

        /// <summary>
        /// Specular Power
        /// </summary>
        public int SpecularPower { get; set; }

        /// <summary>
        /// Emissive Color (RGBA)
        /// </summary>
        public Vector4 EmissiveColor { get; set; }

        /// <summary>
        /// Index Start inside IndexBuffer
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Number of indices to draw
        /// </summary>
        public int IndexCount { get; set; }
    }
}
