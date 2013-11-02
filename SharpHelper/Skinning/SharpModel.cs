using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;


namespace SharpHelper.Skinning
{
    /// <summary>
    /// Animated Model
    /// </summary>
    public class SharpModel : IDisposable
    {
        /// <summary>
        /// Model Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Device Pointer
        /// </summary>
        public SharpDevice Device { get; protected set; }

        /// <summary>
        /// Geometries loaded
        /// </summary>
        public List<Geometry> Geometries { get; private set; }

        /// <summary>
        /// Children Nodes
        /// </summary>
        public List<Node> Children { get; private set; }

        /// <summary>
        /// Animation Manager
        /// </summary>
        public List<AnimationManager> Animations { get; private set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="model">Model Data</param>
        public SharpModel(SharpDevice device, ModelData model)
        {
            this.Name = model.Name;
            this.Device = device;

            Children = new List<Node>();
            Geometries = new List<Geometry>();
            Animations = new List<AnimationManager>();

            foreach (var m in model.Nodes)
            {
                var c = new Node(Device, m, this);
                Children.Add(c);
            }

            //Animations
            Animations = new List<AnimationManager>();
            foreach (Animation anim in model.Animations)
            {
                //carica le animazioni
                Animations.Add(new AnimationManager(anim));
            }

            Initialize();
            SetTime(0);
        }


        internal void Initialize()
        {
            //Associate node to manager
            foreach (AnimationManager man in Animations)
            {
                AssociateNode(man);
            }

            //Create Matrix Palette
            foreach (Node n in Children)
            {
                n.CreatePalettes(this);
            }
        }

        //Associate Node to correct animation set
        private void AssociateNode(AnimationManager manager)
        {
            foreach (var a in manager.Sets)
            {
                a.TargetNode = GetNodeByName(a.Target);
            }
        }

        /// <summary>
        /// Get Node by name
        /// </summary>
        /// <param name="name">Node Name</param>
        /// <returns>Node or null</returns>
        public Node GetNodeByName(string name)
        {
            var val = Children.Where(n => n.Name == name).FirstOrDefault();
            if (val != null)
                return val;

            val = Children.Select(n => n.GetByName(name)).Where(n => n != null).FirstOrDefault();
            if (val == null)
                return null;

            return val;
        }

        /// <summary>
        /// Set animation times
        /// </summary>
        /// <param name="tick">Ticks</param>
        public void SetTime(float tick)
        {
            if (Animations.Count > 0)
            {
                foreach (var a in this.Animations.First().Sets)
                {
                    a.SetTime(tick);
                }
            }
        }

        /// <summary>
        /// Draw Animated Model
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="information">Information</param>
        public void Draw(SharpDevice device,SkinShaderInformation information)
        {
            //Iterate each geometry
            foreach (Geometry g in Geometries)
            {
                g.Apply(information);
                g.Draw(device.DeviceContext);
            }
        }

        /// <summary>
        /// Dispose all
        /// </summary>
        public void Dispose()
        {
            foreach (var g in Geometries)
                g.Dispose();
        }
    }
}
