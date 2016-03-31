using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Windows.Forms;



namespace WOOP
{
    public interface IWGameField
    {
        void render();

        Size CellsInField {set; get;}
        Size cellSize { get; }

        Point ScreenToWorld(Point p, ref bool ok);
        Point WorldToScreen(Point p, ref bool ok);
        Point WorldToScreenCellCenter(Point p, ref bool ok);
        Rectangle ScreenToWorldRect(Rectangle rect, ref bool ok);

        bool WorldPointInScreen(Point p);

        List<WGraphicsDebugger> graphicDebuggers { get; }
    }

    public class WGameField : WXNAControl, IWGameField
	{
        public List<WGraphicsDebugger> graphicDebuggers { get; private set; }

        WorldTextures DarkCoverTextures;

        public WGameField()
        {
            this.BorderStyle = BorderStyle.FixedSingle;

            CellsInField = new Size(20,20);
            graphicDebuggers = new List<WGraphicsDebugger>();

            this.MouseDown +=new MouseEventHandler(WGameField_MouseDown);
            this.MouseUp +=new MouseEventHandler(WGameField_MouseUp);
            this.MouseMove +=new MouseEventHandler(WGameField_MouseMove);

            DarkCoverTextures = new WorldTextures();
            DarkCoverTextures.loadFromDir(W.core.path + "/textures/darkcover");
        }

        
        void WGameField_MouseDown(Object sender, MouseEventArgs e)
        {
            W.core.emitGameEvent(this, new WMouseDownEvent { x = e.X, y = e.Y, button = e.Button });
            
            if (e.Button == MouseButtons.Left) startRubberBand(e.X, e.Y);

            if (e.Button == MouseButtons.Right)
            {
                bool ok = false;
                Point gp = ScreenToWorld(new Point(e.X, e.Y), ref ok);
                W.core.emitGameEvent(this, new WRightClickEvent { GamePos = gp });
            }
        }

        void WGameField_MouseMove(Object sender, MouseEventArgs e)
        { 
            W.core.emitGameEvent(this, new WMouseMoveEvent { x = e.X, y = e.Y });
            if (e.Button == MouseButtons.Left) changeRubberBand(e.X, e.Y);
        }

        void WGameField_MouseUp(Object sender, MouseEventArgs e)
        {
            W.core.emitGameEvent(this, new WMouseUpEvent { x = e.X, y = e.Y });
            if (e.Button == MouseButtons.Left) endRubberBand(e.X, e.Y);
        }

        int rx, ry;
        void startRubberBand(int X, int Y)
        {
            rubberBandVisible = true;
            rx = X;
            ry = Y;

            rubberBand.X = X;
            rubberBand.Y = Y;

            rubberBand.Width = 0;
            rubberBand.Height = 0;
        }

        void changeRubberBand(int X, int Y)
        {
            if (rubberBandVisible)
            {
                int w = X - rx;
                int h = Y - ry;

                if (w < 0)
                {
                    w = -w;
                    rubberBand.X = X;
                }
                else rubberBand.X = rx;

                if (h < 0)
                {
                    h = -h;
                    rubberBand.Y = Y;
                }
                else rubberBand.Y = ry;

                rubberBand.Width = w;
                rubberBand.Height = h;
            }
        }

        void endRubberBand(int X, int Y)
        {
            rubberBandVisible = false;

            bool ok = false;
            Rectangle gameRect = ScreenToWorldRect(rubberBand, ref ok);
            W.core.world.correctUnboundsRect(ref gameRect);

            W.core.emitGameEvent(this, new WRubberBandEvent { rect = gameRect });
        }


        public override void UserDraw()
        {
            render();
        }
        
        
        public void render()
        {
            W.core.world.render(this);
            W.core.units.render(this);
            darkCoverRender(this);
            debugRender(this);
            W.core.shells.render(this);
            rubberBandRender(this); 
        }



