using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SharpDX;
using SharpHelper.Skinning;

namespace SharpHelper
{
    /// <summary>
    /// Importer for collada
    /// </summary>
    public static class ColladaImporter
    {
        /// <summary>
        /// Namespace
        /// </summary>
        public const string ColladaNamespace = @"http://www.collada.org/2005/11/COLLADASchema";

        internal enum ChannelCode
        {
            Position,
            Normal,
            Tangent,
            Binormal,
            TexCoord1,
            TexCoord2,
            Joint,
            Weight,
            None
        }

        /// <summary>
        /// Define custom Semantic Associations
        /// </summary>
        public class SemanticAssociations
        {
            /// <summary>
            /// Position
            /// </summary>
            public string Position { get; set; }

            /// <summary>
            /// Normal
            /// </summary>
            public string Normal { get; set; }

            /// <summary>
            /// Tangent
            /// </summary>
            public string Tangent { get; set; }

            /// <summary>
            /// Binormal
            /// </summary>
            public string Binormal { get; set; }

            /// <summary>
            /// Coordinate texture 1
            /// </summary>
            public string TexCoord1 { get; set; }

            /// <summary>
            /// Cordinate texture 2
            /// </summary>
            public string TexCoord2 { get; set; }

            /// <summary>
            /// Joint
            /// </summary>
            public string Joint { get; set; }

            /// <summary>
            /// Weight
            /// </summary>
            public string Weight { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public SemanticAssociations()
            {
                Position = "VERTEX";
                Normal = "NORMAL";
                Tangent = "TEXTANGENT";
                Binormal = "TEXBINORMAL";
                TexCoord1 = "TEXCOORD";
                TexCoord2 = "TEXCOORD";
                Joint = "JOINT";
                Weight = "WEIGHT";
            }

            internal ChannelCode this[string semantic]
            {
                get
                {
                    if (semantic == Position)
                        return ChannelCode.Position;
                    if (semantic == Normal)
                        return ChannelCode.Normal;
                    if (semantic == Tangent)
                        return ChannelCode.Tangent;
                    if (semantic == Binormal)
                        return ChannelCode.Binormal;
                    if (semantic == TexCoord1)
                        return ChannelCode.TexCoord1;
                    if (semantic == TexCoord2)
                        return ChannelCode.TexCoord2;
                    if (semantic == Joint)
                        return ChannelCode.Joint;
                    if (semantic == Weight)
                        return ChannelCode.Weight;
                    return ChannelCode.None;

                }
            }

        }

        /// <summary>
        /// Load From File
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="association">Custom Semantic Association</param>
        /// <returns>Model</returns>
        public static ModelData Import(string filename, SemanticAssociations association)
        {
            ModelData data = new ModelData();

            XDocument doc = XDocument.Load(filename);

            //search for visual scene
            //instance_visual_scene
            XElement scene = doc.GetNode("instance_visual_scene");


            if (scene == null)
                return null;

            XElement sceneReference = doc.GetReference(scene);
            data.Name = sceneReference.GetAttribute("name");


            //Search for Nodes
            foreach (XElement c in sceneReference.GetChildren("node"))
            {
                data.Nodes.Add(LoadNodes(c, association));
            }


            //Search for Animations
            XElement animation = doc.GetNode("library_animations");
            Animation anim = new Animation();
            if (animation != null)
            {
                var list = animation.GetNodes("animation");

                foreach (XElement a in list)
                    anim.Nodes.Add(CreateAnimationTrack(a));

            }

            data.Animations.Add(anim);
            return data;
        }

        /// <summary>
        /// Load From File
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Model</returns>
        public static ModelData Import(string filename)
        {
            return Import(filename, new SemanticAssociations());
        }


        #region Geometry

