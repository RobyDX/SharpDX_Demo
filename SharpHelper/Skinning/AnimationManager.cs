using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;


namespace SharpHelper.Skinning
{
    /// <summary>
    /// Animation Manager
    /// </summary>
    public class AnimationManager
    {
        /// <summary>
        /// Controllers of Animations
        /// </summary>
        public List<AnimationSet> Sets { get; set; }

        /// <summary>
        /// Animation Length
        /// </summary>
        public float Duration { get; private set; }

        internal AnimationManager(Animation animation)
        {
            Sets = new List<AnimationSet>();

            foreach (var n in animation.Nodes)
            {
                //Add valid animation
                if (n.Interpolation != 0)
                {
                    Sets.Add(new AnimationSet(n));
                }
            }

            Duration = 0;
            if (Sets.Count > 0)
                Duration = (from s in Sets select s.Ticks.Max()).Max();

        }
    }


    /// <summary>
    /// Animation Controller
    /// </summary>
    public class AnimationSet
    {

        /// <summary>
        /// Interpolation Type
        /// </summary>
        public InterpolationType InterpolationMode { get; set; }

        /// <summary>
        /// List of keys
        /// </summary>
        public List<float> Ticks { get; set; }

        /// <summary>
        /// Output Matrix Array
        /// </summary>
        public List<AnimationData> Output { get; set; }

        /// <summary>
        /// Input Tangent Matrices (for bezier animation)
        /// </summary>
        public List<AnimationData> Tangent_In { get; set; }

        /// <summary>
        /// Output Tangent Matrices (for bezier animation)
        /// </summary>
        public List<AnimationData> Tangent_Out { get; set; }

        /// <summary>
        /// Name of node that is target of this animation set
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Node target of this animation
        /// </summary>
        public Node TargetNode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node Data to load</param>
        public AnimationSet(AnimationNode node)
        {

            int stride = node.Output.Count / node.Input.Count;
            int count = node.Input.Count;
            Ticks = new List<float>(node.Input);
            Output = node.Output.Select(m => new AnimationData(m)).ToList();

            Tangent_In = new List<AnimationData>();
            Tangent_Out = new List<AnimationData>();
            if (node.In_Tangent != null && node.In_Tangent.Count > 0)
                Tangent_In = node.In_Tangent.Select(m => new AnimationData(m)).ToList();
            if (node.Out_Tangent != null && node.Out_Tangent.Count > 0)
                Tangent_Out = node.Out_Tangent.Select(m => new AnimationData(m)).ToList();
            InterpolationMode = (InterpolationType)node.Interpolation;
            Target = node.Target;
        }

        /// <summary>
        /// Set Current Matrix
        /// </summary>
        /// <param name="tick"></param>
        public void SetTime(float tick)
        {
            TargetNode.PreComputed = GetMatrix(tick);
        }

        /// <summary>
        /// Get Matrix Interpolation
        /// </summary>
        /// <param name="tick">Animation Time</param>
        /// <returns>Computed Matrices</returns>
        public Matrix GetMatrix(float tick)
        {

            //binary search
            float timeA = Ticks.Last();
            float timeB = Ticks.Last();
            int indexA = Ticks.Count - 1;
            int indexB = Ticks.Count - 1;

            int first = 0;
            int last = Ticks.Count - 2;
            while (first <= last)
            {
                int mid = (first + last) / 2;
                if (tick >= Ticks[mid] && tick <= Ticks[mid + 1])
                {
                    timeA = Ticks[mid];
                    timeB = Ticks[mid + 1];
                    indexA = mid;
                    indexB = mid + 1;
                    break;
                }
                else if (tick < Ticks[mid])
                    last = mid - 1;
                else if (tick > Ticks[mid])
                    first = mid + 1;

            }


            //A*i + B(i-1)=C
            //i= (C - B)/(A+B)
            float interpolation = (tick - timeA) / (timeB - timeA);

            //Get Matrix Interpolation
            if (InterpolationMode == InterpolationType.Linear)
            {
                return AnimationData.Lerp(Output[indexA], Output[indexB], interpolation);
            }
            else if (InterpolationMode == InterpolationType.Bezier)
            {
                //bezier
                return AnimationData.Bezier(Output[indexA], Output[indexB], Tangent_In[indexB], Tangent_Out[indexA], interpolation);
            }
            else
                throw new NotImplementedException("Only Linear and Bezier Animation Supported");
        }
    }

    /// <summary>
    /// Animation Data
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// Rotation
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Translation
        /// </summary>
        public Vector3 Translation { get; set; }

        /// <summary>
        /// Scale
        /// </summary>
        public Vector3 Scaling { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix">Matrix to decompose</param>
        public AnimationData(Matrix matrix)
        {
            Vector3 s;
            Vector3 t;
            Quaternion r;
            matrix.Decompose(out s, out r, out t);

            Rotation = r;
            Scaling = s;
            Translation = t;
        }

        /// <summary>
        /// Linear Interpolation
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <param name="interpolation">Value from 0 to 1</param>
        /// <returns>Matrix</returns>
        public static Matrix Lerp(AnimationData start, AnimationData end, float interpolation)
        {
            Quaternion q = Quaternion.Slerp(start.Rotation, end.Rotation, interpolation);
            Vector3 s = Vector3.Lerp(start.Scaling, end.Scaling, interpolation);
            Vector3 t = Vector3.Lerp(start.Translation, end.Translation, interpolation);

            return Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);
        }

        /// <summary>
        /// Bezier Interpolation
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <param name="tangent_in">Input Tangent</param>
        /// <param name="tangent_out">Output Tangent</param>
        /// <param name="interpolation">Value from 0 to 1</param>
        /// <returns>Matrix</returns>
        public static Matrix Bezier(AnimationData start, AnimationData end, AnimationData tangent_in, AnimationData tangent_out, float interpolation)
        {
            //F=P0*(1-s)^3 + 3C0 s(1-s)^2 + 3C1 s^2(1-s) + Ps^3

            float p0 = (1 - interpolation) * (1 - interpolation) * (1 - interpolation);
            float p1 = 3 * interpolation * (1 - interpolation) * (1 - interpolation);
            float p2 = 3 * interpolation * interpolation * (1 - interpolation);
            float p3 = interpolation * interpolation * interpolation;

            Quaternion q = p0 * start.Rotation + p1 * tangent_out.Rotation + p2 * tangent_in.Rotation + p3 * end.Rotation;
            Vector3 s = p0 * start.Scaling + p1 * tangent_out.Scaling + p2 * tangent_in.Scaling + p3 * end.Scaling;
            Vector3 t = p0 * start.Translation + p1 * tangent_out.Translation + p2 * tangent_in.Translation + p3 * end.Translation;

            return Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);
        }
    }

    /// <summary>
    /// Interpolation Type
    /// </summary>
    public enum InterpolationType
    {
        /// <summary>
        /// Linear
        /// </summary>
        Linear = 1,
        /// <summary>
        /// Bezier
        /// </summary>
        Bezier = 2
    }


}
