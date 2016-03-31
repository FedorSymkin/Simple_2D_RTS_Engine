using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using WOOP;


namespace WOOP
{

    public interface IWWorld
    {
        bool pointInWorld(int x, int y);
        bool pointInWorld(Point pos);
        bool correctUnbounds(ref Point p, bool forRect = false);
        bool correctUnboundsRect(ref Rectangle r);
        uint getTerrain(Point pos);
        uint getTerrain(int x, int y);
        void setTerrain(Point pos, uint value);
        void setTerrain(int x, int y, uint value);
        Size size { set; get; }
        int Width { set; get; }
        int Height { set; get; }
        List<WUnit> UnitsInWorld(Point pos);
        List<WUnit> UnitsInWorld(int x, int y);
        WUnit UnitInWorld(Point pos);
        WUnit UnitInWorld(int x, int y);
        
        void loadFromFile(String filename);
        void loadTextures();

        Color getMinimapPointColor(uint terrainValue);
        void render(IWXNAControl g);
        void tick(uint dt);
	
		//Automatically generated:
		String terrainType(UInt32 terrainValue);
	}

    public class WWorld : IWWorld
    {
        uint[,] terrain;
        List<WUnit>[,] unitsInWorldArray;
        WorldTextures textures = new WorldTextures();
        public WWaterGraph graph = new WWaterGraph();

        public WWorld()
        {
            loadTextures();
        }

        public bool pointInWorld(Point pos) { return pointInWorld(pos.X, pos.Y); }
        public bool pointInWorld(int x, int y)
        {
            return ((x >= 0) && (x < Width) && (y >= 0) && (y < Height));
        }

        public uint getTerrain(Point pos) { return getTerrain(pos.X, pos.Y); }
        public uint getTerrain(int x, int y)
        {
            if (pointInWorld(x,y)) 
            {
                return terrain[x,y];
            }
            else
            {   
                logm("Error: trying to get out-of-bounds terrain");
                return 0;
            }
        }

        public void setTerrain(Point pos, uint value) { setTerrain(pos.X, pos.Y, value); }
        public void setTerrain(int x, int y, uint value)
        {
            if (pointInWorld(x,y)) 
            {
                terrain[x, y] = value;
                W.core.miniMap.changedPoints.Add(new Point(x, y));
                logtodo("Checking for units in setTerrain");
            }
            else logm("Error: trying to set out-of-bounds terrain");           
        }


        int _Width;
        int _Height;
        Size _size = new Size();
        public int Width { get { return _Width; } set { size = new Size(value, Height); } }
        public int Height { get { return _Height; } set { size = new Size(Width, value); } }
        public Size size
        {
            get { return _size; }
            set
            {
                if ((value.Width <= 0) || (value.Height <= 0))
                {
                    logm("Error: invalid size");
                    return;
                }

                terrain = new uint[value.Width, value.Height];
                unitsInWorldArray = new List<WUnit>[value.Width, value.Height];

                for (int x = 0; x < value.Width; ++x)
                    for (int y = 0; y < value.Height; ++y)
                    {
                        unitsInWorldArray[x, y] = new List<WUnit>();
                    }

                logm("World resized to " + value.ToString());

                _size = new Size(terrain.GetLength(0), terrain.GetLength(1));
                _Width = _size.Width;
                _Height = _size.Height;
            }
        }

        public bool correctUnbounds(ref Point p, bool forRect = false)
        {
            Point old = p;
            bool res = false;

            if (p.X < 0) { p.X = 0; res = true; }
            if (p.Y < 0) { p.Y = 0; res = true; }

            if (!forRect)
            {
                if (p.X >= Width) { p.X = Width - 1; res = true; }
                if (p.Y >= Height) { p.Y = Height - 1; res = true; }
            }
            else
            {
                if (p.X > Width) { p.X = Width - 1; res = true; }
                if (p.Y > Height) { p.Y = Height - 1; res = true; }
            }


            if (res) logm(String.Format("Unbounds corrected from {0} to {1}", old, p));
            return res;
        }

        public bool correctUnboundsRect(ref Rectangle r)
        {
            bool res = false;

            Point p1 = WUtilites.TopLeft(r);
            Point p2 = WUtilites.BottomRight(r);

            if (correctUnbounds(ref p1, true)) res = true;
            if (correctUnbounds(ref p2, true)) res = true;

            WUtilites.SetTopLeft(ref r, p1);
            WUtilites.SetBottomRight(ref r, p2);

            return res;
        }


        public List<WUnit> UnitsInWorld(Point pos) { return UnitsInWorld(pos.X, pos.Y); }
        public List<WUnit> UnitsInWorld(int x, int y)
        {
            if (unitsInWorldArray[x, y] == null) unitsInWorldArray[x, y] = new List<WUnit>();
            return unitsInWorldArray[x, y];
        }