        private static ModelNode LoadNodes(XElement element, SemanticAssociations association)
        {
            ModelNode node = new ModelNode();

            node.Name = element.GetAttribute("id");

            //type
            string type = element.GetAttribute("type").ToLower();
            if (string.IsNullOrEmpty(type) || type == "node")
                node.Type = NodeType.Node;
            else
                node.Type = NodeType.Joint;

            //world matrix
            node.World = ComputeMatrix(element);

            //Iterate each node
            foreach (XElement c in element.GetChildren("node"))
            {
                //Load Children
                node.Children.Add(LoadNodes(c, association));
            }

            //Iterate Geometries
            foreach (XElement g in element.GetNodes("instance_geometry"))
            {
                node.Geometries.AddRange(LoadGeometries(g.GetReference(), association));
            }

            //Get Animation Controller
            foreach (XElement c in element.GetNodes("instance_controller"))
            {
                SkinInformation skin;
                node.Geometries.AddRange(LoadController(c, association, out skin));
                node.Skinning = skin;
            }

            return node;
        }


        private static Matrix ComputeMatrix(XElement node)
        {
            Matrix matrix = Matrix.Identity;
            foreach (XElement element in node.Elements())
            {
                String n = element.Name.LocalName;
                switch (element.Name.LocalName)
                {
                    case "translate":
                        Vector3 t = Vector3FromString(element.Value);
                        matrix *= Matrix.Translation(t);
                        break;
                    case "rotate":
                        Vector4 r = Vector4FromString(element.Value);
                        matrix *= Matrix.RotationAxis(new Vector3() { X = r.X, Y = r.Y, Z = r.Z }, r.W);
                        break;
                    case "scale":
                        Vector3 s = Vector3FromString(element.Value);
                        matrix *= Matrix.Scaling(s);
                        break;
                    case "matrix":
                        matrix *= MatrixFromString(element.Value);
                        break;
                }
            }
            return matrix;
        }


        private static List<ModelGeometry> LoadGeometries(XElement geometryNode, SemanticAssociations association, List<Vector4> weights = null, List<Vector4> joints = null)
        {
            List<ModelGeometry> geometries = new List<ModelGeometry>();

            //geometry
            if (geometryNode.GetNode("mesh") != null)
            {
                //if mesh load data
                var mesh = geometryNode.GetNode("mesh");
                
                var sources = mesh.GetNodes("source");
                var vertices = mesh.GetNodes("vertices");

                //load data, only triangles supported
                
                foreach (var elem in mesh.Elements())
                {
                    ModelGeometry geometry = new ModelGeometry();
                    geometry.Name = geometryNode.GetAttribute("id");
                    //lines
                    //linestrips
                    switch (elem.Name.LocalName)
                    {
                        case "polygons":
                            break;
                        case "polylist":
                            break;
                        case "triangles":
                            GetTriangleMesh(geometry, elem, mesh, association, weights, joints);
                            geometry.Optimize();
                            geometries.Add(geometry);
                            break;
                        case "trifans":
                            break;
                        case "tristrips":
                            break;
                        default:
                            break;
                    }


                }

            }

            return geometries;
        }

        #endregion





        //Load Triangle Mesh
        private static void GetTriangleMesh(ModelGeometry model, XElement triangle, XElement mesh, SemanticAssociations association, List<Vector4> weights = null, List<Vector4> joints = null)
        {
            //Indices
            List<int> indices = GetIntList(triangle.GetNode("p").Value);

            //Load Channel
            var inputs = triangle.GetNodes("input");
            List<ChannelData> inputData = new List<ChannelData>();
            foreach (XElement elem in inputs)
            {
                inputData.Add(new ChannelData(elem));

            }

            //Load Material
            model.Material = new MaterialData();
            model.Material = GetMaterial(triangle.Document, triangle.GetAttribute("material"));

            //Create Vertices
            int k = 0;
            while (k < indices.Count)
            {
                VertexFormat v = new VertexFormat();

                foreach (ChannelData e in inputData)
                {
                    var data = e.GetChannel(indices[k]);

                    switch (association[e.Semantic])
                    {
                        case ChannelCode.Position:
                            v.Position = new Vector3(data);

                            //Add weights and joints if loaded for skinned model
                            if (weights != null)
                            {
                                v.Weight = weights[indices[k]];
                            }

                            if (joints != null)
                            {
                                v.Joint = joints[indices[k]];
                            }

                            break;
                        case ChannelCode.Normal:
                            v.Normal = new Vector3(data);
                            break;
                        case ChannelCode.Tangent:
                            v.Tangent = new Vector3(data);
                            break;
                        case ChannelCode.Binormal:
                            v.Binormal = new Vector3(data);
                            break;
                        case ChannelCode.TexCoord1:
                            if (data.Length >= 2)
                                v.TextureSet1 = new Vector2(data.Take(2).ToArray());
                            break;
                        case ChannelCode.TexCoord2:
                            v.TextureSet2 = new Vector2(data.Take(2).ToArray());
                            break;
                        case ChannelCode.Joint:
                            v.Joint = new Vector4(data);
                            break;
                        case ChannelCode.Weight:
                            v.Weight = new Vector4(data);
                            break;
                        case ChannelCode.None:

                            break;
                        default:
                            break;
                    }

                    k++;
                }


                //Save to model
                model.Vertices.Add(v);
                model.Indices.Add(model.Indices.Count);
            }

        }



