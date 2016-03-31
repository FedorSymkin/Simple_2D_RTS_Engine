using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using WOOP;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Drawing.Drawing2D;


namespace WOOP
{
    public static class WUtilites
    {
        public static WUnit getMedianUnit(List<WUnit> units)
        {
            if (units.Count == 0) return null;
            if (units.Count <= 2) return units[0];

            List<WUnit> sorted = new List<WUnit>(units);
            sorted.Sort(new Comparison<WUnit>((unit1, unit2) => unit1.pos.X - unit2.pos.X));
            int medianX = sorted[units.Count / 2].pos.X;

            sorted.Sort(new Comparison<WUnit>((unit1, unit2) => unit1.pos.Y - unit2.pos.Y));
            int medianY = sorted[units.Count / 2].pos.Y;

            Point median = new Point(medianX, medianY);

            WUnit res = null;
            int minRange = 0;

            foreach (var unit in sorted)
            {
                int range = calc2Drange(unit.pos, median);
                if ((res == null) || (range < minRange))
                {
                    res = unit;
                    minRange = range;
                }
            }

            return res;
        }

        public static int calcGroupFragmentationRate(List<WUnit> units)
        {
            WUnit median = getMedianUnit(units);
            int maxRange = 0;
            foreach (var unit in units)
            {
                int range = calc2Drange(unit.pos, median.pos);
                if (range > maxRange) maxRange = range;
            }

            return maxRange;
        }

        public static void MixBitmapsTransparency(Bitmap dest, Bitmap src)
        {
                for (int x = 0; x < src.Width; x++)
                    for (int y = 0; y < src.Height; y++)
                    {
                        Color p = src.GetPixel(x, y);
                        if (!((p.R == 255) && (p.G == 255) && (p.B == 255)))
                        {
                            dest.SetPixel(x, y, p);
                        }
                    }               
        }

        public static Bitmap RotateImage(Bitmap img, float rotationAngle)
        {
            //create an empty Bitmap image


            Bitmap bmp = new Bitmap(img);

          //  System.Console.WriteLine(img.PixelFormat);
            //System.Console.WriteLine(bmp.PixelFormat);


            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            WUtilites.ReplacePixels(ref bmp, Color.Black, Color.White);
            //return the image
            return bmp;
        }


        public static bool IsType(Type children, Type parent)
        {
            return ((children.IsSubclassOf(parent)) || (children == parent));
        }

        public static void ReplacePixels(ref Bitmap bmp, Color from, Color to)
        {
            for (int x = 0; x < bmp.Width; x++)
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color c = bmp.GetPixel(x, y);
                    if ((c.R == from.R) && (c.G == from.G) && (c.B == from.B))
                    {
                        bmp.SetPixel(x, y, to);
                    }
                }
        }

        public static IEnumerable<Type> GetSubtypes(Type parent)
        {
            return Assembly.GetAssembly(typeof(WUtilites)).GetTypes()
                           .Where(type => parent.IsAssignableFrom(type));
        }

        static public Rectangle AreaDiv(Rectangle area, float scale)
        {
            RectangleF res = new RectangleF();
            res.X = (float)area.X + (1 - scale) * (float)area.Width / 2;
            res.Y = (float)area.Y + (1 - scale) * (float)area.Height / 2;
            res.Width = (float)area.Width * scale;
            res.Height = (float)area.Height * scale;

            return new Rectangle((int)res.X, (int)res.Y, (int)res.Width, (int)res.Height);
        }

        static public Point TopLeft(Rectangle r) { return new Point(r.Left, r.Top); }
        static public Point BottomRight(Rectangle r) { return new Point(r.Right, r.Bottom); }
        static public void SetTopLeft(ref Rectangle r, Point value) {r.Location = value;}
        static public void SetBottomRight(ref Rectangle r, Point value) 
        {
            r.Size = new Size(value.X - r.Location.X, value.Y - r.Location.Y); 
        }

        static public int calc2Drange(Point p1, Point p2)
        {
            int dx = (Math.Abs(p2.X - p1.X));
            int dy = (Math.Abs(p2.Y - p1.Y));

            if (dx >= dy) return dx; else return dy;
        }

        static public double angleBetweenPoints(Point p1, Point p2)
        {
            if (p1 == p2) return 0;

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double r = Math.Sqrt(dx * dx + dy * dy);
            double res = Math.Acos(dx/r);
            if (dy >= 0) res = -res;
            return res;
        }

        static public Rectangle SetRectXYXY(int x1, int x2, int y1, int y2)
        {
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        static public Rectangle SetRectXYXY(Point topLeft, Point bottomRight)
        {
            return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }


        static public void setAnchors(Control c)
        {
            c.Anchor = AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left;
        }

        static public Bitmap CopyBitmap(Bitmap srcBitmap, Rectangle section)
        {
            return srcBitmap.Clone(section, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        static public Point rectCenter(Rectangle rect)
        {
            return new Point(rect.X + rect.Width/2, rect.Y + rect.Height/2);
        }

        static public Rectangle setRect(Point topLeft, Point bottomRight)
        {
            return setRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        }

        static public Rectangle setRect(int x1, int y1, int x2, int y2)
        {
            Rectangle r = new Rectangle();

            int w = x2 - x1;
            int h = y2 - y1;

            if (w < 0)
            {
                w = -w;
                r.X = x2;
            }
            else r.X = x1;



            if (h < 0)
            {
                h = -h;
                r.Y = y2;
            }
            else r.Y = y1;



            r.Width = w;
            r.Height = h;

            return r;
        }

        static IWTexture _NoTextureStub = null;
        public static IWTexture NoTextureStub()
        {
            if (_NoTextureStub == null)
            {
                try
                {
                    _NoTextureStub = new XNABitmap(W.core.path + "/noTextureStub.png");
                    return _NoTextureStub;
                }
                catch
                {
                    W.core.FatalError("Cannot load NoTextureStub");
                    return null;
                }
            } else return _NoTextureStub;
        }   
    }

  
    public class WTimer : IDisposable
    {
        public delegate void onTimerEvent(WTimer sender);

        public int intervalMs = 1000;
        public onTimerEvent onTimer;
        public void start() { counter = 0; enabled = true; }
        public void stop() { enabled = false; }
        public bool enabled = false;

        uint counter = 0;
        public WTimer(onTimerEvent onTimer, int intervalMs)
        {
            this.onTimer = onTimer;
            this.intervalMs = intervalMs;

            W.core.timers.Add(this);
        }

        public void tick(uint dt)
        {
            if (enabled)
            {
                counter += dt;

                if (counter >= intervalMs)
                {
                    onTimer(this);
                    counter = 0;
                }
            }
        }

        public void Dispose()
        {
            W.core.timers.Remove(this);
        }
    }
}
