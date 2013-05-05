using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX;
using System.Globalization;

namespace SharpHelper
{
    /// <summary>
    /// Load an obj model
    /// </summary>
    public class WaveFrontModel
    {
        private static CultureInfo infos = CultureInfo.InvariantCulture;

        /// <summary>
        /// Name of the model
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Vertex data
        /// </summary>
        public List<StaticVertex> VertexData { get; private set; }


        /// <summary>
        /// Vertex data in Tangent Format
        /// </summary>
        public List<TangentVertex> TangentData { get; private set; }

        /// <summary>
        /// Index data
        /// </summary>
        public List<int> IndexData { get; private set; }

        /// <summary>
        /// Mesh material
        /// </summary>
        public List<Material> MeshMaterial { get; private set; }

        /// <summary>
        /// Face counts
        /// </summary>
        public List<int> FaceCounts { get; private set; }

        /// <summary>
        /// Material description
        /// </summary>
        public class Material
        {
            /// <summary>
            /// Name
            /// </summary>
            public string MaterialName;
            /// <summary>
            /// Ambient color
            /// </summary>
            public Vector3 Ambient;
            /// <summary>
            /// Diffuse color
            /// </summary>
            public Vector3 Diffuse;
            /// <summary>
            /// Specular color
            /// </summary>
            public Vector3 Specular;
            /// <summary>
            /// Shininess
            /// </summary>
            public float Shininess;
            /// <summary>
            /// Texture name
            /// </summary>
            public string DiffuseMap;

            /// <summary>
            /// Normal Texture name
            /// </summary>
            public string NormalMap;
        }

        /// <summary>
        /// Fuse 2 model
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Sum</returns>
        public static WaveFrontModel operator +(WaveFrontModel a, WaveFrontModel b)
        {
            WaveFrontModel c = new WaveFrontModel();

            c.Name = a.Name;
            c.VertexData.AddRange(a.VertexData);
            c.VertexData.AddRange(b.VertexData);
            c.IndexData.AddRange(a.IndexData);

            c.IndexData.AddRange((from i in b.IndexData select (i + a.VertexData.Count)).ToArray());
            c.MeshMaterial.AddRange(a.MeshMaterial);
            c.MeshMaterial.AddRange(b.MeshMaterial);

            int tot = 0;
            foreach (int i in a.FaceCounts)
            {
                c.FaceCounts.Add(i + tot);
                tot += i;
            }

            foreach (int i in b.FaceCounts)
            {
                c.FaceCounts.Add(i + tot);
                tot += i;
            }

            return c;
        }

        private WaveFrontModel()
        {
            this.IndexData = new List<int>();
            this.Name = "";
            this.VertexData = new List<StaticVertex>();
            this.MeshMaterial = new List<Material>();
            this.FaceCounts = new List<int>();
        }

        /// <summary>
        /// Create obj from file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Models</returns>
        public static WaveFrontModel[] CreateFromObj(string filename)
        {
            List<WaveFrontModel> geom = new List<WaveFrontModel>();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textures = new List<Vector2>();
            List<int> faces = new List<int>();
            List<Material> materials = new List<Material>();
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(filename))
            {
                for (; ; )
                {
                    string l = reader.ReadLine();
                    if (reader.EndOfStream)
                        break;

                    if (l.Contains("#") || string.IsNullOrEmpty(l.Trim()))
                        continue;

                    lines.Add(l);
                }
            }

            foreach (string line in lines)
            {
                if (line.Contains("v "))
                {
                    positions.Add(GetPosition(line));
                }
                else if (line.Contains("vn "))
                {
                    normals.Add(GetNormal(line));
                }
                else if (line.Contains("vt "))
                {
                    textures.Add(GetTexture(line));
                }
                else if (line.Contains("mtllib "))
                {
                    string path = System.IO.Path.GetDirectoryName(filename) + "\\" + line.Replace("mtllib", "").Trim();
                    materials.AddRange(LoadMaterial(path));
                }
            }

            string currentMesh = "default";
            Material currentMate = new Material();

            foreach (string line in lines)
            {
                if (line.Contains("f "))
                {
                    faces.AddRange(GetFace(line));
                }
                else if (line.Contains("usemtl "))
                {
                    if (faces.Count > 0)
                        geom.Add(CreateGeom(currentMesh, positions, normals, textures, faces, currentMate));

                    currentMate = (from m in materials where m.MaterialName == line.Replace("usemtl", "").Trim() select m).First();

                    faces.Clear();
                }
                else if (line.Contains("g "))
                {
                    if (faces.Count > 0)
                        geom.Add(CreateGeom(currentMesh, positions, normals, textures, faces, currentMate));

                    currentMesh = line.Replace("g", "").Trim();

                    faces.Clear();
                }
                else if (line.Contains("# "))
                {
                    //commento
                }
            }


