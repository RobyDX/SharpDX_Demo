using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;


namespace TutorialD1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //target of rendering
        WindowRenderTarget target;

        //factory for creating 2D elements
        SharpDX.Direct2D1.Factory factory = new SharpDX.Direct2D1.Factory();
        //this one is for creating DirectWrite Elements
        SharpDX.DirectWrite.Factory factoryWrite = new SharpDX.DirectWrite.Factory();

        //some brush
        SharpDX.Direct2D1.Brush redBrush;
        SharpDX.Direct2D1.Brush whiteBrush;
        SharpDX.Direct2D1.Brush gradient;

        //textformat (equivalent of .Net Font)
        SharpDX.DirectWrite.TextFormat textFormat;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Init Direct Draw
            //Set Rendering properties
            RenderTargetProperties renderProp = new RenderTargetProperties()
            {
                DpiX = 0,
                DpiY = 0,
                MinLevel = FeatureLevel.Level_10,
                PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
                Type = RenderTargetType.Hardware,
                Usage = RenderTargetUsage.None
            };

            //set hwnd target properties (permit to attach Direct2D to window)
            HwndRenderTargetProperties winProp = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(this.ClientSize.Width, this.ClientSize.Height),
                PresentOptions = PresentOptions.Immediately
            };

            //target creation
            target = new WindowRenderTarget(factory, renderProp, winProp);

            //create red and white brushes
            redBrush = new SharpDX.Direct2D1.SolidColorBrush(target, SharpDX.Color.Red);
            whiteBrush = new SharpDX.Direct2D1.SolidColorBrush(target, SharpDX.Color.White);

            //create a linear gradient brush
            var grad = new LinearGradientBrushProperties()
            {
                StartPoint = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2),
                EndPoint = new Vector2(ClientSize.Width, ClientSize.Height)
            };

            var stopCollection = new GradientStopCollection(target, new GradientStop[] 
            {
                new GradientStop(){Color=SharpDX.Color.Azure ,Position=0F},
                new GradientStop(){Color=SharpDX.Color.Yellow,Position=0.2F},
                new GradientStop(){Color=SharpDX.Color.Green,Position=0.4F},
                new GradientStop(){Color=SharpDX.Color.Red,Position=1F},
            }, ExtendMode.Mirror);

            gradient = new SharpDX.Direct2D1.LinearGradientBrush(target, grad, stopCollection);



            //create textformat
            textFormat = new SharpDX.DirectWrite.TextFormat(factoryWrite, "Arial", 36);

            //avoid artifacts
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //draw elements
            Draw();
            //force refresh
            this.Invalidate();
        }

        private void Draw()
        {
            //begin rendering
            target.BeginDraw();
            //clear target
            target.Clear(SharpDX.Color.CornflowerBlue);

            //draw a rounded box
            target.FillRoundedRectangle(new RoundedRectangle()
            {
                RadiusX = 10,
                RadiusY = 10,
                Rect = new SharpDX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height)
            }, gradient);

            //draw some ellipse
            for (int i = 0; i < 20; i++)
            {
                target.DrawEllipse(new Ellipse(new Vector2(ClientSize.Width / 2, ClientSize.Height / 2), 20 * i, 20 * i), redBrush);
            }

            //draw text
            target.DrawText("Hello Direct2D", textFormat, new SharpDX.RectangleF(0, 0, 400, 200), whiteBrush);

            //end drawing
            target.EndDraw();


        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //resize target
            target.Resize(new Size2(this.ClientSize.Width, this.ClientSize.Height));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            redBrush.Dispose();
            //release resource
            redBrush.Dispose();
            whiteBrush.Dispose();
            gradient.Dispose();
            target.Dispose();
            factory.Dispose();
            factoryWrite.Dispose();
        }
    }
}
