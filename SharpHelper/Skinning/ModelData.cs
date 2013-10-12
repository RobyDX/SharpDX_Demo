using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SharpDX;

namespace SharpHelper.Skinning
{
    /// <summary>
    /// Skinned Vertex Format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexFormat
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
        /// Texture Set 1
        /// </summary>
        public Vector2 TextureSet1;

        /// <summary>
        /// Texture Set 2
        /// </summary>
        public Vector2 TextureSet2;

        /// <summary>
        /// Binormal
        /// </summary>
        public Vector3 Binormal;

        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// Joint
        /// </summary>
        public Vector4 Joint;

        /// <summary>
        /// Weight
        /// </summary>
        public Vector4 Weight;

        /// <summary>
        /// Byte Size
        /// </summary>
        public const int Size = 96;

        /// <summary>
        /// Return Vertex as Float Array
        /// </summary>
        /// <returns></returns>
        internal float[] GetArray()
        {
            List<float> v = new List<float>();
            v.AddRange(Position.ToArray());
            v.AddRange(Normal.ToArray());
            v.AddRange(TextureSet1.ToArray());
            v.AddRange(TextureSet2.ToArray());
            v.AddRange(Tangent.ToArray());
            v.AddRange(Binormal.ToArray());
            v.AddRange(Joint.ToArray());
            v.AddRange(Weight.ToArray());
            return v.ToArray();
        }