        private static MaterialData GetMaterial(XDocument document, string materialName)
        {
            MaterialData matData = new MaterialData();

            XElement mate = document.GetByID(materialName);
            if (mate == null)
                return matData;
            XElement node = mate.GetNode("instance_effect");
            if (node == null)
                return matData;

            XElement effect = document.GetReference(node);
            //Get Data from Standard Material
            if (effect != null)
            {
                matData.Ambient = GetColor(effect, "ambient");
                matData.Diffuse = GetColor(effect, "diffuse");
                matData.DiffuseTexture = GetTextureName(effect, "diffuse");
                matData.Ambient = GetColor(effect, "ambient");
                matData.Emissive = GetColor(effect, "emission");
                matData.Specular = GetColor(effect, "specular");
                matData.SpecularPower = GetValue(effect, "shininess");
            }

            return matData;
        }



        private static Vector4 GetColor(XElement element, string elementName)
        {
            Vector4 color = new Vector4();
            XElement elementList = element.GetNode(elementName);
            if (elementList != null)
            {
                XElement res = elementList.GetNode("color");
                if (res != null)
                {
                    var data = GetFloatList(res.Value);
                    color.X = data[0];
                    color.Y = data[1];
                    color.Z = data[2];
                    if (data.Count > 3)
                        color.W = data[3];
                }
            }

            return color;
        }

        private static float GetValue(XElement element, string elementName)
        {
            XElement elementList = element.GetNode(elementName);
            if (elementList != null)
            {
                XElement res = elementList.GetNode("float");
                if (res != null)
                {
                    var data = GetFloatList(res.Value);
                    return data[0];
                }
            }
            return 0;
        }

        private static string GetTextureName(XElement element, string elementName)
        {
            XElement elementList = element.GetNode(elementName);
            if (elementList == null)
                return "";

            XElement res = elementList.GetNode("texture");
            if (res == null)
                return "";

            //Get Texture Sampler
            string texSampler = res.Attribute("texture").Value;
            res = element.GetNodes("newparam").Where(t => t.Attribute("sid").Value == texSampler).FirstOrDefault();
            if (res == null)
                return "";

            //Get Source
            string origin = res.GetNode("source").Value;
            res = element.GetNodes("newparam").Where(t => t.Attribute("sid").Value == origin).FirstOrDefault();
            if (res == null)
                return "";

            //Get Image
            string immagine = res.GetNode("init_from").Value;
            XElement tex = element.Document.GetNodes("image").Where(t => t.Attribute("id").Value == immagine).FirstOrDefault();

            if (tex == null)
                return "";

            //Get Texture Path
            return tex.GetNode("init_from").Value;

        }

        #region Model

