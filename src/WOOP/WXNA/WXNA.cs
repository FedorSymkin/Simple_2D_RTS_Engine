using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsGraphicsDevice;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;


namespace WOOP
{
    public interface IWTexture
    {
        void fromFile(String filename);
        void fromBitmap(System.Drawing.Bitmap bmp);
        int Width { get; }
        int Height { get; }
    }

    public interface IWXNAControl
    {
        int dps { get; }
        void UserDraw(); //to override 

        void FillAll(System.Drawing.Color color);

        void DrawString(String str, Font font, SolidBrush brush, System.Drawing.Rectangle rect);
        void DrawString(String str, Font font, SolidBrush brush, int x, int y);
        void DrawString(String str, Font font, SolidBrush brush, int x, int y, int w, int h);
        void DrawString(String str, Font font, SolidBrush brush, System.Drawing.Point p);

        void DrawEllipse(Pen pen, int x, int y, int w, int h);
        void DrawEllipse(Pen pen, System.Drawing.Rectangle rect);

        void DrawRectangle(Pen pen, int x, int y, int w, int h);
        void DrawRectangle(Pen pen, System.Drawing.Rectangle rect);

        void DrawLine(Pen pen, System.Drawing.Point start, System.Drawing.Point end);
        void DrawLine(Pen pen, int x1, int y1, int x2, int y2);

        void FillRectangle(SolidBrush brush, int x, int y, int w, int h);
        void FillRectangle(SolidBrush brush, System.Drawing.Rectangle rect);

        void FillEllipse(SolidBrush brush, int x, int y, int w, int h);
        void FillEllipse(SolidBrush brush, System.Drawing.Rectangle rect);

        void DrawPoint(System.Drawing.Point point, System.Drawing.Color color);
        void DrawPolyline(Pen pen, List<System.Drawing.Point> points);

        void DrawImage(IWTexture bitmap, System.Drawing.Rectangle rect);
        void DrawImage(IWTexture bitmap, int x, int y, int w, int h);
    }

    public class WXNAControl : GraphicsDeviceControl, IWXNAControl
    {
        // S E R V I C E     P A R T*======================================================================================================*//
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        Texture2D pixel;
        static public GraphicsDevice gd;
        public System.Drawing.Color backgroundColor = System.Drawing.Color.Black;

        private int myInt = -2;

        protected override void Initialize()
        {
            gd = this.GraphicsDevice;
            this.Dock = System.Windows.Forms.DockStyle.Fill;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, this.Width + myInt, this.Height + myInt, 0, 0, 1);


            


            pixel = new Texture2D(gd, 1, 1);
            pixel.SetData(new Microsoft.Xna.Framework.Color[] { Microsoft.Xna.Framework.Color.White });

            //Application.Idle += delegate { Invalidate(); };
            this.Resize += new EventHandler(onResize);
        }

        int tickmem = Environment.TickCount;
        int fCnt = 0;
        int _dps = 0;
        void defineDPS()
        {
            fCnt++;

            int tick = Environment.TickCount;
            if (tick - tickmem >= 1000)
            {
                _dps = fCnt;
                
                tickmem = tick;
                fCnt = 0;            
            }
        }

        public int dps { get { return _dps; } }