            if (faces.Count > 0)
                geom.Add(CreateGeom(currentMesh, positions, normals, textures, faces, currentMate));

            foreach (WaveFrontModel model in geom)
                model.Optimize();

            return geom.ToArray();
        }



        private static WaveFrontModel CreateGeom(string name, List<Vector3> position, List<Vector3> normals, List<Vector2> texture, List<int> faces, Material mate)
        {
            WaveFrontModel geom = new WaveFrontModel();
            geom.Name = name;
            geom.VertexData = new List<StaticVertex>();
            geom.IndexData = new List<int>();

            int stride = 0;


            if (position.Count > 0)
                stride++;


            if (normals.Count > 0)
                stride++;


            if (texture.Count > 0)
                stride++;


            int vertexCount = faces.Count / stride;

            int k = 0;


            for (int i = 0; i < vertexCount; i++)
            {
                StaticVertex v = new StaticVertex();
                if (position.Count > 0)
                {
                    v.Position = position[faces[k] - 1];
                    k++;
                }

                if (texture.Count > 0)
                {
                    v.TextureCoordinate = texture[faces[k] - 1];
                    k++;
                }

                if (normals.Count > 0)
                {
                    v.Normal = normals[faces[k] - 1];
                    k++;
                }
                geom.VertexData.Add(v);

            }

            for (int i = 0; i < (geom.VertexData.Count); i++)
            {
                geom.IndexData.Add((short)i);
            }

            geom.MeshMaterial.Add(mate);
            geom.FaceCounts.Add(geom.IndexData.Count);
            return geom;

        }

        private static string[] Parts(string name, string line)
        {
            return line.Replace(name, "").Trim().Split(' ');
        }

        private static Vector3 GetPosition(string line)
        {
            string[] parts = Parts("v", line);
            return new Vector3(
                float.Parse(parts[0], infos),
                float.Parse(parts[1], infos),
                float.Parse(parts[2], infos));
        }

        private static Vector3 GetNormal(string line)
        {
            string[] parts = Parts("vn", line);
            return new Vector3(
                float.Parse(parts[0], infos),
                float.Parse(parts[1], infos),
                float.Parse(parts[2], infos));
        }

        private static Vector2 GetTexture(string line)
        {
            string[] parts = Parts("vt", line);
            return new Vector2(
                float.Parse(parts[0], infos),
               1 - float.Parse(parts[1], infos));
        }

        private static int[] GetFace(string line)
        {
            string[] parts = Parts("f", line);
            List<int> lists = new List<int>();
            foreach (String s in parts)
            {
                lists.AddRange((from ss in s.Split('/') select int.Parse(ss)).ToArray());
            }
            return lists.ToArray();
        }

        private static WaveFrontModel Create(StreamReader reader)
        {
            WaveFrontModel geom = new WaveFrontModel();

            return geom;
        }


        private static Material[] LoadMaterial(String filename)
        {
            Material current = null;
            List<Material> materials = new List<Material>();
            using (StreamReader reader = new StreamReader(filename))
            {


                for (; ; )
                {
                    if (reader.EndOfStream)
                        break;
                    string line = reader.ReadLine();



                    if (line.Contains("newmtl"))
                    {
                        if (current != null)
                            materials.Add(current);

                        current = new Material();
                        current.MaterialName = line.Replace("newmtl", "").Trim();
                    }
                    else if (line.Contains("map_Kd"))
                    {
                        current.DiffuseMap = line.Replace("map_Kd", "").Trim();
                    }
                    else if (line.Contains("map_Ka"))
                    {

                    }
                    else if (line.Contains("Ka"))
                    {
                        float[] val = (from s in line.Replace("Ka", "").Trim().Split(' ') select float.Parse(s, infos)).ToArray();
                        current.Ambient = new Vector3(val[0], val[1], val[2]);
                    }
                    else if (line.Contains("Kd"))
                    {
                        float[] val = (from s in line.Replace("Kd", "").Trim().Split(' ') select float.Parse(s, infos)).ToArray();
                        current.Diffuse = new Vector3(val[0], val[1], val[2]);
                    }
                    else if (line.Contains("Ks"))
                    {
                        float[] val = (from s in line.Replace("Ks", "").Trim().Split(' ') select float.Parse(s, infos)).ToArray();
                        current.Specular = new Vector3(val[0], val[1], val[2]);
                    }
                    else if (line.Contains("Ns"))
                    {
                        float[] val = (from s in line.Replace("Ns", "").Trim().Split(' ') select float.Parse(s, infos)).ToArray();
                        current.Shininess = val.First();
                    }
                }
            }

            if (current != null)
                materials.Add(current);

            return materials.ToArray();
        }