        private static List<ModelGeometry> LoadController(XElement element, SemanticAssociations association, out SkinInformation skinInfo)
        {
            skinInfo = new SkinInformation();
            XElement skin = element.GetReference().GetChild("skin");
            if (skin == null)
            {
                //only skinned animation supported
                throw new NotSupportedException("Only Skin Animation supported");
            }

            //target geometry
            string geometryUrl = skin.GetAttribute("source");
            
            //bind matrix
            XElement bind = skin.GetChild("bind_shape_matrix");

            if (bind == null)
                skinInfo.BindMatrix = Matrix.Identity;
            else
                skinInfo.BindMatrix = MatrixFromString(bind.Value);

            //load joint and input
            XElement joints = skin.GetChild("joints");

            var jointName = joints.GetChildren("input").Where(i => i.GetAttribute("semantic") == "JOINT").First();
            skinInfo.JointNames = jointName.GetSource().Value.Replace('\n', ' ').Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();


            var bindMatrix = joints.GetChildren("input").Where(i => i.GetAttribute("semantic") == "INV_BIND_MATRIX").First();
            var values = GetFloatList(bindMatrix.GetSource().Value);

            for (int i = 0; i < values.Count; i += 16)
            {
                skinInfo.InverseBinding.Add(new Matrix(values.GetRange(i, 16).ToArray()).ToTranspose());
            }


            //Weight
            XElement vertex_weights = skin.GetChild("vertex_weights");
            var weights = GetFloatList(vertex_weights.GetChildren("input").Where(i => i.GetAttribute("semantic") == "WEIGHT").First().GetSource().Value);
            var v = GetIntList(vertex_weights.GetChild("v").Value);
            var vCount = GetIntList(vertex_weights.GetChild("vcount").Value);

            //Prepare vertex data
            int k = 0;
            List<Vector4> weightsTemp = new List<Vector4>();
            List<Vector4> jointsTemp = new List<Vector4>();

            foreach (int i in vCount)
            {
                Vector4 wei = new Vector4();
                Vector4 joy = new Vector4();


                for (int j = 0; j < i; j++)
                {
                    switch (j)
                    {
                        case 0:
                            joy.X = v[k]; k++;
                            wei.X = weights[v[k]]; k++;
                            break;
                        case 1:
                            joy.Y = v[k]; k++;
                            wei.Y = weights[v[k]]; k++;
                            break;
                        case 2:
                            joy.Z = v[k]; k++;
                            wei.Z = weights[v[k]]; k++;
                            break;
                        case 3:
                            joy.W = v[k]; k++;
                            wei.W = weights[v[k]]; k++;
                            break;
                        default:
                            break;
                    }
                }

                float sum = wei.X + wei.Y + wei.Z + wei.W;
                wei *= 1.0F / sum;

                weightsTemp.Add(wei);
                jointsTemp.Add(joy);
            }

            return LoadGeometries(element.Document.GetByID(geometryUrl.Replace("#", "")), association, weightsTemp, jointsTemp);
        }

        #endregion


        #region Animation Data

        private static AnimationNode CreateAnimationTrack(XElement animationData)
        {
            AnimationNode track = new AnimationNode();

            //Get source, every source contain input and ouput data (input is time, output is matrix)
            var channel = animationData.GetChild("channel");
            //cerca eventuali sotto animazioni
            foreach (XElement elem in animationData.GetChildren("animation"))
            {
                track.Children.Add(CreateAnimationTrack(elem));
            }

            //search for valid channel
            if (channel == null)
                return track;

            //get all source
            XElement source = channel.GetSource();
            var input = source.GetChildren("input").Where(i => i.GetAttribute("semantic") == "INPUT").FirstOrDefault();
            var output = source.GetChildren("input").Where(i => i.GetAttribute("semantic") == "OUTPUT").FirstOrDefault();
            var in_tangent = source.GetChildren("input").Where(i => i.GetAttribute("semantic") == "IN_TANGENT").FirstOrDefault();
            var out_tangent = source.GetChildren("input").Where(i => i.GetAttribute("semantic") == "OUT_TANGENT").FirstOrDefault();
            var interpolation = source.GetChildren("input").Where(i => i.GetAttribute("semantic") == "INTERPOLATION").FirstOrDefault();


            //tangenti in_out 
            if (in_tangent != null)
                in_tangent = in_tangent.GetSource();
            if (out_tangent != null)
                out_tangent = out_tangent.GetSource();


            //get target of animation
            string target = channel.GetAttribute("target");

            //get parameter of animations (example, name/transform)
            string[] targetParts = channel.GetAttribute("target").Split(new char[] { '/' });
            
            track.Target = targetParts[0];
            string animationType = targetParts[1].ToLower();
            animationType = animationType.Replace("(", " ").Replace(")", " ").Replace(".", " ");
            string[] parts = animationType.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            //keyframes
            track.Output = GetKeyFrames(animationType, output);

            track.In_Tangent = GetKeyFrames(animationType, in_tangent);
            track.Out_Tangent = GetKeyFrames(animationType, out_tangent);

            //input key frame
            input = input.GetSource();
            interpolation = interpolation.GetSource();

            //get interpolation time 
            string interpolationType = interpolation.Value.Replace('\n', ' ').Split(' ').Where(s => !string.IsNullOrEmpty(s)).FirstOrDefault();
            if (interpolationType == "LINEAR")
                track.Interpolation = Interpolation.Linear;
            else if (interpolationType == "BEZIER")
                track.Interpolation = Interpolation.Bezier;
            else
                track.Interpolation = Interpolation.Undefined;

            //get key frame
            track.Input = GetFloatList(input.GetChild("float_array").Value);
            
            return track;
        }