        void onResize(object sender, EventArgs e)
        {
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, this.Width + myInt, this.Height + myInt, 0, 0, 1);           
        }

        protected override void Draw()
        {
            defineDPS();
            FillAll(backgroundColor);
            //spriteBatch.Begin();
            UserDraw();
            //spriteBatch.End();
        }

        public virtual void UserDraw() 
        {
            //empty
        }

        private static System.Drawing.Bitmap getTextBitmap(Font font, String text, System.Drawing.Color color)
        {
            // Get text size
            var bitmap = new System.Drawing.Bitmap(1, 1);
            var graphics = System.Drawing.Graphics.FromImage(bitmap);
            var textSize = graphics.MeasureString(text, font);

            // Draw text on bitmap
            bitmap = new System.Drawing.Bitmap((int)textSize.Width, (int)textSize.Height);
            graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            graphics.DrawString(text, font, new System.Drawing.SolidBrush(color), 0f, 0f);

            return bitmap;
        }

        public static Texture2D TextureFromBitmap(GraphicsDevice gdev, System.Drawing.Bitmap bmp)
        {
            Stream fs = new MemoryStream();
            bmp.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
            var tex = Texture2D.FromStream(gdev, fs);
            fs.Close();
            fs.Dispose();

            return tex;
        }

        Microsoft.Xna.Framework.Rectangle XNARect(System.Drawing.Rectangle stdRect)
        {
            return XNARect(stdRect.X, stdRect.Y, stdRect.Width, stdRect.Height);
        }

        Microsoft.Xna.Framework.Rectangle XNARect(int x, int y, int w, int h)
        {
            return new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
        }


        // * P U B L I C      P A R T======================================================================================================*//

        public void FillAll(System.Drawing.Color color)
        {
            gd.Clear(stdColorToXNAColor(color));
        }


        public void DrawString(String str, Font font, SolidBrush brush, System.Drawing.Rectangle rect)
        { DrawString(str, font, brush, rect.X, rect.Y); }

        public void DrawString(String str, Font font, SolidBrush brush, int x, int y, int w, int h)
        { DrawString(str, font, brush, x, y); }

        public void DrawString(String str, Font font, SolidBrush brush, System.Drawing.Point p)
        { DrawString(str, font, brush, p.X, p.Y); }

        public void DrawString(String str, Font font, SolidBrush brush, int x, int y)
        {
            System.Drawing.Bitmap bmp = getTextBitmap(font, str, brush.Color);
            Texture2D t = TextureFromBitmap(gd, bmp);
            DrawSprite(t, XNARect(x, y, bmp.Width, bmp.Height), Microsoft.Xna.Framework.Color.White);
        }

        void DrawSprite(Texture2D t, Microsoft.Xna.Framework.Vector2 vector2, Microsoft.Xna.Framework.Color backColor)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(t, vector2, backColor);
            spriteBatch.End();
        }

        void DrawSprite(Texture2D t, Microsoft.Xna.Framework.Rectangle rect, Microsoft.Xna.Framework.Color backColor)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(t, rect, backColor);
            spriteBatch.End();
        }

        public void DrawEllipse(Pen pen, int x, int y, int w, int h)
        {
            DrawRectangle(pen, x,y,w,h);
        }

        public void DrawEllipse(Pen pen, System.Drawing.Rectangle rect)
        {
            DrawRectangle(pen, rect);
        }

        public void DrawRectangle(Pen pen, System.Drawing.Rectangle rect)
        { DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height); }

        public void DrawRectangle(Pen pen, int x, int y, int w, int h)
        {
            int r = x + w;
            int b = y + h;

            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            System.Drawing.Point p1 = new System.Drawing.Point(x, y);
            System.Drawing.Point p2 = new System.Drawing.Point(r, y);
            System.Drawing.Point p3 = new System.Drawing.Point(r, b);
            System.Drawing.Point p4 = new System.Drawing.Point(x, b);
            
            DrawLine(pen, p1, p2);
            DrawLine(pen, p2, p3);
            DrawLine(pen, p3, p4);
            DrawLine(pen, p4, p1);
        }

        public void DrawLine(Pen pen, System.Drawing.Point start, System.Drawing.Point end)
        {
            DrawLine(pen, start.X, start.Y, end.X, end.Y);
        }


        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            points.Add(new System.Drawing.Point(x1,y1));
            points.Add(new System.Drawing.Point(x2,y2));
            DrawPolyline(pen, points);
        }

        public void DrawImage(IWTexture bitmap, System.Drawing.Rectangle rect)
        { DrawImage(bitmap, rect.X, rect.Y, rect.Width, rect.Height); }
        
        public void DrawImage(IWTexture bitmap, int x, int y, int w, int h)
        {
            XNABitmap b = (XNABitmap)bitmap;
            DrawSprite(b.XNATex, XNARect(x, y, w, h), Microsoft.Xna.Framework.Color.White);
        }

        Texture2D clrTexture = null;
        public void FillRectangle(SolidBrush brush, System.Drawing.Rectangle rect)
        { FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height); }

        public void FillRectangle(SolidBrush brush, int x, int y, int w, int h)
        {
            clrTexture = new Texture2D(gd, 1, 1, false, SurfaceFormat.Color);
            clrTexture.SetData<Microsoft.Xna.Framework.Color>(new Microsoft.Xna.Framework.Color[] { stdColorToXNAColor(brush.Color) });

            DrawSprite(clrTexture, XNARect(x, y, w, h), Microsoft.Xna.Framework.Color.White);
        }

        public void FillEllipse(SolidBrush brush, int x, int y, int w, int h)
        {
            FillRectangle(brush, x,y,w,h);
        }

        public void FillEllipse(SolidBrush brush, System.Drawing.Rectangle rect)
        {
            FillRectangle(brush, rect);
        }

        public void DrawPoint(System.Drawing.Point point, System.Drawing.Color color)
        {
            //DrawLine(new Pen(color), point, new System.Drawing.Point(point.X + 1, point.Y + 1));
            //DrawSprite(pixel, new Vector2(point.X, point.Y), stdColorToXNAColor(color));
        }

        public void DrawPolyline(Pen pen, List<System.Drawing.Point> points)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[points.Count];
            int i = 0;
            foreach (var p in points)
            {
                vertices[i].Position = new Vector3(p.X, p.Y, 0);
                vertices[i].Color.A = pen.Color.A;
                vertices[i].Color.R = pen.Color.R;
                vertices[i].Color.G = pen.Color.G;
                vertices[i].Color.B = pen.Color.B;
                i++;
            }

            basicEffect.CurrentTechnique.Passes[0].Apply();
            gd.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, 1);
        }

        Microsoft.Xna.Framework.Color stdColorToXNAColor(System.Drawing.Color color)
        {
            Microsoft.Xna.Framework.Color XnaColor = new Microsoft.Xna.Framework.Color();
            XnaColor.A = color.A;
            XnaColor.R = color.R;
            XnaColor.G = color.G;
            XnaColor.B = color.B;
            return XnaColor;
        }


    }

    public class XNABitmap : IWTexture
    {
        Texture2D _XNATex = null;
        public Texture2D XNATex
        {
            get
            {
                if (_XNATex == null)
                {
                    if (_file != "") fromFile(_file);
                    else if (_bitm != null) fromBitmap(_bitm);
                }
                return _XNATex;
            }
        }


        static System.Drawing.Color alphaColor = System.Drawing.Color.Black;
        void LoadAlpha()
        {
            Microsoft.Xna.Framework.Color[] cdata = new Microsoft.Xna.Framework.Color[XNATex.Width * XNATex.Height];
            XNATex.GetData<Microsoft.Xna.Framework.Color>(cdata);
            for (int i = 0; i < cdata.Count(); i++)
            {
                var clr = cdata[i];

                if ((clr.R == alphaColor.R) && (clr.G == alphaColor.G) && (clr.B == alphaColor.B)) cdata[i].A = 0;
            }
            XNATex.SetData<Microsoft.Xna.Framework.Color>(cdata);
        }

        public XNABitmap(String filename)
        {
            fromFile(filename);
        }

        public XNABitmap(Bitmap bitmap)
        {
            fromBitmap(bitmap);
        }

        String _file = "";
        public void fromFile(String filename)
        {
            _file = filename;
            _bitm = null;
            if (WXNAControl.gd != null)
            {
                using (FileStream fs = File.OpenRead(filename))
                {
                    _XNATex = Texture2D.FromStream(WXNAControl.gd, fs);
                    LoadAlpha();
                }
            }
        }

        Bitmap _bitm = null;
        public void fromBitmap(System.Drawing.Bitmap bmp)
        {
            _bitm = bmp;
            _file = "";
            if (WXNAControl.gd != null)
            {
                _XNATex = WXNAControl.TextureFromBitmap(WXNAControl.gd, bmp);
                LoadAlpha();
            }
        }

        public int Width{ get { return XNATex.Width; }}
        public int Height { get { return XNATex.Width; } }   
    }
}