        /// <summary>
        /// Optimize model
        /// </summary>
        public void Optimize()
        {
            GenerateIndices();
            GenerateNormal();
            GenerateTangentBinormal();
        }


        private void GenerateNormal()
        {

            for (int i = 0; i < VertexData.Count; i++)
            {
                StaticVertex v = VertexData[i];
                v.Normal = new Vector3();
                VertexData[i] = v;
            }

            for (int i = 0; i < IndexData.Count; i += 3)
            {

                StaticVertex p1 = VertexData[IndexData[i]];
                StaticVertex p2 = VertexData[IndexData[i + 1]];
                StaticVertex p3 = VertexData[IndexData[i + 2]];

                Vector3 V1 = p2.Position - p1.Position;
                Vector3 V2 = p3.Position - p1.Position;

                Vector3 N = Vector3.Cross(V1, V2);
                N.Normalize();

                p1.Normal += N;
                p2.Normal += N;
                p3.Normal += N;

                VertexData[IndexData[i]] = p1;
                VertexData[IndexData[i + 1]] = p2;
                VertexData[IndexData[i + 2]] = p3;
            }

            //normalize
            for (int i = 0; i < VertexData.Count; i++)
            {
                StaticVertex v = VertexData[i];
                v.Normal.Normalize();
                VertexData[i] = v;
            }

        }

        private void GenerateTangentBinormal()
        {
            TangentData = VertexData.Select(v => new TangentVertex()
            {
                Position = v.Position,
                Normal = v.Normal,
                TextureCoordinate = v.TextureCoordinate
            }).ToList();


            //resetta i vettori
            for (int i = 0; i < VertexData.Count; i++)
            {
                TangentVertex v = TangentData[i];
                v.Normal = new Vector3();
                v.Tangent = new Vector3();
                v.Binormal = new Vector3();
                TangentData[i] = v;
            }

            for (int i = 0; i < IndexData.Count; i += 3)
            {
                TangentVertex P0 = TangentData[IndexData[i]];
                TangentVertex P1 = TangentData[IndexData[i + 1]];
                TangentVertex P2 = TangentData[IndexData[i + 2]];


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

                float s1 = P1.TextureCoordinate.X - P0.TextureCoordinate.X;
                float t1 = P1.TextureCoordinate.Y - P0.TextureCoordinate.Y;
                float s2 = P2.TextureCoordinate.X - P0.TextureCoordinate.X;
                float t2 = P2.TextureCoordinate.Y - P0.TextureCoordinate.Y;


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

                //aggiungi

                P0.Normal += normal;
                P1.Normal += normal;
                P2.Normal += normal;

                P0.Tangent += tangent;
                P1.Tangent += tangent;
                P2.Tangent += tangent;

                P0.Binormal += binormal;
                P1.Binormal += binormal;
                P2.Binormal += binormal;

                TangentData[IndexData[i]] = P0;
                TangentData[IndexData[i + 1]] = P1;
                TangentData[IndexData[i + 2]] = P2;
            }

            //normalize
            for (int i = 0; i < VertexData.Count; i++)
            {
                TangentVertex v = TangentData[i];
                v.Normal.Normalize();
                v.Binormal.Normalize();
                v.Tangent.Normalize();
                TangentData[i] = v;
            }
        }

        private void GenerateIndices()
        {
            List<StaticVertex> tempVertices = new List<StaticVertex>();
            List<int> tempIndices = new List<int>();

            foreach (StaticVertex v in VertexData)
            {
                //verifica se esiste un vertice già nella lista
                int i = 0;
                bool found = false;
                foreach (StaticVertex v2 in tempVertices)
                {
                    if (StaticVertex.Compare(v, v2))
                    {
                        //travato
                        found = true;
                        break;
                    }
                    i++;
                }

                if (found)
                {
                    tempIndices.Add(i);
                    StaticVertex v2 = tempVertices[i];
                    //somma le normali
                }
                else
                {
                    i = tempVertices.Count;
                    tempVertices.Add(v);
                    tempIndices.Add(i);
                }

                //normali
                StaticVertex vTemp = tempVertices[i];
                vTemp.Normal += v.Normal;
                tempVertices[i] = vTemp;
            }


            //normalizzazione finale
            VertexData.Clear();
            //foreach (VertexFormat v in tempVertices)
            //{
            //    v.Normal.Normalize();
            //    Vertices.Add(v);
            //}
            VertexData.AddRange(tempVertices);

            IndexData.Clear();
            IndexData.AddRange(tempIndices);

        }
    }

}