        private static List<float[]> Group(this List<float> values, int n)
        {
            List<float[]> temp = new List<float[]>();
            for (int i = 0; i < values.Count; i += n)
            {
                temp.Add(values.GetRange(i, n).ToArray());
            }
            return temp;
        }

        private static List<Matrix> GetKeyFrames(string animationType, XElement element)
        {
            
            if (element == null)
                return null;
            
            XElement output = null;
            if (element.Attribute("source") != null)
                output = element.GetSource();
            else
                output = element;

            if (output == null)
                return null;

            List<float> values = GetFloatList(output.GetChild("float_array").Value);


            //Create animation data by type
            if (animationType.Contains("rot"))
            {
                if (animationType.Contains("x"))
                {
                    if (animationType.Contains("angle"))
                        return values.Group(1).Select(f => Matrix.RotationX(MathUtil.DegreesToRadians(f.First()))).ToList();
                    else
                        return values.Group(1).Select(f => Matrix.RotationX(f.First())).ToList();
                }
                else if (animationType.Contains("y"))
                    if (animationType.Contains("angle"))
                        return values.Group(1).Select(f => Matrix.RotationY(MathUtil.DegreesToRadians(f.First()))).ToList();
                    else
                        return values.Group(1).Select(f => Matrix.RotationY(f.First())).ToList();
                else if (animationType.Contains("z"))
                    if (animationType.Contains("angle"))
                        return values.Group(1).Select(f => Matrix.RotationZ(MathUtil.DegreesToRadians(f.First()))).ToList();
                    else
                        return values.Group(1).Select(f => Matrix.RotationZ(f.First())).ToList();
                else if (animationType.Contains("angle"))
                {
                    List<Matrix> tempMat = new List<Matrix>();
                    foreach (var v in values.Group(2))
                    {
                        switch ((int)v.First())
                        {
                            case 0:
                                tempMat.Add(Matrix.RotationX(MathUtil.DegreesToRadians(v.Last())));
                                break;
                            case 1:
                                tempMat.Add(Matrix.RotationY(MathUtil.DegreesToRadians(v.Last())));
                                break;
                            case 2:
                                tempMat.Add(Matrix.RotationZ(MathUtil.DegreesToRadians(v.Last())));
                                break;
                        }
                    }
                    return tempMat;
                }
                else
                    return values.Group(4).Select(f => Matrix.RotationAxis(new Vector3(f[0], f[1], f[2]), f[3])).ToList();
            }
            else if (animationType.Contains("translation"))
            {
                if (animationType.Contains("x"))
                    return values.Group(1).Select(f => Matrix.Translation(f.First(), 0, 0)).ToList();
                else if (animationType.Contains("y"))
                    return values.Group(1).Select(f => Matrix.Translation(0, f.First(), 0)).ToList();
                else if (animationType.Contains("z"))
                    return values.Group(1).Select(f => Matrix.Translation(0, 0, f.First())).ToList();
                else
                    return values.Group(3).Select(f => Matrix.Translation(f[0], f[1], f[2])).ToList();
            }
            else if (animationType.Contains("scale"))
            {
                if (animationType.Contains("x"))
                    return values.Group(1).Select(f => Matrix.Scaling(f.First(), 1, 1)).ToList();
                else if (animationType.Contains("y"))
                    return values.Group(1).Select(f => Matrix.Scaling(1, f.First(), 1)).ToList();
                else if (animationType.Contains("z"))
                    return values.Group(1).Select(f => Matrix.Scaling(1, 1, f.First())).ToList();
                else
                    return values.Group(3).Select(f => Matrix.Scaling(f[0], f[1], f[2])).ToList();

            }
            else if (animationType.Contains("transform") || animationType.Contains("matrix"))
            {
                string coordinate = animationType.Replace("transform", "");
                coordinate = coordinate.Replace("matrix", "");
                if (coordinate.Trim() == "")
                    return values.Group(16).Select(f => Matrix.Transpose(new Matrix(f))).ToList();
                else
                {
                    coordinate = coordinate.Replace(")", " ");
                    coordinate = coordinate.Replace("(", " ");
                    coordinate = coordinate.Replace(" ", " ");
                    var parts = coordinate.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Matrix m = Matrix.Identity;

                    if (parts.Length == 2)
                    {
                        int x = int.Parse(parts.First());
                        int y = int.Parse(parts.Last());
                        List<Matrix> tempM = new List<Matrix>();
                        foreach (float f in values)
                        {
                            Matrix mat = Matrix.Identity;
                            mat[x, y] = f;
                            tempM.Add(mat);

                        }
                        return tempM;
                    }
                    else
                    {
                        throw new NotSupportedException("Not Valid Animation Format");
                    }
                }
            }

            throw new NotSupportedException("Not Valid Animation Format");
        }

