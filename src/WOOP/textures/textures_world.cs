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
    //<Side, Single border texture or Single corner texture>
    public class SingleBordersBank : Dictionary<String, Bitmap>
    {
        public void make(Bitmap borders, Size frameSize, int randomIndex)
        {
            int w = frameSize.Width;
            int h = frameSize.Height;
            int x = 0;
            int y = randomIndex * h;
            Bitmap border = WUtilites.CopyBitmap(borders, new Rectangle(x, y, w, h));
            x = w;
            Bitmap corner = WUtilites.CopyBitmap(borders, new Rectangle(x, y, w, h));


            this.Add("Up", new Bitmap(border));
            border.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.Add("Right", new Bitmap(border));
            border.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.Add("Down", new Bitmap(border));
            border.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.Add("Left", new Bitmap(border));

            this.Add("UpRight", new Bitmap(corner));
            this.Add("RightUp", new Bitmap(corner));
            corner.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.Add("RightDown", new Bitmap(corner));
            this.Add("DownRight", new Bitmap(corner));
            corner.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.Add("DownLeft", new Bitmap(corner));
            this.Add("LeftDown", new Bitmap(corner));
            corner.RotateFlip(RotateFlipType.Rotate90FlipNone);
            this.Add("LeftUp", new Bitmap(corner));
            this.Add("UpLeft", new Bitmap(corner));
        }

        public Bitmap get(String key)
        {
            Bitmap b = null;
            TryGetValue(key, out b);
            return b;
        }
    }

    //<Signature, mixed border texture>
    public class BordersBank : List<Bitmap>
    {
        public void make(Bitmap borders, Size frameSize, int randomIndex)
        {
            SingleBordersBank sb = new SingleBordersBank();
            sb.make(borders, frameSize, randomIndex);

            for (int i = 0; i < 16; i++)
            {
                Bitmap res = new Bitmap(frameSize.Width, frameSize.Height);
                using (Graphics g = Graphics.FromImage(res))
                {
                    g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, frameSize.Width, frameSize.Height));
                }

                bool b0 = ((i & 0x1) != 0);
                bool b1 = ((i & 0x2) != 0);
                bool b2 = ((i & 0x4) != 0);
                bool b3 = ((i & 0x8) != 0);


                if (b0) WUtilites.MixBitmapsTransparency(res, sb.get("Up"));
                if (b1) WUtilites.MixBitmapsTransparency(res, sb.get("Right"));
                if (b2) WUtilites.MixBitmapsTransparency(res, sb.get("Down"));
                if (b3) WUtilites.MixBitmapsTransparency(res, sb.get("Left"));

                if (b0 && b1) WUtilites.MixBitmapsTransparency(res, sb.get("UpRight"));
                if (b1 && b2) WUtilites.MixBitmapsTransparency(res, sb.get("RightDown"));
                if (b2 && b3) WUtilites.MixBitmapsTransparency(res, sb.get("DownLeft"));
                if (b3 && b0) WUtilites.MixBitmapsTransparency(res, sb.get("LeftUp"));

                this.Add(res);
            }
        }
    }





    //<Signature, texture>
    public class WorldTexturesCell : List<IWTexture>
    {
        public void make(Bitmap src, BordersBank borders)
        {
            Bitmap templ = new Bitmap(src);
            WUtilites.ReplacePixels(ref templ, Color.White, Color.Black);

            for (int i = 0; i < 16; i++)
            {
                Bitmap res = new Bitmap(templ);
                if (i > 0)
                {
                    WUtilites.MixBitmapsTransparency(res, borders[i]);
                }

                IWTexture tex = new XNABitmap(res);
                this.Add(tex);
            }
        }
    }

    //<Terrain code, texture cell>
    public class WorldTexturesBank : List<WorldTexturesCell>
    {
        public void loadFromDir(String dir, BordersBank borders)
        {
            for (int i = 0; ; i++)
            {
                String srcFile = dir + "/" + i + ".bmp";
                if (File.Exists(srcFile))
                {
                    Bitmap src = new Bitmap(srcFile);
                    WorldTexturesCell cell = new WorldTexturesCell();
                    cell.make(src, borders);
                    this.Add(cell);
                }
                else break;
            }
        }
    }

    //<Random, Bank of whole textures>
    public class WorldTextures : List<WorldTexturesBank>
    {
        public IWTexture getTexture(int terrainType, int bordersSignature, int randomValue)
        {
            if (this.Count > 0)
            {
                WorldTexturesBank bank = this[randomValue % this.Count];
                WorldTexturesCell cell = bank[terrainType];
                IWTexture tex = cell[bordersSignature];
                return tex;
            }

            return WUtilites.NoTextureStub();
        }

        public void loadFromDir(String dir)
        {
            Bitmap example = new Bitmap(dir + "/0.bmp");
            Bitmap bordersTotal = new Bitmap(dir + "/borders.bmp");
            //WUtilites.ReplacePixels(ref bordersTotal, Color.White, Color.Black);

            Size frameSize = example.Size;
            int randomsCount = bordersTotal.Height / frameSize.Height;
            for (int i = 0; i < randomsCount; i++)
            {
                BordersBank bb = new BordersBank();
                bb.make(bordersTotal, frameSize, i);

                WorldTexturesBank bank = new WorldTexturesBank();
                bank.loadFromDir(dir, bb);
                this.Add(bank);
            }
        }
    }
}