        /// <summary>
        /// Compare 2 Vertices
        /// </summary>
        /// <param name="a">First Vertex</param>
        /// <param name="b">Secon Vertex</param>
        /// <returns>Result</returns>
        public static bool Compare(VertexFormat a, VertexFormat b)
        {
            return a.Position == b.Position &&
                a.TextureSet1 == b.TextureSet1 &&
                a.TextureSet2 == b.TextureSet2 &&
                a.Weight == b.Weight &&
                a.Joint == b.Joint;
        }

    }


    /// <summary>
    /// Material Data
    /// </summary>
    public class MaterialData
    {

        /// <summary>
        /// Ambient Color
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
        /// Emissiva Color
        /// </summary>
        public Vector4 Emissive { get; set; }

        /// <summary>
        /// Diffuse Texture Name
        /// </summary>
        public string DiffuseTexture { get; set; }

        /// <summary>
        /// Normal Texture Name
        /// </summary>
        public string NormalTexture { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialData()
        {
            Ambient = new Vector4(0, 0, 0, 0);
            DiffuseTexture = "";
            NormalTexture = "";

            Diffuse = new Vector4();
            Specular = new Vector4(0, 0, 0, 0);
            Emissive = new Vector4();
            SpecularPower = 0;
        }

    }

    /// <summary>
    /// Model Data
    /// </summary>
    public class ModelData
    {
        /// <summary>
        /// Model Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public List<ModelNode> Nodes { get; set; }

        /// <summary>
        /// Animations
        /// </summary>
        public List<Animation> Animations { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelData()
        {
            Nodes = new List<ModelNode>();
            Animations = new List<Animation>();
        }
    }

    /// <summary>
    /// Node Type
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// Joint
        /// </summary>
        Joint,
        /// <summary>
        /// Node
        /// </summary>
        Node
    }

    /// <summary>
    /// Model Node
    /// </summary>
    public class ModelNode
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Node Type
        /// </summary>
        public NodeType Type { get; set; }

        /// <summary>
        /// World Matrix
        /// </summary>
        public Matrix World { get; set; }

        /// <summary>
        /// Children Nodes
        /// </summary>
        public List<ModelNode> Children { get; set; }

        /// <summary>
        /// Geometries Inside this node
        /// </summary>
        public List<ModelGeometry> Geometries { get; set; }

        /// <summary>
        /// Skinning Information
        /// </summary>
        public SkinInformation Skinning { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelNode()
        {
            Children = new List<ModelNode>();
            Geometries = new List<ModelGeometry>();
            World = Matrix.Identity;
        }
    }

    /// <summary>
    /// Skin Information
    /// </summary>
    public class SkinInformation
    {
        /// <summary>
        /// Bind Matrix
        /// </summary>
        public Matrix BindMatrix { get; set; }

        /// <summary>
        /// Joint Names List
        /// </summary>
        public List<string> JointNames { get; set; }


        /// <summary>
        /// Inverse Binding Matrices
        /// </summary>
        public List<Matrix> InverseBinding { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkinInformation()
        {
            JointNames = new List<string>();
            InverseBinding = new List<Matrix>();
        }

    }

    /// <summary>
    /// Geometry Data
    /// </summary>
    public class ModelGeometry
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Vertices List
        /// </summary>
        public List<VertexFormat> Vertices { get; set; }

        /// <summary>
        /// Indices List
        /// </summary>
        public List<int> Indices { get; set; }

        /// <summary>
        /// Material
        /// </summary>
        public MaterialData Material { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        public ModelGeometry()
        {
            Vertices = new List<VertexFormat>();
            Indices = new List<int>();
        }


        /// <summary>
        /// Optimize Geometry
        /// </summary>
        public void Optimize()
        {
            CorrectTexture();
            GenerateIndices();
            GenerateNormal();
            GenerateTangentBinormal();
        }

        private void CorrectTexture()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                VertexFormat v = Vertices[i];
                v.TextureSet1.Y = 1.0F - v.TextureSet1.Y;
                v.TextureSet2.Y = 1.0F - v.TextureSet2.Y;
                Vertices[i] = v;
            }
        }


        private void GenerateNormal()
        {

            for (int i = 0; i < Vertices.Count; i++)
            {
                VertexFormat v = Vertices[i];
                v.Normal = new Vector3();
                Vertices[i] = v;
            }

            for (int i = 0; i < Indices.Count; i += 3)
            {

                VertexFormat p1 = Vertices[Indices[i]];
                VertexFormat p2 = Vertices[Indices[i + 1]];
                VertexFormat p3 = Vertices[Indices[i + 2]];

                Vector3 V1 = p2.Position - p1.Position;
                Vector3 V2 = p3.Position - p1.Position;

                Vector3 N = Vector3.Cross(V1, V2);
                N.Normalize();

                p1.Normal += N;
                p2.Normal += N;
                p3.Normal += N;

                Vertices[Indices[i]] = p1;
                Vertices[Indices[i + 1]] = p2;
                Vertices[Indices[i + 2]] = p3;
            }

            //normalize
            for (int i = 0; i < Vertices.Count; i++)
            {
                VertexFormat v = Vertices[i];
                v.Normal.Normalize();
                Vertices[i] = v;
            }

        }

        private void GenerateTangentBinormal()
        {
            //Reset Vertices
            for (int i = 0; i < Vertices.Count; i++)
            {
                VertexFormat v = Vertices[i];
                v.Normal = new Vector3();
                v.Tangent = new Vector3();
                v.Binormal = new Vector3();
                Vertices[i] = v;
            }

            for (int i = 0; i < Indices.Count; i += 3)
            {
                VertexFormat P0 = Vertices[Indices[i]];
                VertexFormat P1 = Vertices[Indices[i + 1]];
                VertexFormat P2 = Vertices[Indices[i + 2]];


                Vector3 e0 = P1.Position - P0.Position;
                Vector3 e1 = P2.Position - P0.Position;
                Vector3 normal = Vector3.Cross(e0, e1);
                //using Eric Lengyel's approach with a few modifications
                //from Mathematics for 3D Game Programmming and Computer Graphics
                // want to be able to trasform a vector in Object Space to Tangent Space
                // such that the x-axis cooresponds to the 's' direction and the
                // y-axis corresponds to the 't' direction, and the z-axis corresponds
                // to <0,0,1>, straight up out of the texture map

                Vector3 P = P1.Position - P0.Position;
                Vector3 Q = P2.Position - P0.Position;

                float s1 = P1.TextureSet1.X - P0.TextureSet1.X;
                float t1 = P1.TextureSet1.Y - P0.TextureSet1.Y;
                float s2 = P2.TextureSet1.X - P0.TextureSet1.X;
                float t2 = P2.TextureSet1.Y - P0.TextureSet1.Y;


                //we need to solve the equation
                // P = s1*T + t1*B
                // Q = s2*T + t2*B
                // for T and B


                //this is a linear system with six unknowns and six equatinos, for TxTyTz BxByBz
                //[px,py,pz] = [s1,t1] * [Tx,Ty,Tz]
                // qx,qy,qz     s2,t2     Bx,By,Bz

                //multiplying both sides by the inverse of the s,t matrix gives
                //[Tx,Ty,Tz] = 1/(s1t2-s2t1) *  [t2,-t1] * [px,py,pz]
                // Bx,By,Bz                      -s2,s1	    qx,qy,qz  

                //solve this for the unormalized T and B to get from tangent to object space

                float tmp = 0.0f;
                if (Math.Abs(s1 * t2 - s2 * t1) <= 0.0001f)
                {
                    tmp = 1.0f;
                }
                else
                {
                    tmp = 1.0f / (s1 * t2 - s2 * t1);
                }

                Vector3 tangent = new Vector3();
                Vector3 binormal = new Vector3();

                tangent.X = (t2 * P.X - t1 * Q.X);
                tangent.Y = (t2 * P.Y - t1 * Q.Y);
                tangent.Z = (t2 * P.Z - t1 * Q.Z);

                tangent = tangent * tmp;

                binormal.X = (s1 * Q.X - s2 * P.X);
                binormal.Y = (s1 * Q.Y - s2 * P.Y);
                binormal.Z = (s1 * Q.Z - s2 * P.Z);

                binormal = binormal * tmp;

                normal.Normalize();
                tangent.Normalize();
                binormal.Normalize();

                //Add Normal

                P0.Normal += normal;
                P1.Normal += normal;
                P2.Normal += normal;

                P0.Tangent += tangent;
                P1.Tangent += tangent;
                P2.Tangent += tangent;

                P0.Binormal += binormal;
                P1.Binormal += binormal;
                P2.Binormal += binormal;

                Vertices[Indices[i]] = P0;
                Vertices[Indices[i + 1]] = P1;
                Vertices[Indices[i + 2]] = P2;
            }

            //normalize
            for (int i = 0; i < Vertices.Count; i++)
            {
                VertexFormat v = Vertices[i];
                v.Normal.Normalize();
                v.Binormal.Normalize();
                v.Tangent.Normalize();
                Vertices[i] = v;
            }
        }

        private void GenerateIndices()
        {
            List<VertexFormat> tempVertices = new List<VertexFormat>();
            List<int> tempIndices = new List<int>();

            foreach (VertexFormat v in Vertices)
            {
                //Search for existing vertex
                int i = 0;
                bool found = false;
                foreach (VertexFormat v2 in tempVertices)
                {
                    if (VertexFormat.Compare(v, v2))
                    {
                        //found
                        found = true;
                        break;
                    }
                    i++;
                }

                //In found join the normals
                if (found)
                {
                    tempIndices.Add(i);
                    VertexFormat v2 = tempVertices[i];
                }
                else
                {
                    i = tempVertices.Count;
                    tempVertices.Add(v);
                    tempIndices.Add(i);
                }

                //normal
                VertexFormat vTemp = tempVertices[i];
                vTemp.Normal += v.Normal;
                tempVertices[i] = vTemp;
            }


            //Normalize all Vertices
            Vertices.Clear();
            foreach (VertexFormat v in tempVertices)
            {
                v.Normal.Normalize();
                Vertices.Add(v);
            }
            Vertices.AddRange(tempVertices);

            Indices.Clear();
            Indices.AddRange(tempIndices);

        }
    }

    /// <summary>
    /// Animations
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// Animation Nodes
        /// </summary>
        public List<AnimationNode> Nodes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Animation()
        {
            Nodes = new List<AnimationNode>();
        }

    }


    /// <summary>
    /// Interpolation Type
    /// </summary>
    public enum Interpolation
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Linear
        /// </summary>
        Linear = 1,
        /// <summary>
        /// Bezier
        /// </summary>
        Bezier = 2
    }

    /// <summary>
    /// Animation Node
    /// </summary>
    public class AnimationNode
    {
        /// <summary>
        /// Children Nodes
        /// </summary>
        public List<AnimationNode> Children { get; set; }

        /// <summary>
        /// Model Node that is target of this animation node
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Input
        /// </summary>
        public List<float> Input { get; set; }

        /// <summary>
        /// Output matrices
        /// </summary>
        public List<Matrix> Output { get; set; }

        /// <summary>
        /// Input Tangents
        /// </summary>
        public List<Matrix> In_Tangent { get; set; }

        /// <summary>
        /// Output Tangents
        /// </summary>
        public List<Matrix> Out_Tangent { get; set; }

        /// <summary>
        /// Interpolation Type
        /// </summary>
        public Interpolation Interpolation { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationNode()
        {
            Target = "";
            Input = new List<float>();
            Output = new List<Matrix>();
            In_Tangent = new List<Matrix>();
            Out_Tangent = new List<Matrix>();
            Interpolation = 0;
            Children = new List<AnimationNode>();
        }
    }
}