        void darkCoverRender(IWXNAControl g)
        {
            for (int x = W.core.viewRect.Left; x < W.core.viewRect.Right; x++)
                for (int y = W.core.viewRect.Top; y < W.core.viewRect.Bottom; y++)
                {
                    int sig = 0;
                    if (W.core.players.watchPlayer.mapIsOpen[x, y])
                    {
                        if (!W.core.players.watchPlayer.openedPoint(x, y - 1)) sig |= 0x1;
                        if (!W.core.players.watchPlayer.openedPoint(x + 1, y)) sig |= 0x2;
                        if (!W.core.players.watchPlayer.openedPoint(x, y + 1)) sig |= 0x4;
                        if (!W.core.players.watchPlayer.openedPoint(x - 1, y)) sig |= 0x8;
                    }
                    if (sig != 0)
                    {
                        IWTexture tex = DarkCoverTextures.getTexture(0, sig, W.core.world.getRandomOfCell(x,y));
                        int drawX = (x - W.core.viewRect.Left) * cellSize.Width;
                        int drawY = (y - W.core.viewRect.Top) * cellSize.Height;
                        g.DrawImage(tex, drawX, drawY, cellSize.Width, cellSize.Height); 
                    }       
                }
        }


        Rectangle rubberBand = new Rectangle();
        bool rubberBandVisible = false;
        void rubberBandRender(IWXNAControl g)
        {
           if (rubberBandVisible)     
                g.DrawRectangle(new Pen(Color.Blue), rubberBand);            
        }

        void debugRender(IWXNAControl g)
        {
            logt("Game field debug draw");
            foreach (WGraphicsDebugger dbg in graphicDebuggers)
            {
                if (dbg.enabled)
                {
                    logt("DRAW...");
                    dbg.debugDraw(this, g, this.DisplayRectangle);

                    Rectangle renderWorldRect = W.core.viewRect;
                    W.core.world.correctUnboundsRect(ref renderWorldRect);

                    for (int x = renderWorldRect.Left; x < renderWorldRect.Right; x++)
                        for (int y = renderWorldRect.Top; y < renderWorldRect.Bottom; y++)
                        {
                            uint code = W.core.world.getTerrain(x, y);
                            Size cellSize = W.core.gameField.cellSize;

                            int drawX = (x - renderWorldRect.Left) * cellSize.Width;
                            int drawY = (y - renderWorldRect.Top) * cellSize.Height;

                            dbg.debugDrawCell(this, g, new Point(x, y), new Rectangle(drawX, drawY, cellSize.Width, cellSize.Height));
                        }
                }
            }
        }

        public Size CellsInField { set; get; }
        public Size cellSize
        {
            get
            {
                return new Size(this.Width / CellsInField.Width, this.Height / CellsInField.Height);
            }
        }

        public Point ScreenToWorld(Point p, ref bool ok)
        {
            Point srceenPoint = W.core.players.humanPlayer.screenPoint;

            int dX = p.X / cellSize.Width;
            int dY = p.Y / cellSize.Height;

            int pX = srceenPoint.X + dX;
            int pY = srceenPoint.Y + dY;

            Point res = new Point(pX, pY);
            ok = W.core.world.pointInWorld(res);

            if (!ok) logm("Warning: ScreenToWorld returns point out of world!");

            return res;
        }

        public Rectangle ScreenToWorldRect(Rectangle rect, ref bool ok)
        {
            ok = true;
            bool localOk = false;
            Point p1 = this.ScreenToWorld(WUtilites.TopLeft(rect), ref localOk); if (!localOk) ok = false;
            Point p2 = this.ScreenToWorld(WUtilites.BottomRight(rect), ref localOk); if (!localOk) ok = false;

            return WUtilites.SetRectXYXY(p1, p2);
        }

        public Point WorldToScreen(Point p, ref bool ok)
        {
            Point srceenPoint = W.core.players.humanPlayer.screenPoint;

            int dX = p.X - srceenPoint.X;
            int dY = p.Y - srceenPoint.Y;

            int scrX = dX * cellSize.Width;
            int scrY = dY * cellSize.Height;

            ok = (
                (scrX > 0) &&
                (scrY > 0) &&
                (scrX < this.Width) &&
                (scrY < this.Height)
               );

            if (!ok) logm("Warning: WorldToScreen returns point out of screen!");

            return new Point(scrX, scrY);
        }


        public Point WorldToScreenCellCenter(Point p, ref bool ok)
        {
            Point tmpp = WorldToScreen(p, ref ok);
            tmpp.X += cellSize.Width / 2;
            tmpp.Y += cellSize.Height / 2;
            return tmpp;
        }

        public bool WorldPointInScreen(Point p)
        {
            bool res = false;

            WorldToScreen(p, ref res);
            return res;
        }

        String LogTag { get { return "Game field: "; } }
        void logm(String text) { W.core.textLogs.GameFieldLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    }
}