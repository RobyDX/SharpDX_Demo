using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using System.IO;

namespace SharpHelper.Skinning
{
    
    /// <summary>
    /// Geometry Material
    /// </summary>
    public class Material
    {
        
        /// <summary>
        /// Ambient
        /// </summary>
        public Vector4 Ambient { get; set; }

        /// <summary>
        /// Diffuse Color
        /// </summary>
        public Vector4 Diffuse { get; set; }

        /// <summary>
        /// Specular Color
        /// </summary>
        public Vector4 Specular { get; set; }

        /// <summary>
        /// Specular Power
        /// </summary>
        public float SpecularPower { get; set; }

        /// <summary>
        /// Emissive Color
        /// </summary>
        public Vector4 Emissive { get; set; }

        /// <summary>
        /// Diffuse Texture Name
        /// </summary>
        public string DiffuseTextureName { get; set; }

        /// <summary>
        /// Normal Texture Name
        /// </summary>
        public string NormalTextureName { get; set; }

        /// <summary>
        /// Diffuse Texture 
        /// </summary>
        public ShaderResourceView DiffuseTexture { get; set; }

        /// <summary>
        /// Normal Texture
        /// </summary>
        public ShaderResourceView NormalTexture { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Material loaded</param>
        public Material(MaterialData material)
        {
            Diffuse = material.Diffuse;
            Ambient = material.Ambient;
            Specular = material.Specular;
            SpecularPower = material.SpecularPower;
            Emissive = material.Emissive;

            DiffuseTextureName = Path.GetFileName(material.DiffuseTexture);

        }


    }
}