        public WUnit UnitInWorld(Point pos) { return UnitInWorld(pos.X, pos.Y); }
        public WUnit UnitInWorld(int x, int y)
        {
            List<WUnit> uns = UnitsInWorld(x, y);
            if (uns.Count == 0) return null;
            else return uns[0];
        }

        bool anotherNeighborTerrain(uint currCode, int x, int y)
        {
            if (pointInWorld(x, y))
            {
                return (terrain[x, y] != currCode);
            }
            return false;
        }

        int getNeighborsSig(int x, int y)
        {
            int res = 0;
            uint code = terrain[x, y];

            if (anotherNeighborTerrain(code, x, y-1)) res |= 0x1;
            if (anotherNeighborTerrain(code, x+1, y)) res |= 0x2;
            if (anotherNeighborTerrain(code, x, y+1)) res |= 0x4;
            if (anotherNeighborTerrain(code, x-1, y)) res |= 0x8;

            return res;
        }


        int[,] rndMatrix;
        void updateRndMatrix()
        {
            uint crc32 = CRC32.calc(terrain);
            Random RGen = new Random((int)crc32);

            rndMatrix = new int[this.Width, this.Height];
            for (int x = 0; x < this.Width; x++)
                for (int y = 0; y < this.Height; y++)
                {
                    rndMatrix[x, y] = RGen.Next();
                }
        }

        public int getRandomOfCell(int x, int y)
        {
            return rndMatrix[x, y];
        }


        public void render(IWXNAControl g)
        {
            logt("World render");

            Rectangle renderWorldRect = W.core.viewRect;
            correctUnboundsRect(ref renderWorldRect);

            logt("renderWorldRect = " + renderWorldRect.ToString());

            for (int x = renderWorldRect.Left; x < renderWorldRect.Right; x++)
            for (int y = renderWorldRect.Top; y < renderWorldRect.Bottom; y++)
            if (W.core.players.watchPlayer.mapIsOpen[x, y])
            {
                uint code = terrain[x, y];
                int sig = getNeighborsSig(x,y);
                int rvl = rndMatrix[x, y];
                IWTexture pict = textures.getTexture((int)code, sig, rvl);

                Size cellSize = W.core.gameField.cellSize;

                int drawX = (x - renderWorldRect.Left) * cellSize.Width;
                int drawY = (y - renderWorldRect.Top) * cellSize.Height;

                g.DrawImage(pict, drawX, drawY, cellSize.Width, cellSize.Height);
            }
        }

        public void tick(uint dt)
        {
            logt("World TICK");
        }

        virtual public String terrainType(uint terrainValue)
        {
            switch (terrainValue)
            {
                case 0: return "allowed";
                default: return "denied";
            }
        }

        public void loadFromFile(String filename)
        {
            Bitmap pict = new Bitmap(filename);
            size = new Size(pict.Width, pict.Height);

            for (int x = 0; x < pict.Width; x++)
                for (int y = 0; y < pict.Height; y++)
                {
                    terrain[x, y] = deciphePixel(pict.GetPixel(x, y));
                }

            logm("World loaded from file: "+filename);
            logm("make water graph...");
            //double s = Math.Sqrt(this.Width * this.Height) / 32;
            int s = 1;
            if (Width * Height >= 130 * 130) s = 2;
            graph.make(this, new WMovingUnit(), s + 1);
            logm("ok");

            W.core.miniMap.DoUpdateAll = true;

            updateRndMatrix();
            foreach (var p in W.core.players) p.updateMapIsOpenArray();
        }

        virtual protected uint deciphePixel(Color pix)
        {
            //TODO: ������� ��-�����������
            if (pix.R < 200) return 1;

            //if (Color.FromArgb(255, 255, 255, 255).Equals(pix)) return 0;
            //if (Color.FromArgb(255, 0, 0, 0).Equals(pix)) return 1;

            return 0;
        }

        public void loadTextures()
        {
            String dirName = String.Format("{0}/textures/world/{1}", W.core.path, this.GetType().Name);
            textures.loadFromDir(dirName);
            logm("World textured loaded ");
        }

        public Color getMinimapPointColor(uint terrainValue)
        {
            //logtodo("make World.getMinimapPointColor correctly (as middle color)");

            switch (terrainValue)
            {
                case 0: return Color.Green;
                case 1: return Color.Blue;
            }

            return Color.White;
        }

        String LogTag { get { return String.Format("World {0}: ", this.GetType().ToString()); } }
        void logm(String text) {W.core.textLogs.MainWorldLog.log(LogTag + text);}
        void logtodo(String text) { W.core.textLogs.TODOLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    
    }
}