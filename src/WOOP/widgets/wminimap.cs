using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public interface IWMiniMap
    {
        uint pixelPerPoint { get; set; }
        void render();
        List<WGraphicsDebugger> graphicDebuggers { get; }
        Point ScreenToWorld(Point p, ref bool ok);
    }

    public class WMiniMap : WXNAControl, IWMiniMap
	{
        public List<WGraphicsDebugger> graphicDebuggers { get; private set; }

        public uint pixelPerPoint { get; set; }

        public WMiniMap()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            pixelPerPoint = 2;
            graphicDebuggers = new List<WGraphicsDebugger>();

            W.core.registerEventHandlerItem(typeof(WMouseDownEvent), new WGameEventHandler(gameEventMouseDown));
            this.MouseDown += new MouseEventHandler(WMiniMap_MouseDown);
            this.MouseMove += new MouseEventHandler(WMiniMap_MouseMove);

            changedPoints = new List<Point>();
        }

        void WMiniMap_MouseDown(object sender, MouseEventArgs e)
        {
            loge("clicked");
            W.core.emitGameEvent(sender, new WMouseDownEvent { x = e.X, y = e.Y, button = e.Button });

            if (e.Button == MouseButtons.Right)
            {
                bool ok = false;
                Point gp = ScreenToWorld(new Point(e.X, e.Y), ref ok);
                W.core.emitGameEvent(this, new WRightClickEvent { GamePos = gp });
            }
        }

        void WMiniMap_MouseMove(object sender, MouseEventArgs e)
        {
            W.core.emitGameEvent(sender, new WMouseMoveEvent { x = e.X, y = e.Y });
        }

        void gameEventMouseDown(Object sender, WGameEvent e)
        {
            WMouseDownEvent ev = ((WMouseDownEvent)e);
            if (sender is WMiniMap)
            {
                if (ev.button == MouseButtons.Left)
                {
                    loge("Mouse down on" + ev.pos.ToString() + " button " + ev.button);

                    bool ok = false;
                    Point p = ScreenToWorld(((WMouseDownEvent)e).pos, ref ok);
                    p.X -= W.core.gameField.CellsInField.Width/2;
                    p.Y -= W.core.gameField.CellsInField.Height/2; 

                    W.core.players.humanPlayer.screenPoint = p;
                }
            }
        }
        public override void UserDraw()
        {
            render();
        }

		public void render()
		{
            renderContent(this);
            renderSquare(this);
            debugRender(this);
		}

        void debugRender(IWXNAControl g)
        {
            foreach (WGraphicsDebugger dbg in graphicDebuggers)
            {
                if (dbg.enabled)
                {
                    dbg.debugDraw(this, g, this.DisplayRectangle);

                    for (int x = 0; x < W.core.world.Width; x++)
                        for (int y = 0; y < W.core.world.Height; y++)
                        {
                            uint code = W.core.world.getTerrain(x, y);
                            int drawX = x * (int)pixelPerPoint;
                            int drawY = y * (int)pixelPerPoint;

                            dbg.debugDrawCell(this, g, new Point(x, y), new Rectangle(drawX, drawY, (int)pixelPerPoint - 1, (int)pixelPerPoint - 1));
                        }
                }
            }
        }

        public void tick(uint dt)
        {
            updateBuffer();
        }


        public List<Point> changedPoints { get; private set; }
        public bool DoUpdateAll { get; set; }
        Bitmap WFbuff = null;
        XNABitmap buffer = null;
        void updateBuffer()
        {
            if (DoUpdateAll)
            {
                for (int x = 0; x < W.core.world.Width; x++)
                    for (int y = 0; y < W.core.world.Height; y++)
                    {
                        changedPoints.Add(new Point(x, y));
                    }

                DoUpdateAll = false;
            }



            if (changedPoints.Count > 0)
            {
                bool cr = false;
                if (WFbuff == null) cr = true;
                else if (WFbuff.Width != W.core.world.Width * (int)pixelPerPoint) cr = true;
                else if (WFbuff.Height != W.core.world.Height * (int)pixelPerPoint) cr = true;

                if (cr)  WFbuff = new Bitmap(W.core.world.Width * (int)pixelPerPoint, W.core.world.Height * (int)pixelPerPoint);


                using (Graphics gr = Graphics.FromImage(WFbuff))
                {
                    foreach (var p in changedPoints)
                    {
                        Color col = Color.Black;
                        if (W.core.players.watchPlayer.mapIsOpen[p.X, p.Y])
                        {
                            List<WUnit> uns = W.core.world.UnitsInWorld(p);
                        
                            if (uns.Count > 0)
                            {
                                col = uns[0].OwnerPlayer.color;
                            }
                            else
                            {
                                uint code = W.core.world.getTerrain(p.X, p.Y);
                                col = W.core.world.getMinimapPointColor(code);
                            }
                        }

                        gr.FillRectangle(new SolidBrush(col), p.X * (int)pixelPerPoint,
                                        p.Y * (int)pixelPerPoint,
                                        (int)pixelPerPoint,
                                        (int)pixelPerPoint);
                    }
                }

                buffer = new XNABitmap(WFbuff);
                changedPoints.Clear();
            }
        }


        void renderContent(IWXNAControl g)
        {
            if (buffer != null)
                g.DrawImage(buffer, 0, 0, buffer.Width, buffer.Height);  
        }

        void renderSquare(IWXNAControl g)
        {
            g.DrawRectangle(
                new Pen(Color.LightGreen, 2),
                (int)pixelPerPoint * W.core.screenPoint.X,
                (int)pixelPerPoint * W.core.screenPoint.Y,
                (int)pixelPerPoint * (W.core.gameField.CellsInField.Width),
                (int)pixelPerPoint * (W.core.gameField.CellsInField.Height));
        }

        public Point ScreenToWorld(Point p, ref bool ok)
        {
            int mapx = (int)(p.X / this.pixelPerPoint);
            int mapy = (int)(p.Y / this.pixelPerPoint);

            Point res = new Point(mapx, mapy);
            ok = W.core.world.pointInWorld(res);

            if (!ok) logm("Warning: ScreenToWorld returns point out of world!");

            return res;
        }

        String LogTag { get { return "Minimap: "; } }
        void logm(String text) { W.core.textLogs.WidgetsLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
        void loge(String text) { W.core.textLogs.UserEventLog.log(LogTag + text); }
    }
}