        private static Interpolation Convert(string text)
        {
            if (text == "LINEAR")
                return Interpolation.Linear;
            else if (text == "BEZIER")
                return Interpolation.Bezier;
            else return (Interpolation)0;
        }

        #endregion


        #region Utilities

        private static XElement GetByID(this XDocument document, string id)
        {
            return document.Descendants().Where(n => n.Attribute("id") != null && n.Attribute("id").Value == id).FirstOrDefault();
        }

        private static XElement GetReference(this XDocument document, XElement reference)
        {
            string url = reference.Attribute("url").Value.Replace("#", "");
            return document.Descendants().Where(n => n.Attribute("id") != null && n.Attribute("id").Value == url).FirstOrDefault();
        }

        private static XElement GetSource(this XDocument document, XElement reference)
        {
            string url = reference.Attribute("source").Value.Replace("#", "");
            return document.Descendants().Where(n => n.Attribute("id") != null && n.Attribute("id").Value == url).FirstOrDefault();
        }

        private static XElement GetReference(this XElement reference)
        {
            string url = reference.Attribute("url").Value.Replace("#", "");
            return reference.Document.Descendants().Where(n => n.Attribute("id") != null && n.Attribute("id").Value == url).FirstOrDefault();
        }

        private static XElement GetSource(this XElement reference)
        {
            string url = reference.Attribute("source").Value.Replace("#", "");
            return reference.Document.Descendants().Where(n => n.Attribute("id") != null && n.Attribute("id").Value == url).FirstOrDefault();
        }


        private static XElement GetNode(this XElement element, string name)
        {
            return element.Descendants(XName.Get(name, ColladaNamespace)).FirstOrDefault();
        }

        private static IEnumerable<XElement> GetNodes(this XElement element, string name)
        {
            return element.Descendants(XName.Get(name, ColladaNamespace));
        }

        private static XElement GetNode(this XDocument document, string name)
        {
            return document.Descendants(XName.Get(name, ColladaNamespace)).FirstOrDefault();
        }

        private static IEnumerable<XElement> GetNodes(this XDocument document, string name)
        {
            return document.Descendants(XName.Get(name, ColladaNamespace));
        }

        private static XElement GetChild(this XElement element, string name)
        {
            return element.Element(XName.Get(name, ColladaNamespace));
        }

        private static IEnumerable<XElement> GetChildren(this XElement element, string name)
        {
            return element.Elements(XName.Get(name, ColladaNamespace));
        }

