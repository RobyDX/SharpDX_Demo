using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SharpHelper.Skinning
{
    /// <summary>
    /// Skinning Information
    /// </summary>
    public class SkinData
    {
        /// <summary>
        /// Bind Matrix
        /// </summary>
        public Matrix BindMatrix { get; set; }

        /// <summary>
        /// Inverse Bind Matrix
        /// </summary>
        public List<Matrix> InverseBindMatrix { get; set; }

        /// <summary>
        /// Joint
        /// </summary>
        public List<Node> JointNodes { get; private set; }

        /// <summary>
        /// Joint Names
        /// </summary>
        public List<string> JointNames { get; set; }


        internal void Init(SharpModel model)
        {
            Matrix[] m = new Matrix[256];
            m = m.Select(me => Matrix.Identity).ToArray();

            List<Matrix> tempMatrices = new List<Matrix>();
            for (int i = 0; i < JointNames.Count; i++)
            {
                string s = JointNames[i];
                var node = model.GetNodeByName(s);

                JointNodes.Add(node);
            }
        }


        internal Matrix[] GetPalette()
        {
            Matrix[] m = new Matrix[256];
            m = m.Select(me => Matrix.Identity).ToArray();
            int i = 0;

            foreach (Node n in JointNodes)
            {
                var node = n;

                Matrix currentMat = node.PreComputed;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    currentMat *= node.PreComputed;
                }

                m[i] = BindMatrix * InverseBindMatrix[i] * currentMat;
                
                i++;
            }
            return m;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkinData()
        {
            BindMatrix = Matrix.Identity;
            InverseBindMatrix = new List<Matrix>();
            JointNodes = new List<Node>();
            JointNames = new List<string>();
        }
    }
}
