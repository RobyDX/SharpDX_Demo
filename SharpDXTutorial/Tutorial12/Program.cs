using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpHelper;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Tutorial12
{
    public struct ResultData
    {
        public float functionResult;
        public int x;
        public float unused;
        public float unused2;

        public override string ToString()
        {
            return string.Format("X: {0} Y: {1}", x, functionResult);
        }
    }

    static class Program
    {


        static double FACT(int n)
        {
            double tot = 1.0;
            while (n > 1)
            {
                tot *= (float)n;
                n--;
            }
            return tot;
        }

        static float MacLaurin(float x)
        {
            float tot = 1;
            for (int i = 0; i < 10; i++)
            {
                tot += (float)(Math.Pow(x, i) / FACT(i));
            }
            return tot;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (!SharpDevice.IsDirectX11Supported())
            {
                System.Windows.Forms.MessageBox.Show("DirectX11 Not Supported");
                return;
            }

            int repetition = (64 * 32) * (64 * 32);
            SharpComputeDevice<ResultData> computer = new SharpComputeDevice<ResultData>("../../HLSL.txt", "CS", repetition);

            Console.WriteLine("Executing Mac Laurin Series of Sin(x)");
            Console.WriteLine("With x that go from 0 to " + repetition);
            Console.WriteLine();

            //Start Compute Shader Algorithm
            Console.WriteLine("STARTING ComputeShader Algorithm");

            //start timer
            Stopwatch st = new Stopwatch();
            st.Start();

            //execute compute shader
            computer.Begin();
            computer.Start(64, 64, 1);
            computer.End();

            //stop timer
            st.Stop();

            //get result
            ResultData[] data = computer.ReadData(repetition);


            int csTime = (int)st.ElapsedMilliseconds;

            Console.WriteLine(string.Format("Compute Shader Time: {0} ms", csTime));

            //Start CPU Algorithm
            Console.WriteLine();
            Console.WriteLine("STARTING CPU Algorithm");
            float[] values = new float[repetition];
            st.Start();
            for (int i = 0; i < repetition; i++)
            {
                values[i] = MacLaurin(i / 1000.0F);
            }
            st.Stop();
            int cpuTime = (int)st.ElapsedMilliseconds;
            Console.WriteLine(string.Format("CPU Time: {0} ms", cpuTime));


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(string.Format("Your GPU is {0} times better than your CPU ", cpuTime / csTime));
            Console.WriteLine();
            Console.WriteLine("Check Sample Results");

            for (int i = 1; i < 10; i++)
            {
                int x = i * 10000;
                Console.WriteLine(string.Format("TEST {0} ComputeShader: {1} CPU: {2} ", i, data[x].functionResult, values[x]));
            }
            
            Console.Read();

        }
    }
}
