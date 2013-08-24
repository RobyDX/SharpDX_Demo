using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SharpHelper.Skinning
{
    /// <summary>
    /// Model Node
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// World Matrix
        /// </summary>
        public Matrix World { get; set; }

        //Pre Computed Matrix
        internal Matrix PreComputed { get; set; }
        
        /// <summary>
        /// Children Node
        /// </summary>
        public List<Node> Children { get; private set; }
        
        /// <summary>
        /// Parent
        /// </summary>
        public Node Parent { get; private set; }

        /// <summary>
        /// Skinning information
        /// </summary>
        public SkinData Skinning { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="node">Parent Node</param>
        /// <param name="Model">Sharp Model</param>
        public Node(SharpDevice device, ModelNode node, SharpModel Model)
        {
            
            Name = node.Name;
            World = node.World;
            PreComputed = this.World;
            Children = new List<Node>();

            //Create Child Nodes
            foreach (var m in node.Children)
            {
                var c = new Node(device, m, Model);
                c.Parent = this;
                Children.Add(c);
            }

            //Create Node Geometries
            foreach (var g in node.Geometries)
            {
                var g3d = new Geometry(device, g, node.Skinning != null);
                g3d.Node = this;
                Model.Geometries.Add(g3d);
            }

            //Get Skinning Data
            if (node.Skinning != null)
            {
                Skinning = new SkinData()
                {
                    BindMatrix = node.Skinning.BindMatrix,
                    InverseBindMatrix = new List<Matrix>(node.Skinning.InverseBinding),
                    JointNames = new List<string>(node.Skinning.JointNames),
                };
            }
        }

        /// <summary>
        /// Get Node By Name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Node</returns>
        public Node GetByName(string name)
        {
            foreach (Node n in Children)
            {
                if (n.Name == name)
                    return n;
                else
                {
                    var c = n.GetByName(name);
                    if (c != null)
                        return c;
                }
            }

            return null;
        }

        //Create palette
        internal void CreatePalettes(SharpModel model)
        {
            if (Skinning != null)
                Skinning.Init(model);

            foreach (var n in Children)
            {
                if (n.Skinning != null)
                    n.Skinning.Init(model);
            }
        }

        /// <summary>
        /// Get Computed Node Matrix
        /// </summary>
        /// <returns>Matrix of this Node</returns>
        public Matrix GetNodeMatrix()
        {
            var model = this;
            Matrix currentMat = model.PreComputed;
            while (model.Parent != null)
            {
                model = model.Parent;
                currentMat *= model.PreComputed;
            }

            return currentMat;
        }

        
    }
}
