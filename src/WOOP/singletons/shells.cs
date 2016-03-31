using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    public interface IWShells
    {

    }

    public interface IWShell
    {

    }

    public class WShells : List<WShell>, IWShells
    {
        Dictionary<Type, ShellTextures> textures = new Dictionary<Type, ShellTextures>();

        public void tick(uint dt)
        {
            for (int i = 0; i < this.Count; )
            {
                if (!this[i].willRemoved)
                {
                    this[i].tick(dt);
                    i++;
                }
                else
                {
                    this.Remove(this[i]);
                }
            }
        }

        public WShell createShell()
        {
            WShell s = new WShell();
            this.Add(s);
            return s;
        }


        public void render(IWXNAControl g)
        {
            foreach (var s in this) s.render(g);
        }

        public ShellTextures getTextures(Type unitType)
        {
            ShellTextures t;
            if (textures.TryGetValue(unitType, out t))
            {
                return t;
            }
            else
            {
                t = new ShellTextures();
                t.loadFromDir(String.Format("{0}/textures/units/{1}/shell", W.core.path, unitType.Name));
                textures.Add(unitType, t);
                return t;
            }
        }
    }

    public class WShell : IWShell
    {
        public float SpeedMsPerCell { get; private set; }
        public int damagePower { get; set; }
        public int absX { get { return (int)Math.Round(fabsX); } }
        public int absY { get { return (int)Math.Round(fabsY); } }
        public Point targetCell { get; private set; }
        public WUnit parentUnit { get; private set; }
        public bool willRemoved { get {return _willRemoved;} }
        public ShellTextures textures { get; private set; }
        public int animationTimeMs { get; private set; }

        float fabsX;
        float fabsY;
        public float absSpeedX;
        public float absSpeedY;
        int angle;

        bool useAnimatedShell;
        float flightTime = 0;
        public void init(WUnit parentUnit, Point sourceCell, Point targetCell, ShellTextures textures, float SpeedMsPerCell, int damagePower, bool useAnimatedShell)
        {
            if (sourceCell != targetCell)
            {
                this.parentUnit = parentUnit;
                this.targetCell = targetCell;
                this.textures = textures;
                this.SpeedMsPerCell = SpeedMsPerCell;
                this.damagePower = damagePower;
                this.useAnimatedShell = useAnimatedShell;
                animationTimeMs = 200;

                fabsX = sourceCell.X * W.core.gameField.cellSize.Width;
                fabsY = sourceCell.Y * W.core.gameField.cellSize.Height;

                calcParams(sourceCell);
                flightTime = WUtilites.calc2Drange(sourceCell, targetCell) * SpeedMsPerCell;
            }
            else
            {
                W.core.logm("Error: trying to create cell from point to same point!");
                removeLater();
            }
        }
        void calcParams(Point sourceCell)
        {   
            //get params
            int DX = (targetCell.X - sourceCell.X);
            int DY = (targetCell.Y - sourceCell.Y);
            float aDX = Math.Abs(DX);
            float aDY = Math.Abs(DY);

            //calc speed
            absSpeedX = W.core.gameField.cellSize.Width / SpeedMsPerCell;
            absSpeedY = W.core.gameField.cellSize.Height / SpeedMsPerCell;

            if ((aDX == 0) || (aDY == 0))
            {
                if (aDX == 0) absSpeedX = 0;
                if (aDY == 0) absSpeedY = 0;
            }
            else
            {
                if (aDX > aDY)
                {
                    float ratio = aDY / aDX;
                    absSpeedY *= ratio;
                }
                else
                {
                    float ratio = aDX / aDY;
                    absSpeedX *= ratio;
                }
            }

            if (DX < 0) absSpeedX = -absSpeedX;
            if (DY < 0) absSpeedY = -absSpeedY;

            //calc angle
            double ang;
            if (DX != 0)
            {
                double R = Math.Sqrt(DX * DX + DY * DY);
                double cs = (double)DX / R;
                ang = Math.Acos(cs) * 180 / Math.PI;
            }
            else
            {
                ang = 90;
            }

            if (DY < 0) ang = -ang;

            angle = (int)Math.Round(ang);
        }

        void updatePos(uint dt)
        {
            fabsX += absSpeedX * (float)dt;
            fabsY += absSpeedY * (float)dt;
        }


       
        
        bool flight = true;
        public void tick(uint dt)
        {
            if (flight)
            {
                tickFlight(dt);
            }
            else
            {
                tickAnimate(dt);
            }
        }


        float ctrFlight = 0;
        void tickFlight(uint dt)
        {
            updatePos(dt);
            ctrFlight += dt;
            if (ctrFlight >= flightTime)
            {
                onEnd(targetCell);
                if (useAnimatedShell) flight = false;
                else removeLater();
            }
        }

        int animProgress = 0;
        int currAnimFrame = 0;
        void tickAnimate(uint dt)
        {
            animProgress += (int)dt;
            if (animProgress < animationTimeMs)
            {
                int cnt = textures.AnimateTextureFramesCount();
                double fFrame = (double)animProgress * (double)cnt / (double)animationTimeMs;

                currAnimFrame = (int)Math.Round(fFrame);
                if (currAnimFrame >= cnt) currAnimFrame = cnt - 1;
            }
            else
            {
                removeLater();
            }
        }

        public virtual void onEnd(Point cell)
        {
            List<WUnit> uns = W.core.world.UnitsInWorld(cell);
            if (uns.Count > 0)
            {
                uns[0].Damage(parentUnit, damagePower);
            }
        }

        public void render(IWXNAControl g)
        {
            int x = this.absX - (W.core.screenPoint.X * W.core.gameField.cellSize.Width);
            int y = this.absY - (W.core.screenPoint.Y * W.core.gameField.cellSize.Height);
            int w = W.core.gameField.cellSize.Width;
            int h = W.core.gameField.cellSize.Height;

            IWTexture tex;
            if (flight)
            {
                tex = textures.getMainTexture(angle);
            }
            else
            {
                tex = textures.getAnimateTexture(currAnimFrame);
            }
            g.DrawImage(tex, new Rectangle(x, y, w, h));
        }


        bool _willRemoved = false;
        public void removeLater()
        {
            _willRemoved = true;
        }
    }
}