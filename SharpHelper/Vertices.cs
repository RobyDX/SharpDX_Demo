using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SharpHelper
{
    /// <summary>
    /// Textured Vertex
    /// </summary>
    public struct TexturedVertex
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Texture coordinate
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="textureCoordinate">Texture Coordinate</param>
        public TexturedVertex(Vector3 position,Vector2 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }
    }

    /// <summary>
    /// Static Vertex
    /// </summary>
    public struct StaticVertex
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Texture coordinate
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Compare 2 vertices on Position and Texture Coordinate
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <returns></returns>
        internal static bool Compare(StaticVertex a, StaticVertex b)
        {
            return a.Position == b.Position &&
                a.TextureCoordinate == b.TextureCoordinate;
        }


    }


    /// <summary>
    /// Tangent Vertex
    /// </summary>
    public struct TangentVertex
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// Binormal
        /// </summary>
        public Vector3 Binormal;

        /// <summary>
        /// Texture coordinate
        /// </summary>
        public Vector2 TextureCoordinate;
    }

    /// <summary>
    /// Vertex Colored
    /// </summary>
    public struct ColoredVertex
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Color
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position XYZ</param>
        /// <param name="color">Vertex Color</param>
        public ColoredVertex(Vector3 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }
}