        private static string GetAttribute(this XElement element, string name)
        {
            var res = element.Attribute(name);
            if (res == null)
                return string.Empty;

            return res.Value;
        }

        private static Vector3 Vector3FromString(string array)
        {
            array = array.Replace('\n', ' ');
            string[] val = array.Split(new char[] { ' ' });
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
            info.NumberDecimalSeparator = ".";
            return new Vector3()
            {
                X = float.Parse(val[0], info),
                Y = float.Parse(val[1], info),
                Z = float.Parse(val[2], info)
            };
        }

        private static Vector4 Vector4FromString(string array)
        {
            array = array.Replace('\n', ' ');
            string[] val = array.Split(new char[] { ' ' });
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
            info.NumberDecimalSeparator = ".";
            return new Vector4()
            {
                X = float.Parse(val[0], info),
                Y = float.Parse(val[1], info),
                Z = float.Parse(val[2], info),
                W = float.Parse(val[3], info),
            };
        }

        private static Matrix MatrixFromString(string array)
        {
            array = array.Replace('\n', ' ');
            string[] val = array.Split(new char[] { ' ' });
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
            info.NumberDecimalSeparator = ".";
            Matrix mat = new Matrix();

            mat.M11 = float.Parse(val[0], info);
            mat.M21 = float.Parse(val[1], info);
            mat.M31 = float.Parse(val[2], info);
            mat.M41 = float.Parse(val[3], info);

            mat.M12 = float.Parse(val[4], info);
            mat.M22 = float.Parse(val[5], info);
            mat.M32 = float.Parse(val[6], info);
            mat.M42 = float.Parse(val[7], info);

            mat.M13 = float.Parse(val[8], info);
            mat.M23 = float.Parse(val[9], info);
            mat.M33 = float.Parse(val[10], info);
            mat.M43 = float.Parse(val[11], info);

            mat.M14 = float.Parse(val[12], info);
            mat.M24 = float.Parse(val[13], info);
            mat.M34 = float.Parse(val[14], info);
            mat.M44 = float.Parse(val[15], info);

            return mat;
        }

        private static List<float> GetFloatList(string array)
        {
            array = array.Replace('\n', ' ');
            List<float> data = new List<float>();

            string[] number = array.Split(new char[] { ' ' });

            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
            info.NumberDecimalSeparator = ".";

            for (int i = 0; i < number.Length; i++)
            {
                if (number[i] != "")
                    data.Add(float.Parse(number[i], info));
            }

            return data;
        }

        private static List<int> GetIntList(string array)
        {
            array = array.Replace('\n', ' ');
            List<int> data = new List<int>();

            string[] number = array.Split(new char[] { ' ' });

            for (int i = 0; i < number.Length; i++)
            {
                if (number[i] != "")
                    data.Add(int.Parse(number[i]));
            }

            return data;
        }

        private static Matrix ToTranspose(this Matrix matrix)
        {
            return Matrix.Transpose(matrix);
        }
        #endregion

        #region Support

        private class ChannelData
        {
            public int IndexOffset { get; set; }

            public string Semantic { get; set; }

            public List<float> Data { get; set; }

            public int Stride { get; set; }

            public int Offset { get; set; }

            public ChannelData(XElement element)
            {
                Semantic = element.Attribute("semantic").Value;

                //get vertices
                var temp = element.GetSource();
                if (temp.Name.LocalName.ToLower() == "vertices")
                {
                    temp = temp.GetNode("input").GetSource();
                }

                //Get stride
                XElement teck = temp.GetNode("technique_common");
                if (teck != null)
                {
                    int s = 1;
                    int.TryParse(temp.GetNode("accessor").Attribute("stride").Value, out s);
                    Stride = s;
                }
                Data = GetFloatList(temp.GetNode("float_array").Value);
            }

            public float[] GetChannel(int pos)
            {
                float[] res = new float[Stride];
                for (int i = 0; i < Stride; i++)
                {
                    res[i] = Data[pos * Stride + i];
                }
                return res;
            }

        }
        
        #endregion

    }
}
