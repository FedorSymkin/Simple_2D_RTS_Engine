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
using System.Collections;

namespace WOOP
{
    public class TexturesLine : List<List<IWTexture>>
    {
        public static void PrepareTexture(ref Bitmap pict)
        {
            ApplyColor(ref pict, Color.Black, Color.FromArgb(1, 1, 1));
            ApplyColor(ref pict, Color.White, Color.Black);
            ApplyColor(ref pict, Color.FromArgb(127, 127, 127), Color.Black);
        }

        int getFramesCount(String filename) //format: <fname>frXX.bmp, where XX - count of frames
        {
            String strCnt = filename;
            strCnt = strCnt.Remove(strCnt.Count() - 4, 4); //remove ".bmp"
            strCnt = Regex.Split(strCnt, "_fr").Last();
            return Convert.ToInt32(strCnt);
        }

        public bool loadFromSingleDir(String dir, bool rotates)
        {
            try
            {
                string[] files = Directory.GetFiles(dir, "*.bmp");
                if (files.Count() == 1)
                {
                    String fname = files[0];
                    return this.loadFromFile(fname, rotates);
                }
                else
                {
                    logm("loadFromSingleDir error: dir contains != 1 bmp files. Dir = " + dir);
                    return false;
                }
            }
            catch
            {
                logm("Error: cannot load textures from dir " + dir);
                return false;
            }
        }

        static Color playerTransparentColor = Color.FromArgb(255, 237, 28, 36);
        static void ApplyColor(ref Bitmap bmp, Color from, Color to)
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

        public bool loadFromFile(String filename, bool rotates, Color? color = null)
        {
            try
            {
                int framesCount = getFramesCount(filename);

                Bitmap pict = new Bitmap(filename);
                PrepareTexture(ref pict);

                int rotatesCnt = rotates ? 8 : 1;
                int w = pict.Width / framesCount;
                int h = pict.Height / rotatesCnt;

                for (int r = 0; r < rotatesCnt; ++r)
                {
                    List<IWTexture> simpleLine = new List<IWTexture>();
                    for (int i = 0; i < framesCount; i++)
                    {
                        Bitmap b = WUtilites.CopyBitmap(pict, new Rectangle(i * w, r * h, w, h));
                        if (color != null) ApplyColor(ref b, playerTransparentColor, color.Value);

                        IWTexture frame = new XNABitmap(b);     
                        simpleLine.Add(frame);
                    }
                    this.Add(simpleLine);
                }

                logm("Textures loaded: " + filename);
                return true;
            }
            catch
            {
                logm("Error: cannot load textures file " + filename);
                return false;
            }
        }

        String LogTag { get { return ""; } }
        void logm(String text) { W.core.textLogs.TexturesLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    } 
}