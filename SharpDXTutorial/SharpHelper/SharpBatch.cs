using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

namespace SharpHelper
{
    /// <summary>
    /// Permit to draw font
    /// </summary>
    public class SharpBatch : IDisposable
    {
        /// <summary>
        /// Graphics device pointer
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Device reference
        /// </summary>
        public SharpDevice Device { get; private set; }

        /// <summary>
        /// Main font for drawing text
        /// </summary>
        public SpriteFont Font { get; private set; }

        /// <summary>
        /// Batch
        /// </summary>
        public SpriteBatch Batch { get; set; }

        /// <summary>
        /// Create a batch manager for drawing text and sprite
        /// </summary>
        /// <param name="device">Device pointer</param>
        /// <param name="filename">Path of the font file</param>
        public SharpBatch(SharpDevice device, string filename)
        {
            Device = device;
            GraphicsDevice = GraphicsDevice.New(Device.Device);
            GraphicsDevice.SetViewports(Device.DeviceContext.Rasterizer.GetViewports()[0]);
            Batch = new SpriteBatch(GraphicsDevice);
            Font = SpriteFont.Load(GraphicsDevice, filename);

        }

        /// <summary>
        /// Begin a 2D drawing session
        /// </summary>
        public void Begin()
        {
            Batch.Begin(SpriteSortMode.Immediate, null);
        }

        /// <summary>
        /// End drawing session
        /// </summary>
        public void End()
        {
            Batch.End();
        }

        /// <summary>
        /// Resize graphics device
        /// </summary>
        public void Resize()
        {
            GraphicsDevice.SetViewports(Device.DeviceContext.Rasterizer.GetViewports()[0]);
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        public void DrawString(string text, int x, int y)
        {
            Batch.DrawString(Font, text, new Vector2(x, y), Color.Black);
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="color">Text color</param>
        public void DrawString(string text, int x, int y, Color color)
        {
            Batch.DrawString(Font, text, new Vector2(x, y), color);
        }



        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Font.Dispose();
        }

    }
}
