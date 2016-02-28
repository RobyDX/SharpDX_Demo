using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;

namespace SharpHelper
{
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.Mathematics.Interop;
    /// <summary>
    /// Permit to draw font
    /// </summary>
    public class SharpBatch : IDisposable
    {
        /// <summary>
        /// Device reference
        /// </summary>
        public SharpDevice Device { get; private set; }

        private SharpDX.DirectWrite.TextFormat _directWriteTextFormat;
        private SharpDX.Direct2D1.SolidColorBrush _directWriteFontColor;
        private SharpDX.Direct2D1.RenderTarget _direct2DRenderTarget;
        

        private string _fontName = "Calibri";
        private int _fontSize = 14;
        private Color _fontColor = Color.White;

        /// <summary>
        /// Create a batch manager for drawing text and sprite
        /// </summary>
        /// <param name="device">Device pointer</param>
        internal SharpBatch(SharpDevice device)
        {
            Device = device;
        }
        

        /// <summary>
        /// Begin a 2D drawing session
        /// </summary>
        public void Begin()
        {
            if (_direct2DRenderTarget != null)
                _direct2DRenderTarget.BeginDraw();
        }

        /// <summary>
        /// End drawing session
        /// </summary>
        public void End()
        {
            if (_direct2DRenderTarget != null)
                _direct2DRenderTarget.EndDraw();
        }

        /// <summary>
        /// Release all resources
        /// </summary>
        internal void Release()
        {
            Utilities.Dispose(ref _directWriteTextFormat);
            Utilities.Dispose(ref _directWriteFontColor);
            Utilities.Dispose(ref _direct2DRenderTarget);
        }

        /// <summary>
        /// Update all resources
        /// </summary>
        /// <param name="backBuffer">BackBuffer</param>
        internal void UpdateResources(SharpDX.Direct3D11.Texture2D backBuffer)
        {
            
            var d2dFactory = new SharpDX.Direct2D1.Factory();
            var d2dSurface = backBuffer.QueryInterface<Surface>();
            _direct2DRenderTarget = new SharpDX.Direct2D1.RenderTarget(d2dFactory, d2dSurface, new SharpDX.Direct2D1.RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
            d2dSurface.Dispose();
            d2dFactory.Dispose();

            var directWriteFactory = new SharpDX.DirectWrite.Factory();
            _directWriteTextFormat = new SharpDX.DirectWrite.TextFormat(directWriteFactory, _fontName, _fontSize) { TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near };
            _directWriteFontColor = new SharpDX.Direct2D1.SolidColorBrush(_direct2DRenderTarget, _fontColor);
            directWriteFactory.Dispose();
        }

        /// <summary>
        /// Set font parameters
        /// </summary>
        /// <param name="fontColor">Font Color</param>
        /// <param name="fontName">Font Name</param>
        /// <param name="fontSize">Font Size</param>
        public void SetFont(Color fontColor, string fontName, int fontSize)
        {
            _fontColor = fontColor;
            _fontName = fontName;
            _fontSize = fontSize;

            var directWriteFactory = new SharpDX.DirectWrite.Factory();
            _directWriteTextFormat = new SharpDX.DirectWrite.TextFormat(directWriteFactory, _fontName, _fontSize) { TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading, ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near };
            _directWriteFontColor = new SharpDX.Direct2D1.SolidColorBrush(_direct2DRenderTarget, _fontColor);
            directWriteFactory.Dispose();
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="x">Left position</param>
        /// <param name="y">Top position</param>
        /// <param name="width">Max width</param>
        /// <param name="height">Max heigh</param>
        public void DrawString(string text, int x, int y, int width = 800, int height = 600)
        {
            if (_directWriteFontColor == null)
                return;

            _direct2DRenderTarget.DrawText(text, _directWriteTextFormat, new RawRectangleF(x, y, width, height), _directWriteFontColor);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Utilities.Dispose(ref _directWriteTextFormat);
            Utilities.Dispose(ref _directWriteFontColor);
            Utilities.Dispose(ref _direct2DRenderTarget);
        }


    }
}
