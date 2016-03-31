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
    public class ShellTextures
    {
        IWTexture[] mainTextures = new XNABitmap[360];
        TexturesLine animations = new TexturesLine();
        Bitmap sample;

        public void loadFromDir(String dir)
        {
            string[] files = Directory.GetFiles(dir, "*.bmp");
            foreach (var fpath in files)
            {
                String fname = Path.GetFileName(fpath);
                if (fname == "shell.bmp") sample = new Bitmap(fpath);
                else if (fname.Contains("animate")) animations.loadFromFile(fpath, false);
            }
        }

        public IWTexture getMainTexture(int angleDeg)
        {
            int a = angleDeg;
            while (a < 0) a += 360;
            while (a >= 360) a -= 360;

            if (mainTextures[a] == null) loadRotatedTexture(a);

            return mainTextures[a];
        }

        void loadRotatedTexture(int angleDeg)
        {
            Bitmap rb = WUtilites.RotateImage(sample, angleDeg);
            TexturesLine.PrepareTexture(ref rb);
            mainTextures[angleDeg] = new XNABitmap(rb);
        }

        public int AnimateTextureFramesCount()
        {
            return animations[0].Count;
        }

        public IWTexture getAnimateTexture(int i)
        {
            return animations[0][i];
        }
    }
}